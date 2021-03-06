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
    
    public partial class payee
    {
        public int payeeID { get; set; }
        public int BankAccountID { get; set; }
        public int SubCategoryID { get; set; }
        public string AccountNumber { get; set; }
        public string PayeeName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string URL { get; set; }
        public string Frequency { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public int PayeeTypeID { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual bankaccount bankaccount { get; set; }
        public virtual subcategory subcategory { get; set; }

        public virtual ICollection<bill> Bills { get; set; }
        public virtual ICollection<payeecategory> PayeeCategory { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }
}
