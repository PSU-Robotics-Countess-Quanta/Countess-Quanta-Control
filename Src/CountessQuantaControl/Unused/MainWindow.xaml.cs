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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CountessQuantaControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Customer
    {
        public string Name { get; set; }
    }

    public class CheckedListItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isChecked;
        private T item;

        public CheckedListItem()
        { }

        public CheckedListItem(T item, bool isChecked = false)
        {
            this.item = item;
            this.isChecked = isChecked;
        }

        public T Item
        {
            get { return item; }
            set
            {
                item = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Item"));
            }
        }


        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<CheckedListItem<Customer>> Customers { get; set; }
        public ObservableCollection<string> sequenceCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            sequenceCollection = new ObservableCollection<string>();
            sequenceCollection.Add("TestString1");
            sequenceCollection.Add("TestString2");

            //SequenceList sequenceList = new SequenceList();

            //sequenceList.AddSequence(testSequence);
            //sequenceList.AddSequence(testSequence2);


            Customers = new ObservableCollection<CheckedListItem<Customer>>();

            Customers.Add(new CheckedListItem<Customer>(new Customer() { Name = "Kelly Smith" }));
            Customers.Add(new CheckedListItem<Customer>(new Customer() { Name = "Joe Brown" }));
            Customers.Add(new CheckedListItem<Customer>(new Customer() { Name = "Herb Dean" }));
            Customers.Add(new CheckedListItem<Customer>(new Customer() { Name = "John Paul" }));

            DataContext = this;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ServoManager servoManager = new ServoManager();
            servoManager.AddServo(new Servo(1));
            servoManager.AddServo(new Servo(2));
            servoManager.AddServo(new Servo(3));

            servoManager.GenerateExampleConfigFile("ExampleConfig.xml");

            SequenceProcessor sequenceProcessor = new SequenceProcessor(servoManager);

            // Create test sequence 1.
            Sequence testSequence = new Sequence("Sequence 1");

            Frame frame1 = new Frame("Frame 1");
            frame1.AddServoPosition(new ServoPosition(1, 100));
            frame1.AddServoPosition(new ServoPosition(2, 200));
            frame1.AddServoPosition(new ServoPosition(3, 300));

            Frame frame2 = new Frame("Frame 2");
            frame2.AddServoPosition(new ServoPosition(1, 400));
            frame2.AddServoPosition(new ServoPosition(2, 500));
            frame2.AddServoPosition(new ServoPosition(3, 600));

            testSequence.AddFrame(frame1);
            testSequence.AddFrame(frame2);

            // Create test sequence 2.
            Sequence testSequence2 = new Sequence("Sequence 2");

            Frame frame3 = new Frame("Frame 3");
            frame3.AddServoPosition(new ServoPosition(1, 900));
            frame3.AddServoPosition(new ServoPosition(2, 800));
            frame3.AddServoPosition(new ServoPosition(3, 700));

            Frame frame4 = new Frame("Frame 4");
            frame4.AddServoPosition(new ServoPosition(1, 600));
            frame4.AddServoPosition(new ServoPosition(2, 500));
            frame4.AddServoPosition(new ServoPosition(3, 400));

            testSequence2.AddFrame(frame3);
            testSequence2.AddFrame(frame4);

            SequenceList sequenceList = new SequenceList();

            sequenceList.AddSequence(testSequence);
            sequenceList.AddSequence(testSequence2);

            sequenceList.SaveToXml("SequenceList.xml");


            SequenceList sequenceList2 = new SequenceList();
            sequenceList2.LoadFromXml("SequenceList.xml");

            //sequenceProcessor.RunSequence(testSequence);
        }
    }
}
