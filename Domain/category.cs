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
    
    public partial class category
    {
        public int categoryID { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }

        public virtual ICollection<payeecategory> PayeeCategory { get; set; }
        public virtual ICollection<product> Products { get; set; }
        public virtual ICollection<subcategory> SubCategories { get; set; }
    }
}