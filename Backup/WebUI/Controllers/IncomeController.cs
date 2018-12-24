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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
    public class IncomeController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBankAccountRepository BankAccountRepository;
        private IConstantRepository ConstantRepository;
        private ICategoryRepository CategoryRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private IPayeeRepository PayeeRepository;
        private IIncomeRepository IncomeRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();


        public IncomeController(IBankAccountRepository BankaccountParam, IConstantRepository ConstantParam, ICategoryRepository CategoryParam,
            ISubCategoryRepository SubCategoryParam, IPayeeRepository PayeeParam, IIncomeRepository IncomeParam)
        {
            BankAccountRepository = BankaccountParam;
            ConstantRepository = ConstantParam;
            CategoryRepository = CategoryParam;
            SubCategoryRepository = SubCategoryParam;
            PayeeRepository = PayeeParam;
            IncomeRepository = IncomeParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "FinanceStaff") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }

        //
        // GET: /Income/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult Index()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "FinanceLead")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
            }

            return PartialView();
        }

        //
        // GET: /Income/Details/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult Details()
        {
            GetData();
            DateTime aDate = DateTime.Now;
            
            int days = aDate.DayOfYear;
            ViewBag.BeginDate = aDate.AddDays(-10).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Income/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult Create()
        {
            GetData();
            IEnumerable<income> IncomeList;
            IncomeList = IncomeRepository.GetIncomeByStatus("Inactive");

            ViewBag.RecordCount = IncomeList.Count();
            foreach (var i in IncomeList)
            {
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                if (i.CashAmount != null) { i.TotalIncome += (decimal)i.CashAmount; }
                if (i.CheckAmount != null) { i.TotalIncome += (decimal)i.CheckAmount; }
                if (i.CoinAmount != null) { i.TotalIncome += (decimal)i.CoinAmount; }

            }

            decimal cash = (decimal)IncomeList.Sum(e => e.CashAmount);
            ViewBag.TotalCash = cash;
            decimal check = (decimal)IncomeList.Sum(e => e.CheckAmount);
            ViewBag.TotalCheck = check;
            decimal coin = (decimal)IncomeList.Sum(e => e.CoinAmount);
            ViewBag.TotalCoin = coin;

            ViewBag.TotalAmount = cash + check + coin;

            ViewBag.IncomeList = IncomeList;

            return PartialView(new income { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive" });
        } 

        //
        // POST: /Income/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(income income)
        {
            try
            {
                subcategory subCat = SubCategoryRepository.GetBySubCategoryID(income.subCategoryID);

                if (income.bankAccountID == 0) { income.bankAccountID = subCat.bankAccountID; }
                if (income.Title == null) { income.Title = subCat.SubCategoryName; }
                if (income.Comment == null) { income.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.incomes.Add(income);
                    db.SaveChanges();
                    IncomeRepository.AddRecord(income);
                    TempData["Message2"] = "Income added successfully.";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding income";
            }
            GetData();
            return PartialView(income);
        }
        
        //
        // GET: /Income/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult Edit(int IncomeID)
        {
            GetData();
            income income = db.incomes.Find(IncomeID);
            return PartialView(income);
        }

        //
        // POST: /Income/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(income income)
        {
            try
            {
                subcategory subCat = SubCategoryRepository.GetBySubCategoryID(income.subCategoryID);

                if (income.bankAccountID == 0) { income.bankAccountID = subCat.bankAccountID; }
                if (income.Comment == null) { income.Comment = ""; }
                income.Title = subCat.SubCategoryName;

                income.bankAccountID = subCat.bankAccountID;

                if (ModelState.IsValid)
                {
                    db.Entry(income).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = "Record update successfully.";
                     GetData();
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error editing record.";
            }
            GetData();
            return PartialView(income);
        }

        //
        // GET: /Income/Delete/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult Delete(int IncomeID)
        {
            ViewBag.IncomeID = IncomeID;
            income income = IncomeRepository.GetIncomeByID(IncomeID);
            ViewBag.TotalAmount = income.CashAmount + income.CheckAmount + income.CoinAmount;
            return PartialView(income);
        }

        //
        // POST: /Income/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int IncomeID)
        {
            income income = IncomeRepository.GetIncomeByID(IncomeID);
            IncomeRepository.DeleteRecord(income);
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData()
        {
            int id = 0;

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> SubCategoryList;
            //SubCategoryList = SubCategoryRepository.GetIncomeCategoryList();
            SubCategoryList = SubCategoryRepository.GetIncomeCategoryNoParentList();
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);

            Dictionary<int, string> BankAccountList;
            BankAccountList = BankAccountRepository.GetBankAccountList();
            ViewBag.BankAccountList = new SelectList(BankAccountList, "Key", "Value", id);



        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult List(DateTime bDate, DateTime eDate, string code = "", int codeID = 0, string ReportType = "")
        {
            ViewBag.ReportType = ReportType;
            IEnumerable<income> IncomeList;

            if(code == "BankAccountSearch")
            {
                IncomeList = IncomeRepository.GetIncomeByBankAccount(codeID, bDate.Date, eDate.Date);
            }
            else if(code == "CategorySearch")
            {
                IncomeList = IncomeRepository.GetIncomeByCategory(codeID, bDate.Date, eDate.Date);
            }
            else if (code == "DateRangeSearch")
            {
                IncomeList = IncomeRepository.GetIncomeByDateRange(bDate.Date, eDate.Date);
            }
            else
            {
                IncomeList = IncomeRepository.GetIncomeByStatus("Inactive").OrderByDescending(e => e.IncomeDate);
            }

            ViewBag.RecordCount = IncomeList.Count();
            foreach(var i in IncomeList)
            {
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
            }

            decimal cash =  (decimal)IncomeList.Sum(e => e.CashAmount);
            ViewBag.TotalCash = cash;
            decimal check = (decimal) IncomeList.Sum(e => e.CheckAmount);
            ViewBag.TotalCheck = check;
            decimal coin = (decimal) IncomeList.Sum(e => e.CoinAmount);
            ViewBag.TotalCoin = coin;

            ViewBag.TotalAmount = cash + check + coin;

           // GetData();
            return PartialView(IncomeList.OrderByDescending(e => e.IncomeDate));
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult ListActive()
        {
            GetData();
            IEnumerable<income> IncomeList;
            IncomeList = IncomeRepository.GetIncomeByStatus("Inactive");

            ViewBag.RecordCount = IncomeList.Count();
            foreach(var i in IncomeList)
            {
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
            }

            decimal cash =  (decimal)IncomeList.Sum(e => e.CashAmount);
            ViewBag.TotalCash = cash;
            decimal check = (decimal) IncomeList.Sum(e => e.CheckAmount);
            ViewBag.TotalCheck = check;
            decimal coin = (decimal) IncomeList.Sum(e => e.CoinAmount);
            ViewBag.TotalCoin = coin;

            ViewBag.TotalAmount = cash + check + coin;

            return PartialView(IncomeList);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult AdminReport()
        {

            return PartialView();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public string CommitIncome()
        {
            try
            {
                IEnumerable<income> incomes = db.incomes.Where(e => e.Status == "Inactive");
                foreach (income i in incomes)
                {
                    i.Status = "Active";
                }
                db.SaveChanges();
               
            }
            catch (Exception ex)
            {
                return ("Error occurred committing income data.");
            }
            return ("Income data committed successfully.");
        }

        [RoleAuthentication(Roles = "Pastor,WebMaster")]
         public ActionResult PastorDashBoard()
         {
             DateTime aDate = DateTime.Now.Date;
             ViewBag.BeginDate = aDate.ToShortDateString();
             ViewBag.EndDate7 = aDate.AddDays(-6);
             ViewBag.EndDate60 = aDate.AddDays(60);
             ViewBag.EndDate15 = aDate.AddDays(-14);
             return View();
         }
    }
}