using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain;
using WebUI.Models.churchdatabaseEntities;
using Domain.Abstract;
using Domain.Concrete;
using WebUI.Filters;

namespace WebUI.Controllers
{
  //  [RoleAuthentication()]
    public class AttendanceController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IAttendanceRepository AttendanceRepository;
        private ICalendarRepository CalendarRepository;
        private IMemberRepository MemberRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public AttendanceController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IAttendanceRepository AttendanceParam,
                                    ICalendarRepository CalendarParam, IMemberRepository MemberParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            AttendanceRepository = AttendanceParam;
            CalendarRepository = CalendarParam;
            MemberRepository = MemberParam;

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
        // GET: /Attendance/

        public ActionResult Index()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Attendance/Details/5

        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /Attendance/Create

        public ActionResult Create(int calendarID)
        {
            GetData();
            return View(new attendance {calendarID = calendarID, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString() });
        } 

        //
        // POST: /Attendance/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(attendance attendance)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }

                if (ModelState.IsValid)
                {
                    db.attendances.Add(attendance);
                    db.SaveChanges();
                    AttendanceRepository.AddRecord(attendance);
                    TempData["Message2"] = "attendance record added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding Attendance";
            }
            GetData();

            return PartialView(attendance);
        }
        
        //
        // GET: /Attendance/Edit/5

        public ActionResult Edit(int AttandenceID)
        {
            attendance attendance = AttendanceRepository.GetAttendanceByID(AttandenceID);
            return PartialView(attendance);
        }

        //
        // POST: /Attendance/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(attendance attendance)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(attendance).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Attendance update successfully.");
                    GetData();
                    return RedirectToAction("Edit", new { AttandenceID = attendance.attendanceID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing attendance.");
            }
            GetData();
            return PartialView(attendance);
        }

        //
        // GET: /Attendance/Delete/5
 
        public ActionResult Delete(int AttandenceID)
        {
            ViewBag.AttandenceID = AttandenceID;
            attendance attendance = AttendanceRepository.GetAttendanceByID(AttandenceID);
            return PartialView(attendance);
        }

        //
        // POST: /Attendance/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int AttandenceID)
        {
            attendance attendance = AttendanceRepository.GetAttendanceByID(AttandenceID);
            int calendarID = attendance.calendarID;
            AttendanceRepository.DeleteRecord(attendance);
            return RedirectToAction("MeetingAttendance", new { calendarID = calendarID});
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData()
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<int, string> CalendarList;
            CalendarList = CalendarRepository.GetCalendarList();
            ViewBag.CalendarList = new SelectList(CalendarList, "Key", "Value", id);
        }

        public ActionResult List(DateTime EventDate, string code = "", int codeID = 0)
        {
            GetData();
            IEnumerable<attendance> AttendanceList;

            if (code == "MemberSearch")
            {
                AttendanceList = AttendanceRepository.GetAttencanceByMember(codeID);
            }
            else if (code == "CalendarSearch")
            {
                AttendanceList = AttendanceRepository.GetAttendanceByCalendar(codeID);
            }
            else
            {
                AttendanceList = AttendanceRepository.GetAttendanceByDate(EventDate);
            }


            ViewBag.RecordCount = AttendanceList.Count();

            return PartialView(AttendanceList);
        }

        public void MinistryCalendar(int ministryID)
        {
            int id = 0;
            Dictionary<int, string> CalendarList;
            CalendarList = CalendarRepository.GetMinistryCalendarList(ministryID);
            ViewBag.CalendarList = new SelectList(CalendarList, "Key", "Value", id);
        }

        public void AddAttendance(int memberID, int calendarID)
        {
            attendance attd = new attendance();
            attd.memberID = memberID;
            attd.calendarID = calendarID;
            attd.RollCall = DateTime.Now;
            attd.EnteredBy = User.Identity.Name.ToString();
            attd.DateEntered = DateTime.Now.Date;
            db.attendances.Add(attd);
            db.SaveChanges();
            AttendanceRepository.AddRecord(attd);
        }

        public ActionResult MeetingAttendance(int calendarID)
        {
            IEnumerable<attendance> meetingAttendance = AttendanceRepository.GetAttendanceByCalendar(calendarID);
            ViewBag.AttendanceRecordCount = meetingAttendance.Count();
            foreach (var a in meetingAttendance)
            {
                a.member = MemberRepository.GetMemberByID(a.memberID);
            }
            ViewBag.MeetingAttendees = meetingAttendance;
           return PartialView();
        }
    }
}