// Brad Pitney
// ECE 579
// Winter 2014

// ServoManager handles all communication with the Pololu Maestro servo controller. It implements
// methods for changing servo settings and performing moves with multiple servos.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using Pololu.Usc;
using Pololu.UsbWrapper;
using Microsoft.Kinect;

namespace CountessQuantaControl
{
    public class ServoManager
    {
        // This is a storage class that holds data related to a single robot servo.
        public class Servo
        {
            // The servo index used by the Pololu servo controller.
            public long index;

            // A descriptive name for the servo, for when it is referenced in log messages, etc.
            public string name;

            // The limits used to bound selected positions and speeds.
            public double positionLimitMin;
            public double positionLimitMax;
            public long speedLimitMin;
            public long speedLimitMax;

            // The default servo settings.
            public double defaultPosition;
            public long defaultSpeed;
            public long defaultAcceleration;


            // These are the raw servo state values read from the Pololu servo controller.

            // Current servo position in units of 0.25 μs.
            [XmlIgnore]
            public ushort polledPosition;

            // Target servo position in units of 0.25 μs.
            [XmlIgnore]
            public ushort polledTarget;

            // Servo speed in units of 0.25 μs / (10 ms)
            [XmlIgnore]
            public ushort polledSpeed;

            // Servo acceleration in units of (0.25 μs) / (10 ms) / (80 ms).
            [XmlIgnore]
            public byte polledAcceleration;


            // Used to track whether the servo is currently moving.
            [XmlIgnore]
            public bool isMoving = false;

            [XmlIgnore]
            public bool isRunningHighPriorityMove = false;

            [XmlIgnore]
            public Object movePriorityLock = new Object();
        }

        List<Servo> servoList = new List<Servo>();
        Usc uscDevice = null;
        Object uscLock = new Object();
        bool isEnabled = true;

        public ServoManager()
        {
        }

        public ServoManager(string fileName)
        {
            LoadServoConfiguration(fileName, false);
        }

        // Connects to the Pololu Maestro servo controller through USB. Only one controller is 
        // currently expected, so this method just connects to the first controller it sees.
        // Based on the 'connectToDevice' method from MaestroEasyExample in the pololu-usb-sdk.
        public void ConnectToHardware()
        {
            try
            {
                DisconnectFromHardware();

                // Get a list of all connected devices of this type.
                List<DeviceListItem> connectedDevices = Usc.getConnectedDevices();

                foreach (DeviceListItem dli in connectedDevices)
                {
                    // If you have multiple devices connected and want to select a particular
                    // device by serial number, you could simply add a line like this:
                    //   if (dli.serialNumber != "00012345"){ continue; }

                    lock (uscLock)
                    {
                        uscDevice = new Usc(dli); // Connect to the device.
                    }

                    break;  // Use first device (should only be one, anyway).
                }

                if (uscDevice == null)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "ConnectToHardware() failed, no servo hardware found.");
                }
                else
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "ConnectToHardware() succeeded, connected to servo hardware.");
                }

                InitializeHardware();
            }
            catch (System.Exception ex)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in Initialize(): " + ex.Message);
            }
        }

        // Disconnects from the Pololu Maestro servo controller.
        // Based on the 'TryToDisconnect' method from MaestroAdvancedExample in the pololu-usb-sdk.
        public void DisconnectFromHardware()
        {
            lock (uscLock)
            {
                if (uscDevice == null)
                {
                    // Already disconnected
                    return;
                }

                try
                {
                    uscDevice.Dispose();  // Disconnect
                }
                catch (Exception ex)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "DisconnectFromHardware failed to cleaning disconnect the servo hardware: " + ex.Message);
                }
                finally
                {
                    // do this no matter what
                    uscDevice = null;
                }
            }
        }

        public bool IsConnected()
        {
            lock (uscLock)
            {
                return (uscDevice != null);
            }
        }

        // Sets the servo controller with the default speeds and accelerations, for each servo.
        // This should be called whenever servo hardware is connected, or when new default 
        // servo parameters have been loaded.
        private void InitializeHardware()
        {
            if (!IsConnected())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "InitializeHardware() failed, not connected to servo hardware.");
                return;
            }

            if (servoList.Count == 0)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "InitializeHardware() failed, no servos have been defined.");
                return;
            }

            foreach (Servo servo in servoList)
            {
                SetServoSpeed(servo, servo.defaultSpeed);
                SetServoAcceleration(servo, servo.defaultAcceleration);
                UpdateServoValues();
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "InitializeHardware() succeeded, servo hardware was initialized.");
        }

        // Loads the configuration settings for each servo from the xml file specified by 'fileName', 
        // and then initializes the servo hardware with these new settings. The InitializeHardware 
        // step can be skipped, if desired (e.g. on startup when the hardware is not yet connected).
        public void LoadServoConfiguration(string fileName, bool initializeHardware = true)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "Loading servo config from " + fileName);

            XmlSerializer SerializerObj = new XmlSerializer(typeof(List<Servo>));
            FileStream ReadFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            servoList = (List<Servo>)SerializerObj.Deserialize(ReadFileStream);
            ReadFileStream.Close();

            foreach (Servo servo in servoList)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Loaded Servo " + servo.index + ": Name = " + servo.name + ", PosMin = " + servo.positionLimitMin + ", PosMax = " + servo.positionLimitMax + ", SpeedMin = " + servo.speedLimitMin + ", SpeedMax = " + servo.speedLimitMax);
            }

            if (initializeHardware)
            {
                InitializeHardware();
            }
        }

        // Creates an example servo configuration xml file to provide a template for creating 
        // a complete servo configuration file.
        public void GenerateExampleConfigFile(string fileName)
        {
            Servo exampleServo = new Servo();
            exampleServo.index = 0;
            exampleServo.name = "Example Servo";
            exampleServo.positionLimitMax = 0;
            exampleServo.positionLimitMin = 0;
            exampleServo.speedLimitMax = 0;
            exampleServo.speedLimitMin = 0;

            List<Servo> exampleServoList = new List<Servo>();
            exampleServoList.Add(exampleServo);

            XmlSerializer SerializerObj = new XmlSerializer(typeof(List<Servo>));
            TextWriter WriteFileStream = new StreamWriter(fileName);
            SerializerObj.Serialize(WriteFileStream, exampleServoList);
            WriteFileStream.Close();
        }

        public long GetServoIndex(string servoName)
        {

            Servo servo = servoList.Find(x => x.name == servoName);

            if (servo != null)
            {
                return servo.index;
            }
            else
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "GetServoIndex() failed to find servo '" + servoName + "'.");
                return -1;
            }
        }

        // Reads the servo status from the servo controller hardware, and then updates the 
        // servoList with these new polled values. Also tracks whether each servo is currently 
        // moving by comparing their current and target positions.
        public void UpdateServoValues()
        {
            if (!IsConnected())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "UpdateServoValues() failed, not connected to servo hardware.");
                return;
            }

            if (servoList.Count == 0)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "UpdateServoValues() failed, no servos have been defined.");
                return;
            }

            try
            {
                // Get the servo parameters from the hardware.
                ServoStatus[] servoStatusArray;
                long servoCount;

                lock (uscLock)
                {
                    if (uscDevice == null)
                    {
                        throw new System.Exception("uscDevice is null");
                    }

                    uscDevice.getVariables(out servoStatusArray);
                    servoCount = uscDevice.servoCount;
                }

                // Update the servoList with these parameters.
                foreach (Servo servo in servoList)
                {
                    if (servo.index < 0 || servo.index >= servoCount)
                    {
                        ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "UpdateServoValues() failed, servo index out of range. Servo index = " + servo.index.ToString());
                    }
                    else
                    {
                        servo.polledPosition = servoStatusArray[servo.index].position;
                        servo.polledTarget = servoStatusArray[servo.index].target;
                        servo.polledSpeed = servoStatusArray[servo.index].speed;
                        servo.polledAcceleration = servoStatusArray[servo.index].acceleration;

                        ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Servo " + servo.index.ToString() + ": Target = " + servo.polledTarget.ToString() + ", Position = " + servo.polledPosition.ToString() + ", Speed = " + servo.polledSpeed.ToString() + ", Acceleration = " + servo.polledAcceleration.ToString());

                        if (servo.isMoving == false && servo.polledTarget != servo.polledPosition)
                        {
                            // Servo has started moving.

                            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Servo " + servo.index + " has started moving from " + servo.polledPosition.ToString() + " to " + servo.polledTarget.ToString());

                            servo.isMoving = true;
                        }
                        else if (servo.isMoving == true && servo.polledTarget == servo.polledPosition)
                        {
                            // Servo has stopped moving.

                            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Servo " + servo.index + " has stopped moving at " + servo.polledPosition.ToString());

                            servo.isMoving = false;
                        }

                        if (servo.isMoving)
                        {
                            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Servo " + servo.index + " is at position " + servo.polledPosition.ToString());
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in UpdateServoValues(): " + ex.Message);
            }
        }

        // Sets the target position of the specified servo, causing this servo to begin moving 
        // to the new position. This target position is first bounded within the servo's min/max 
        // position limits, and a warning is logged if a position outside of these limits was 
        // specified.
        private void SetServoPosition(Servo servo, double position)
        {
            if (position < servo.positionLimitMin)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Requested servo " + servo.index.ToString() + " position " + position.ToString() + " bound to minimum limit " + servo.positionLimitMin.ToString());

                // Bound to this limit.
                position = servo.positionLimitMin;
            }

            if (position > servo.positionLimitMax)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Requested servo " + servo.index.ToString() + " position " + position.ToString() + " bound to maximum limit " + servo.positionLimitMax.ToString());

                // Bound to this limit.
                position = servo.positionLimitMax;
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting servo " + servo.index.ToString() + " position to " + position.ToString());

            try
            {
                lock (uscLock)
                {
                    if (!isEnabled)
                    {
                        return;
                    }

                    if (uscDevice == null)
                    {
                        throw new System.Exception("uscDevice is null");
                    }

                    // Send this value to the hardware.
                    // Note that the servo position values are handled in this class in units of μs, 
                    // to match the convention used by Pololu's Maestro Control Center application. 
                    // However, the servo controller hardware expects the position represented as an 
                    // integer value in 0.25 μs. The local value must be multiplied by 4 to convert 
                    // to these units.
                    uscDevice.setTarget((byte)servo.index, (ushort)(position * 4));
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in SetServoPosition(): " + ex.Message);
            }
        }

        // Sets the speed of the specified servo, in the servo controller hardware. This speed 
        // value is first bounded within the servo's min/max speed limits, and a warning is 
        // logged if a speed outside of these limits was specified.
        private void SetServoSpeed(Servo servo, long speed)
        {
            if (speed < servo.speedLimitMin)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Requested servo " + servo.index.ToString() + " speed " + speed.ToString() + " bound to minimum limit " + servo.speedLimitMin.ToString());

                // Bound to this limit.
                speed = servo.speedLimitMin;
            }

            if (speed > servo.speedLimitMax)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "Requested servo " + servo.index.ToString() + " speed " + speed.ToString() + " bound to maximum limit " + servo.speedLimitMax.ToString());

                // Bound to this limit.
                speed = servo.speedLimitMax;
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting servo " + servo.index.ToString() + " speed to " + speed.ToString());

            try
            {
                lock (uscLock)
                {
                    if (uscDevice == null)
                    {
                        throw new System.Exception("uscDevice is null");
                    }

                    // Send this value to the hardware.
                    uscDevice.setSpeed((byte)servo.index, (ushort)speed);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in SetServoSpeed(): " + ex.Message);
            }
        }

        // Sets the acceleration of the specified servo, in the servo controller hardware.
        private void SetServoAcceleration(Servo servo, long acceleration)
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "Setting servo " + servo.index.ToString() + " acceleration to " + acceleration.ToString());

            try
            {
                lock (uscLock)
                {
                    if (uscDevice == null)
                    {
                        throw new System.Exception("uscDevice is null");
                    }

                    // Send this value to the hardware.
                    uscDevice.setAcceleration((byte)servo.index, (ushort)acceleration);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in SetServoAcceleration(): " + ex.Message);
            }
        }

        // Disables all servos by sending target position values of '0' to the hardware, for 
        // each servo. This '0' value has a special function of relaxing the servo, rather 
        // than moving to this position.
        public void DisableAllServos()
        {
            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Info, "Disabling all servos.");

            lock (uscLock)
            {
                isEnabled = false;
            }

            foreach (Servo servo in servoList)
            {
                try
                {
                    lock (uscLock)
                    {
                        if (uscDevice == null)
                        {
                            throw new System.Exception("uscDevice is null");
                        }

                        uscDevice.setTarget((byte)servo.index, 0);
                    }
                }
                catch (System.Exception ex)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "Caught exception in DisableAllServos(): " + ex.Message);
                }
            }
        }

        public void EnableAllServos()
        {
            lock (uscLock)
            {
                isEnabled = true;
            }
        }

        public bool IsEnabled()
        {
            lock (uscLock)
            {
                return isEnabled;
            }
        }

        // Calculates the required servo speed to reach the position specified by servoPosition, 
        // in the amount of time indicated by timeToDestination. Then starts this move and returns.
        private void StartTimedMove(ServoPosition servoPosition, double timeToDestination)
        {
            Servo servo = servoList.Find(x => x.index == servoPosition.index);
            ushort servoSpeed = 0;

            if (servo == null)
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "StartTimedMove failed, servo " + servoPosition.index.ToString() + " not found.");
                return;
            }

            if (servo.polledPosition == 0)
            {
                // If the servo position is 0, then the servo is off and we have no information about its actual position.
                // We don't know the actual move distance, so just use the default servo speed.

                servoSpeed = (ushort)servo.defaultSpeed;
            }
            else
            {
                // Convert current position to μs (the hardware uses 0.25 μs increments).
                double currentPosition = ((double)servo.polledPosition) / 4;

                // Position difference in μs.
                double positionDifference = Math.Abs(servoPosition.position - currentPosition);

                // Required speed in (μs/second).
                double calculatedSpeed;
                if (timeToDestination != 0)
                {
                    calculatedSpeed = positionDifference / timeToDestination;
                }
                else
                {
                    // If the desired move time is instantaneous, use the max allowed servo speed.
                    calculatedSpeed = servo.speedLimitMax;
                }

                // Convert speed from (1 μs / second) to (0.25 μs / 10 ms), used by the hardware.
                servoSpeed = (ushort)(calculatedSpeed * (4.0 / 100.0));
            }

            SetServoSpeed(servo, servoSpeed);

            SetServoPosition(servo, servoPosition.position);
        }

        // The time (in milliseconds) between hardware reads, when waiting for a move to complete.
        const int pollPeriod_ms = 100;

        // The time (in seconds) in excess of the expected timeToDestination that the servo move 
        // process is allowed before timing out.
        const int pollTimeoutAdjustment = 2;

        // Polls the servo hardware to determine if the servos are still moving. Returns when all 
        // servos in servoPositionList have completed their moves.
        private void WaitForMoveComplete(List<ServoPosition> servoPositionList, double timeToDestination)
        {
            // Create a list of servos to monitor.
            List<Servo> servosToMonitor = new List<Servo>();
            foreach (ServoPosition servoPosition in servoPositionList)
            {
                Servo servo = servoList.Find(x => x.index == servoPosition.index);

                if (servo == null)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Error, "WaitForMoveComplete failed, servo " + servoPosition.index.ToString() + " not found.");
                    return;
                }

                servosToMonitor.Add(servo);
            }


            // Poll servo positions and wait until all servos reach their destinations.
            double pollTimeout = timeToDestination + pollTimeoutAdjustment;
            int pollTimeoutCount = (int)(pollTimeout * 1000 / (double)pollPeriod_ms);
            int currentPollCount = 0;

            while (true)
            {
                if (!IsEnabled())
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "WaitForMoveComplete interrupted, servos are disabled.");
                    return;
                }

                if (currentPollCount >= pollTimeoutCount)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "WaitForMoveComplete timeout, servos failed to reach destination in " + pollTimeout.ToString() + " seconds.");
                    return;
                }

                currentPollCount++;

                UpdateServoValues();

                // Determine if any servos in the list are still moving.
                bool servoIsMoving = false;
                foreach (Servo servo in servosToMonitor)
                {
                    if (servo.isMoving)
                    {
                        servoIsMoving = true;
                    }
                }

                if (!servoIsMoving)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "WaitForMoveComplete succeeded, all servos reached destinations.");
                    return;
                }

                Thread.Sleep(pollPeriod_ms);
            }
        }

        // Starts performing all moves specified by servoPositionList, and then waits for these 
        // servos to finish moving.
        public void MoveServos(List<ServoPosition> servoPositionList, double timeToDestination)
        {
            if (!IsEnabled())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Warning, "MoveServos failed, servos are disabled.");
                return;
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "MoveServos moving servos over " + timeToDestination + " seconds.");

            foreach (ServoPosition servoPosition in servoPositionList)
            {
                Servo servo = servoList.Find(x => x.index == servoPosition.index);

                lock (servo.movePriorityLock)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "MoveServos set isRunningHighPriorityMove for servo " + servo.index.ToString() + ".");
                    servo.isRunningHighPriorityMove = true;
                }
            }

            if (!IsConnected())
            {
                // Simulate the move.
                Thread.Sleep((int)(timeToDestination * 1000));

                foreach (ServoPosition servoPosition in servoPositionList)
                {
                    Servo servo = servoList.Find(x => x.index == servoPosition.index);

                    lock (servo.movePriorityLock)
                    {
                        ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "MoveServos cleared isRunningHighPriorityMove for servo " + servo.index.ToString() + ".");
                        servo.isRunningHighPriorityMove = false;
                    }
                }

                return;
            }

            foreach (ServoPosition servoPosition in servoPositionList)
            {
                StartTimedMove(servoPosition, timeToDestination);
            }

            WaitForMoveComplete(servoPositionList, timeToDestination);

            foreach (ServoPosition servoPosition in servoPositionList)
            {
                Servo servo = servoList.Find(x => x.index == servoPosition.index);

                lock (servo.movePriorityLock)
                {
                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "MoveServos cleared isRunningHighPriorityMove for servo " + servo.index.ToString() + ".");
                    servo.isRunningHighPriorityMove = false;
                }
            }
        }

        // Starts the specified servo move using the specified speed. StartSpeedMove is used by PersonTracking 
        // and RobotSpeech, which are lower priority moves than those issued through Sequences. StartSpeedMove 
        // motion can be interrupted by Sequence moves, and new StartSpeedMove moves are ignored if a Sequence 
        // is already moving this servo.
        public void StartSpeedMove(ServoPosition servoPosition, long servoSpeed)
        {
            if (!IsEnabled())
            {
                ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "StartSpeedMove failed, servos are disabled.");
                return;
            }

            ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "StartSpeedMove moving servo " + servoPosition.index.ToString() + " to position " + servoPosition.position.ToString() + " at speed " + servoSpeed.ToString() + ".");

            Servo servo = servoList.Find(x => x.index == servoPosition.index);

            lock (servo.movePriorityLock)
            {
                if (servo.isRunningHighPriorityMove)
                {
                    // Ignore this command if a higher priority (i.e. Sequence) move is already running.

                    ErrorLogging.AddMessage(ErrorLogging.LoggingLevel.Debug, "StartSpeedMove, isRunningHighPriorityMove is set for servo " + servo.index.ToString() + ". Aborting move.");
                    return;
                }

                if (!IsConnected())
                {
                    return;
                }

                SetServoSpeed(servo, servoSpeed);
                SetServoPosition(servo, servoPosition.position);
            }
        }
    }
}
