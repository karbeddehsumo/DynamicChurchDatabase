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
    
    public partial class calendar
    {
        public int calendarID { get; set; }
        public int ministryID { get; set; }
        public int EventType { get; set; }
        public System.DateTime CalendarDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Conductor { get; set; }
        public bool PublicAccess { get; set; }
        public Nullable<int> DocumentID { get; set; }
        public Nullable<int> PictureID { get; set; }
        public Nullable<System.DateTime> CalendarEndDate { get; set; }
        public Nullable<bool> DisplayBanner { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

        public string CalendarDateTime { get { return string.Format("{0},{1} {2} - {3}", CalendarDate.DayOfWeek, 				CalendarDate.ToShortDateString(), StartTime, EndTime); } }

        public virtual ministry ministry { get; set; }

        public virtual ICollection<attendance> Attendances { get; set; }
 	public document document { get; set; }

	public string ReturnMethod { get; set; }
        public DateTime ReturnBeginDate { get; set; }
        public DateTime ReturnEndDate { get; set; }
        public string ReturnSearchType { get; set; }
        public int ReturnCodeID { get; set; }
        public string ReturnCodeName { get; set; }
        public string ReturnCallerType { get; set; }

	 
         public IEnumerable<HttpPostedFileBase> files { get; set; }

	public string EventTypeDesc { get; set; }
	public picture picture { get; set; }
    }
}
