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
    
    public class BankAccountController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBankAccountRepository BankAccountRepository;
        private IConstantRepository ConstantRepository;
        private IIncomeRepository IncomeRepository;
        private IExpenseRepository ExpenseRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public BankAccountController(IBankAccountRepository BankaccountParam, IConstantRepository ConstantParam, IIncomeRepository IncomeParam, IExpenseRepository ExpenseParam)
        {
            BankAccountRepository = BankaccountParam;
            ConstantRepository = ConstantParam;
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
        public ActionResult Create()
        {
            return PartialView(new bankaccount { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(),Status="Active"});
        } 

        //
        // POST: /BankAccount/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(bankaccount bankaccount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.bankaccounts.Add(bankaccount);
                    db.SaveChanges();
                    BankAccountRepository.AddRecord(bankaccount);
                    TempData["Message2"] = "Bank account created successfully.";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error ocurred creating bank account.";
            }

            return PartialView(bankaccount);
        }
        
        //
        // GET: /BankAccount/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int BankAccountID)
        {
            GetData();
            bankaccount bankaccount = BankAccountRepository.GetBankAccountByID(BankAccountID);
            return PartialView(bankaccount);
        }

        //
        // POST: /BankAccount/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(bankaccount bankaccount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(bankaccount).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", bankaccount.BankName);
                    return RedirectToAction("List");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", bankaccount.BankName);
            }
            return PartialView(bankaccount);
        }

        //
        // GET: /BankAccount/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Delete(int BankAccountID)
        {
            ViewBag.BankAccountID = BankAccountID;
            bankaccount bankaccount = db.bankaccounts.Find(BankAccountID);
            return PartialView(bankaccount);
        }

        //
        // POST: /BankAccount/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int BankAccountID)
        {
            bankaccount bankaccount = db.bankaccounts.Find(BankAccountID);
            bool hasIncome = IncomeRepository.IncomeByBankAccount(BankAccountID);
            bool hasExpense = ExpenseRepository.ExpenseByBankAccount(BankAccountID);
            if ((hasIncome || hasExpense) == false)
            {
                db.bankaccounts.Remove(bankaccount);
                db.SaveChanges();
            }
            else
            {
                TempData["Message2"] = string.Format("Can not delete. Account has income or expense data.");
            }
            
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

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult List()
        {
            IEnumerable<bankaccount> bankaccountList = BankAccountRepository.GetAllBankAccount();
            ViewBag.RecordCount = bankaccountList.Count();
            return PartialView(bankaccountList);
        }
    }
}