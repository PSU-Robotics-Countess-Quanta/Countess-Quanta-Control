// Brad Pitney
// ECE 510
// Spring 2014

// This is a C# test program that tries to connect to the modified ARIA dll using 
// the methods defined in the CSharpWrapper files.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSharpWrapperTest
{
    class Program
    {
        // Define the methods that will be used from the ARIA dll.

        [DllImport("AriaDebugVC10.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int TestMethod_dll(int number);

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


        // Test the functionality of these methods.
        static void Main(string[] args)
        {
            Console.WriteLine("Testing the DLL:");
            Console.WriteLine(" TestMethod(12) = {0}", TestMethod_dll(12));

            bool isConnected = IsConnected_dll();
            Console.WriteLine("IsConnected_dll returned {0}", isConnected);

            int returnValue = InitializeRobot_dll();
            Console.WriteLine("InitializeRobot_dll returned {0}", returnValue);

            isConnected = IsConnected_dll();
            Console.WriteLine("IsConnected_dll returned {0}", isConnected);

            Console.WriteLine("Setting rotation velocity to 10 for 2 seconds...");
            setRotVel_dll(10);
            System.Threading.Thread.Sleep(2000);

            Console.WriteLine("Setting rotation velocity to 0");
            setRotVel_dll(0);

            Console.WriteLine("Sleeping for 5 seconds...");
            System.Threading.Thread.Sleep(5000);
        }
    }
}
