Countess-Quanta-Control
=======================

<h2>Introduction</h2>
<p>The purpose of this document is to describe the functionality of the Countess Quanta robot and to help new developers in becoming familiar with the robot’s hardware and software systems. This document starts by describing the robot hardware components and the key software features. It then provides instructions on how to connect to the main hardware components individually, using their own development tools. Next, it describes how to build the CountessQuantaControl software and use this to control the robot. The CountessQuantaControl software structure is then discussed, along with how to add new features to the code. Finally, a list of future development tasks is presented, to provide some ideas for future hardware and software work. As the robot is modified and as new features are developed, this document should be updated to include these changes.</p>

<h2>Overview of the Countess Quanta Robot</h2>
Countess Quanta is a wheeled, humanoid robot planned to fulfill several roles:
<ul>
<li>To interact with humans and provide information about the Portland State University robotics program.</li>
<li>To interact with humans and provide information about the Portland State University robotics program.</li>
<li>To act as a guide robot, and show people around the PSU building.</li>
<li>To participate in the PSU Robot Theater.</li>
</ul>
<img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta1.jpg></img>
<img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta2.jpg></img>

<h2>Hardware Components</h2>
<h3>Upper Body</h3>

<p>The upper half of Countess Quanta includes two arms and a head animated by a set of 16 servos. These servos are controlled through a Pololu Mini Maestro 18 USB servo controller, which is mounted in the left shoulder of the robot. This device connects through USB to a laptop PC, which sends the servo commands to animate the upper body of the robot. The servo power is supplied from batteries in the robot base, allowing the servos to operate while disconnected from external power.</p>

<p>The right arm is controlled by a series of six servos, which allow the robot to point at objects, wave, and strum the lap harp instrument mounted on the front of the body. The left arm is mounted upright, and uses one servo for waving and five servos for controlling the five fingers. These fingers can curl and extend independently, which allow for more complex hand motions, such as displaying numbers or playing rock-paper-scissors. For the head, one servo turns the head left and right, while another lets the head look up and down. A third servo lets the eyes look left and right, and a fourth servo opens and closes the mouth.</p>

<h3>Lower Body</h3>
<p>The lower part of Countess Quanta serves as a wheeled platform on which everything else is mounted. This wheeled base is actually a Pioneer 2 Mobile Robot developed by ActivMedia Robotics. This robot contains three batteries which allow the wheels and upper body servos to operate for several hours when disconnected from external power. A Belkin USB-to-serial adapter is used to connect the Pioneer 2 robot to the laptop PC, which sends motion commands to the wheels. This allows for animations that move and rotate the whole Countess Quanta body, and for turning to face people during interactions. Only a small subset of the full Pioneer 2 capabilities is currently utilized by the control software. The Pioneer 2 provides advanced navigation and sonar sensing capabilities that still need to be integrated into the control system.
</p>

<h3>Sensors</h3>
The main sensor for Countess Quanta is the Microsoft Xbox 360 Kinect sensor mounted on at the robot’s base
<img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta3.jpg></img>

<p>This Kinect communicates with a laptop PC through a USB adapter cable. This adapter cable also provides power to the Kinect through an attached AC power plug, although this currently prevents Countess Quanta from operating on battery power alone. The Kinect device includes a color camera and infrared laser depth sensor for locating objects in 3D space. It also provides a multi-array microphone, which allows for detecting which direction a sound is coming from. Countess Quanta currently uses the Kinect to track the location of people, to detect specific gestures people are making, and to detect spoken commands.</p>

<p>The Pioneer 2 robot provides other sensor systems that are not currently integrated into the control software. The Pioneer 2 provides three sonar arrays which each consist of eight individual sensors (gold-colored circles). Two of these arrays are installed in the front and rear of the Pioneer 2 base, just above the wheels. The third array is located on the front of Countess Quanta’s upper body. This third sonar array is not currently hooked up. The sonars can be used to detect distances between the robot and surrounding obstacles. The Pioneer 2 also provides a set of collision sensors around the edge of the robot, near the ground. These are used to detect physical collisions with objects while moving.</p>

<h3>Software Control</h3>
These hardware components are integrated through control software called CountessQuantaControl, which was developed starting in Winter of 2014. CountessQuantaControl is a C# application that uses Windows Presentation Foundation (WPF). It was developed mainly with Visual Studio 2010, and runs under Windows 7 and Windows 8. Here are the main features of this software:
<ul>
<li>Configures and manages the servo controller and Pioneer 2.</li>
<li>Coordinates robot motions as sequences of animation frames.</li>
<li>Stores move sequences and servo configurations in human-readable xml files.</li>
<li>Provides speech and gesture recognition through the Kinect.</li>
<li>Provides person tracking capabilities through the Kinect.</li>
<li>Provides speech synthesis with animated mouth motion.</li>
<li>Provides a simple graphical user interface (GUI) for testing and control.</li>
<li>Provides a versatile error logging system.</li>
</ul>

<h2>Controlling the Robot Hardware Components</h2>
<p>This section provides instructions for how to connect to each of the main hardware components using a Windows laptop PC, and control these devices using their own development software.</p>

<h3>Required Cables</h3>
<p>Four cables are required to connect the Countess Quanta hardware components.</p>

<ol>
<li><p>Power cable for the Pioneer 2 robot. The cable in the lab is labeled with a PeopleBot/GuideBot sticker. Here are the details and a picture:
AU48 - 120 - 120T
Input 120VAC 60Hz
Output 12VDC 1200mA</p>
<img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta4.jpg></img>
</li>
<li>A standard ‘USB A Male to Mini-B cable’ is used for connecting the laptop to the servo controller.</li>
<li><p>An RS-232 serial cable and Belkin F5U409 USB-to-serial adapter is used to connect the Pioneer 2 robot to a laptop USB port:</p><img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta5.jpg></img></li>
<li><p>4)	An Xbox 360 Kinect adapter cable is used for connecting the Kinect hardware to a laptop USB port. The Xbox Kinect uses a proprietary connector to provide both power and data transfer. This adapter converts this Kinect plug into a separate AC power plug which must be connected to a power outlet, and a standard USB plug which is connected to the laptop PC:</p><img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta6.jpg></img></li>

</ol>

<h3>Powering on the Robot</h3>
<ol>
<li>The power switch for the Pioneer 2 is located near Countess Quanta’s right wheel. Switch this on to power up the Pioneer 2 and the servo system. Make sure to switch this off after using the robot, to avoid draining the battery.</li>
<li><p>Just below the power switch is a port for plugging in the power cable. Connect the power cable when possible to avoid draining the battery:</p><img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta7.jpg></img></li>
</ol>

<h3>Controlling the Servo Hardware</h3>
<ol>
<li><p>Power on the robot and use a USB A Male to Mini-B cable to connect the laptop PC to the Pololu Mini Maestro servo controller located at the left shoulder of Countess Quanta, as shown:</p>
<img src=https://github.com/PSU-Robotics-Countess-Quanta/Countess-Quanta-Control/blob/master/Readme%20Resources/CountessQuanta8.jpg></img>
</li>
<li>In the Bin\Servos directory, unzip maestro-windows-130422.zip and run the setup.exe program to install the Maestro Control Center application.</li>
<li>Run the Maestro Control Center application. If the Connected To: box shows Not Connected, then use the dropdown to select the servo controller.</li>
<li><p>The Maestro Control Center should show a list of 18 servos (numbered 0 to 17). Currently, 16 of these servos are used in the robot (servos 4 and 7 are not used). To move a servo, click its Enabled checkbox to power up the servo, then move the slider to change the servo’s position. To power down a servo, uncheck the Enabled checkbox. Below is a list of which servo controls which body part:

Servo 0 - Wrist left/right
Servo 1 - Wrist rotation
Servo 2 - Wrist Up/down
Servo 3 - Arm Up/down
Servo 4 - not used
Servo 5 - Arm rotation
Servo 6 - Shoulder left/right
Servo 7 - not used
Servo 8 - Eyes left/right
Servo 9 - Head up/down
Servo 10 - Head left/right
Servo 11 - Mouth open/close
Servo 12 - Ring finger
Servo 13 - Middle finger
Servo 14 - Pinky finger
Servo 15 - Left arm
Servo 16 - Thumb
Servo 17 - Index finger
</p></li>
</ol>

<h3>Controlling the Wheel Hardware</h3>

<ol>
<li>No official Windows 7 driver exists for the Belkin F5U409 USB-to-serial adapter, but a functional 3rd party driver is included in the Bins\Pioneer2 directory. To get the Belkin device to work in Windows 7, open this directory and run the U232-13.2.98.130315.exe program to install the driver.</li>

<li>Power on the robot and connect the serial cable / Belkin adapter from the Pioneer 2 to the laptop. The device should appear in Windows as Staples Serial On USB Port (COM8).</li>

<li>In the Windows Start menu, navigate to Control Panel -> Hardware and Sound -> Devices and Printers.</li>

<li>Right-click on the Staples Serial On USB Port (COM8) device and select Properties. Select the Hardware tab and click the Properties button.</li>

<li>On the Port Settings tab, change the Bits per second field to 38400. The other settings should show 8 data bits, 1 stop bit, no parity, no hardware handshaking. Click OK to close these dialogs.</li>

<li>In the Bin\Pioneer2 directory, run the ARIA-2.7.6.exe program to install the “ActivMedia Robotics Interface for Applications” software and example code.</li>

<li>In C:\Program Files\MobileRobots\ARIA\examples, run the All_Examples solution for your version of Visual Studio.</li>

<li>In this solution, build the simpleConnect project. This will build also build the AriaDLL project.</li>

<li><p>Open a command prompt and navigate to C:\Program Files\MobileRobots\ARIA\bin. Run the simpleConnect application with the following command (if the Belkin adapter was assigned to a different COM port, you will need to substitute COM8 for the correct COM port number):

simpleConnect.DebugVC10.exe -rp COM8</p></li>

<li><p>If the simpleConnect application was able to communicate with the Pioneer 2 robot, it should return something like the following:

Could not connect to simulator, connecting to robot through serial port COM8.
Syncing 0
Syncing 1
Syncing 2
Connected to robot.
Name: AM_PeopleBot
Type: Pioneer
Subtype: p2pb
Loaded robot parameters from p2pb.p
simpleConnect: Connected.
simpleConnect: Pose=(0.00,0.00,0.00), Trans. Vel=0.00, Battery=13.40V
simpleConnect: Sleeping for 3 seconds...
simpleConnect: Ending robot thread...
Disconnecting from robot.
simpleConnect: Exiting.

If simpleConnect is able to communicate with the Pioneer 2, then CountessQuantaControl should be able to as well.
</p>
</li>
</ol>

<h3>Controlling the Kinect Hardware</h3>
<ol>
<li>Connect the Xbox 360 Kinect adapter cable to the Kinect and the laptop USB port. On the adapter cable, connect the AC power plug into an outlet to power up the Kinect.</li>

<li>Download the 1.8 SDK from <a href=http://www.microsoft.com/en-us/download/details.aspx?id=40278>here</a>, and run KinectSDK-v1.8-Setup.exe to install the Kinect SDK.</li>

<li>Download the Kinect Toolkit from <a href=http://www.microsoft.com/en-us/download/details.aspx?id=40276>here</a>, and run KinectDeveloperToolkit-v1.8.0-Setup.exe to install the Kinect Developer Toolkit.</li>

<li>In the Windows Start menu, click Kinect for Windows SDK v1.8 -> Developer Toolkit Browser v1.8.0 (Kinect for Windows) to run the Toolkit Browser.</li>

<li>The Toolkit Browser contains many example programs that demonstrate the features of the Kinect. Scroll down to Skeleton Basics-WPF and click Run to start this program.</li>

<li>This should connect to the Kinect hardware and display a black window. Stand in front of the Kinect to appear on the display as a skeleton visualization. This skeleton functionality is used in the CountessQuantaControl software for person tracking and gesture recognition.</li>

<li></li>
<ol>

