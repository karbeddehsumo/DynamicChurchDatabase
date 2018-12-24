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
using WebUI.Filters;

namespace WebUI.Controllers
{
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class MeetingAgendaController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IMeetingRepository MeetingRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMeetingAgendaRepository MeetingAgendaRepository;
        private IMeetingNotesRepository MeetingNoteRepository;
        private ITaskRepository TaskRepository;
        private IGoalRepository GoalRepository;
        private IAttendanceRepository AttendanceRepository;
        private IMemberRepository MemberRepository;
        private IActionItemRepository ActionItemRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MeetingAgendaController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IMeetingRepository MeetingParam,
            IMinistryMemberRepository MinistryMemberParam, IMeetingAgendaRepository MeetingAgendaParam, IMeetingNotesRepository MeetingNoteParam,
            IGoalRepository GoalParam, ITaskRepository TaskParam, IAttendanceRepository AttendanceParam, IMemberRepository MemberParam, IActionItemRepository ActionItemParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            MeetingRepository = MeetingParam;
            MinistryMemberRepository = MinistryMemberParam;
            MeetingAgendaRepository = MeetingAgendaParam;
            MeetingNoteRepository = MeetingNoteParam;
            GoalRepository = GoalParam;
            TaskRepository = TaskParam;
            AttendanceRepository = AttendanceParam;
            MemberRepository = MemberParam;
            ActionItemRepository = ActionItemParam;

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
        // GET: /MeetingAgenda/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /MeetingAgenda/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /MeetingAgenda/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int MeetingID = 0, int taskID  = 0)
        {
            GetData(MeetingID);
           if (taskID > 0)
            {
                ViewBag.isTask = true;
                task task = TaskRepository.GetTaskByID(taskID);
             return PartialView(new meetingagenda {Description=task.Title, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", meetingID = MeetingID, TaskID = taskID });
            }
            else
            {
                ViewBag.isTask = false;
             return PartialView(new meetingagenda {DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", meetingID = MeetingID, TaskID = taskID });
            }
        } 

        //
        // POST: /MeetingAgenda/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(meetingagenda meetingagenda)
        {
            try
            {
                //if (budget.Comment == null) { budget.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.meetingagendas.Add(meetingagenda);
                    db.SaveChanges();
                    MeetingAgendaRepository.AddRecord(meetingagenda);
                    TempData["Message2"] = "Meeting agendata created successfully.";
                    GetData(meetingagenda.meetingID);
                    return RedirectToAction("Create", new { MeetingID = meetingagenda.meetingID, taskID=0 });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding meeting agenda";
            }
            GetData(meetingagenda.meetingID);

            return PartialView(meetingagenda);
        }
        
        //
        // GET: /MeetingAgenda/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int MeetingAgendaID)
        {
            meetingagenda meetingagenda = MeetingAgendaRepository.GetAgendaByID(MeetingAgendaID);
            GetData(meetingagenda.meetingID);
            return PartialView(meetingagenda);
        }

        //
        // POST: /MeetingAgenda/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(meetingagenda meetingagenda)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(meetingagenda).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Meeting agenda update successfully.");
                    GetData(meetingagenda.meetingID);
                    return RedirectToAction("Edit", new { MeetingAgendaID = meetingagenda.meetingAgendaID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing meeting agenda.");
            }
            GetData(meetingagenda.meetingID);
            return PartialView(meetingagenda);
        }

        //
        // GET: /MeetingAgenda/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Delete(int MeetingAgendaID)
        {
            ViewBag.MeetingAgendaID = MeetingAgendaID;
            meetingagenda meetingagenda = MeetingAgendaRepository.GetAgendaByID(MeetingAgendaID);
            //Delete meeting note
            meetingnote note = MeetingNoteRepository.GetMeetingNotesByAgenda(meetingagenda.meetingAgendaID);
            MeetingNoteRepository.DeleteRecord(note);
            return PartialView(meetingagenda);
        }

        //
        // POST: /MeetingAgenda/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MeetingAgendaID)
        {
            meetingagenda meetingagenda = MeetingAgendaRepository.GetAgendaByID(MeetingAgendaID);
            MeetingAgendaRepository.DeleteRecord(meetingagenda);
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int meetingID = 0)
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Meeting Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<int, string> MinistryMeetingList;
            MinistryMeetingList = MeetingRepository.GetMinistryMeetingList(meetingID);
            ViewBag.MinistryMeetingList = new SelectList(MinistryMeetingList, "Key", "Value", id);

            Dictionary<int, string> MeetingAgenda;
            MeetingAgenda = MeetingAgendaRepository.GetAgendaList(meetingID);
            ViewBag.MeetingAgenda = new SelectList(MeetingAgenda, "Key", "Value", id); 

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "")
        {
            IEnumerable<meetingagenda> AgendaList;

            if(SearchType == "MinistrySearch")
            {
                AgendaList = MeetingAgendaRepository.GetAgendaByMinistry(codeID,bDate,eDate);
            }
            else if (SearchType == "MeetingSearch")
            {
                AgendaList = MeetingAgendaRepository.GetAgendaByMeeting(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                AgendaList = MeetingAgendaRepository.GetAgendaByStatus(code);
            }
            else
            {
                AgendaList = MeetingAgendaRepository.GetAgendaByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = AgendaList.Count();

            return PartialView(AgendaList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult AddAgenda(int MeetingID)
        {
            ViewBag.MeetingID = MeetingID;



            string code = string.Format("MeetingIDTask={0}",MeetingID);
            ViewBag.TaskActionItemCode = code;
            code = string.Format("MeetingIDAgenda={0}", MeetingID);
            ViewBag.AgendaActionItemCode = code;
            IEnumerable<task> ActionItems = ActionItemRepository.GetActionItemTaskList(code);
            ViewBag.ActionItems = ActionItems;
            ViewBag.ActionItemRecordCount = ActionItems.Count();
                
            meeting meeting = MeetingRepository.GetMeetingByID(MeetingID);
            ViewBag.Heading = meeting.Title;

            int id = 0;
            Dictionary<int, string> GoalList;
            GoalList = GoalRepository.GetGoalList(meeting.ministryID);
            ViewBag.GoalList = new SelectList(GoalList, "Key", "Value", id);

            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(meeting.ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            ViewBag.CalendarID = meeting.CalendarID;
            

            return PartialView();
        }

        public ActionResult MeetingAgendaList(int MeetingID)
        {
            IEnumerable<meetingagenda> AgendaList = MeetingAgendaRepository.GetAgendaByMeeting(MeetingID);
            ViewBag.RecordCount = AgendaList.Count();
            int n = 0;
            foreach (var i in AgendaList)
            {
                i.task = TaskRepository.GetTaskByID(i.TaskID);
                meetingnote note = MeetingNoteRepository.GetMeetingNotesByAgenda(i.meetingAgendaID);
                if (note != null)
                {
                    i.HasNotes = true;
                }
                else
                {
                    i.HasNotes = false;
                }

            }
            return PartialView(AgendaList);
        }

        public void AddGeneralAgenda(int meetingID2, string Description, int taskID = 0)
        {
            meetingagenda agenda = new meetingagenda();
            if (taskID > 0)
            {
                task task = TaskRepository.GetTaskByID(taskID);
                agenda.TaskID = taskID;
            }          
            agenda.meetingID = meetingID2;
            agenda.Description = Description;
            agenda.Status = "Active";
            agenda.EnteredBy = User.Identity.Name.ToString();
            agenda.DateEntered = DateTime.Now.Date;
            db.meetingagendas.Add(agenda);
            db.SaveChanges();
            MeetingAgendaRepository.AddRecord(agenda);
        }

    }
}