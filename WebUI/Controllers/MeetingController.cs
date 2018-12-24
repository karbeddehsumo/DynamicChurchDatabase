using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;
using System.IO;
using System.Text;
using WebUI.Filters;

namespace WebUI.Controllers
{
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class MeetingController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IMeetingRepository MeetingRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private ICalendarRepository CalendarRepository;
        private IMeetingAgendaRepository MeetingAgendaRepository;
        private IAttendanceRepository AttendanceRepository;
        private IMeetingNotesRepository MeetingNotesRepository;
        private IMemberRepository MemberRepository;
        private ITaskRepository TaskRepository;
        private IDocumentRepository DocumentRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MeetingController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IMeetingRepository MeetingParam,
            IMinistryMemberRepository MinistryMemberParam, ICalendarRepository CalendarParam, IMeetingAgendaRepository MeetingAgendaRepoistory,
            IAttendanceRepository AttendanceParam, IMeetingNotesRepository MeetingNoteParam, IMemberRepository MemberParam, ITaskRepository TaskParam,
            IDocumentRepository DocumentParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            MeetingRepository = MeetingParam;
            MinistryMemberRepository = MinistryMemberParam;
            CalendarRepository = CalendarParam;
            MeetingAgendaRepository = MeetingAgendaRepoistory;
            AttendanceRepository = AttendanceParam;
            MeetingNotesRepository = MeetingNoteParam;
            MemberRepository = MemberParam;
            TaskRepository = TaskParam;
            DocumentRepository = DocumentParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "Officer") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }
        //
        // GET: /Meeting/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            ViewBag.BeginDate = DateTime.Now.AddDays(-90).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Meeting/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            ViewBag.BeginDate = DateTime.Now.AddDays(-90).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;
            GetData(ministryID);
            return PartialView();
        }

        
        // GET: /Meeting/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int MinistryID, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            GetData(MinistryID);
            return PartialView(new meeting {ministryID=MinistryID, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Held" });
        } 

        //
        // POST: /Meeting/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(meeting meeting, string CallerType = "")
        {
            meeting mtg = db.meetings.FirstOrDefault(e => e.meetingID == meeting.meetingID && e.Title == meeting.Title);
            if (mtg != null)
            {
                TempData["Message2"] = "Meeting already exist";
                return PartialView(meeting);
            }
            try
            {
                //if (budget.Comment == null) { budget.Comment = ""; }
                constant cont = ConstantRepository.GetConstantListByCategory("Ministry Activity").FirstOrDefault(e => e.ConstantName == "Ministry Meeting");
                //set meeting on calendar
                calendar cal = new calendar();
                cal.ministryID = meeting.ministryID;
                cal.EventType = cont.constantID;
                cal.CalendarDate = meeting.meetingDate;
                cal.StartTime = meeting.StartTime;
                cal.EndTime = meeting.EndTime;
                cal.Location = meeting.meetingVenue;
                cal.Title = meeting.Title;
                cal.Conductor = meeting.Conductor;
                cal.PublicAccess = false;
                cal.Description = "";
                cal.EnteredBy = User.Identity.Name.ToString();
                cal.DateEntered = System.DateTime.Today;
                cal.Status = "Active";
                db.calendars.Add(cal);
                db.SaveChanges();

                CalendarRepository.AddRecord(cal);
                meeting.CalendarID = cal.calendarID;
                
                if (ModelState.IsValid)
                {
                    db.meetings.Add(meeting);
                    db.SaveChanges();
                    MeetingRepository.AddRecord(meeting);
                    TempData["Message2"] = "Meeting created successfully.";
                    GetData(meeting.ministryID);
                    if (CallerType == "Ministry")
                    {
                        return RedirectToAction("Create", new {MinistryID=meeting.ministryID,CallerType = "Ministry"});
                    }
                    else
                    {
                        return RedirectToAction("Create");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding meeting record";
            }
            GetData(meeting.ministryID);

            return PartialView(meeting);
        }
        
        //
        // GET: /Meeting/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int MeetingID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnSearchType = "", int ReturnCodeID = 0, string ReturnCode = "", string ReturnCallerType = "", string ReturnMethod = "")
        {
            ViewBag.CallerType = ReturnCallerType;
            meeting meeting = MeetingRepository.GetMeetingByID(MeetingID);
            meeting.ReturnBeginDate = ReturnBeginDate;
            meeting.ReturnBeginDate = ReturnEndDate;
            meeting.ReturnSearchType = ReturnSearchType;
            meeting.ReturnCodeID = ReturnCodeID;
            meeting.ReturnCode = ReturnCode;
            meeting.ReturnCallerType = ReturnCallerType;
            meeting.ReturnMethod = ReturnMethod;

            meeting.ministry = MinistryRepository.GetMinistryByID(meeting.ministryID);
            GetData(meeting.ministryID);
            return PartialView(meeting);
        }

        //
        // POST: /Meeting/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(meeting meeting)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {

                    //document
                    foreach (var file in meeting.files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            int documentTypeID = ConstantRepository.GetConstantByName("Property Document").constantID;
                            // Get file info
                            document document = new document();
                            document.Title = string.Format("{0} document", meeting.Title);
                            document.DocumentTypeID = documentTypeID;
                            document.Status = "Active";
                            document.EnteredBy = User.Identity.Name.ToString();
                            document.DateEntered = System.DateTime.Today;
                            document.FileName = Path.GetFileName(file.FileName);
                            document.ContentLength = file.ContentLength;
                            document.ContentType = file.ContentType;
                            document.Author = "Meeting Document";
                            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                           // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                            file.SaveAs(path);
                            db.documents.Add(document);
                            db.SaveChanges();
                            meeting.DocumentID = document.documentID;
                        }

                    }

                    db.Entry(meeting).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("meeting record update successfully.");
                    GetData(meeting.ministryID);
                    /*       if (CallerType == "Ministry")
                           {
                               return Redirect(string.Format("/Home/Admin?Page=Meeting&MinistryID={0}&CallerType={1}", meeting.ministryID, "Ministry"));
                           }
                           else
                           {
                               return Redirect("/Home/Admin?Page=Meeting");
                           }
              
                           if (meeting.ReturnMethod == "AdminList")
                           {
                               return RedirectToAction("AdminList", new { bDate=meeting.ReturnBeginDate,  eDate=meeting.ReturnEndDate,  SearchType =meeting.ReturnSearchType,  codeID =meeting.ReturnCodeID,  code = meeting.ReturnCode,  CallerType =meeting.ReturnCallerType});
                           }
                           else
                           {
                               return RedirectToAction("List", new { bDate=meeting.ReturnBeginDate,  eDate=meeting.ReturnEndDate,  SearchType = meeting.ReturnSearchType,  codeID = meeting.ReturnCodeID,  code =meeting.ReturnCode});
                           }
       */
                    return Redirect(ReturnUrl);
                    
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} meeting record.", meeting.Title);
            }
            GetData(meeting.ministryID);
            return PartialView(meeting);
        }

        //
        // GET: /Meeting/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Delete(int MeetingID)
        {
            ViewBag.MeetingID = MeetingID;
            meeting meeting = MeetingRepository.GetMeetingByID(MeetingID);
            return PartialView(meeting);
        }

        //
        // POST: /Meeting/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MeetingID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnSearchType = "", int ReturnCodeID = 0, string ReturnCode = "", string ReturnCallerType = "", string ReturnMethod = "")
        {
            try
            {
                meeting meeting = MeetingRepository.GetMeetingByID(MeetingID);
                calendar cal = CalendarRepository.GetCalendarByID(meeting.CalendarID);
                //delete meeting agendas
                IEnumerable<meetingagenda> agenda = MeetingAgendaRepository.GetAgendaByMeeting(MeetingID);
                if (agenda.Count() > 0)
                {
                    foreach (var a in agenda)
                    {
                        MeetingAgendaRepository.DeleteRecord(a);
                    }
                }
                //delete meeting attendees
                if (cal != null)
                {
                    IEnumerable<attendance> attendance = AttendanceRepository.GetAttendanceByCalendar(cal.calendarID);
                    if (attendance.Count() > 0)
                    {
                        foreach (var a in attendance)
                        {
                            AttendanceRepository.DeleteRecord(a);
                        }
                    }
                    //delete meeting from calendar
                    CalendarRepository.DeleteRecord(cal);
                }
                MeetingRepository.DeleteRecord(meeting);
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error deleting meeting record.");
            }
            if (ReturnMethod == "AdminList")
            {
                return RedirectToAction("AdminList", new { bDate = ReturnBeginDate, eDate = ReturnEndDate, SearchType = ReturnSearchType, codeID = ReturnCodeID, code = ReturnCode, CallerType = ReturnCallerType });
            }
            else
            {
                return RedirectToAction("List", new { bDate = ReturnBeginDate, eDate = ReturnEndDate, SearchType = ReturnSearchType, codeID = ReturnCodeID, code = ReturnCode });
            }
            
            
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int ministryID = 0)
        {
            int id = 0;

            if (ministryID > 0)
            {
                Dictionary<int, string> MinistryMemberList;
                MinistryMemberList = MinistryMemberRepository.GetMinistryMemberList(ministryID);
                ViewBag.MinistryMemberList = new SelectList(MinistryMemberList, "Key", "Value", id);
            }

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Meeting Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            if (ministryID > 0)
            {
                MinistryList = new Dictionary<int, string>();
                string MinistryName = MinistryRepository.GetMinistryByID(ministryID).MinistryName;
                MinistryList.Add(ministryID, MinistryName);
            }
            else
            {
                MinistryList = MinistryRepository.GetMinistryList();
            }
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);
/*
            Dictionary<int, string> MeetingList;
            MeetingList = MeetingRepository.GetMeetingList();
            ViewBag.MeetingList = new SelectList(MeetingList, "Key", "Value", id);
*/
            Dictionary<string, string> MeetingVenueList;
            MeetingVenueList = ConstantRepository.GetConstantByCategory("Church Venue");
           // MeetingVenueList.Add("Conference Call","Conference Call");
            ViewBag.MeetingVenueList = new SelectList(MeetingVenueList, "Key", "Value", id);

            Dictionary<string, string> timeIntervals = new Dictionary<string, string>();
            TimeSpan startTime = new TimeSpan(0, 0, 0);
            DateTime startDate = new DateTime(DateTime.MinValue.Ticks); // Date to be used to get shortTime format.
            for (int i = 0; i < 48; i++)
            {
                int minutesToBeAdded = 30 * i;      // Increasing minutes by 30 minutes interval
                TimeSpan timeToBeAdded = new TimeSpan(0, minutesToBeAdded, 0);
                TimeSpan t = startTime.Add(timeToBeAdded);
                DateTime result = startDate + t;
                timeIntervals.Add(result.ToShortTimeString(), result.ToShortTimeString());      // Use Date.ToShortTimeString() method to get the desired format                
            }
            ViewBag.TimeIncrementList = new SelectList(timeIntervals, "Key", "Value", id);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", bool canEdit = true, string Requestor = "")
        {
            ViewBag.Requestor = Requestor;
            ViewBag.CanEdit = canEdit;
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.SearchType = SearchType;
            ViewBag.CodeID = codeID;
            ViewBag.Code = code;
            ViewBag.CallerType = "";
            ViewBag.MinistryID = codeID;

            IEnumerable<meeting> MeetingList;

            if (SearchType == "MinistrySearch")
            {
                MeetingList = MeetingRepository.GetMeetingByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                MeetingList = MeetingRepository.GetMeetingByStatus(code);
            }
            else
            {
                MeetingList = MeetingRepository.GetMeetingByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = MeetingList.Count();

            return PartialView(MeetingList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult AdminList(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", string CallerType = "")
        {
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.SearchType = SearchType;
            ViewBag.CodeID = codeID;
            ViewBag.Code = code;
            ViewBag.CallerType = CallerType;


            IEnumerable<meeting> MeetingList;
            ViewBag.CallerType = CallerType;

            if (SearchType == "MinistrySearch")
            {
                MeetingList = MeetingRepository.GetMeetingByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                MeetingList = MeetingRepository.GetMeetingByStatus(code);
            }
            else
            {
                MeetingList = MeetingRepository.GetMeetingByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = MeetingList.Count();

            foreach (meeting m in MeetingList)
            {
                m.ministry = MinistryRepository.GetMinistryByID(m.ministryID);
                if(m.CalendarID > 0)
                {
                    m.calendar = CalendarRepository.GetCalendarByID(m.CalendarID);
                }
                if (m.DocumentID != null)
                {
                    m.document = DocumentRepository.GetDocumentByID((int)m.DocumentID);
                }
            }

            return PartialView(MeetingList);
        }

        public ActionResult MeetingReport(int MeetingID)
        {
            meeting meeting = MeetingRepository.GetMeetingByID(MeetingID);
            meeting.ministry = MinistryRepository.GetMinistryByID(meeting.ministryID);
            IEnumerable<meetingagenda> agenda = MeetingAgendaRepository.GetAgendaByMeeting(MeetingID);
            ViewBag.HasAgenda = false;
            if (agenda != null)
            {
                ViewBag.HasAgenda = true;
                foreach (meetingagenda a in agenda)
                {
                    if (a.TaskID > 0)
                    {
                        a.task = TaskRepository.GetTaskByID(a.TaskID);
                        a.task.member = MemberRepository.GetMemberByID(a.task.AssignTo);
                    }
                }
                ViewBag.Agenda = agenda;
            }

            IEnumerable<meetingnote> notes = MeetingNotesRepository.GetMeetingNotesByMeeting(MeetingID);
              ViewBag.HasNotes = false;
              if (notes != null)
              {
                  ViewBag.HasNotes = true;
                  foreach (meetingnote n in notes)
                  {
                      n.meetingagenda = MeetingAgendaRepository.GetAgendaByID(n.MeetingAgendaID);
                  }
                  ViewBag.Notes = notes;
              }

           int CalendarID = meeting.CalendarID;
           IEnumerable<attendance> attendees = AttendanceRepository.GetAttendanceByCalendar(CalendarID);
           ViewBag.HasAttendee = false;
            if(attendees != null)
            {
                ViewBag.HasAttendee = true;
                foreach (attendance a in attendees)
                {
                    a.member = MemberRepository.GetMemberByID(a.memberID);
                }

                ViewBag.Attendees = attendees;
            }
           
          
           return PartialView(meeting);
        }

    }
}