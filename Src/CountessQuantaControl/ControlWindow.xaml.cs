// Brad Pitney
// ECE 579
// Winter 2014

// Implements the logic behind the ControlWindow GUI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Speech.Synthesis;


namespace CountessQuantaControl
{

    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        const string sequenceFileName = "SequenceFile.xml";
        const string servoConfigFileName = "ServoConfig.xml";

        ServoManager servoManager;
        SequenceProcessor sequenceProcessor;
        KinectManager kinectManager;
        SkeletonViewer skeletonViewer;
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        System.Windows.Threading.DispatcherTimer logUpdateTimer = new System.Windows.Threading.DispatcherTimer();

        public ControlWindow()
        {
            InitializeComponent();

            servoManager = new ServoManager(servoConfigFileName);
            servoManager.ConnectToHardware();
            UpdateConnectedTextblock(servoManager.IsConnected(), servoHardwareState);

            sequenceProcessor = new SequenceProcessor(servoManager, sequenceFileName);

            kinectManager = new KinectManager(sequenceProcessor);
            kinectManager.InitializeKinect();
            UpdateConnectedTextblock(kinectManager.IsConnected(), kinectHardwareState);

            logUpdateTimer.Tick += new EventHandler(logUpdateTimer_Tick);
            logUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            logUpdateTimer.Start();
        }

        private void UpdateConnectedTextblock(bool connected, TextBlock textBlock)
        {
            if (connected)
            {
                textBlock.Text = "Connected";
                textBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                textBlock.Text = "Not Connected";
                textBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void logUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (ErrorLogging.NewLogMessageAvailable())
            {
                LoggingTextBox.Text = ErrorLogging.GetLogString();
                LoggingScrollViewer.ScrollToEnd();
            }
        }

        private void InitializeHardware_Click(object sender, RoutedEventArgs e)
        {
            servoManager.LoadServoConfiguration(servoConfigFileName, false);

            servoManager.ConnectToHardware();
            UpdateConnectedTextblock(servoManager.IsConnected(), servoHardwareState);

            kinectManager.InitializeKinect();
            UpdateConnectedTextblock(kinectManager.IsConnected(), kinectHardwareState);
        }

        private void LoadServoConfig_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RunSequence_Click(object sender, RoutedEventArgs e)
        {
            sequenceProcessor.LoadSequenceFile(sequenceFileName);
            sequenceProcessor.RunSequence("Test Sequence");
        }


        private void GenerateExampleXmls_Click(object sender, RoutedEventArgs e)
        {
            // Generate example sequence file.

            Frame exampleFrame1 = new Frame("Example Frame 1");
            Frame exampleFrame2 = new Frame("Example Frame 2");

            exampleFrame1.AddServoPosition(new ServoPosition(0, 100));
            exampleFrame1.AddServoPosition(new ServoPosition(1, 500));
            exampleFrame1.timeToDestination = 1;

            exampleFrame2.AddServoPosition(new ServoPosition(0, 200));
            exampleFrame2.AddServoPosition(new ServoPosition(1, 600));
            exampleFrame2.timeToDestination = 0.5;

            Sequence exampleSequence1 = new Sequence("Example Sequence 1");
            Sequence exampleSequence2 = new Sequence("Example Sequence 2");

            exampleSequence1.AddFrame(exampleFrame1);
            exampleSequence1.AddFrame(exampleFrame2);

            exampleSequence2.AddFrame(exampleFrame1);
            exampleSequence2.AddFrame(exampleFrame2);

            SequenceList exampleSequenceList = new SequenceList();
            exampleSequenceList.AddSequence(exampleSequence1);
            exampleSequenceList.AddSequence(exampleSequence2);

            exampleSequenceList.SaveToXml("ExampleSequenceFile.xml");


            // Generate example servo configuration file.

            servoManager.GenerateExampleConfigFile("ExampleServoConfig.xml");
        }

        private void SkeletonViewer_Click(object sender, RoutedEventArgs e)
        {
            if (skeletonViewer == null)
            {
                skeletonViewer = new SkeletonViewer(kinectManager);
            }

            skeletonViewer.Show();
        }

        private void TestSpeech_Click(object sender, RoutedEventArgs e)
        {
            speechSynthesizer.Speak("This is a test");
        }

        private void RelaxServos_Click(object sender, RoutedEventArgs e)
        {
            servoManager.DisableAllServos();
        }

        private void speechRecognitionEnableCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.EnableSpeechRecognition(((CheckBox)sender).IsChecked == true);
            }
        }

        private void gestureRecognitionEnableCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.EnableGestureRecognition(((CheckBox)sender).IsChecked == true);
            }
        }

        private void personTrackingEnableCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (kinectManager != null)
            {
                kinectManager.EnablePersonTracking(((CheckBox)sender).IsChecked == true);
            }
        }
    }
}
