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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class MinistryExpenseController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IMinistryExpenseRepository MinistryExpenseRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MinistryExpenseController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IMinistryExpenseRepository MinistryExpenseParam,
            IMinistryMemberRepository MinistryMemberParam, ISubCategoryRepository SubCategoryParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            MinistryExpenseRepository = MinistryExpenseParam;
            MinistryMemberRepository = MinistryMemberParam;
            SubCategoryRepository = SubCategoryParam;

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

                    if ((user.role.Name == "Officer") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }

                    if ((user.role.Name == "FinanceStaff") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor3 = true;
                    }
                }
            }
        }

        //
        // GET: /MinistryExpense/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Index(int ministryID = 0)
        {
            ViewBag.MinistryID = ministryID;
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /MinistryExpense/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Details(int ministryID = 0)
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.BeginDate = DateTime.Now.AddDays(-90).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /MinistryExpense/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Create(int ministryID)
        {
            GetData(ministryID);
            ViewBag.MinistryID = ministryID;
            return PartialView(new ministryexpense { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), ministryID = ministryID });
        } 

        //
        // POST: /MinistryExpense/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(ministryexpense ministryexpense)
        {
            try
            {
                if (ministryexpense.Comment == null) { ministryexpense.Comment = ""; }
                if (ministryexpense.CheckNumber == null) { ministryexpense.CheckNumber = ""; }

                if (ModelState.IsValid)
                {
                    db.ministryexpenses.Add(ministryexpense);
                    db.SaveChanges();
                    MinistryExpenseRepository.AddRecord(ministryexpense);
                    TempData["Message2"] = "Ministry expense record added successfully.";
                    GetData(ministryexpense.ministryExpenseID);
                    return RedirectToAction("Create", new { ministryID = ministryexpense.ministryID});
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding ministry expense record";
            }
            GetData(ministryexpense.ministryExpenseID);

            return PartialView(ministryexpense);
        }
        
        //
        // GET: /MinistryExpense/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Edit(int MinistryExpenseID)
        {
            ministryexpense ministryexpense = MinistryExpenseRepository.GetExpenseByID(MinistryExpenseID);
            GetData(ministryexpense.ministryID);
            return PartialView(ministryexpense);
        }

        //
        // POST: /MinistryExpense/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ministryexpense ministryexpense)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(ministryexpense).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Ministry expense update successfully.");
                    GetData(ministryexpense.ministryExpenseID);
                    return RedirectToAction("Details", new { ministryID = ministryexpense.ministryID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} ministry expense record.", ministryexpense.Title);
            }
            GetData(ministryexpense.ministryExpenseID);
            return PartialView(ministryexpense);
        }

        //
        // GET: /MinistryExpense/Delete/5

        public ActionResult Delete(int MinistryExpenseID)
        {
            ViewBag.MinistryExpenseID = MinistryExpenseID;
            ministryexpense ministryexpense = MinistryExpenseRepository.GetExpenseByID(MinistryExpenseID);
            return PartialView(ministryexpense);
        }

        //
        // POST: /MinistryExpense/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MinistryExpenseID)
        {
            ministryexpense ministryexpense = MinistryExpenseRepository.GetExpenseByID(MinistryExpenseID);
            MinistryExpenseRepository.DeleteRecord(ministryexpense);
            return RedirectToAction("Index", new { ministryID = ministryexpense.ministryID });
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

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<int, string> SubCategoryList;
            SubCategoryList = ConstantRepository.GetConstantByMinistryExpenseType(ministryID);
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);
            ViewBag.MinistryCode = string.Format("{0}&E", ministryID);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<ministryexpense> MinistryExpenseList;

            if (SearchType == "MinistrySearch")
            {
                MinistryExpenseList = MinistryExpenseRepository.GetExpenseByMinistry(codeID, bDate, eDate);
            }
            else if (SearchType == "CategorySearch")
            {
                MinistryExpenseList = MinistryExpenseRepository.GetExpenseByCategory(codeID, codeID2, bDate, eDate);
            }
            else
            {
                MinistryExpenseList = MinistryExpenseRepository.GetExpenseByDateRange(bDate, eDate);
            }

            foreach (var i in MinistryExpenseList)
            {
                i.FundTitle = ConstantRepository.GetConstantID(i.subCategoryID).Value1;
            }

            ViewBag.RecordCount = MinistryExpenseList.Count();

            decimal sum = MinistryExpenseList.Sum(e => e.Amount);
            ViewBag.Heading = string.Format("Total: {0:c}", sum);

            return PartialView(MinistryExpenseList);
        }
    }
}