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
    using System.Web;
    
    public partial class property
    {
        public int propertyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int subCategoryID { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public Nullable<int> PictureID { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        
        public virtual subcategory subcategory { get; set; }
        public virtual ICollection<propertyinventory> PropertyInventories { get; set; }
	public IEnumerable<HttpPostedFileBase> files { get; set; }
	 public virtual picture picture { get; set; }
    }
}
