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
    
    public partial class story
    {
        public int StoryID { get; set; }
        public int MinistryID { get; set; }
        public string Header { get; set; }
        public string StoryLine { get; set; }
        public Nullable<int> PictureID { get; set; }
        public Nullable<int> VideoID { get; set; }
        public Nullable<int> StoryTypeID { get; set; }
        public System.DateTime StoryDate { get; set; }
        public Nullable<bool> IsPublic { get; set; }
        public string Status { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        
 	    public IEnumerable<HttpPostedFileBase> files { get; set; }
	    public IEnumerable<HttpPostedFileBase> MainPic { get; set; }
        public IEnumerable<HttpPostedFileBase> GroupPic { get; set; }

        public virtual picture picture { get; set; }
	    public string StoryType{ get; set; }
	    public bool HasGroupPictures { get; set; }

    }
}
