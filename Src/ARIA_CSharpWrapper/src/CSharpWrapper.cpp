// Brad Pitney
// ECE 510
// Spring 2014

// This file provides basic Pioneer 2 initialization and wheel control that can 
// be accessed from a C# application. The ARIA directMotionExample was adapted 
// to implement this file.

#include "CSharpWrapper.h"
#include "Aria.h"

// ConnHandler deals with connection events, and was copied from directMotionExample.
class ConnHandler
{
public:
    // Constructor
    ConnHandler(ArRobot *robot);
    // Destructor, its just empty
    ~ConnHandler(void) {}
    // to be called if the connection was made
    void connected(void);
    // to call if the connection failed
    void connFail(void);
    // to be called if the connection was lost
    void disconnected(void);
protected:
    // robot pointer
    ArRobot *myRobot;
    // the functor callbacks
    ArFunctorC<ConnHandler> myConnectedCB;
    ArFunctorC<ConnHandler> myConnFailCB;
    ArFunctorC<ConnHandler> myDisconnectedCB;
};

ConnHandler::ConnHandler(ArRobot *robot) :
myConnectedCB(this, &ConnHandler::connected),  
    myConnFailCB(this, &ConnHandler::connFail),
    myDisconnectedCB(this, &ConnHandler::disconnected)

{
    myRobot = robot;
    myRobot->addConnectCB(&myConnectedCB, ArListPos::FIRST);
    myRobot->addFailedConnectCB(&myConnFailCB, ArListPos::FIRST);
    myRobot->addDisconnectNormallyCB(&myDisconnectedCB, ArListPos::FIRST);
    myRobot->addDisconnectOnErrorCB(&myDisconnectedCB, ArListPos::FIRST);
}

// just exit if the connection failed
void ConnHandler::connFail(void)
{
    printf("directMotionDemo connection handler: Failed to connect.\n");
    myRobot->stopRunning();
    Aria::exit(1);
    return;
}

// turn on motors, and off sonar, and off amigobot sounds, when connected
void ConnHandler::connected(void)
{
    printf("directMotionDemo connection handler: Connected\n");
    myRobot->comInt(ArCommands::SONAR, 0);
    myRobot->comInt(ArCommands::ENABLE, 1);
    myRobot->comInt(ArCommands::SOUNDTOG, 0);
}

// lost connection, so just exit
void ConnHandler::disconnected(void)
{
    printf("directMotionDemo connection handler: Lost connection, exiting program.\n");
    Aria::exit(0);
}


ArRobot CSharpWrapperRobot;
ArArgumentBuilder builder;
ArArgumentParser argParser(&builder);
ArRobotConnector con(&argParser, &CSharpWrapperRobot);
ConnHandler ch(&CSharpWrapperRobot);

// This method connects to the Pioneer 2, which triggers initialization through ConnHandler.
AREXPORT int InitializeRobot_dll()
{
    // Connect to the Pioneer 2 using COM port 'COM8'.
    builder.add("-rp COM8");

    Aria::init();

    argParser.loadDefaultArguments();

    if(!Aria::parseArgs())
    {
        Aria::logOptions();
        Aria::exit(1);
        return 1;
    }

    if(!con.connectRobot())
    {
        ArLog::log(ArLog::Normal, "directMotionExample: Could not connect to the robot. Exiting.");

        if(argParser.checkHelpAndWarnUnparsed()) 
        {
            Aria::logOptions();
        }
        Aria::exit(1);
        return 1;
    }

    ArLog::log(ArLog::Normal, "directMotionExample: Connected.");

    if(!Aria::parseArgs() || !argParser.checkHelpAndWarnUnparsed())
    {
        Aria::logOptions();
        Aria::exit(1);
    }

    // Run the robot processing cycle in its own thread. Note that after starting this
    // thread, we must lock and unlock the ArRobot object before and after
    // accessing it.
    CSharpWrapperRobot.runAsync(false);
}

// A simple test of communication with the dll.
AREXPORT int TestMethod_dll(int number)
{
    return(number+number);
}

// Disconnect from the Pioneer 2.
AREXPORT void DisconnectRobot_dll()
{
    CSharpWrapperRobot.lock();
    CSharpWrapperRobot.stopRunning(false);
    CSharpWrapperRobot.unlock();

    //Aria::exit(0);
}

// Check if the Pioneer 2 is connected.
AREXPORT bool IsConnected_dll()
{
    bool isConnected = false;

    CSharpWrapperRobot.lock();
    isConnected = CSharpWrapperRobot.isConnected();
    CSharpWrapperRobot.unlock();

    return isConnected;
}

// Set translational velocity (mm/s) of the Pioneer 2 wheels.
AREXPORT void setVel_dll(double velocity)
{
    CSharpWrapperRobot.lock();
    CSharpWrapperRobot.setVel(velocity);
    CSharpWrapperRobot.unlock();
}

// Set the velocity (mm/s) of the Pioneer 2's right and left wheels.
AREXPORT void setVel2_dll(double leftVelocity, double rightVelocity)
{
    CSharpWrapperRobot.lock();
    CSharpWrapperRobot.setVel2(leftVelocity, rightVelocity);
    CSharpWrapperRobot.unlock();
}

// Set rotational velocity (deg/s) of the Pioneer 2 wheels.
AREXPORT void setRotVel_dll(double velocity)
{
    CSharpWrapperRobot.lock();
    CSharpWrapperRobot.setRotVel(velocity);
    CSharpWrapperRobot.unlock();
}