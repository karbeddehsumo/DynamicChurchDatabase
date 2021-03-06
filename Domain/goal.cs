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
    
    public partial class goal
    {
        public int goalID { get; set; }
        public int ministryID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<int> AssignedTo { get; set; }
        public Nullable<System.DateTime> BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<decimal> CompletionRatio { get; set; }
        public string SupervisorComment { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual ministry ministry { get; set; }
        public virtual ministrymember PersonAssignedTo { get; set; }
        public virtual ICollection<programevent> ProgramEvents { get; set; }
        public virtual ICollection<task> Tasks { get; set; }
    }
}
