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
    
    public partial class ScreenAction
    {
        public int ScreenActionID { get; set; }
        public string ButtonName { get; set; }
        public string ButtonDesc { get; set; }
        public byte[] ButtonPic { get; set; }
        public Nullable<int> ActionID { get; set; }
    
        public virtual Action Action { get; set; }
    }
}