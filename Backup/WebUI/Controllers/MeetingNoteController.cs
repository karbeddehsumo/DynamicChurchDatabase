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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class MeetingNoteController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IMeetingRepository MeetingRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMeetingNotesRepository MeetingNoteRepository;
        private IMeetingAgendaRepository MeetingAgendaRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MeetingNoteController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IMeetingRepository MeetingParam,
            IMinistryMemberRepository MinistryMemberParam, IMeetingNotesRepository MeetingNoteParam, IMeetingAgendaRepository MeetingAgendaParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            MeetingRepository = MeetingParam;
            MinistryMemberRepository = MinistryMemberParam;
            MeetingNoteRepository = MeetingNoteParam;
            MeetingAgendaRepository = MeetingAgendaParam;

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
        // GET: /MeetingNote/
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /MeetingNote/Details/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /MeetingNote/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int MeetingAgendaID = 0)
        {
            string Agenda = MeetingAgendaRepository.GetAgendaByID(MeetingAgendaID).Description;
            ViewBag.Heading = Agenda;
            GetData();
            return PartialView(new meetingnote { NoteDate = System.DateTime.Today, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", MeetingAgendaID = MeetingAgendaID });
        } 

        //
        // POST: /MeetingNote/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(meetingnote meetingnote)
        {
            try
            {
                //if (budget.Comment == null) { budget.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.meetingnotes.Add(meetingnote);
                    db.SaveChanges();
                    MeetingNoteRepository.AddRecord(meetingnote);
                    TempData["Message2"] = "Meeting notes created successfully.";
                    GetData();
                    return Content("Meeting notes created successfully.");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding meeting notes";
            }
            GetData();

            return PartialView(meetingnote);
        }
        
        //
        // GET: /MeetingNote/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int MeetingAgendaID = 0)
        {
            GetData();
            int MeetingNoteID = MeetingNoteRepository.GetMeetingNotesByAgenda(MeetingAgendaID).meetingNoteID;
            meetingnote meetingnote = MeetingNoteRepository.GetMeetingNotesByID(MeetingNoteID);
            string Agenda = MeetingAgendaRepository.GetAgendaByID(MeetingAgendaID).Description;
            ViewBag.Heading = Agenda;
            return PartialView(meetingnote);
        }

        //
        // POST: /MeetingNote/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(meetingnote meetingnote)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(meetingnote).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Meeting note update successfully.");
                    GetData();
                    return Content("Meeting note update successfully.");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing meeting note.");
            }
            GetData();
            return PartialView(meetingnote);
        }

        //
        // GET: /MeetingNote/Delete/5

        public ActionResult Delete(int MeetingNoteID)
        {
            ViewBag.MeetingNoteID = MeetingNoteID;
            meetingnote meetingnote = MeetingNoteRepository.GetMeetingNotesByID(MeetingNoteID);
            return PartialView(meetingnote);
        }

        //
        // POST: /MeetingNote/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MeetingNoteID)
        {
            meetingnote meetingnote = MeetingNoteRepository.GetMeetingNotesByID(MeetingNoteID);
            MeetingNoteRepository.DeleteRecord(meetingnote);
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
            IEnumerable<meetingnote> NotesList;
             /*
            if(SearchType == "AgendaSearch")
            {
                NotesList = MeetingNoteRepository.GetMeetingNotesByAgenda(codeID);
            }
            else */
                if(SearchType == "MinistrySearch")
            {
                NotesList = MeetingNoteRepository.GetMeetingNotesByMinistry(codeID,bDate,eDate);
            }
                    /*
            else if(SearchType == "MeetingSearch")
            {
                NotesList = MeetingNoteRepository.GetMeetingNotesByAgenda(codeID);
            }
            else
                     * */
             else if (SearchType == "StatusSearch")
            {
                NotesList = MeetingNoteRepository.GetMeetingNotesByStatus(code);
            }
            else
            {
                NotesList = MeetingNoteRepository.GetMeetingNotesByDateRange(bDate,eDate);
            }

            ViewBag.RecordCount = NotesList.Count();

            return PartialView(NotesList);
        }
    }
}