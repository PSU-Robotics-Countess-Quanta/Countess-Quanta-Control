// Brad Pitney
// ECE 510
// Spring 2014

// AriaManager uses a modified ARIA dll to connect to the Pioneer 2 robot which 
// serves as the wheeled base for Countess Quanta.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CountessQuantaControl
{
    public class AriaManager
    {
        // This section defines the methods that will be used in the modified ARIA dll.

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int InitializeRobot_dll();

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DisconnectRobot_dll();

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsConnected_dll();

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setVel_dll(double velocity);

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setVel2_dll(double leftVelocity, double rightVelocity);

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setRotVel_dll(double velocity);


        Object stopTimerLock = new Object();
        bool isStopping = false;
        System.Timers.Timer stopWheelsTimer = new System.Timers.Timer(1000);
        EventWaitHandle waitHandle = new AutoResetEvent(false);
        Object movePriorityLock = new Object();
        bool isRunningHighPriorityMove = false;
        Object ariaLock = new Object();
        bool isEnabled = true;
        bool isHardwareInitialized = false;

        public void InitializeAria()
        {
            stopWheelsTimer.AutoReset = false;
            stopWheelsTimer.Elapsed += OnStopWheelsTimerEvent;
        }

        public void InitializeAriaHardware()
        {
            // Connect to Aria hardware
            int hr = InitializeRobot_dll();
            switch (hr)
            {
                case 0:
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "InitializeAria() succeeded, connected to robot base control.");
                    break;
                case 1:
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "InitializeAria() failed, could not parse arguments.");
                    break;
                case 2:
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "InitializeAria() failed, could not connect to robot base.");
                    break;
            }

            isHardwareInitialized = true;
        }

        // [Need to hook this up]
        public void CloseAria()
        {
            StopTimer();
            StopMotion();
            DisconnectRobot_dll();
        }

        public bool IsConnected()
        {
            if (!isHardwareInitialized)
            {
                return false;
            }

            return IsConnected_dll();
        }

        public void EnableMotion()
        {
            lock (ariaLock)
            {
                isEnabled = true;
            }
        }

        public void DisableMotion()
        {
            lock (ariaLock)
            {
                isEnabled = false;
            }

            StopMotion();
        }

        public bool IsEnabled()
        {
            lock (ariaLock)
            {
                return isEnabled;
            }
        }

        // Sets the translation velocity (mm/s) using both wheels.
        private void SetVelocity(double velocity)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting wheel translation velocity to " + velocity.ToString());

            if (IsConnected())
            {
                lock (ariaLock)
                {
                    if (!isEnabled)
                    {
                        return;
                    }

                    //ArRobot::setVel(velocity);

                    setVel_dll(velocity);
                }
            }
        }

        // Sets the rotation velocity (deg/s) using both wheels.
        private void SetRotationVelocity(double velocity)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting wheel rotation velocity to " + velocity.ToString());

            if (IsConnected())
            {
                lock (ariaLock)
                {
                    if (!isEnabled)
                    {
                        return;
                    }

                    //ArRobot::setRotVel(velocity);

                    setRotVel_dll(velocity);
                }
            }
        }

        // Sets the velocity (mm/s) of each wheel separately.
        private void SetWheelVelocity(double leftWheelVelocity, double rightWheelVelocity)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting left wheel velocity to " + leftWheelVelocity.ToString() + " and right wheel velocity to " + rightWheelVelocity.ToString());

            if (IsConnected())
            {
                lock (ariaLock)
                {
                    if (!isEnabled)
                    {
                        return;
                    }

                    //ArRobot::setVel2(leftWheelVelocity, rightWheelVelocity);

                    setVel2_dll(leftWheelVelocity, rightWheelVelocity);
                }
            }
        }

        // Stops the current motion of the wheels.
        private void StopMotion()
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Stopping wheel motion.");

            if (IsConnected())
            {
                lock (ariaLock)
                {
                    // Always stop wheels, regardless of isEnabled flag.

                    setVel_dll(0);
                    setRotVel_dll(0);
                }
            }
        }

        // Moves both of the wheels to achieve the specified translation (in millimeters) and rotation (in degrees)
        // over the time specified by timeToDestination (in seconds).
        public void MoveBothWheels(double translationChange, double rotationChange, double timeToDestination)
        {
            if (!IsEnabled())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "MoveBothWheels failed, motion is disabled.");
                return;
            }

            lock (movePriorityLock)
            {
                isRunningHighPriorityMove = true;
            }

            double translationVelocity = translationChange / timeToDestination;
            double rotationVelocity = rotationChange / timeToDestination;

            // Robot base is backwards with respect to torso, so translation velocity must be reversed.
            translationVelocity = -translationVelocity;

            StopTimer();

            // Start wheel motion.
            SetVelocity(translationVelocity);
            SetRotationVelocity(rotationVelocity);

            // Wait for motion to complete.
            StartTimer(timeToDestination);
            waitHandle.WaitOne();

            lock (movePriorityLock)
            {
                isRunningHighPriorityMove = false;
            }
        }

        // Moves the wheels to achieve the specified change in left and right wheel positions (in millimeters) 
        // over the time specified by timeToDestination (in seconds).
        public void MoveEachWheel(double leftWheelChange, double rightWheelChange, double timeToDestination)
        {
            if (!IsEnabled())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "MoveEachWheel failed, motion is disabled.");
                return;
            }

            lock (movePriorityLock) 
            {
                isRunningHighPriorityMove = true;
            }

            double leftWheelVelocity = leftWheelChange / timeToDestination;
            double rightWheelVelocity = rightWheelChange / timeToDestination;

            // Robot base is backwards with respect to torso, so wheels are swapped (right is left) and velocity is reversed.
            double newLeftWheelVelocity = -rightWheelVelocity;
            double newRightWheelVelocity = -leftWheelVelocity;

            StopTimer();

            // Start wheel motion.
            SetWheelVelocity(newLeftWheelVelocity, newRightWheelVelocity);

            // Wait for motion to complete.
            StartTimer(timeToDestination);
            waitHandle.WaitOne();

            lock (movePriorityLock)
            {
                isRunningHighPriorityMove = false;
            }
        }

        private void StartTimer(double timeToDestination)
        {
            // Start timer.
            stopWheelsTimer.Interval = timeToDestination * 1000;    // Set timer interval in milliseconds.
            stopWheelsTimer.Enabled = true;
        }

        private void StopTimer()
        {
            // Stop timer.
            stopWheelsTimer.Stop();

            // If timer event is already running, wait for it to complete.
            while (true)
            {
                lock (stopTimerLock)
                {
                    if (!isStopping)
                    {
                        // Not in the middle of event, so just return.
                        return;
                    }
                }

                // Event is being processed, so wait and check again.
                Thread.Sleep(10);
            }
        }

        private void OnStopWheelsTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            lock (stopTimerLock)
            {
                isStopping = true;
            }

            StopMotion();
            waitHandle.Set();

            lock (stopTimerLock)
            {
                isStopping = false;
            }
        }

        // Used by PersonTracking to track a person by rotating the robot. This tracking motion can be 
        // interrupted by moves issued through Sequences, since these are higher priority.
        public void SetPersonTrackingRotation(double velocity)
        {
            if (!IsEnabled())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "SetPersonTrackingRotation failed, motion is disabled.");
                return;
            }

            lock (movePriorityLock)
            {
                if (isRunningHighPriorityMove)
                {
                    // Ignore this command if a higher priority (i.e. Sequence) move is already running.
                    return;
                }

                SetRotationVelocity(velocity);
            }
        }
    }
}
