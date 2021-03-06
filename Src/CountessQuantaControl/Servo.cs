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
    
    public partial class Servo
    {
        public Servo()
        {
            this.ServoActions = new HashSet<ServoAction>();
        }
    
        public int ServoID { get; set; }
        public string ServoName { get; set; }
        public int ServoIndex { get; set; }
        public decimal PosLimitMax { get; set; }
        public decimal PosLimitMin { get; set; }
        public decimal SpeedLimMax { get; set; }
        public decimal SpeedLimMin { get; set; }
        public decimal DefaultPos { get; set; }
        public decimal DefaultSpeed { get; set; }
        public decimal DefaultAcceleration { get; set; }
    
        public virtual ICollection<ServoAction> ServoActions { get; set; }
    }
}
