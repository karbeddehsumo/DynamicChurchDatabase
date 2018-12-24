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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
    public class BillController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IBillRepository billRepository;
        private IConstantRepository ConstantRepository;
        private IPayeeCategoryRepository PayeeCategoryRepository;
        private IPayeeRepository PayeeRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public BillController(IBillRepository billParam, IConstantRepository constantParam, IPayeeCategoryRepository PayeeCategoryParam, IPayeeRepository payeeParam)
        {
            billRepository = billParam;
            ConstantRepository = constantParam;
            PayeeCategoryRepository = PayeeCategoryParam;
            PayeeRepository = payeeParam;

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
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Index()
        {
            DateTime bDate = DateTime.Today;
            DateTime eDate = bDate.AddDays(15);
            IEnumerable<bill> bills = billRepository.GetBillByDueDateRange(bDate, eDate);
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            return PartialView(bills);
        }

        //
        // GET: /Bill/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Details()
        {
            GetData();

            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.AddDays(15).ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Bill/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Create()
        {
            GetData();
            DateTime eDate = DateTime.Today;
            DateTime bDate = eDate.AddDays(-15);
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            IEnumerable<bill> CurrentBills = billRepository.GetBillByDueDateRange(bDate.Date, eDate.Date);
            ViewBag.CurrentBills = CurrentBills;
            ViewBag.BillRecordCount = CurrentBills.Count();

            return PartialView(new bill { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status="New" });
        } 

        //
        // POST: /Bill/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(bill bill)
        {
            if (bill.payeeID == 0)
            {
                TempData["Message2"] = "Error adding bill. Please select payee.";
                GetData();
                ViewBag.Year = 0;
                return PartialView(bill);
            }
            try
            {
                payee p = PayeeRepository.GetPayeeByID(bill.payeeID);
                bill.AccountNumber = p.AccountNumber;
                bill.PayeeName = p.PayeeName;
                if (bill.Comment == null) { bill.Comment = ""; }

                if (ModelState.IsValid)
                {
                    db.bills.Add(bill);
                    db.SaveChanges();
                    billRepository.AddRecord(bill);
                    TempData["Message2"] = "Bill added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error adding bill";
            }
            GetData();
            return PartialView(bill);
        }
        
        //
        // GET: /Bill/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Edit(int BillID,  DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnType = "", int ReturnBillID = 0)
        {
            bill bill = billRepository.GetBillByID(BillID);
            if (ReturnType == "List")
            {
                bill.ReturnBeginDate = ReturnBeginDate;
                bill.ReturnEndDate = ReturnEndDate;
                bill.ReturnBillID = ReturnBillID;
            }
            
            GetData();
            return PartialView(bill);
        }

        //
        // POST: /Bill/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(bill bill)
        {
            if (bill.Comment == null) { bill.Comment = ""; }
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(bill).State = EntityState.Modified;
                    db.SaveChanges();
                     TempData["Message2"] = string.Format("{0} record update successfully.", bill.PayeeName);
                     GetData();
                     if (bill.ReturnType == "List")
                    {
                        return RedirectToAction("List", new { bDate = bill.ReturnBeginDate, eDate = bill.ReturnEndDate, codeID = bill.ReturnBillID });
                    }
                    else
                    {
                      return RedirectToAction("Create");
                    }
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", bill.PayeeName);
            }
            GetData();
            return PartialView(bill);
        }

        //
        // GET: /Bill/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Delete(int BillID)
        {
            ViewBag.BillID = BillID;
            bill bill = billRepository.GetBillByID(BillID);
            bill.payee = PayeeRepository.GetPayeeByID(bill.payeeID);
            return PartialView(bill);
        }

        //
        // POST: /Bill/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int BillID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnType = "", int ReturnBillID = 0)
        {
            bill bill = billRepository.GetBillByID(BillID);
            billRepository.DeleteRecord(bill);
            if (ReturnType == "List")
            {
                return RedirectToAction("List", new { bDate = ReturnBeginDate, eDate = ReturnEndDate, codeID = ReturnBillID });
            }
            else
            {
                return RedirectToAction("Create");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData()
        {
            int id = 0;

            //Member Status List
            Dictionary<int, string> BillPayeeList;
            BillPayeeList = billRepository.GetBillList();
            ViewBag.BillPayeeList = new SelectList(BillPayeeList, "Key", "Value", id);

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> PayeeList;
           // PayeeList = PayeeRepository.GetPayeeListByType("Utility");
            PayeeList = PayeeRepository.GetPayeeListNonStaff();
            ViewBag.PayeeList = new SelectList(PayeeList, "Key", "Value", id);

        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult List(DateTime bDate, DateTime eDate, int codeID = 0)
        {
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.BillID = codeID;

            IEnumerable<bill> BillList;

            if (codeID > 0)
            {
                BillList = billRepository.GetBillByPayeeDateRange(codeID, bDate.Date, eDate.Date);
            }
            else
            {
                BillList = billRepository.GetBillByDueDateRange(bDate.Date, eDate.Date);
            }

            ViewBag.RecordCount = BillList.Count();
            return PartialView(BillList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult ExpenseList()
        {
            DateTime bdate = DateTime.Now;
            DateTime edate = bdate.AddDays(15);
            IEnumerable<bill> BillList = billRepository.GetAllBill();

            ViewBag.BillsDue = billRepository.GetBillByDueDateRange(bdate.Date, edate.Date);
            ViewBag.RecordCount = BillList.Count();
            return PartialView(BillList);
        }
    }
}