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
    
    public partial class payeecategory
    {
        public int payeeID { get; set; }
        public int categoryID { get; set; }

        public virtual payeecategory payee_category { get; set; }
        public virtual category category { get; set; }

    }
}