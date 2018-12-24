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
    public class ExpenseController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBankAccountRepository BankAccountRepository;
        private IConstantRepository ConstantRepository;
        private ICategoryRepository CategoryRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private IPayeeRepository PayeeRepository;
        private IExpenseRepository ExpenseRepository;
        private IBillRepository BillRepository;
        private ISubCategoryItemRepository SubCategoryItemRepository;
        private IIncomeRepository IncomeRepository;
        private IBankBalanceRepository BankBalanceRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        //public ExpenseController() { }

        public ExpenseController(IBankAccountRepository BankaccountParam, IConstantRepository ConstantParam, ICategoryRepository CategoryParam,
            ISubCategoryRepository SubCategoryParam, IPayeeRepository PayeeParam, IExpenseRepository ExpenseParam, IBillRepository BillParam,
            ISubCategoryItemRepository SubCategoryItemParam, IIncomeRepository IncomeParam, IBankBalanceRepository BankBalanceParam)
        {
            BankAccountRepository = BankaccountParam;
            ConstantRepository = ConstantParam;
            CategoryRepository = CategoryParam;
            SubCategoryRepository = SubCategoryParam;
            PayeeRepository = PayeeParam;
            ExpenseRepository = ExpenseParam;
            BillRepository = BillParam;
            SubCategoryItemRepository = SubCategoryItemParam;
            IncomeRepository = IncomeParam;
            BankBalanceRepository = BankBalanceParam;

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
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor1 = true;
                    }
                }
            }
        }
        //
        // GET: /Expense/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Index()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Expense/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
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
        // GET: /Expense/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Create(int payeeID = 0, decimal Amount = 0, int billID = 0)
        {
            GetData();
            //get pending bills
            IEnumerable<bill> bills = BillRepository.GetAllBill();
            ViewBag.PendingBills = bills;
            ViewBag.PendingBillsRecordCount = bills.Count();
            //get pedning expenses
            IEnumerable<expense> expenses = db.expenses.Where(e => e.Status == "Unpaid");
            foreach (var i in expenses)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
            }
            ViewBag.PendingExpenses = expenses;
            ViewBag.PendingExpenseRecordCount = expenses.Count();
            

            ViewBag.ExpenseType = "";
            if (payeeID > 0)
            {
                payee payee = PayeeRepository.GetPayeeByID(payeeID);
                ViewBag.ExpenseType = SubCategoryRepository.GetDisplayName(payee.SubCategoryID);

                return PartialView(new expense { PendingBillID = billID, Payee = payee.PayeeName, subCategoryID = payee.SubCategoryID, bankAccountID = payee.BankAccountID, Amount = Amount, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Unpaid" });
            }
            else
            {
              return PartialView(new expense { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Unpaid" });
            }
        } 

        //
        // POST: /Expense/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(expense expense)
        {

            try
            {
                subcategory subCat = SubCategoryRepository.GetBySubCategoryID(expense.subCategoryID);

                if (expense.bankAccountID == 0) { expense.bankAccountID = subCat.bankAccountID; }
                if (expense.Comment == null) { expense.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.expenses.Add(expense);
                    db.SaveChanges();
                    ExpenseRepository.AddRecord(expense);
                    TempData["Message2"] = "Expense added successfully.";

                    if (expense.PendingBillID > 0)
                    {
                        bill bill = db.bills.Find(expense.PendingBillID);
                        bill.Status = "Processing";
                        db.SaveChanges();
                        BillRepository.AddRecord(bill);
                    }
                    GetData();
                    @ViewBag.ExpenseType = "";
                    return RedirectToAction("Create", "Expense", new { payeeID = 0});
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error adding expense";
            }
            GetData();
            @ViewBag.ExpenseType = "";
            return PartialView(expense);
        }
        
        //
        // GET: /Expense/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int ExpenseID, string RequestType = "")
        {
            ViewBag.RequestType = RequestType;
            if (RequestType == "CommittedUpdate")
            {

            }
            else
            {
                //get pending bills
                IEnumerable<bill> bills = BillRepository.GetAllBill();
                ViewBag.PendingBills = bills;
                //get pedning expenses
                IEnumerable<expense> expenses = db.expenses.Where(e => e.Status == "Unpaid");
                foreach (var i in expenses)
                {
                    i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                    i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                }
                ViewBag.PendingExpenses = expenses;
            }

            expense expense = ExpenseRepository.GetExpenseByID(ExpenseID);
            GetData();
            return PartialView(expense);
        }

        //
        // POST: /Expense/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(expense expense, string RequestType)
        {
            try
            {
                subcategory subCat = SubCategoryRepository.GetBySubCategoryID(expense.subCategoryID);

                if (expense.bankAccountID == 0) { expense.bankAccountID = subCat.bankAccountID; }
                if (expense.Comment == null) { expense.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.Entry(expense).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", expense.Payee);
                    GetData();
                    if (RequestType == "CommittedUpdate")
                    {
                        return RedirectToAction("List", "Expense", new {code="DateRangeSearch",bDate=DateTime.Today.Date.AddDays(-10),eDate=DateTime.Today.Date});
                    }
                    else
                    {
                        return RedirectToAction("Create");
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", expense.Payee);
            }
            GetData();
            return PartialView(expense);

        }

        //
        // GET: /Expense/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Delete(int ExpenseID)
        {
            expense expense = ExpenseRepository.GetExpenseByID(ExpenseID);

            if (expense.PendingBillID > 0)
            {
                //reset bill status
                bill b = db.bills.Find(expense.PendingBillID);
                b.Status = "New";
                db.SaveChanges();
            }
            //get pending bills
            IEnumerable<bill> bills = BillRepository.GetAllBill();
            ViewBag.PendingBills = bills;
            //get pedning expenses
            IEnumerable<expense> expenses = db.expenses.Where(e => e.Status == "Unpaid");
            foreach (var i in expenses)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
            }
            ViewBag.PendingExpenses = expenses;
            ViewBag.ExpenseID = ExpenseID;

            expense.subcategory = SubCategoryRepository.GetBySubCategoryID(expense.subCategoryID);
            return PartialView(expense);
        }

        //
        // POST: /Expense/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ExpenseID)
        {
            expense expense = ExpenseRepository.GetExpenseByID(ExpenseID);
            ExpenseRepository.DeleteRecord(expense);
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

            Dictionary<int, string> BankAccountList;
            BankAccountList = BankAccountRepository.GetBankAccountList();
            ViewBag.BankAccountList = new SelectList(BankAccountList, "Key", "Value", id);
            //Prepare subcategory list display name
            IEnumerable<subcategory> SubCategories = SubCategoryRepository.GetCategoryByCategoryType("Expense");
            foreach (var s in SubCategories)
            {
                s.DisplayName = SubCategoryRepository.GetDisplayName(s.subCategoryID);
            }

            Dictionary<int, string> SubCategoryList;
          //  SubCategoryList = SubCategories.OrderBy(e => e.DisplayName).ToDictionary(e => e.subCategoryID,e => e.DisplayName);
            SubCategoryList = SubCategoryRepository.GetExpenseCategoryNoParentList();
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);

            Dictionary<string, string> PayeeListRecent;
            //PayeeListRecent = ExpenseRepository.GetRecentPayeeList();
            PayeeListRecent = ExpenseRepository.GetRecentPayeeList();
            ViewBag.PayeeListRecent = new SelectList(PayeeListRecent, "Key", "Value", id);

        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult List(DateTime bDate, DateTime eDate, string code = "", int codeID = 0, string ReportType = "")
        {
            ViewBag.ReportType = ReportType;
            GetData();
            IEnumerable<expense> ExpenseList;

            if (code == "BankAccountSearch")
            {
                ExpenseList = ExpenseRepository.GetExpenseByBankAccount(codeID, bDate, eDate);
            }
            else if (code == "CategorySearch")
            {
                ExpenseList = ExpenseRepository.GetExpenseByCategory(codeID, bDate, eDate);
            }
            else if (code == "DateRangeSearch")
            {
                ExpenseList = ExpenseRepository.GetExpenseByDateRange(bDate.Date, eDate.Date);
            }
            else if(code == "CurrentExpense")
            {
                ExpenseList = ExpenseRepository.GetExpenseByStatus("Unpaid");
            }
            else
            {
                ExpenseList = ExpenseRepository.GetExpenseByDate(bDate);
            }

            ViewBag.RecordCount = ExpenseList.Count();

            foreach (var i in ExpenseList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
            }
            ViewBag.TotalExpense = ExpenseList.Sum(e => e.Amount);
            return PartialView(ExpenseList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult ListActive()
        {
            //GetData();
            IEnumerable<expense> ExpenseList;
            ExpenseList = ExpenseRepository.GetExpenseByStatus("Unpaid");

            ViewBag.RecordCount = ExpenseList.Count();
            foreach (var i in ExpenseList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
            }

            return PartialView(ExpenseList);
        }

        [HttpPost, ActionName("CommitExpenses")]
        public ActionResult CommitExpenses()
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                bankaccount aBank;
                bankbalance Lastbalance = new bankbalance();
                bankbalance NewBankBalance = new bankbalance();
                IEnumerable<expense> PendingExpenses = db.expenses.Where(e => e.Status == "Unpaid");
                foreach (int bankID in PendingExpenses.Select(e => e.bankAccountID).Distinct())
                {
                    Lastbalance = BankBalanceRepository.GetLastBankBlance(bankID);
                    if (Lastbalance == null)
                    {
                        aBank = BankAccountRepository.GetBankAccountByID(bankID);
                        NewBankBalance.BeginDate = BankAccountRepository.GetBankAccountByID(bankID).DateEntered;
                        NewBankBalance.BeginingBalance = IncomeRepository.GetIncomeTotalByBankAccount(bankID, aBank.DateEntered, System.DateTime.Today);
                    }
                    else
                    {
                        NewBankBalance.BeginDate = Lastbalance.EndDate.AddDays(1);
                        NewBankBalance.BeginingBalance = Lastbalance.EndingBalance;
                    }

                    NewBankBalance.BankAccountID = bankID;
                    NewBankBalance.EndDate = System.DateTime.Today;
                    NewBankBalance.LastIncomeID = IncomeRepository.GetLastIncomeRecordID();
                    NewBankBalance.LastExpenseID = ExpenseRepository.GetLastExpenseRecordID();
                    decimal revenueAmt = IncomeRepository.GetPendingIncomeTotalByBankAccount(bankID);
                    if (revenueAmt == null)
                    {
                        NewBankBalance.RevenueAmount = 0;
                    } 
                    else
                    {
                        NewBankBalance.RevenueAmount = revenueAmt;
                    }                  
                    NewBankBalance.ExpenseAmount = ExpenseRepository.GetPendingExpenseTotalByBankAccount(bankID);
                    NewBankBalance.EndingBalance = NewBankBalance.BeginingBalance + NewBankBalance.RevenueAmount - NewBankBalance.ExpenseAmount;
                
                    BankBalanceRepository.AddRecord(NewBankBalance);
                }

                //Activate pending income and expense records
                foreach (expense i in PendingExpenses)
                {
                    i.Status = "Active";
                }
                db.SaveChanges();

                IEnumerable<income> PendingIncomes = IncomeRepository.GetIncomeByStatus("Inactive");
                foreach (income i in PendingIncomes)
                {
                    i.Status = "Active";
                }
                db.SaveChanges();

                //update bills status
                IEnumerable<bill> processingBills = BillRepository.GetBillByStatus("Processing");
                foreach (bill i in processingBills)
                {
                    i.Status = "Paid";
                }
                db.SaveChanges();
                    TempData["Message2"] = string.Format("Expense committed successfully");
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error:Expense fail to commit");
            }
            return Redirect(ReturnUrl);
            
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult PayExpense()
        {
            IEnumerable<bill> bills = BillRepository.GetAllBill();
            ViewBag.PendingBills = bills;
            IEnumerable <expense> expenses = db.expenses.Where(e => e.Status == "Unpaid");
            foreach(var i in expenses)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
            }
            ViewBag.PendingExpenses = expenses;
            return PartialView();
        }

      //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult PayeeSearching(string term)
        {
            IEnumerable<expense> recentExpenseList = ExpenseRepository.GetMostRecentPayeeList();
            var payeeList = recentExpenseList.Where(e => e.Payee.ToLower().Contains(term))
                .Take(10)
                .Select(e => new
                {
                    label = (string)(e.Payee),
                    value = (string)(e.Payee),
                    subcategoryid = e.subCategoryID
                });
            return Json(payeeList, JsonRequestBehavior.AllowGet);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult ReconcileCheck(int bankID)
        {
            bankaccount bank = BankAccountRepository.GetBankAccountByID(bankID);
            ViewBag.Title = string.Format("Reconciliation for {0}",bank.BankNameAccountNumber);
            IEnumerable<expense> checkList = ExpenseRepository.GetUnReconcile(bankID);
            ViewBag.RecordCount = checkList.Count();
            foreach (var i in checkList)
            {
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.subCategoryID);
            }
            return PartialView(checkList);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public void CommitReconcileCheck(int expenseID, bool flag)
        {
            expense exp = db.expenses.Find(expenseID);
            exp.Reconcile = flag;
            db.SaveChanges();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff")]
        public ActionResult ReconcileView()
        {
            GetData();
            return PartialView();
        }
 
    }
}