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
   //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
    public class BudgetController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBudgetRepository BudgetRepository;
        private IConstantRepository ConstantRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private ISubCategoryItemRepository SubCategoryItemRepository;
        private IIncomeRepository IncomeRepository;
        private IExpenseRepository ExpenseRepository;
        private IBankAccountRepository BankAccountRepository;
        private ICategoryRepository CategoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public BudgetController(IBudgetRepository BudgetParam, IConstantRepository constantParam, ISubCategoryRepository SubCategoryParam,
                                IIncomeRepository IncomeParam, IExpenseRepository ExpenseParam, IBankAccountRepository BankAccountParam,
                                ISubCategoryItemRepository SubCategoryItemParam, ICategoryRepository CategoryParam)
        {
            BudgetRepository = BudgetParam;
            ConstantRepository = constantParam;
            SubCategoryRepository = SubCategoryParam;
            IncomeRepository = IncomeParam;
            ExpenseRepository = ExpenseParam;
            BankAccountRepository = BankAccountParam;
            SubCategoryItemRepository = SubCategoryItemParam;
            CategoryRepository = CategoryParam;

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
        // GET: /Budget/

        public ActionResult Index()
        {
            
            ViewBag.BudgetYear = DateTime.Now.Year;
            return PartialView();
        }

        //
        // GET: /Budget/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult AdminDetails()
        {
            ViewBag.BudgetYear =  DateTime.Now.Year.ToString();
            GetData();
            return PartialView();
        }

        public ActionResult Details()
        {
            ViewBag.BudgetYear = DateTime.Now.Year;
            GetData();
            return PartialView();
        }
        //
        // GET: /Budget/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Create(string BudgetType)
        {
            ViewBag.BudgetYear = DateTime.Today.Year;
            ViewBag.BudgetType = BudgetType;
            GetData(BudgetType);
            return PartialView(new budget { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", Type = BudgetType });
        } 

        //
        // POST: /Budget/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(budget budget)
        {
            try
            {
                if (budget.Comment == null) { budget.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.budgets.Add(budget);
                    db.SaveChanges();
                    BudgetRepository.AddRecord(budget);
                    TempData["Message2"] = "Budget record added successfully.";
                    GetData(budget.Type);
                    return RedirectToAction("Create", new { BudgetType = budget.Type});
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding budget item";
            }
            GetData(budget.Type);

            return PartialView(budget);
        }
        
        //
        // GET: /Budget/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int BudgetID)
        {
           
            budget budget = BudgetRepository.GetBudgetByID(BudgetID);
            GetData(budget.Type);
            return PartialView(budget);
        }

        //
        // POST: /Budget/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(budget budget)
        {
            try
            {
                if (budget.Comment == null) { budget.Comment = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(budget).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("budget data update successfully.");
                    GetData(budget.Type);
                    return RedirectToAction("Edit", new { BudgetID = budget.budgetID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} budget data.", budget.budgetID);
            }
            GetData(budget.Type);
            return PartialView(budget);
        }

        //
        // GET: /Budget/Delete/5
 
        public ActionResult Delete(int BudgetID)
        {
            ViewBag.BudgetID = BudgetID;
            budget budget = BudgetRepository.GetBudgetByID(BudgetID);
            budget.subcategory = SubCategoryRepository.GetBySubCategoryID(budget.SubCategoryID);
            return PartialView(budget);
        }

        //
        // POST: /Budget/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int BudgetID)
        {
            budget budget = BudgetRepository.GetBudgetByID(BudgetID);
            BudgetRepository.DeleteRecord(budget);
           // return RedirectToAction("List");
            return Content("Record deleted successfully");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(string BudgetType = "Income")
        {
            int id = 0;

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> SubCategoryList;
            if (BudgetType == "Income")
            {
                SubCategoryList = SubCategoryRepository.GetIncomeCategoryList();
            }
            else
            {
                SubCategoryList = SubCategoryRepository.GetExpenseCategoryList();
            }
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);

            Dictionary<string, string> BudgetYearList = new Dictionary<string, string>();
            int ayear = DateTime.Now.Year;
            for (int i = ayear - 2; i <= ayear+1; i += 1)
            {
                BudgetYearList.Add(i.ToString(),i.ToString());
            }
            ViewBag.BudgetYearList = new SelectList(BudgetYearList, "Key", "Value", id);


            Dictionary<string, string> BudgetYearCreateList = new Dictionary<string, string>();
            int byear = DateTime.Now.Year;
            for (int i = byear-1; i <= byear + 1; i += 1)
            {
                BudgetYearCreateList.Add(i.ToString(), i.ToString());
            }
            ViewBag.BudgetYearCreateList = new SelectList(BudgetYearCreateList, "Key", "Value", id);

            Dictionary<int, string> BankAccountList;
            BankAccountList = BankAccountRepository.GetBankAccountList();
            ViewBag.BankAccountList = new SelectList(BankAccountList, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult IncomeBudgetList(int BudgetYear)
        {
            IEnumerable<budget> BudgetList = BudgetRepository.GetBudgetByTypeYear(BudgetYear, "Income");
            ViewBag.RecordCount = BudgetList.Count();
            ViewBag.RecordSum = BudgetList.Sum(e => e.Amount);
            DateTime BeginDate = Convert.ToDateTime("1/1/" + BudgetYear.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + BudgetYear.ToString());

            decimal cash;
            decimal check;
            decimal coin;

            foreach (var i in BudgetList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
                cash = (decimal) IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CashAmount);
                check = (decimal)IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CheckAmount);
                coin = (decimal)IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CoinAmount);
                i.TotalActualAmount = cash + check + coin;
                i.Ratio = i.TotalActualAmount / i.Amount;
            }

            //Actual income Total
            cash = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CashAmount);
            check = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CheckAmount);
            coin = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CoinAmount);
            decimal TotalIncome = cash + check + coin;

            decimal BudgetSum = BudgetList.Sum(e => e.Amount);
            decimal ratio;
            if (BudgetSum == 0)
            {
                ratio = 0;
            }
            else
            {
                ratio = TotalIncome / BudgetSum;
            }

            ViewBag.Headering = string.Format("{0} Income Budget (Proposed Amt: {1:c}...Actual Amt: {2:c}...Ratio: {3:0.00%})", BudgetYear, BudgetSum, TotalIncome, ratio);
            return PartialView("List", BudgetList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult ExpenseBudgetList(int BudgetYear)
        {
            IEnumerable<budget> BudgetList = BudgetRepository.GetBudgetByTypeYear(BudgetYear, "Expense");
            ViewBag.RecordCount = BudgetList.Count();
             DateTime BeginDate = Convert.ToDateTime("1/1/" + BudgetYear.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + BudgetYear.ToString());

            
            foreach (var i in BudgetList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
                i.TotalActualAmount = ExpenseRepository.GetTotalExpenseByCategoryIncludeChildren(i.SubCategoryID, BeginDate, EndDate);
                i.Ratio = i.TotalActualAmount / i.Amount;
            }
            decimal BudgetSum = BudgetList.Sum(e => e.Amount);
            ViewBag.Headering = string.Format("{0} Expense Budget - Total: {1:c}", BudgetYear, BudgetSum);
            return PartialView("List", BudgetList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult IncomeBudgetListAdmin(int BudgetYear)
        {
            IEnumerable<budget> BudgetList = BudgetRepository.GetBudgetByTypeYear(BudgetYear, "Income");
            ViewBag.RecordCount = BudgetList.Count();
            ViewBag.RecordSum = BudgetList.Sum(e => e.Amount);

            DateTime BeginDate = Convert.ToDateTime("1/1/" + BudgetYear.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + BudgetYear.ToString());

            decimal cash;
            decimal check;
            decimal coin;

            foreach (var i in BudgetList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
                cash = (decimal)IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CashAmount);
                check = (decimal)IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CheckAmount);
                coin = (decimal)IncomeRepository.GetIncomeByCategory(i.SubCategoryID, BeginDate, EndDate).Sum(e => e.CoinAmount);
                i.TotalActualAmount = cash + check + coin;
                i.Ratio = i.TotalActualAmount / i.Amount;
            }
            //Actual income Total
            cash = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CashAmount);
            check = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CheckAmount);
            coin = (decimal)IncomeRepository.GetBudgetIncomeByDateRange(BeginDate, EndDate).Sum(e => e.CoinAmount);
            decimal TotalIncome = cash + check + coin;

            decimal BudgetSum = BudgetList.Sum(e => e.Amount);
            decimal ratio = TotalIncome / BudgetSum;

            ViewBag.Headering = string.Format("{0} Income Budget (Proposed Amt: {1:c}...Actual Amt: {2:c}...Ratio: {3:0.00%})", BudgetYear, BudgetSum, TotalIncome, ratio);
            return PartialView("ListAdmin", BudgetList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult ExpenseBudgetListAdmin(int BudgetYear)
        {
            IEnumerable<budget> BudgetList = BudgetRepository.GetBudgetByTypeYear(BudgetYear, "Expense");
            ViewBag.RecordCount = BudgetList.Count();

             DateTime BeginDate = Convert.ToDateTime("1/1/" + BudgetYear.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + BudgetYear.ToString());

            foreach (var i in BudgetList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
                i.TotalActualAmount = ExpenseRepository.GetTotalExpenseByCategoryIncludeChildren(i.SubCategoryID,BeginDate,EndDate);
            }
            decimal BudgetSum = BudgetList.Sum(e => e.Amount);
            ViewBag.Headering = string.Format("{0} Expense Budget - Total: {1:c}", BudgetYear, BudgetSum);
            return PartialView("ListAdmin", BudgetList);
        }

        public ActionResult AdminBudgetPage(int BudgetYear)
        {
            ViewBag.Year = BudgetYear;
            return PartialView();
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult AccountSummary()
        {
            ViewBag.Year = DateTime.Today.Year;
            GetData();
            return PartialView();
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult AccountSummaryDetail(int BudgetYear, int BankAccountID = 0)
        {
            string sYear = BudgetYear.ToString();
           
            if (BankAccountID != 0)
            {
                
                 ViewBag.Title = string.Format("{0} {1} Total Summary (YTD)",BudgetYear,BankAccountRepository.GetBankAccountByID(BankAccountID).BankName);
                 ViewBag.IncomeTotal = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));
                 ViewBag.IncomeJan = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("1/31/" + sYear));
                 if (DateTime.IsLeapYear(Convert.ToInt16(sYear)))
                 {
                     ViewBag.IncomeFeb = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("2/1/" + sYear).Date, Convert.ToDateTime("2/29/" + sYear).Date);
                 }
                 else
                 {
                     ViewBag.IncomeFeb = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/28/" + sYear));
                 }
                 ViewBag.IncomeMar = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("3/1/" + sYear), Convert.ToDateTime("3/31/" + sYear));
                 ViewBag.IncomeApr = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("4/1/" + sYear), Convert.ToDateTime("4/30/" + sYear));
                 ViewBag.IncomeMay = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("5/1/" + sYear), Convert.ToDateTime("5/31/" + sYear));
                 ViewBag.IncomeJun = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("6/1/" + sYear), Convert.ToDateTime("6/30/" + sYear));
                 ViewBag.IncomeJul = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("7/1/" + sYear), Convert.ToDateTime("7/31/" + sYear));
                 ViewBag.IncomeAug = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("8/1/" + sYear), Convert.ToDateTime("8/31/" + sYear));
                 ViewBag.IncomeSept = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("9/1/" + sYear), Convert.ToDateTime("9/30/" + sYear));
                 ViewBag.IncomeOct = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("10/31/" + sYear));
                 ViewBag.IncomeNov = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("11/30/" + sYear));
                 ViewBag.IncomeDec = IncomeRepository.GetIncomeTotalByBankAccount(BankAccountID, Convert.ToDateTime("12/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));

                ViewBag.ExpenseTotal = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));
                ViewBag.ExpenseJan = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("1/31/" + sYear));
                if (DateTime.IsLeapYear(Convert.ToInt16(sYear)))
                {
                    ViewBag.ExpenseFeb = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/29/" + sYear));
                }
                else
                {
                    ViewBag.ExpenseFeb = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/28/" + sYear));
                }
                    ViewBag.ExpenseMar = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("3/1/" + sYear), Convert.ToDateTime("3/31/" + sYear));
                ViewBag.ExpenseApr = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("4/1/" + sYear), Convert.ToDateTime("4/30/" + sYear));
                ViewBag.ExpenseMay = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("5/1/" + sYear), Convert.ToDateTime("5/31/" + sYear));
                ViewBag.ExpenseJun = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("6/1/" + sYear), Convert.ToDateTime("6/30/" + sYear));
                ViewBag.ExpenseJul = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("7/1/" + sYear), Convert.ToDateTime("7/31/" + sYear));
                ViewBag.ExpenseAug = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("8/1/" + sYear), Convert.ToDateTime("8/31/" + sYear));
                ViewBag.ExpenseSept = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("9/1/" + sYear), Convert.ToDateTime("9/30/" + sYear));
                ViewBag.ExpenseOct = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("10/31/" + sYear));
                ViewBag.ExpenseNov = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("11/30/" + sYear));
                ViewBag.ExpenseDec = ExpenseRepository.GetExpenseTotalByBankAccount(BankAccountID, Convert.ToDateTime("12/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));
            }
            else
            {
                ViewBag.Title = string.Format("{0} All Account Summary (YTD)", BudgetYear);
                ViewBag.IncomeTotal = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));
                ViewBag.IncomeJan = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("1/31/" + sYear));
                if (DateTime.IsLeapYear(Convert.ToInt16(sYear)))
                {
                    ViewBag.IncomeFeb = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/29/" + sYear));
                }
                else
                {
                    ViewBag.IncomeFeb = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/28/" + sYear));
                }
                     ViewBag.IncomeMar = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("3/1/" + sYear), Convert.ToDateTime("3/31/" + sYear));
                ViewBag.IncomeApr = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("4/1/" + sYear), Convert.ToDateTime("4/30/" + sYear));
                ViewBag.IncomeMay = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("5/1/" + sYear), Convert.ToDateTime("5/31/" + sYear));
                ViewBag.IncomeJun = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("6/1/" + sYear), Convert.ToDateTime("6/30/" + sYear));
                ViewBag.IncomeJul = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("7/1/" + sYear), Convert.ToDateTime("7/31/" + sYear));
                ViewBag.IncomeAug = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("8/1/" + sYear), Convert.ToDateTime("8/31/" + sYear));
                ViewBag.IncomeSept = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("9/1/" + sYear), Convert.ToDateTime("9/30/" + sYear));
                ViewBag.IncomeOct = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("10/31/" + sYear));
                ViewBag.IncomeNov = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("11/30/" + sYear));
                ViewBag.IncomeDec = IncomeRepository.GetIncomeTotalByDateRange(Convert.ToDateTime("12/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));

                ViewBag.ExpenseTotal = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));
                ViewBag.ExpenseJan = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("1/1/" + sYear), Convert.ToDateTime("1/31/" + sYear));
                if (DateTime.IsLeapYear(Convert.ToInt16(sYear)))
                {
                    ViewBag.ExpenseFeb = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/29/" + sYear));
                }
                else
                {
                    ViewBag.ExpenseFeb = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("2/1/" + sYear), Convert.ToDateTime("2/28/" + sYear));
                }
                    ViewBag.ExpenseMar = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("3/1/" + sYear), Convert.ToDateTime("3/31/" + sYear));
                ViewBag.ExpenseApr = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("4/1/" + sYear), Convert.ToDateTime("4/30/" + sYear));
                ViewBag.ExpenseMay = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("5/1/" + sYear), Convert.ToDateTime("5/31/" + sYear));
                ViewBag.ExpenseJun = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("6/1/" + sYear), Convert.ToDateTime("6/30/" + sYear));
                ViewBag.ExpenseJul = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("7/1/" + sYear), Convert.ToDateTime("7/31/" + sYear));
                ViewBag.ExpenseAug = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("8/1/" + sYear), Convert.ToDateTime("8/31/" + sYear));
                ViewBag.ExpenseSept = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("9/1/" + sYear), Convert.ToDateTime("9/30/" + sYear));
                ViewBag.ExpenseOct = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("10/31/" + sYear));
                ViewBag.ExpenseNov = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("10/1/" + sYear), Convert.ToDateTime("11/30/" + sYear));
                ViewBag.ExpenseDec = ExpenseRepository.GetExpenseTotalByDateRange(Convert.ToDateTime("12/1/" + sYear), Convert.ToDateTime("12/31/" + sYear));

            }

            ViewBag.NetTotal = Convert.ToDecimal(ViewBag.IncomeTotal) - Convert.ToDecimal(ViewBag.ExpenseTotal);
            ViewBag.NetJan = Convert.ToDecimal(ViewBag.IncomeJan) - Convert.ToDecimal(ViewBag.ExpenseJan);
            ViewBag.NetFeb = Convert.ToDecimal(ViewBag.IncomeFeb) - Convert.ToDecimal(ViewBag.ExpenseFeb);
            ViewBag.NetMar = Convert.ToDecimal(ViewBag.IncomeMar) - Convert.ToDecimal(ViewBag.ExpenseMar);
            ViewBag.NetApr = Convert.ToDecimal(ViewBag.IncomeApr) - Convert.ToDecimal(ViewBag.ExpenseApr);
            ViewBag.NetMay = Convert.ToDecimal(ViewBag.IncomeMay) - Convert.ToDecimal(ViewBag.ExpenseMay);
            ViewBag.NetJun = Convert.ToDecimal(ViewBag.IncomeJun) - Convert.ToDecimal(ViewBag.ExpenseJun);
            ViewBag.NetJul = Convert.ToDecimal(ViewBag.IncomeJul) - Convert.ToDecimal(ViewBag.ExpenseJul);
            ViewBag.NetAug = Convert.ToDecimal(ViewBag.IncomeAug) - Convert.ToDecimal(ViewBag.ExpenseAug);
            ViewBag.NetSept = Convert.ToDecimal(ViewBag.IncomeSept) - Convert.ToDecimal(ViewBag.ExpenseSept);
            ViewBag.NetOct = Convert.ToDecimal( ViewBag.IncomeOct) - Convert.ToDecimal(ViewBag.ExpenseOct);
            ViewBag.NetNov = Convert.ToDecimal(ViewBag.IncomeNov) - Convert.ToDecimal(ViewBag.ExpenseNov);
            ViewBag.NetDec = Convert.ToDecimal(ViewBag.IncomeDec) - Convert.ToDecimal(ViewBag.ExpenseDec);
            return PartialView();
        }

        public ActionResult BudgetCategoryDetails(int SubCategoryID, int categoryID)
        {
            ViewBag.CategoryType = CategoryRepository.GetCategoryByID(categoryID).CategoryName;
            int year = DateTime.Today.Date.Year;
            DateTime BeginDate = Convert.ToDateTime("1/1/" + year.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + year.ToString());
            //IEnumerable<expense> ExpenseList = null;
            List<expense> ExpenseList = new List<expense>();
            IEnumerable<expense> ExpenseList2 = null;
            //IEnumerable<income> IncomeList = null;
            List<income> IncomeList = new List<income>();
            IEnumerable<income> IncomeList2 = null;

            if (ViewBag.CategoryType == "Income")
            {
                IncomeList2 = IncomeRepository.GetIncomeChildren(SubCategoryID, BeginDate, EndDate);
                ViewBag.IncomeList = IncomeList2;



                if (IncomeList2 == null)
                {
                    ViewBag.RecordCount = 0;
                }
                else
                {
                    foreach (var i in IncomeList2)
                    {
                        i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                        i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                    }
                    ViewBag.RecordCount = IncomeList2.Count();
                }
              
            }
            else
            {
                ExpenseList2 = ExpenseRepository.GetExpenseChildren(SubCategoryID, BeginDate, EndDate);
                ViewBag.ExpenseList = ExpenseList2;

                

                if (ExpenseList2 == null)
                {
                    ViewBag.RecordCount = 0;
                }
                else
                {
                    foreach (var i in ExpenseList2)
                    {
                        i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                    }
                    ViewBag.RecordCount = ExpenseList2.Count();
                }
              
            }
            ViewBag.Heading = string.Format("{0} Budget Report Expenses ({1}-{2})", SubCategoryRepository.GetBySubCategoryID(SubCategoryID).SubCategoryName, BeginDate.Date, EndDate.Date);
            return PartialView();
        }
    }

    
}