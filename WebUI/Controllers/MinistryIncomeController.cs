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
    public class MinistryIncomeController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IMinistryIncomeRepository MinistryIncomeRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MinistryIncomeController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IMinistryIncomeRepository MinistryIncomeParam,
            IMinistryMemberRepository MinistryMemberParam, ISubCategoryRepository SubCategoryParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            MinistryIncomeRepository = MinistryIncomeParam;
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
        // GET: /MinistryIncome/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Index(int ministryID = 0)
        {
            ViewBag.MinistryID = ministryID;
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /MinistryIncome/Details/5

        public ActionResult Details(int ministryID)
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.BeginDate = DateTime.Now.AddDays(-90).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /MinistryIncome/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Create(int ministryID = 0)
        {
            GetData(ministryID);
            ViewBag.MinistryID = ministryID;
            return PartialView(new ministryincome { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), ministryID = ministryID });
        } 

        //
        // POST: /MinistryIncome/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(ministryincome ministryincome)
        {
            try
            {
                if (ministryincome.Comment == null) { ministryincome.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.ministryincomes.Add(ministryincome);
                    db.SaveChanges();
                    MinistryIncomeRepository.AddRecord(ministryincome);
                    TempData["Message2"] = "Ministry income record added successfully.";
                    return RedirectToAction("Create", new {ministryID = ministryincome.ministryID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding ministry income record";
            }
            GetData(ministryincome.ministryID);

            return PartialView(ministryincome);
        }
        
        //
        // GET: /MinistryIncome/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Edit(int MinistryIncomeID)
        {
            ministryincome ministryincome = MinistryIncomeRepository.GetIncomeByID(MinistryIncomeID);
            GetData(ministryincome.ministryID);
            GetData(ministryincome.ministryID);
            return PartialView(ministryincome);
        }

        //
        // POST: /MinistryIncome/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ministryincome ministryincome)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(ministryincome).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Ministry income update successfully.");
                    GetData(ministryincome.ministryID);
                    return RedirectToAction("Details", new { ministryID = ministryincome.ministryID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} ministry income record.", ministryincome.Title);
            }
            GetData(ministryincome.ministryID);
            return PartialView(ministryincome);
        }

        //
        // GET: /MinistryIncome/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Delete(int MinistryIncomeID)
        {
            ViewBag.MinistryIncomeID = MinistryIncomeID;
            ministryincome ministryincome = MinistryIncomeRepository.GetIncomeByID(MinistryIncomeID);
            return View(ministryincome);
        }

        //
        // POST: /MinistryIncome/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MinistryIncomeID)
        {
            ministryincome ministryincome = MinistryIncomeRepository.GetIncomeByID(MinistryIncomeID);
            MinistryIncomeRepository.DeleteRecord(ministryincome);
            return RedirectToAction("Index", new { ministryID = ministryincome.ministryID });
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
            SubCategoryList = ConstantRepository.GetConstantByMinistryIncomeType(ministryID);
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);
            ViewBag.MinistryCode = string.Format("{0}&I", ministryID);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<ministryincome> MinistryIncomeList;
            ViewBag.MinistryID = codeID;

            if (SearchType == "MinistrySearch")
            {
                MinistryIncomeList = MinistryIncomeRepository.GetIncomeByMinistry(codeID, bDate, eDate);
            }
            else if (SearchType == "CategorySearch")
            {
                MinistryIncomeList = MinistryIncomeRepository.GetIncomeByCategory(codeID,codeID2, bDate, eDate);
            }
            else
            {
                MinistryIncomeList = MinistryIncomeRepository.GetIncomeByDateRange(bDate, eDate);
            }

            foreach (var i in MinistryIncomeList)
            {
                i.FundTitle = ConstantRepository.GetConstantID(i.subCategoryID).Value1;
            }

            ViewBag.RecordCount = MinistryIncomeList.Count();

            decimal sum = MinistryIncomeList.Sum(e => e.Amount);
            ViewBag.Heading = string.Format("Total: {0:c}", sum);
            

            return PartialView(MinistryIncomeList);
        }
    }
}