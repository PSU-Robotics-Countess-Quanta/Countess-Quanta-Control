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
#ifndef ARCONFIGARG_H
#define ARCONFIGARG_H

#include "ariaTypedefs.h"
#include "ariaUtil.h"
#include "ArFunctor.h"

class ArArgumentBuilder;

/// Argument class for ArConfig
/** 
    ArConfigArg stores information about a parameter (name, description, type),
    and a pointer to the variable that will actually store the value. This
    variable is normally stored and used by whatever class or module has added the
    parameter to ArConfig.  (In addition, there are some special types of
    ArConfigArg that behave differently such as the "holder" types and separator,
    these are used internally or in special cases.)

    Which constructor you use determines the value type of the ArConfigArg object.
  
    A typical idiom for creating ArConfigArg objects and adding them to ArConfig
    is to create a temporary ArConfigArg in the call to ArConfig::addParam():

    @code
    config->addParam(ArConfigArg("MyParameter", &myTarget, "Example Parameter"), "Example Section");
    @endcode

    Where <tt>config</tt> is a pointer to an ArConfig object or subclass, and
    <tt>myTarget</tt> is a variable (e.g. int) that is a class member whose instance will not
    be destroyed before the end of the program, or which will remove the parameter
    from ArConfig before being destroyed (the pointer to <tt>myTarget</tt> that is stored
    in ArConfig must not become invalid.)  The ArConfigArg object passed to
    addParam() will be copied and stored in ArConfig.
    


    @swignote Swig cannot determine the correct constructor to use
     based on most target langugages types, so you must use subclasses
     defined for various types. Or, use the constructor that accepts 
     functors for dealing with arguments.  Also, Swig cannot use pointers
     to change variables, so you must create ArConfigArg objects, passing
     in default values, and retain references to those objects,
     in addition to passing them to ArConfig, and read new values from those
     objects if ArConfig changes; or pass functors to ArConfigArg instead
     of the initial value.
*/
class ArConfigArg
{
public:

  typedef enum 
  { 
    INVALID, ///< An invalid argument, the argument wasn't created correctly
    INT, ///< Integer argument
    DOUBLE, ///< Double argument
    STRING, ///< String argument
    BOOL, ///< Boolean argument
    FUNCTOR, ///< Argument that handles things with functors
    DESCRIPTION_HOLDER, ///< Argument that just holds a description
    STRING_HOLDER, ///< this one is for holding strings and reading them in and writing them out but not really letting them get sent anywhere (its for unknown config parameters (so they don't get lost if a feature is turned off)
    SEPARATOR, ///< Empty argument that merely acts as a separator within a (large) section.
	  CPPSTRING, ///< C++ std::string target    
	  LAST_TYPE = CPPSTRING ///< Last value in the enumeration
  } Type;

  enum {
    TYPE_COUNT = LAST_TYPE + 1 ///< Number of argument types
  };

  /// Default empty contructor
  AREXPORT ArConfigArg();
  /// Constructor for making an integer argument by pointer (4 bytes)
  AREXPORT ArConfigArg(const char * name, int *pointer, 
		       const char * description = "", 
		       int minInt = INT_MIN, 
		       int maxInt = INT_MAX); 
  /// Constructor for making an int argument thats a short (2 bytes)
  AREXPORT ArConfigArg(const char * name, short *pointer, 
		       const char * description = "", 
		       int minInt = SHRT_MIN, 
		       int maxInt = SHRT_MAX); 
  /// Constructor for making an int argument thats a ushort (2 bytes)
  AREXPORT ArConfigArg(const char * name, unsigned short *pointer, 
		       const char * description = "", 
		       int minInt = 0, 
		       int maxInt = USHRT_MAX); 
  /// Constructor for making an char (1 byte) argument by pointer (treated as int)
  AREXPORT ArConfigArg(const char * name, unsigned char *pointer, 
		       const char * description = "", 
		       int minInt = 0,
		       int maxInt = 255); 
  /// Constructor for making a double argument by pointer
  AREXPORT ArConfigArg(const char * name, double *pointer,
		       const char * description = "", 
		       double minDouble = -HUGE_VAL,
		       double maxDouble = HUGE_VAL); 
  /// Constructor for making a boolean argument by pointer
  AREXPORT ArConfigArg(const char * name, bool *pointer,
		       const char * description = ""); 
  /// Constructor for making an argument of a string by pointer (see details)
  AREXPORT ArConfigArg(const char *name, char *str, 
		       const char *description,
		       size_t maxStrLen);
  /// Constructor for making an argument of a C++ std::string 
  AREXPORT ArConfigArg(const char *name, std::string *str, const char *description);
  /// Constructor for making an integer argument
  AREXPORT ArConfigArg(const char * name, int val, 
		       const char * description = "", 
		       int minInt = INT_MIN, 
		       int maxInt = INT_MAX); 
  /// Constructor for making a double argument
  AREXPORT ArConfigArg(const char * name, double val,
		       const char * description = "", 
		       double minDouble = -HUGE_VAL,
		       double maxDouble = HUGE_VAL); 
  /// Constructor for making a boolean argument
  AREXPORT ArConfigArg(const char * name, bool val,
		       const char * description = ""); 
  /// Constructor for making an argument that has functors to handle things
  AREXPORT ArConfigArg(const char *name, 
		 ArRetFunctor1<bool, ArArgumentBuilder *> *setFunctor, 
		 ArRetFunctor<const std::list<ArArgumentBuilder *> *> *getFunctor,
		 const char *description);

  /// Constructor for just holding a description (for ArConfig)
  AREXPORT ArConfigArg(const char *str, Type type = DESCRIPTION_HOLDER);
  /// Constructor for holding an unknown argument (STRING_HOLDER)
  AREXPORT ArConfigArg(const char *name, const char *str);
  /// Constructs a new argument of the specified type.
  AREXPORT ArConfigArg(Type type);


  /// Copy constructor
  AREXPORT ArConfigArg(const ArConfigArg & arg);
  /// Assignment operator
  AREXPORT ArConfigArg &operator=(const ArConfigArg &arg);
  /// Destructor
  AREXPORT virtual ~ArConfigArg();

  /// Gets the type of the argument
  AREXPORT ArConfigArg::Type getType(void) const;
  /// Gets the name of the argument
  AREXPORT const char *getName(void) const;
  /// Gets the long description of the argument
  AREXPORT const char *getDescription(void) const;

  /// Gets a string describing the type
  AREXPORT const char *getTypeDescr() const {
    switch(getType())
    {
      case INVALID:
        return "INVALID";
      case INT:
        return "INT";
      case DOUBLE:
        return "DOUBLE";
      case STRING:
        return "STRING";
      case BOOL:
        return "BOOL";
      case FUNCTOR:
        return "FUNCTOR";
      case DESCRIPTION_HOLDER:
        return "DESCRIPTION_HOLDER";
      case STRING_HOLDER:
        return "STRING_HOLDER";
      case SEPARATOR:
        return "SEPARATOR";
      case CPPSTRING:
        return "CPPSTRING";
    }
    return "UNKNOWN";
  }

  /// Sets the argument value, for int arguments
  AREXPORT bool setInt(int val, char *errorBuffer = NULL,
		       size_t errorBufferLen = 0, bool doNotSet = false);
  /// Sets the argument value, for double arguments
  AREXPORT bool setDouble(double val, char *errorBuffer = NULL,
			  size_t errorBufferLen = 0, bool doNotSet = false);
  /// Sets the argument value, for bool arguments
  AREXPORT bool setBool(bool val, char *errorBuffer = NULL,
			size_t errorBufferLen = 0, bool doNotSet = false);
  /// Sets the argument value for ArArgumentBuilder arguments
  AREXPORT bool setString(const char *str, char *errorBuffer = NULL,
			  size_t errorBufferLen = 0, bool doNotSet = false);

  /// Sets the argument value for string arguments
  AREXPORT bool setCppString(const std::string &str,
                          char *errorBuffer = NULL, size_t errorBufferLen = 0, 
                          bool doNotSet = false);

  /// Sets the argument by calling the setFunctor callback
  AREXPORT bool setArgWithFunctor(ArArgumentBuilder *argument, 
				  char *errorBuffer = NULL,
				  size_t errorBufferLen = 0,
				  bool doNotSet = false);

  /// Gets the argument value, use with INT values only
  AREXPORT int getInt(void) const; 
  /// Gets the argument value, use with DOUBLE values only
  AREXPORT double getDouble(void) const;
  /// Gets the argument value, use with BOOL values only
  AREXPORT bool getBool(void) const;
  /// Gets the argument value, use with STRING type values only
  AREXPORT const char *getString(void) const;

  /// Get a copy of the value, use with CPPSTRING type values only
  AREXPORT std::string getCppString(bool *ok = NULL) const;

  /// Gets the argument value, which is a list of argumentbuilders here
  AREXPORT const std::list<ArArgumentBuilder *> *getArgsWithFunctor(void) const;

  /// Logs the type, name, and value of this argument
  AREXPORT void log(bool verbose = false) const;
  
  /// Gets the minimum int value
  AREXPORT int getMinInt(void) const;
  /// Gets the maximum int value
  AREXPORT int getMaxInt(void) const;
  /// Gets the minimum double value
  AREXPORT double getMinDouble(void) const;
  /// Gets the maximum double value
  AREXPORT double getMaxDouble(void) const;

  /// Gets the priority (only used by ArConfig)
  AREXPORT ArPriority::Priority getConfigPriority(void) const;
  /// Sets the priority (only used by ArConfig)
  AREXPORT void setConfigPriority(ArPriority::Priority priority);

  /// Returns the display hint for this arg, or NULL if none is defined.
  AREXPORT const char *getDisplayHint() const;
  /// Sets the display hint for this arg.
  AREXPORT void setDisplayHint(const char *hintText);

  /// Sets whether to ignore bounds or not (default is to not to
  AREXPORT void setIgnoreBounds(bool ignoreBounds = false);

  /// Checks only the name, type, and value attributes and returns whether they are equal.
  AREXPORT bool isValueEqual(const ArConfigArg &other) const;
  
  /// If the given source is of the same type, copies its value to this arg
  /**
   * Note that this method currently only works for the primitive arg
   * types (i.e. int, bool, etc.).  It doesn't copy functors or description
   * holders.
   *
   * @param source the ArConfigArg whose value is to be copied to this arg
   * @return bool true if the value was copied; false if the source was of a 
   * different (or non-copyable) type
  **/
  AREXPORT bool setValue(const ArConfigArg &source);

  /// Gets whether this value has been set since it was last cleared or not
  bool isValueSet(void) { return myValueSet; }
  
  /// Tells the configArg that the value hasn't been set
  void clearValueSet(void) { myValueSet = false; }

 
private:
  /// Internal helper function
  void clear(bool initial);
  void copy(const ArConfigArg &arg);
  
  void set(ArConfigArg::Type type,
           const char *name,
           const char *description);

protected:
  enum IntType {
    INT_NOT, ///< Not an int
    INT_INT, ///< An int (4 bytes) 
    INT_SHORT, ///< A short (2 bytes)
    INT_UNSIGNED_SHORT, ///< An unsigned short (2 bytes)
    INT_UNSIGNED_CHAR ///< An unsigned char (1 byte)
  };
  
  ArConfigArg::Type myType;
  std::string myName;
  std::string myDescription;
  bool myOwnPointedTo;
  int *myIntPointer;
  short *myIntShortPointer;
  unsigned short *myIntUnsignedShortPointer;
  unsigned char *myIntUnsignedCharPointer;
  int myMinInt, myMaxInt;
  double *myDoublePointer;
  double myMinDouble, myMaxDouble;
  bool *myBoolPointer;
  char *myStringPointer;
  size_t myMaxStrLen;
  bool myUsingOwnedString;
  std::string myString;
  std::string *myCppStringPtr;
  ArPriority::Priority myConfigPriority;
  ArConfigArg::IntType myIntType;
  bool myIgnoreBounds;
  ArRetFunctor1<bool, ArArgumentBuilder *> *mySetFunctor;
  ArRetFunctor<const std::list<ArArgumentBuilder *> *> *myGetFunctor;
  std::string myDisplayHint;
  bool myValueSet;
};

#endif // ARARGUMENT_H
