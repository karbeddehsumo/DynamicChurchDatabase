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
    
    public partial class propertyinventory
    {
        public int propertyInventoryID { get; set; }
        public int propertyID { get; set; }
        public Nullable<System.DateTime> purchaseDate { get; set; }
        public Nullable<decimal> Value { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public string AssignedTo { get; set; }
        public string Condition { get; set; }
        public string Status { get; set; }
        public string TagNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public Nullable<int> PictureID { get; set; }
        public Nullable<int> DocumentID { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        

        public virtual property property { get; set; }
	public IEnumerable<HttpPostedFileBase> files { get; set; }
	 public virtual picture picture { get; set; }
 	 public IEnumerable<HttpPostedFileBase> documentFile { get; set; }
	public virtual document document { get; set; }
    }
}
