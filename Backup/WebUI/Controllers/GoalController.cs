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
    public class GoalController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IGoalRepository GoalRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMemberRepository MemberRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public GoalController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IGoalRepository GoalParam, IMinistryMemberRepository MinistryMemberParam,
            IMemberRepository MemberParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            GoalRepository = GoalParam;
            MinistryMemberRepository = MinistryMemberParam;
            MemberRepository = MemberParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
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
        //
        // GET: /Goal/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Goal/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details(int ministryID = 0)
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.AddDays(90).ToShortDateString();
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /Goal/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int ministryID = 0)
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();

            GetData(ministryID);
            ViewBag.HasMinistryID = false;
            if (ministryID > 0)
            {
                ViewBag.HasMinistryID = true;
                ViewBag.MinistrtyID = ministryID;
                return PartialView(new goal { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), CompletionRatio = 0, Status = "Active", ministryID = ministryID });
            }
            else
            {
                return PartialView(new goal { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), CompletionRatio = 0, Status = "Active" });
            }
        } 

        //
        // POST: /Goal/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(goal goal)
        {
            try
            {
                if (goal.Comment == null) { goal.Comment = ""; }
                if (goal.Description == null) { goal.Description = ""; }

                if (ModelState.IsValid)
                {
                    db.goals.Add(goal);
                    db.SaveChanges();
                    TempData["Message2"] = "Goal record added successfully.";
                    GetData(goal.ministryID);
                    GoalRepository.AddRecord(goal);
                   // return RedirectToAction("Create", new {ministryID =goal.ministryID});
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding goal";
            }
            GetData(goal.ministryID);
            return PartialView(goal);
        }
        
        //
        // GET: /Goal/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int GoalID)
        {
            goal goal = GoalRepository.GetGoalByID(GoalID);
            goal.ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            GetData(goal.ministryID);
            return PartialView(goal);
        }

        //
        // POST: /Goal/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(goal goal)
        {
            string MinistryName = db.ministries.Find(goal.ministryID).MinistryName;
            try
            {
                if (goal.Comment == null) { goal.Comment = ""; }
                if (goal.Description == null) { goal.Description = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(goal).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Goal update successfully.");
                    GetData(goal.ministryID);
                    return RedirectToAction("Details");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} goal.", MinistryName);
            }
            GetData(goal.ministryID);

            return PartialView(goal);
        }

        //
        // GET: /Goal/Delete/5

        public ActionResult Delete(int GoalID)
        {
            ViewBag.GoalID = GoalID;
            goal goal = GoalRepository.GetGoalByID(GoalID);
            return PartialView(goal);
        }

        //
        // POST: /Goal/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int GoalID)
        {
            goal goal = GoalRepository.GetGoalByID(GoalID);
            GoalRepository.DeleteRecord(goal);
            return RedirectToAction("Details");
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

            Dictionary<string, string> GoalTypeList;
            GoalTypeList = ConstantRepository.GetConstantByCategory("Goal Type");
            ViewBag.GoalTypeList = new SelectList(GoalTypeList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            if (ministryID > 0)
            {
            Dictionary<int, string> MinistryMemberList;
            MinistryMemberList = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MinistryMemberList = new SelectList(MinistryMemberList, "Key", "Value", id);
            }

            string ratio;
            Dictionary<string, string> CompletionRatioList = new Dictionary<string, string>();
            for (int i = 0; i <= 100; i += 5)
            {
                ratio = string.Format("{0}",i);
                CompletionRatioList.Add(ratio, ratio);
            }
            ViewBag.CompletionRatioList = new SelectList(CompletionRatioList, "Key", "Value", id);

            Dictionary<string, string> CompletionRatioList2 = new Dictionary<string, string>();
            string Quarter = "Less than Quarter";
            string Half = "Less than Half";
            string Thirds = "Less than Third";
            string Full = "Less than Full";
            string Complete = "Fully Complete";

            CompletionRatioList2.Add(Quarter, Quarter);
            CompletionRatioList2.Add(Half, Half);
            CompletionRatioList2.Add(Thirds, Thirds);
            CompletionRatioList2.Add(Full, Full);
            CompletionRatioList2.Add(Complete, Complete);
            ViewBag.CompletionRatioList2 = new SelectList(CompletionRatioList2, "Key", "Value", id);

        }


        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string codeName = "", string codeName2 = "")
        {
            IEnumerable<goal> GoalList;
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;

            if (SearchType == "MinistrySearch")
            {
                GoalList = GoalRepository.GetGoalByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                GoalList = GoalRepository.GetGoalByStatus(codeName);
            }
            else if (SearchType == "Begin Date Search")
            {
                GoalList = GoalRepository.GetGoalBeginning(bDate, eDate);
            }
            else if (SearchType == "End Date Search")
            {
                GoalList = GoalRepository.GetGoalEnding(bDate, eDate);
            }
            else if (SearchType == "CompletionRatioSearch")
            {
                GoalList = GoalRepository.GetGoalByCompletionRatio(codeName);
            }
            else
            {
                GoalList = GoalRepository.GetGoalByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = GoalList.Count();

            foreach (goal g in GoalList)
            {
                g.ministry = MinistryRepository.GetMinistryByID(g.ministryID);
            }

            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Officer")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                }
            }


            return PartialView(GoalList);
        }

        public ActionResult ListMinistry(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string codeName = "", string codeName2 = "")
        {
            IEnumerable<goal> GoalList;

            if (SearchType == "MinistrySearch")
            {
                GoalList = GoalRepository.GetGoalByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                GoalList = GoalRepository.GetGoalByStatus(codeName);
            }
            else if (SearchType == "Begin Date Search")
            {
                GoalList = GoalRepository.GetGoalBeginning(bDate, eDate);
            }
            else if (SearchType == "End Date Search")
            {
                GoalList = GoalRepository.GetGoalEnding(bDate, eDate);
            }
            else if (SearchType == "CompletionRatioSearch")
            {
                GoalList = GoalRepository.GetGoalByCompletionRatio(codeName);
            }
            else
            {
                GoalList = GoalRepository.GetGoalByDateRange(bDate.Date, eDate.Date);
            }

            foreach (goal g in GoalList)
            {
                g.ministry = MinistryRepository.GetMinistryByID(g.ministryID);
                if(g.AssignedTo > 0)
                {
                    g.PersonAssignedTo.member = MemberRepository.GetMemberByID((int)g.AssignedTo);
                }
            }

            ministry ministry = MinistryRepository.GetMinistryByID(codeID);
            ViewBag.Ministry = ministry;
            ViewBag.RecordCount = GoalList.Count();

            ViewBag.Heading = string.Format("List of Goals for {0}", ministry.MinistryName);
            ViewBag.MinistryName = ministry.MinistryName;

            return PartialView(GoalList);
        }
    }

}