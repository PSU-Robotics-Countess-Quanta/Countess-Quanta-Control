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
using System.Speech.Synthesis;

namespace CountessQuantaControl
{
    public class SequenceProcessor
    {
        ServoManager servoManager;
        SequenceList sequenceList;
        Object sequenceLock = new Object();
        bool sequenceIsRunning = false;
        Sequence runningSequence;
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public SequenceProcessor(ServoManager servoManager, string sequenceFileName)
        {
            this.servoManager = servoManager;
            sequenceList = new SequenceList(sequenceFileName);
        }

        public void LoadSequenceFile(string fileName)
        {
            sequenceList.LoadFromXml(fileName);
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
            Thread oThread = new Thread(new ThreadStart(RunSequenceThread));

            // Start the thread
            oThread.Start();
        }

        // Performs the sequence by stepping through each frame and executing the required 
        // voice synthesizer, delay, and motion actions.
        private void RunSequenceThread()
        {
            if (!servoManager.IsConnected())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Servo hardware is disconnected, running sequence '" + runningSequence.name + "' in simulation mode.");
            }

            // Disable the person tracking feature while a sequence is executing.
            servoManager.PersonTrackingEnable(false);

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "RunSequence(): sequence '" + runningSequence.name + "' started.");

            foreach (Frame frame in runningSequence.GetFrames())
            {
                // Run the speech synthesizer asynchronously. Speaks while continuing to 
                // perform moves in this and subsequent frames.
                if (frame.speechString != null)
                {
                    speechSynthesizer.SpeakAsync(frame.speechString);
                }

                // Wait for the specified amount of time.
                if (frame.delay > 0)
                {
                    Thread.Sleep((int)(frame.delay * 1000));
                }

                // Move all servos in the frame list.
                if (frame.GetServoPositions().Count > 0)
                {
                    servoManager.MoveServos(frame.GetServoPositions(), frame.timeToDestination);
                }
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "RunSequence(): sequence '" + runningSequence.name + "' completed.");

            // Re-enable the person tracking feature.
            servoManager.PersonTrackingEnable(true);

            lock (sequenceLock)
            {
                sequenceIsRunning = false;
            }
        }

        public void PersonTrackingUpdate(SkeletonPoint targetPosition)
        {
            servoManager.PersonTrackingUpdate(targetPosition);
        }

        public event EventHandler<SpeakStartedEventArgs> SpeakStarted
        {
            add { speechSynthesizer.SpeakStarted += value; }
            remove { speechSynthesizer.SpeakStarted -= value; }
        }

        public event EventHandler<SpeakCompletedEventArgs> SpeakCompleted
        {
            add { speechSynthesizer.SpeakCompleted += value; }
            remove { speechSynthesizer.SpeakCompleted -= value; }
        }
    }
}
