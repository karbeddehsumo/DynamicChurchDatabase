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
   //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class VisitorController : Controller
    {
      private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IVisitorRepository VisitorRepository;
        private IProgramEventRepository ProgramEventRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public VisitorController(IConstantRepository ConstantParam, IVisitorRepository VisitorParam, IProgramEventRepository ProgramEventParam)
        {
            ConstantRepository = ConstantParam;
            VisitorRepository = VisitorParam;
            ProgramEventRepository = ProgramEventParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "Officer") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }
        //
        // GET: /Pledge/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Index()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.AddDays(-365).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.AddDays(-365).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Create()
        {
            GetData();
            return PartialView(new visitor { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 


        //
        // POST: /Visitor/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(visitor visitor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.visitors.Add(visitor);
                    db.SaveChanges();
                    VisitorRepository.AddRecord(visitor);
                    TempData["Message2"] = "Visitor added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding visitor";
            }
            GetData();
            return PartialView(visitor);
        }
        
        //
        // GET: /Visitor/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int VisitorID, string EditorType = "")
        {
            ViewBag.EditorType = EditorType;
            GetData();
            visitor visitor = VisitorRepository.GetVisitorByID(VisitorID);
            return PartialView(visitor);
        }

        //
        // POST: /Visitor/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(visitor visitor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(visitor).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Visitor data update successfully.");
                    GetData();
                    //return RedirectToAction("Edit", new { VisitorID = visitor.visitorID });
                    return RedirectToAction("Details", new { bDate = System.DateTime.Now.AddDays(-365), eDate = System.DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing visitor.");
            }
            GetData();
            return PartialView(visitor);
        }

        //
        // GET: /Visitor/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Delete(int VisitorID)
        {
            ViewBag.VisitorID = VisitorID;
            visitor visitor = VisitorRepository.GetVisitorByID(VisitorID);
            return PartialView(visitor);
        }

        //
        // POST: /Visitor/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int VisitorID)
        {
            visitor visitor = VisitorRepository.GetVisitorByID(VisitorID);
            VisitorRepository.DeleteRecord(visitor);
            return RedirectToAction("Details", new { bDate = System.DateTime.Now.AddDays(-365), eDate = System.DateTime.Now });
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
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> VisitorCategoryList;
            VisitorCategoryList = ConstantRepository.GetConstantByCategoryID("Visitor Category");
            ViewBag.VisitorCategoryList = new SelectList(VisitorCategoryList, "Key", "Value", id);

            Dictionary<int, string> BestContactList;
            BestContactList = ConstantRepository.GetConstantByCategoryID("Best Contact");
            ViewBag.BestContactList = new SelectList(BestContactList, "Key", "Value", id);

            Dictionary<int, string> VisitorTitleList;
            VisitorTitleList = ConstantRepository.GetConstantByCategoryID("Visitor Title");
            ViewBag.VisitorTitleList = new SelectList(VisitorTitleList, "Key", "Value", id);

            Dictionary<int, string> LastEventAttended;
            LastEventAttended = ConstantRepository.GetConstantByCategoryID("Last Event Attended");
            ViewBag.LastEventAttendedList = new SelectList(LastEventAttended, "Key", "Value", id);

            Dictionary<int, string> CellProviderList;
            CellProviderList = ConstantRepository.GetConstantByCategoryID("Cell Phone Provider");
            ViewBag.CellProviderList = new SelectList(CellProviderList, "Key", "Value", id);

            Dictionary<string, string> SuffixList;
            SuffixList = ConstantRepository.GetConstantByCategory("Suffix");
            ViewBag.SuffixList = new SelectList(SuffixList, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            GetData();
            ViewBag.BeginDate = bDate.ToShortDateString();
            ViewBag.EndDate = eDate.ToShortDateString();

            IEnumerable<visitor> VisitorList;

            if (SearchType == "CategorySearch")
            {
                VisitorList = VisitorRepository.GetVisitorByCategory(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                VisitorList = VisitorRepository.GetVisitorByStatus(code);
            }
            else if (SearchType == "BestContactSearch")
            {
                VisitorList = VisitorRepository.GetVisitorByBestContact(codeID);
            }
            else if (SearchType == "LastEventAttendedSearch")
            {
                VisitorList = VisitorRepository.GetVisitorByLastEventAttended(codeID);
            }
            else
            {
                VisitorList = VisitorRepository.GetVisitorByEventDate(bDate.Date,eDate.Date);
            }

            foreach(var i in VisitorList)
            {
                if (i.subCategoryID != 0)
                {
                   i.VisitorCategory = ConstantRepository.GetConstantID(i.subCategoryID).Value1;
                }
                if (i.BestContact != null)
                {
                   i.BestContactType = ConstantRepository.GetConstantID((int)i.BestContact).Value1;
                }
                if (i.Title != null)
                {
                   i.Title2 = ConstantRepository.GetConstantID((int)i.Title).Value1;
                }
                if (i.LastEventAttended != null)
                {
                  i.LastEventName = ConstantRepository.GetConstantID((int)i.LastEventAttended).Value1;
                }
            }

            ViewBag.RecordCount = VisitorList.Count();

            return PartialView(VisitorList);
        }
        public void Unsubscribe(int VisitorID = 0)
        {
            visitor visitor = VisitorRepository.GetVisitorByID(VisitorID);
            VisitorRepository.DeleteRecord(visitor);
        }


        public ActionResult UpdateVisitorProfile(int VisitorID = 0)
        {
            GetData();
            visitor visitor;
            using (churchdatabaseEntities db = new churchdatabaseEntities())
            {
                visitor = db.visitors.SingleOrDefault(e => e.visitorID == VisitorID);
                ViewBag.Heading = string.Format("Name: {0}", visitor.FullName);
            }
            return PartialView(visitor);
        }

        public void UpdateVisitor(int VisitorID = 0, string FirstName = "", string LastName = "", string Email = "", string PhoneNumber = "", string Address1 = "",
            string Address2 = "", string City = "", string State = "", string Zip = "", int PhoneProviderID = 0)
        {
            visitor visitor = VisitorRepository.GetVisitorByID(VisitorID);
            if (visitor != null)
            {

                visitor.FirstName = FirstName;
                visitor.LastName = LastName;
                visitor.Email = Email;
                visitor.phoneNumber = PhoneNumber;
                visitor.Address = Address1;
                visitor.Address2 = Address2;
                visitor.City = City;
                visitor.State = State;
                visitor.Zip = Zip;
                visitor.PhoneProviderID = PhoneProviderID;
                db.Entry(visitor).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

    }
}