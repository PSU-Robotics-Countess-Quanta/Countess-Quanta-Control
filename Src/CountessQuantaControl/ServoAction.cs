//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CountessQuantaControl
{
    using System;
    using System.Collections.Generic;
    
    public partial class ServoAction
    {
        public int ServoActionID { get; set; }
        public int Acceleration { get; set; }
        public decimal DurationSec { get; set; }
        public int Position { get; set; }
        public Nullable<int> ServoID { get; set; }
        public Nullable<int> ActionID { get; set; }
    
        public virtual Servo Servo { get; set; }
        public virtual Action Action { get; set; }
    }
}