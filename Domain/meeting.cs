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
    
    public partial class meeting
    {
        public int meetingID { get; set; }
        public System.DateTime meetingDate { get; set; }
        public string meetingVenue { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ministryID { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> NextMeetingDate { get; set; }
        public string Status { get; set; }
        public int CalendarID { get; set; }
        public Nullable<int> DocumentID { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }
        public string Conductor { get; set; }

        public string MeetingDateTime { get { return string.Format("{0},{1} {2} - {3}",meetingDate.DayOfWeek,meetingDate.ToShortDateString(), StartTime, EndTime); } }

        public virtual ministry ministry { get; set; }

        public virtual ICollection<meetingagenda> MeetingAgendas { get; set; }
 	public calendar calendar { get; set; }
        public document document { get; set; }
       
         
         public IEnumerable<HttpPostedFileBase> files { get; set; }
	
	public DateTime ReturnBeginDate {get; set;}
        public DateTime ReturnEndDate {get; set;}
        public string ReturnSearchType {get; set;}
        public int ReturnCodeID {get; set;}
        public string ReturnCode {get; set;}
        public string ReturnCallerType {get; set;}
	public string ReturnMethod { get; set; }
    }
}
