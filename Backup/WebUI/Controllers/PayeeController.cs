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
    public class PayeeController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IPayeeCategoryRepository PayeeCategoryRepository;
        private IPayeeRepository PayeeRepository;
        private IBankAccountRepository BankAccountRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private ISubCategoryItemRepository SubCategoryItemRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        //
        // GET: /Payee/
        public PayeeController(IConstantRepository constantParam, IPayeeCategoryRepository PayeeCategoryParam, IPayeeRepository payeeParam, IBankAccountRepository BankAccountParam,
            ISubCategoryRepository SubCategoryParam, ISubCategoryItemRepository SubCategoryItemParam)
        {
            ConstantRepository = constantParam;
            PayeeCategoryRepository = PayeeCategoryParam;
            PayeeRepository = payeeParam;
            BankAccountRepository = BankAccountParam;
            SubCategoryRepository = SubCategoryParam;
            SubCategoryItemRepository = SubCategoryItemParam;

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
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /Payee/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Details(int id)
        {
            payee payee = db.payees.Find(id);
            return PartialView(payee);
        }

        //
        // GET: /Payee/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Create()
        {
            GetData();
            IEnumerable<payee> PayeeList = PayeeRepository.GetAllPayee();
            foreach (var i in PayeeList)
            {
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.BankAccountID);
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
            }
            ViewBag.RecordCount = PayeeList.Count();
            ViewBag.PayeeList = PayeeList.OrderBy(e => e.PayeeTypeID).ThenBy(e => e.PayeeName);

            return PartialView(new payee { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 

        //
        // POST: /Payee/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(payee payee)
        {
            payee p = PayeeRepository.GetAllPayee().FirstOrDefault(e => e.PayeeName == payee.PayeeName && e.AccountNumber == payee.AccountNumber);
            if (p != null)
            {
                TempData["Message2"] = string.Format("Payee <{0}> already exist", p.PayeeName);
                return RedirectToAction("Create");
            }

            int bankaccountID = db.subcategories.Find(payee.SubCategoryID).bankAccountID;
            if (payee.Email == null) { payee.Email = "Null"; }
            if (payee.PhoneNumber == null) { payee.PhoneNumber = "Null"; }
            if (payee.URL == null) { payee.URL = "Null"; }

            try
            {
                payee.BankAccountID = bankaccountID;

                if (ModelState.IsValid)
                {
                    db.payees.Add(payee);
                    db.SaveChanges();
                    PayeeRepository.AddRecord(payee);
                    TempData["Message2"] = "Payee record created successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error ocurred creating payee.";
            }
            GetData();
            return PartialView(payee);
        }
        
        //
        // GET: /Payee/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int PayeeID)
        {
            GetData();
            payee payee = PayeeRepository.GetPayeeByID(PayeeID);
            return PartialView(payee);
        }

        //
        // POST: /Payee/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(payee payee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(payee).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", payee.PayeeName);
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", payee.PayeeName);
            }
            GetData();
            return PartialView(payee);
        }

        //
        // GET: /Payee/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Delete(int PayeeID)
        {
            ViewBag.PayeeID = PayeeID;
            payee payee = PayeeRepository.GetPayeeByID(PayeeID);
            return PartialView(payee);
        }

        //
        // POST: /Payee/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int PayeeID)
        {
            payee payee = PayeeRepository.GetPayeeByID(PayeeID);
            PayeeRepository.DeleteRecord(payee);
            TempData["Message2"] = string.Format("{0} record deleted successfully.", payee.PayeeName);
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

            Dictionary<int, string> BankAccountList;
            BankAccountList = BankAccountRepository.GetBankAccountList();
            ViewBag.BankAccountList = new SelectList(BankAccountList, "Key", "Value", id);


            Dictionary<string, string> FrequencyList;
            FrequencyList = ConstantRepository.GetConstantByCategory("Bill Payment Frequency");
            ViewBag.FrequencyList = new SelectList(FrequencyList, "Key", "Value", id);

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> PayeeTypeList;
            PayeeTypeList = ConstantRepository.GetConstantByCategoryID("Payee Type");
            ViewBag.PayeeTypeList = new SelectList(PayeeTypeList, "Key", "Value", id);

            //Prepare subcategory list display name
            IEnumerable<subcategory> SubCategories = SubCategoryRepository.GetCategoryByCategoryType("Expense");
            foreach (var s in SubCategories)
            {
                s.DisplayName = SubCategoryRepository.GetDisplayName(s.subCategoryID);
            }

            Dictionary<int, string> SubCategoryList;
            //SubCategoryList = SubCategories.OrderBy(e => e.DisplayName).ToDictionary(e => e.subCategoryID, e => e.DisplayName);
            SubCategoryList = SubCategoryRepository.GetExpenseCategoryNoParentList();
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult List()
        {
            IEnumerable<payee> PayeeList = PayeeRepository.GetAllPayee();
            foreach (var i in PayeeList)
            {
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.BankAccountID);
                i.subcategory = SubCategoryRepository.GetBySubCategoryID(i.SubCategoryID);
            }
            ViewBag.RecordCount = PayeeList.Count();
            return PartialView(PayeeList.OrderBy(e => e.PayeeTypeID).ThenBy(e => e.PayeeName));
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public string PayeeCategory(string Payee)
        {
            payee payee = PayeeRepository.GetPayeeByName(Payee);
            if (payee == null)
            {
                return ("");
            }

            subcategory catgy = SubCategoryRepository.GetBySubCategoryID(payee.SubCategoryID);
            catgy.DisplayName = SubCategoryRepository.GetDisplayName(catgy.subCategoryID);
            return (catgy.DisplayName);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult FrequencyReport()
        {
            IEnumerable<payee> payeeList = PayeeRepository.GetPayeeByFrequency();
            ViewBag.RecordCount = payeeList.Count();
            return PartialView(payeeList);
        }
    }
}