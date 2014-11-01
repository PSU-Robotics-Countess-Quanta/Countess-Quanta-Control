// Brad Pitney
// ECE 510
// Spring 2014

// This file provides basic Pioneer 2 initialization and wheel control that can 
// be accessed from a C# application.

#include "ArExport.h"

extern "C" AREXPORT int TestMethod_dll(int number);
extern "C" AREXPORT int InitializeRobot_dll();
extern "C" AREXPORT void DisconnectRobot_dll();
extern "C" AREXPORT bool IsConnected_dll();
extern "C" AREXPORT void setVel_dll(double velocity);
extern "C" AREXPORT void setVel2_dll(double leftVelocity, double rightVelocity);
extern "C" AREXPORT void setRotVel_dll(double velocity);
