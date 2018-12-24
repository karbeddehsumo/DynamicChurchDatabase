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
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
    public class BankBalanceController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBankAccountRepository BankAccountRepository;
        private IConstantRepository ConstantRepository;
        private IBankBalanceRepository BankBalanceRepository;
        private IIncomeRepository IncomeRepository;
        private IExpenseRepository ExpenseRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public BankBalanceController(IBankAccountRepository BankaccountParam, IConstantRepository ConstantParam, IBankBalanceRepository BankBalanceParam,
                                      IIncomeRepository IncomeParam, IExpenseRepository ExpenseParam)
        {
            BankAccountRepository = BankaccountParam;
            ConstantRepository = ConstantParam;
            BankBalanceRepository = BankBalanceParam;
            IncomeRepository = IncomeParam;
            ExpenseRepository = ExpenseParam;

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
        // GET: /BankAccount/



        //
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /BankAccount/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Create(int BankBalanceID)
        {
            return PartialView(new bankbalance { BankBalanceID = BankBalanceID });
        } 
        //
        // POST: /BankBalance/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(bankbalance bankbalance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bankbalance.LastIncomeID = IncomeRepository.GetLastIncomeRecordID();
                    bankbalance.LastExpenseID = ExpenseRepository.GetLastExpenseRecordID();
                    db.bankbalances.Add(bankbalance);
                    db.SaveChanges();
                    BankBalanceRepository.AddRecord(bankbalance);
                    TempData["Message2"] = "Bank balance created successfully.";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error ocurred creating bank balance.";
            }

            return View(bankbalance);
        }
        
        //
        // GET: /BankBalance/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int BankBalanceID)
        {
            bankbalance bankbalance = BankBalanceRepository.GetByBankBalanceID(BankBalanceID);
            return PartialView(bankbalance);
        }

        //
        // POST: /BankBalance/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(bankbalance bankbalance)
        {
            string bankName = db.bankaccounts.Find(bankbalance.BankAccountID).BankName;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(bankbalance).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} balance update successfully.", bankName);
                    return RedirectToAction("Edit", new { id = bankbalance.BankBalanceID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} balance.", bankName);
            }
            return View(bankbalance);
        }

        //
        // GET: /BankBalance/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Delete(int BankBalanceID)
        {
            ViewBag.BankBalanceID = BankBalanceID;
            bankbalance bankbalance = BankBalanceRepository.GetByBankBalanceID(BankBalanceID);
            return PartialView(bankbalance);
        }

        //
        // POST: /BankBalance/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int BankBalanceID)
        {
            bankbalance bankbalance = BankBalanceRepository.GetByBankBalanceID(BankBalanceID);
            BankBalanceRepository.DeleteRecord(bankbalance);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0)
        {
            IEnumerable<bankbalance> BankBalanceList;

            if (SearchType == "BankAccountSearch")
            {
                BankBalanceList = BankBalanceRepository.GetByBankAccount(codeID);
            }
            else
            {
                BankBalanceList = BankBalanceRepository.GetByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = BankBalanceList.Count();

            return PartialView(BankBalanceList);
        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult GetCurrentBankBalance()
        {
            bankbalance Lastbalance = new bankbalance();
            IEnumerable<bankaccount> bankaccount = BankAccountRepository.GetAllBankAccount();
            foreach (bankaccount bank in bankaccount)
             {
                 Lastbalance = BankBalanceRepository.GetLastBankBlance(bank.bankAccountID);               
                 if (Lastbalance == null)
                 {
                     bank.BeginningBalance = IncomeRepository.GetIncomeTotalByBankAccount(bank.bankAccountID, bank.DateEntered, System.DateTime.Today); 
                     bank.CurrentRevenueTotalAmount = IncomeRepository.GetPendingIncomeTotalByBankAccount(bank.bankAccountID);
                     bank.CurrentExpenseTotalAmount = ExpenseRepository.GetPendingExpenseTotalByBankAccount(bank.bankAccountID);
                 }
                 else
                 {
                     bank.BeginningBalance = Lastbalance.EndingBalance;
                     bank.CurrentRevenueTotalAmount = IncomeRepository.GetPendingIncomeTotalByBankAccount(bank.bankAccountID);
                     bank.CurrentExpenseTotalAmount = ExpenseRepository.GetPendingExpenseTotalByBankAccount(bank.bankAccountID);
                 }
                
             }
            return PartialView(bankaccount);
        }




    }
}