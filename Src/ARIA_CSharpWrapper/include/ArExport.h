/*
Adept MobileRobots Robotics Interface for Applications (ARIA)
Copyright (C) 2004, 2005 ActivMedia Robotics LLC
Copyright (C) 2006, 2007, 2008, 2009, 2010 MobileRobots Inc.
Copyright (C) 2011, 2012, 2013 Adept Technology

     This program is free software; you can redistribute it and/or modify
     it under the terms of the GNU General Public License as published by
     the Free Software Foundation; either version 2 of the License, or
     (at your option) any later version.

     This program is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warranty of
     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.

     You should have received a copy of the GNU General Public License
     along with this program; if not, write to the Free Software
     Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

If you wish to redistribute ARIA under different terms, contact 
Adept MobileRobots for information about a commercial version of ARIA at 
robots@mobilerobots.com or 
Adept MobileRobots, 10 Columbia Drive, Amherst, NH 03031; +1-603-881-7960
*/
#ifndef AREXPORT_H
#define AREXPORT_H

// Including this header file forces AREXPORT to be defined as the dllexport declaration in Windows DLL builds.
// It should only be included in source files in ARIA and other libraries. Header files should include ariaTypedefs instead.

#if defined(_WIN32) || defined(WIN32)

#ifndef SWIG
#ifndef ARIA_STATIC
#undef AREXPORT
#define AREXPORT _declspec(dllexport)
#else // ARIA_STATIC
#define AREXPORT
#endif // ARIA_STATIC
#endif // SWIG

#else // WIN32

#define AREXPORT

#endif // WIN32

#endif // AREXPORT_H


