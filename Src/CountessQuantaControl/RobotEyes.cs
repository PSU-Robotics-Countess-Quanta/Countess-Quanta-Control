// Brad Pitney
// ECE 510
// Spring 2014

// RobotEyes handles control of the robot's LED eyes. It allows for opening 
// and closing each eye (turning LEDs on and off), and provides automated blinking.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CountessQuantaControl
{
    public class RobotEyes
    {
        bool leftEyeOpenState = true;
        bool rightEyeOpenState = true;
        bool blinkInProgress = false;
        Random blinkTimer = new Random();
        Object eyeStateLock = new Object();

        public void InitializeHardware()
        {
            // Initialize both eyes to 'Open'.
            SetEyeState(true, true);

            // Start blinking thread.
            Thread newThread = new Thread(new ThreadStart(Blinking));
            newThread.Start();
        }

        public void SetEyeState(bool leftEyeOpen, bool rightEyeOpen)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "SetEyeState setting left eye " + (leftEyeOpen ? "Open" : "Closed") + " and right eye " + (rightEyeOpen ? "Open" : "Closed") + ".");

            lock (eyeStateLock)
            {
                leftEyeOpenState = leftEyeOpen;
                rightEyeOpenState = rightEyeOpen;

                if (!blinkInProgress)
                {
                    SetHardwareState(leftEyeOpen, rightEyeOpen);
                }
            }
        }

        private void SetHardwareState(bool leftEyeOpen, bool rightEyeOpen)
        {
            // [Add call to hardware]

            //ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "SetHardwareState set left eye " + (leftEyeOpen ? "Open" : "Closed") + " and right eye " + (rightEyeOpen ? "Open" : "Closed") + ".");
        }

        // Blinking variance.
        const double minSecondsBetweenBlinks = 2;
        const double maxSecondsBetweenBlinks = 7;

        private void Blinking()
        {
            while (true)
            {
                int randomTimeUntilNextBlink = blinkTimer.Next((int)(minSecondsBetweenBlinks * 1000), (int)(maxSecondsBetweenBlinks * 1000));
                Thread.Sleep(randomTimeUntilNextBlink);

                //ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Blinking() triggered after " + randomTimeUntilNextBlink.ToString() + "ms, blinking the eyes...");

                lock (eyeStateLock)
                {
                    blinkInProgress = true;
                }

                // Close eyes.
                SetHardwareState(false, false);

                Thread.Sleep(100);

                lock (eyeStateLock)
                {
                    // Restore previous state.
                    SetHardwareState(leftEyeOpenState, rightEyeOpenState);

                    blinkInProgress = false;
                }
            }
        }
    }
}
