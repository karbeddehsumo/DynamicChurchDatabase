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
    
    public partial class ministrymember
    {
        public int MinistryMemberID { get; set; }
        public int ministryID { get; set; }
        public int memberID { get; set; }
        public string OfficeTitle { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<System.DateTime> MembershipDate { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual ICollection<goal> Goals { get; set; }
        public virtual ICollection<task> Tasks { get; set; }
        public member member { get; set; }
    }
}
