//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class spouse
    {
        public int spouse1ID { get; set; }
        public int spouse2ID { get; set; }
        public Nullable<bool> JointTithe { get; set; }
        public string JointTitheTitle { get; set; }
        public int SpouseID { get; set; }
        public System.DateTime AnniversaryDate { get; set; }

        public virtual member person1 { get; set; }
        public virtual member person2 { get; set; }
    }
}
