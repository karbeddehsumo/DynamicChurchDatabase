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
 //   [RoleAuthentication()]
    public class ProgramEventBudgetController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IProgramEventBudgetRepository ProgramEventBudgetRepository;
        private IProgramEventRepository ProgramEventRepository;
        private IMinistryRepository MinistryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ProgramEventBudgetController(IConstantRepository ConstantParam, IProgramEventBudgetRepository ProgramEventBudgetParam, IProgramEventRepository ProgramEventParam,
                          IMinistryRepository MinistryParam)
        {
            ConstantRepository = ConstantParam;
            ProgramEventBudgetRepository = ProgramEventBudgetParam;
            ProgramEventRepository = ProgramEventParam;
            MinistryRepository = MinistryParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                }
            }
        }
        //
        // GET: /Pledge/

        public ViewResult Index()
        {
            GetData();
            return View();
        }

        //
        // GET: /Pledge/Details/5

        public ViewResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return View();
        }

        //
        // GET: /Pledge/Create

        public ActionResult Create(int programEventID = 0)
        {
            GetData();
            return View(new programeventbudget {DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", ProgramEventID = programEventID, ActualTotalAmount=0});
        } 


        //
        // POST: /ProgramEventBudget/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(programeventbudget programeventbudget)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.programeventbudgets.Add(programeventbudget);
                    db.SaveChanges();
                    ProgramEventBudgetRepository.AddRecord(programeventbudget);
                    TempData["Message2"] = "Event budget added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding event budget";
            }
            GetData();

            return View(programeventbudget);
        }
        
        //
        // GET: /ProgramEventBudget/Edit/5
 
        public ActionResult Edit(int ProgramEventBudgetID)
        {
            programeventbudget programeventbudget = ProgramEventBudgetRepository.GetEventBudgetByID(ProgramEventBudgetID);
            return View(programeventbudget);
        }

        //
        // POST: /ProgramEventBudget/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(programeventbudget programeventbudget)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(programeventbudget).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", programeventbudget.Title);
                    GetData();
                    return RedirectToAction("Edit", new { ProgramEventBudgetID = programeventbudget.ProgramEventBudgetID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", programeventbudget.Title);
            }
            GetData();
            return View(programeventbudget);
        }

        //
        // GET: /ProgramEventBudget/Delete/5

        public ActionResult Delete(int ProgramEventBudgetID)
        {
            ViewBag.ProgramEventBudgetID = ProgramEventBudgetID;
            programeventbudget programeventbudget = ProgramEventBudgetRepository.GetEventBudgetByID(ProgramEventBudgetID);
            return View(programeventbudget);
        }

        //
        // POST: /ProgramEventBudget/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ProgramEventBudgetID)
        {
            programeventbudget programeventbudget = ProgramEventBudgetRepository.GetEventBudgetByID(ProgramEventBudgetID);
            ProgramEventBudgetRepository.DeleteRecord(programeventbudget);
            return RedirectToAction("List");
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
            StatusList = ConstantRepository.GetConstantByCategory("Budget Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> EventBudgetList;
            EventBudgetList = ProgramEventBudgetRepository.GetEventBudgetList("Active");
            ViewBag.EventBudgetList = new SelectList(EventBudgetList, "Key", "Value", id);

            Dictionary<int, string> ProgramEventList;
            ProgramEventList = ProgramEventRepository.GetEventList();
            ViewBag.ProgramEventList = new SelectList(ProgramEventList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);
        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<programeventbudget> BudgetList;

            if (SearchType == "EventSearch")
            {
                BudgetList = ProgramEventBudgetRepository.GetEventBudgetByEventID(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                BudgetList = ProgramEventBudgetRepository.GetEventBudgetByStatus(code);
            }
            else
            {
                BudgetList = ProgramEventBudgetRepository.GetEventBudgetByDueDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = BudgetList.Count();

            return View(BudgetList);
        }
    }
}