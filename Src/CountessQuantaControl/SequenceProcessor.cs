// Brad Pitney
// ECE 579
// Winter 2014

// SequenceProcessor handles reading the list of motion sequences from an xml file, and
// executing the selected sequences through the servo manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Kinect;

namespace CountessQuantaControl
{
    public class SequenceProcessor
    {
        ServoManager servoManager;
        AriaManager ariaManager;
        RobotSpeech robotSpeech;
        RobotEyes robotEyes;
        SequenceList sequenceList;
        Object sequenceLock = new Object();
        bool sequenceIsRunning = false;
        Sequence runningSequence;

        public SequenceProcessor(ServoManager servoManager, AriaManager ariaManager, RobotSpeech robotSpeech, RobotEyes robotEyes, string sequenceFileName)
        {
            this.servoManager = servoManager;
            this.ariaManager = ariaManager;
            this.robotSpeech = robotSpeech;
            this.robotEyes = robotEyes;
            sequenceList = new SequenceList(sequenceFileName);
        }

        public void LoadSequenceFile(string fileName)
        {
            sequenceList.LoadFromXml(fileName);
        }

        public List<string> GetSequenceList()
        {
            List<Sequence> listOfSequences = sequenceList.GetSequences();
            List<string> listOfSequenceNames = new List<string>();

            foreach (Sequence sequence in listOfSequences)
            {
                listOfSequenceNames.Add(sequence.name);
            }

            return listOfSequenceNames;
        }

        // Starts running the selected sequence in a new thread. Fails if another sequence 
        // is already running.
        public void RunSequence(string sequenceName)
        {
            Sequence sequence = sequenceList.GetSequences().Find(x => x.name == sequenceName);

            if (sequence == null)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Sequence '" + sequenceName + "' not found.");
                return;
            }

            lock (sequenceLock)
            {
                if (sequenceIsRunning)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "RunSequence() failed to run sequence '" + sequenceName + "', another sequence is already running.");
                    return;
                }
                else
                {
                    sequenceIsRunning = true;
                }
            }

            runningSequence = sequence;
            Thread newThread = new Thread(new ThreadStart(RunSequenceThread));

            // Start the thread
            newThread.Start();
        }

        // This class is used to pass frame and event objects to the servo and wheel motion threads.
        private class SequenceThreadData
        {
            public Frame frame;
            public ManualResetEvent resetEvent;

            public SequenceThreadData(Frame frame, ManualResetEvent resetEvent)
            {
                this.frame = frame;
                this.resetEvent = resetEvent;
            }
        }

        // Performs the sequence by stepping through each frame and executing the required 
        // voice synthesizer, delay, and motion actions.
        private void RunSequenceThread()
        {
            if (!servoManager.IsConnected())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Servo hardware is disconnected, running servos in simulation mode for sequence '" + runningSequence.name + "'.");
            }

            if (!ariaManager.IsConnected())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Robot base hardware is disconnected, running robot base in simulation mode for sequence '" + runningSequence.name + "'.");
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "RunSequenceThread(): sequence '" + runningSequence.name + "' started.");

            foreach (Frame frame in runningSequence.GetFrames())
            {
                // Run the speech synthesizer asynchronously. Speaks while continuing to 
                // perform moves in this and subsequent frames.
                if (frame.speechString != null)
                {
                    robotSpeech.Speak(frame.speechString);
                }

                // Set the state of the LED eyes.
                if (frame.eyeState != null)
                {
                    switch (frame.eyeState)
                    {
                        case "Open":
                            robotEyes.SetEyeState(true, true);
                            break;
                        case "Closed":
                            robotEyes.SetEyeState(false, false);
                            break;
                        case "LeftClosed":
                            robotEyes.SetEyeState(false, true);
                            break;
                        case "RightClosed":
                            robotEyes.SetEyeState(true, false);
                            break;
                    }
                }

                // Wait for the specified amount of time.
                if (frame.delay > 0)
                {
                    Thread.Sleep((int)(frame.delay * 1000));
                }


                // Servo motion and wheel motion are performed simultaneously by creating two separate threads and 
                // waiting until both threads have completed their motion.

                ManualResetEvent[] manualEvents = new ManualResetEvent[]
                { 
                    new ManualResetEvent(false), 
                    new ManualResetEvent(false)
                };

                // Start servo motion thread.
                Thread servoThread = new Thread(new ParameterizedThreadStart(MoveServos));
                servoThread.Start(new SequenceThreadData(frame, manualEvents[0]));

                // Start wheel motion thread.
                Thread wheelsThread = new Thread(new ParameterizedThreadStart(MoveWheels));
                wheelsThread.Start(new SequenceThreadData(frame, manualEvents[1]));

                // Wait until both servo motion and wheel motion has completed.
                WaitHandle.WaitAll(manualEvents);
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "RunSequenceThread(): sequence '" + runningSequence.name + "' completed.");

            lock (sequenceLock)
            {
                sequenceIsRunning = false;
            }
        }

        // Performs servo motions for the current frame, and triggers an event when motion is complete.
        private void MoveServos(object sequenceThreadData)
        {
            Frame frame = ((SequenceThreadData)sequenceThreadData).frame;

            // Move all servos in the frame list.
            if (frame.GetServoPositions().Count > 0)
            {
                servoManager.MoveServos(frame.GetServoPositions(), frame.timeToDestination);
            }

            // Signal that motion is complete.
            ((SequenceThreadData)sequenceThreadData).resetEvent.Set();
        }

        // Performs wheel motions for the current frame, and triggers an event when motion is complete.
        private void MoveWheels(object sequenceThreadData)
        {
            Frame frame = ((SequenceThreadData)sequenceThreadData).frame;

            // Move wheels on the robot base. Translation/rotation moves and individual wheel moves are mutually exclusive, 
            // so we prioritize the translation/rotation moves and ignore individual wheel moves, if translation/rotation is specified.
            if (frame.wheelMove.translation != 0 || frame.wheelMove.rotation != 0)
            {
                ariaManager.MoveBothWheels(frame.wheelMove.translation, frame.wheelMove.rotation, frame.timeToDestination);
            }
            else if (frame.wheelMove.leftWheel != 0 || frame.wheelMove.rightWheel != 0)
            {
                ariaManager.MoveEachWheel(frame.wheelMove.leftWheel, frame.wheelMove.rightWheel, frame.timeToDestination);
            }

            // Signal that motion is complete.
            ((SequenceThreadData)sequenceThreadData).resetEvent.Set();
        }

    }
}
