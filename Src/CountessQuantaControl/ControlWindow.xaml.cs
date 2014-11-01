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
        AriaManager ariaManager;
        RobotSpeech robotSpeech;
        RobotEyes robotEyes;
        SequenceProcessor sequenceProcessor;
        PersonTracking personTracking;
        KinectManager kinectManager;
        SkeletonViewer skeletonViewer;
        System.Windows.Threading.DispatcherTimer logUpdateTimer = new System.Windows.Threading.DispatcherTimer();

        public ControlWindow()
        {
            InitializeComponent();

            servoManager = new ServoManager(servoConfigFileName);
            servoManager.ConnectToHardware();
            UpdateConnectedTextblock(servoManager.IsConnected(), servoHardwareState);

            ariaManager = new AriaManager();
            ariaManager.InitializeAria();
            UpdateConnectedTextblock(ariaManager.IsConnected(), ariaHardwareState);

            robotSpeech = new RobotSpeech(servoManager);

            robotEyes = new RobotEyes();
            robotEyes.InitializeHardware();

            personTracking = new PersonTracking(servoManager, ariaManager);

            sequenceProcessor = new SequenceProcessor(servoManager, ariaManager, robotSpeech, robotEyes, sequenceFileName);

            kinectManager = new KinectManager(sequenceProcessor, personTracking, robotSpeech);
            kinectManager.InitializeKinect();
            UpdateConnectedTextblock(kinectManager.IsConnected(), kinectHardwareState);
            UpdateMotionEnabledDisplay();

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

        private void ToggleMotionEnable()
        {
            if (servoManager.IsEnabled() || ariaManager.IsEnabled())
            {
                servoManager.DisableAllServos();
                ariaManager.DisableMotion();
            }
            else
            {
                servoManager.EnableAllServos();
                ariaManager.EnableMotion();
            }

            UpdateMotionEnabledDisplay();
        }

        private void UpdateMotionEnabledDisplay()
        {
            if (servoManager.IsEnabled() || ariaManager.IsEnabled())
            {
                motionEnableText.Text = "Enabled";
                motionEnableText.Foreground = new SolidColorBrush(Colors.Green);
                motionEnableButton.Content = "Disable Motion";
            }
            else
            {
                motionEnableText.Text = "Disabled";
                motionEnableText.Foreground = new SolidColorBrush(Colors.Red);
                motionEnableButton.Content = "Enable Motion";
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

            ariaManager.InitializeAria();
            UpdateConnectedTextblock(ariaManager.IsConnected(), ariaHardwareState);

            kinectManager.InitializeKinect();
            UpdateConnectedTextblock(kinectManager.IsConnected(), kinectHardwareState);
        }

        private void LoadServoConfig_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RunSequence_Click(object sender, RoutedEventArgs e)
        {
            sequenceProcessor.LoadSequenceFile(sequenceFileName);
            sequenceProcessor.RunSequence(SelectedSequenceBox.Text);
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

        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        private void TestSpeech_Click(object sender, RoutedEventArgs e)
        {
            //System.Collections.ObjectModel.ReadOnlyCollection<InstalledVoice> voices = speechSynthesizer.GetInstalledVoices();
            speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices().Where(i=>i.Enabled && i.VoiceInfo.Gender == VoiceGender.Female).First().VoiceInfo.Name);
            speechSynthesizer.Rate = 0;
            speechSynthesizer.SpeakAsyncCancelAll();
            speechSynthesizer.SpeakAsync(textToSpeechTextBox.Text);
        }

        private void ToggleMotionEnable_Click(object sender, RoutedEventArgs e)
        {
            ToggleMotionEnable();
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
            if (personTracking != null)
            {
                personTracking.Enable(((CheckBox)sender).IsChecked == true);
            }
        }


        private void LoggingLevelBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> loggingLevels = new List<string>();
            loggingLevels.Add(ErrorLogging.LoggingLevel.Error.ToString());
            loggingLevels.Add(ErrorLogging.LoggingLevel.Warning.ToString());
            loggingLevels.Add(ErrorLogging.LoggingLevel.Info.ToString());
            loggingLevels.Add(ErrorLogging.LoggingLevel.Debug.ToString());

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = loggingLevels;
            comboBox.SelectedIndex = 2; // Default to Info.
        }

        private void LoggingLevelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string comboBoxSelectedItem = comboBox.SelectedItem as string;

            var loggingLevelValues = Enum.GetValues(typeof(ErrorLogging.LoggingLevel));
            foreach (ErrorLogging.LoggingLevel loggingLevel in loggingLevelValues)
            {
                if (loggingLevel.ToString() == comboBoxSelectedItem)
                {
                    ErrorLogging.SetLoggingDisplayLevel(loggingLevel);
                    return;
                }
            }
        }

        private void SelectedSequenceBox_DropDownOpened(object sender, EventArgs e)
        {
            sequenceProcessor.LoadSequenceFile(sequenceFileName);
            List<string> listOfSequenceNames = sequenceProcessor.GetSequenceList();

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = listOfSequenceNames;
        }

        private void ConnectAria_Click(object sender, RoutedEventArgs e)
        {
            ariaManager.InitializeAriaHardware();
            UpdateConnectedTextblock(ariaManager.IsConnected(), ariaHardwareState);
        }
    }
}
