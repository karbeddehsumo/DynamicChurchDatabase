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
    
    public partial class programeventbudget
    {
        public int ProgramEventBudgetID { get; set; }
        public int ProgramEventID { get; set; }
        public string Title { get; set; }
        public string Requestor { get; set; }
        public decimal ProjectedTotalAmount { get; set; }
        public Nullable<decimal> ActualTotalAmount { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string CheckNumber { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual programevent programevent { get; set; }
    }
}
