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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor")]
    public class ProgramEventController : Controller
    {
         private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IProgramEventRepository ProgramEventRepository;
        private ICalendarRepository CalendarRepository;
        private IMinistryRepository MinistryRepository;
        private IGoalRepository GoalRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IAttendanceRepository AttendanceRepository;
        private IDocumentRepository DocumentRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ProgramEventController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IProgramEventRepository ProgramEventParam,
                          ICalendarRepository CalendarParam, IMinistryRepository MinistryParam, IGoalRepository GoalParam, IMinistryMemberRepository MinistryMemberParam,
                          IAttendanceRepository AttendanceParam, IDocumentRepository DocumentParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            ProgramEventRepository = ProgramEventParam;
            CalendarRepository = CalendarParam;
            MinistryRepository = MinistryParam;
            GoalRepository = GoalParam;
            MinistryMemberRepository = MinistryMemberParam;
            AttendanceRepository = AttendanceParam;
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
        // GET: /Pledge/

        public ActionResult Index()
        {
           // GetData();
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5

        public ActionResult Details()
        {
           // GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create

        public ActionResult Create(int GoalID = 0)
        {
            goal goal = GoalRepository.GetGoalByID(GoalID);
            ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            ViewBag.Heading = string.Format("Create New Event for ({0} Goal - {1})", ministry.MinistryName, goal.Title);
            GetData(goal.ministryID);
            return PartialView(new programevent { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), goalID = GoalID, Status = "Active" });
        } 

        //
        // POST: /ProgramEvent/Create
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(programevent programevent)
        {
            int ministryID = GoalRepository.GetGoalByID(programevent.goalID).ministryID;
            constant cont = ConstantRepository.GetConstantListByCategory("Ministry Activity").FirstOrDefault(e => e.ConstantName == "Program/Event");
            try
            {               
                //set meeting on calendar
                calendar cal = new calendar();
                cal.ministryID = ministryID;
                cal.EventType = cont.constantID;
                cal.CalendarDate = programevent.C_When;
                cal.StartTime = programevent.BeginTime;
                cal.EndTime = programevent.EndTime;
                cal.Location = programevent.C_Where;
                cal.Title = programevent.Title;
                cal.Conductor = MemberRepository.GetMemberByID(programevent.C_Who).FullNameTitle;
                cal.PublicAccess = true;
                cal.Description = "";
                cal.EnteredBy = User.Identity.Name.ToString();
                cal.DateEntered = System.DateTime.Today;
                cal.Status = "Active";
                db.calendars.Add(cal);
                db.SaveChanges();

                CalendarRepository.AddRecord(cal);
                programevent.CalendarID = cal.calendarID;

                if (ModelState.IsValid)
                {
                    db.programevents.Add(programevent);
                    db.SaveChanges();
                    ProgramEventRepository.AddRecord(programevent);
                    TempData["Message2"] = "Event added successfully.";
                    GetData(ministryID);
                    return RedirectToAction("Create", new { GoalID = programevent .goalID});
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding event";
            }
            GetData(ministryID);
            return PartialView(programevent);
        }
        
        //
        // GET: /ProgramEvent/Edit/5
 
        public ActionResult Edit(int ProgramEventID)
        {
            int goalID = ProgramEventRepository.GetEventByID(ProgramEventID).goalID;
            int ministryID = GoalRepository.GetGoalByID(goalID).ministryID;
            GetData(ministryID);
            programevent programevent = ProgramEventRepository.GetEventByID(ProgramEventID);
            return PartialView(programevent);
        }

        //
        // POST: /ProgramEvent/Edit/5
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(programevent programevent)
        {
            int ministryID = GoalRepository.GetGoalByID(programevent.goalID).ministryID;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(programevent).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record updated successfully.", programevent.Title);
                    GetData(ministryID);
                    //return Content("Record update successfully.");
                    return RedirectToAction("Index","Goal");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", programevent.Title);
            }      
            GetData(ministryID);
            return PartialView(programevent);
        }

        //
        // GET: /ProgramEvent/Delete/5

         public ActionResult Delete(int ProgramEventID)
        {
            ViewBag.ProgramEventID = ProgramEventID;
            programevent programevent = ProgramEventRepository.GetEventByID(ProgramEventID);
            return PartialView(programevent);
        }

        //
        // POST: /ProgramEvent/Delete/5

        [HttpPost, ActionName("Delete")]
         public ActionResult DeleteConfirmed(int ProgramEventID)
        {
            try
            {
                programevent programevent = ProgramEventRepository.GetEventByID(ProgramEventID);
                calendar cal = CalendarRepository.GetCalendarByID(programevent.CalendarID);
               
                //delete meeting attendees
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
                if(programevent.DocumentID != null)
                {
                    document document = DocumentRepository.GetDocumentByID((int)programevent.DocumentID);
                    var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                    //var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                    bool exist = System.IO.File.Exists(string.Format("{0}", path));
                    if (exist)
                    {
                        System.IO.File.Delete(string.Format("{0}", path));
                        //System.IO.File.Delete(@"C:\test.txt");
                    }
                }

                ProgramEventRepository.DeleteRecord(programevent);
                TempData["Message2"] = string.Format("Event record deleted successfully.");
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error deleting event record.");
            }
           // return RedirectToAction("List");
            return RedirectToAction("Index", "Goal");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int ministryID = 0)
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MinistryMemberList;
            MinistryMemberList = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MinistryMemberList = new SelectList(MinistryMemberList, "Key", "Value", id);

            Dictionary<int, string> ProgramEventList;
            ProgramEventList = ProgramEventRepository.GetEventList();
            ViewBag.ProgramEventList = new SelectList(ProgramEventList, "Key", "Value", id);

            Dictionary<int, string> CalendarList;
            CalendarList = CalendarRepository.GetCalendarList();
            ViewBag.CalendarList = new SelectList(CalendarList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<string, string> MeetingVenueList;
            MeetingVenueList = ConstantRepository.GetConstantByCategory("Church Venue");
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

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<programevent> EventList;

            if (SearchType == "MinistrySearch")
            {
                EventList = ProgramEventRepository.GetEventByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                EventList = ProgramEventRepository.GetEventByStatus(code, bDate, eDate);
            }
            else
            {
                EventList = ProgramEventRepository.GetEventByDateRange(bDate, eDate);
            }

              ViewBag.RecordCount = EventList.Count();
  
            int memberID;
            foreach (var i in EventList)
            {
                memberID = Convert.ToInt16(i.C_Who);
                i.EventContact = MemberRepository.GetMemberByID(memberID).FullNameTitle;
            }

            return PartialView(EventList);
        }

        public ActionResult ListMinistry(int ministryID,DateTime bDate, DateTime eDate)
        {
            IEnumerable<programevent> EventList = ProgramEventRepository.GetEventByMinistry(ministryID);
            return PartialView(EventList);
        }

        public ActionResult AddAttendance(int ProgramEventID)
        {
            programevent evnt = ProgramEventRepository.GetEventByID(ProgramEventID);
            ViewBag.CalendarID = evnt.CalendarID;         
            int ministryID = GoalRepository.GetGoalByID(evnt.goalID).ministryID;


            int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            IEnumerable<attendance> meetingAttendance = AttendanceRepository.GetAttendanceByCalendar(evnt.CalendarID);
            ViewBag.AttendanceRecordCount = meetingAttendance.Count();
            foreach (var a in meetingAttendance)
            {
                a.member = MemberRepository.GetMemberByID(a.memberID);
            }
            ViewBag.MeetingAttendees = meetingAttendance;
            return View();
        }

        public ActionResult GoalEvents(int GoalID)
        {
            goal goal = GoalRepository.GetGoalByID(GoalID);
            ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            ViewBag.Header = string.Format("Event List for ({0} Goal- {1})",ministry.MinistryName, goal.Title);
            ViewBag.GoalID = GoalID;
            ViewBag.MinistryID = goal.ministryID;
            IEnumerable<programevent> EventList = ProgramEventRepository.GetEventByGoal(GoalID);
            foreach (programevent p in EventList)
            {
                p.member = MemberRepository.GetMemberByID(p.C_Who);
                p.calendar = CalendarRepository.GetCalendarByID(p.CalendarID);
                if(p.DocumentID != null)
                {
                  p.document = DocumentRepository.GetDocumentByID((int)p.DocumentID);
                }
            }
            ViewBag.RecordCount = EventList.Count();
            return PartialView(EventList);
        }


    }
}