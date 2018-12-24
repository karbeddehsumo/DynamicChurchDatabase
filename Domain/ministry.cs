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
    
    public partial class ministry
    {
        public int ministryID { get; set; }
        public string MinistryName { get; set; }
        public string Description { get; set; }
        public string Contact { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string BriefDescription { get; set; }
        public Nullable<int> PageStyleID { get; set; }
        public string Status { get; set; }
        public string CodeDesc { get; set; }
        public Nullable<bool> StoryCreateAccess { get; set; }
        public Nullable<bool> PictureCreateAccess { get; set; }
        public Nullable<bool> VideoCreateAccess { get; set; }
        public bool IsPublic { get; set; }
        public string DefaultMemberType { get; set; }
        public Nullable<bool> IsGroupMinistry { get; set; }
        public Nullable<int> GroupMinistryID { get; set; }
        public Nullable<int> PictureID { get; set; }
        public string MissionStatement { get; set; }
        public string Title1 { get; set; }
        public string Information1 { get; set; }
        public string Title2 { get; set; }
        public string Information2 { get; set; }
        public string Title3 { get; set; }
        public string Information3 { get; set; }
        public Nullable<bool> DisplayBanner { get; set; }
        public string Vision { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public virtual ICollection<announcement> Announcements { get; set; }
        public virtual ICollection<calendar> Calendars { get; set; }
        public virtual ICollection<goal> Goals { get; set; }
        public virtual ICollection<listtable> ListTables { get; set; }
        public virtual ICollection<meeting> Meetings { get; set; }
        public virtual ICollection<ministryexpense> MinistryExpenses { get; set; }
        public virtual ICollection<ministryincome> MinistryIncomes { get; set; }
        public virtual ICollection<ministrymember> MinistryMembers { get; set; }
        public virtual ICollection<picture> Pictures { get; set; }
        public virtual ICollection<video> Videos { get; set; }
	public string  ParentMinistryName{ get; set; }
 	public string PageStyle { get; set; }

 	
         public IEnumerable<HttpPostedFileBase> files { get; set; }
    }
}
