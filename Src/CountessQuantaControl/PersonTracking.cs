// Brad Pitney
// ECE 510
// Spring 2014

// PersonTracking handles the feature of the robot turning its body/head/eyes to 
// face a person as they move in front of the Kinect.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Threading;

namespace CountessQuantaControl
{
    public class PersonTracking
    {
        ServoManager servoManager;
        AriaManager ariaManager;
        System.Timers.Timer wheelWatchdogTimer = new System.Timers.Timer(1000);

        public PersonTracking(ServoManager servoManager, AriaManager ariaManager)
        {
            this.servoManager = servoManager;
            this.ariaManager = ariaManager;

            wheelWatchdogTimer.AutoReset = false;
            wheelWatchdogTimer.Elapsed += OnWheelWatchdogTimerEvent;
        }

        DateTime lastUpdateTime = new DateTime();
        Object personTrackingLock = new Object();
        bool isAlreadyUpdatingTracking = false;
        bool trackingEnabled = false;

        // Moves the robot's head/eyes to track the specified skeleton joint.
        public void UpdatePosition(SkeletonPoint targetPosition)
        {
            // Only update if tracking is allowed and if we're not already updating in a 
            // different thread.
            lock (personTrackingLock)
            {
                if (!trackingEnabled)
                {
                    return;
                }

                if (isAlreadyUpdatingTracking)
                {
                    return;
                }
                else
                {
                    isAlreadyUpdatingTracking = true;
                }
            }

            // Only update once every 100ms, even if we're receiving new joint positions 
            // more frequently than this.
            TimeSpan timeBetweenUpdates = new TimeSpan(0, 0, 0, 0, 100);

            if (DateTime.Now - lastUpdateTime < timeBetweenUpdates)
            {
                lock (personTrackingLock)
                {
                    isAlreadyUpdatingTracking = false;
                }

                return;
            }

            lastUpdateTime = DateTime.Now;

            // Position of the robot neck and eye servos at which the robot faces straight forward.
            const double servoCenterPostion_HeadHorizontal = 1550;
            const double servoCenterPostion_HeadVertical = 2000;
            const double servoCenterPostion_Eyes = 1600;

            const double servoOffset_HeadVertical = 0;

            // Number of servo position increments per radian of rotation for each of the servos.
            const double servoIncrementsPerRadian_HeadHorizontal = 800;
            const double servoIncrementsPerRadian_HeadVertical = 1000;
            const double servoIncrementsPerRadian_Eyes = 800;

            // Tracking speed of each servo.
            const long servoSpeed_HeadHorizontal = 30;
            const long servoSpeed_HeadVertical = 30;
            const long servoSpeed_Eyes = 1000;

            // Wheeled base tracking parameters.
            const double baseRotationSpeed = 5;
            const double trackingThreshold = 0.1;

            if (servoManager.IsConnected())
            {
                servoManager.UpdateServoValues();
            }

            //Servo headHorizontalServo = servoList.Find(x => x.index == 10);
            //if (headHorizontalServo == null)
            //{
            //    lock (personTrackingLock)
            //    {
            //        isAlreadyUpdatingTracking = false;
            //    }

            //    // Log Error
            //    return;
            //}

            // Calculate the current position of the head.
            //double currentPosition_HeadHorizontal = headHorizontalServo.polledPosition / 4.0;
            //double currentAngle_HeadHorizontal = (servoCenterPostion_HeadHorizontal - currentPosition_HeadHorizontal) / servoIncrementsPerRadian_HeadHorizontal;

            // Calculate the angle to the target joint.
            double targetAngle_Horizontal = Math.Atan(targetPosition.X / targetPosition.Z);
            double targetAngle_Vertical = Math.Atan(targetPosition.Y / targetPosition.Z);

            // Calculate the new head position to face this target joint.
            double newServoPosition_HeadHorizontal = servoCenterPostion_HeadHorizontal - targetAngle_Horizontal * servoIncrementsPerRadian_HeadHorizontal;
            double newServoPosition_HeadVertical = servoCenterPostion_HeadVertical + servoOffset_HeadVertical - targetAngle_Vertical * servoIncrementsPerRadian_HeadVertical;

            // Eye position with head at center.
            double newServoPosition_Eyes = servoCenterPostion_Eyes + targetAngle_Horizontal * servoIncrementsPerRadian_Eyes;

            // Eye position based on current head position.
            //double newServoPosition_Eyes = servoCenterPostion_Eyes + (targetAngle_Horizontal - currentAngle_HeadHorizontal) * servoIncrementsPerRadian_Eyes;

            //ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "PersonTrackingUpdate: targetAngle_Horizontal = " + targetAngle_Horizontal + ", newServoPosition_HeadHorizontal = " + newServoPosition_HeadHorizontal + ", newServoPosition_Eyes = " + newServoPosition_Eyes);


            if (servoManager.IsConnected())
            {
                // Update with head motion.
                //servoManager.StartSpeedMove(new ServoPosition(10, newServoPosition_HeadHorizontal), servoSpeed_HeadHorizontal);
                //servoManager.StartSpeedMove(new ServoPosition(9, newServoPosition_HeadVertical), servoSpeed_HeadVertical);

                // Update with only eye motion.
                //StartSpeedMove(new ServoPosition(10, servoCenterPostion_HeadHorizontal), servoSpeed_HeadHorizontal);
                //StartSpeedMove(new ServoPosition(9, servoCenterPostion_HeadVertical), servoSpeed_HeadVertical);
                //StartSpeedMove(new ServoPosition(8, newServoPosition_Eyes), servoSpeed_Eyes);
            }

            if (ariaManager.IsConnected())
            {
                if (Math.Abs(targetAngle_Horizontal) > trackingThreshold)
                {
                    double trackingVelocity = 0;

                    if (targetAngle_Horizontal > 0)
                    {
                        trackingVelocity = baseRotationSpeed;
                    }
                    else
                    {
                        trackingVelocity = -baseRotationSpeed;
                    }

                    ariaManager.SetPersonTrackingRotation(trackingVelocity);

                    // Restart watchdog timer.
                    wheelWatchdogTimer.Stop();
                    wheelWatchdogTimer.Start();
                }
                else
                {
                    ariaManager.SetPersonTrackingRotation(0);
                    wheelWatchdogTimer.Stop();
                }
            }

            lock (personTrackingLock)
            {
                isAlreadyUpdatingTracking = false;
            }
        }

        // Stop wheel motion automatically if a skeleton position update is not made within the specified time.
        private void OnWheelWatchdogTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            ariaManager.SetPersonTrackingRotation(0);
        }

        // Enable or disable person tracking feature.
        public void Enable(bool enableTracking)
        {
            lock (personTrackingLock)
            {
                trackingEnabled = enableTracking;

                if (enableTracking)
                {
                    return;
                }

                if (!isAlreadyUpdatingTracking)
                {
                    ariaManager.SetPersonTrackingRotation(0);
                    wheelWatchdogTimer.Stop();
                    return;
                }
            }

            // If tracking was disabled, but is currently updating, then wait for last update to complete.
            bool updateIsComplete = false;
            while (!updateIsComplete)
            {
                Thread.Sleep(10);

                lock (personTrackingLock)
                {
                    updateIsComplete = !isAlreadyUpdatingTracking;
                }
            }

            ariaManager.SetPersonTrackingRotation(0);
            wheelWatchdogTimer.Stop();
        }
    }
}
