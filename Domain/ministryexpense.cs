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
    
    public partial class ministryexpense
    {
        public int ministryExpenseID { get; set; }
        public int ministryID { get; set; }
        public int subCategoryID { get; set; }
        public string Title { get; set; }
        public System.DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string CheckNumber { get; set; }
        public string Comment { get; set; }
        public Nullable<bool> Reconcile { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual string FundTitle { get; set; }
        public virtual ministry ministry { get; set; }
    }
}