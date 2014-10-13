// Brad Pitney
// ECE 578
// Fall 2013

// The RobotDisplay class implements a person tracking with a simulated Countess Quanta 
// robot. Skeleton joint data from the Kinect hardware is used to update the location of 
// the target person. This is used to calculate the new neck servo position that would 
// cause the head to turn and face the target, if this were value were set on the physical 
// robot. A top-down representation of the robot and target are displayed on the form.

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
using Microsoft.Kinect;

namespace CountessQuantaControl
{
    /// <summary>
    /// Interaction logic for RobotDisplay.xaml
    /// </summary>
    public partial class RobotDisplay : Window
    {
        public RobotDisplay()
        {
            InitializeComponent();
        }

        // Radius of the circle representing the Countess Quanta robot.
        const double robotRadius = 50;

        // Distance from the top of the form to the robot circle.
        const double robotFromTop = 100;

        // Radius of the circle representing the target person.
        const double targetRadius = 20;

        Ellipse target;
        Line targetLine;

        // This method initializes the circles representing the robot and target, as well 
        // as the line indicating the direction the robot is facing.
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            const double defaultTargetDistance = 200;

            // Circle representing the robot.
            Ellipse robot = new Ellipse();
            robot.Fill = Brushes.AliceBlue;
            robot.StrokeThickness = 2;
            robot.Stroke = Brushes.Black;
            robot.Width = robotRadius * 2;
            robot.Height = robotRadius * 2;
            Canvas.SetTop(robot, robotFromTop);
            Canvas.SetLeft(robot, RobotDisplayCanvas.ActualWidth / 2 - robotRadius);
            RobotDisplayCanvas.Children.Add(robot);

            // Circle representing the target person.
            target = new Ellipse();
            target.Fill = Brushes.LawnGreen;
            target.StrokeThickness = 2;
            target.Stroke = Brushes.Black;
            target.Width = targetRadius * 2;
            target.Height = targetRadius * 2;
            Canvas.SetTop(target, robotFromTop + defaultTargetDistance);
            Canvas.SetLeft(target, RobotDisplayCanvas.ActualWidth / 2 - targetRadius);
            RobotDisplayCanvas.Children.Add(target);

            // Line representing the facing direction of the robot.
            targetLine = new Line();
            targetLine.Stroke = System.Windows.Media.Brushes.Red;
            targetLine.StrokeThickness = 2.5;
            targetLine.X1 = RobotDisplayCanvas.ActualWidth / 2;
            targetLine.Y1 = robotFromTop + robotRadius;
            targetLine.X2 = RobotDisplayCanvas.ActualWidth / 2;
            targetLine.Y2 = robotFromTop + defaultTargetDistance + targetRadius;
            RobotDisplayCanvas.Children.Add(targetLine);
        }

        // The UpdateTargetPosition method is called from the DrawBone method in the sample 
        // 'Skeleton Basics' program. It updates the position of the target and calculates 
        // the required position of the neck servo to track the target on the actual robot.
        public void UpdateTargetPosition(SkeletonPoint targetPosition)
        {
            // Update coordinates of the target joint.
            targetXLabel.Content = "X = " + targetPosition.X.ToString("F2");
            targetYLabel.Content = "Y = " + targetPosition.Y.ToString("F2");
            targetZLabel.Content = "Z = " + targetPosition.Z.ToString("F2");

            // Calculate the new target position, based on the Kinect data.
            double displayScale = 100;
            double targetCenterTop = robotFromTop + robotRadius + targetRadius + targetPosition.Z * displayScale;
            double targetCenterLeft = RobotDisplayCanvas.ActualWidth / 2 + targetPosition.X * displayScale;

            // Update the location of the target circle.
            Canvas.SetTop(target, targetCenterTop - targetRadius);
            Canvas.SetLeft(target, targetCenterLeft - targetRadius);
            targetLine.X2 = targetCenterLeft;
            targetLine.Y2 = targetCenterTop;

            // Update the position of the Countess Quanta neck servo, to track the target.
            //double robotNeckServoPosition = robotNeckServoCenterPostion - Math.Atan(targetPosition.X / targetPosition.Z) * servoIncrementsPerRadian;
            //neckServoLabel.Content = robotNeckServoPosition.ToString("F0");
        }
    }
}
