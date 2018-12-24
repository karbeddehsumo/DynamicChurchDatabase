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
    public class ResponsibilityController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IStaffRepository StaffRepository;
        private IResponsibilityRepository ResponsibilityRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ResponsibilityController(IConstantRepository ConstantParam, IResponsibilityRepository ResponsibilityParam, IStaffRepository StaffParam)
        {
            ConstantRepository = ConstantParam;
            ResponsibilityRepository = ResponsibilityParam;
            StaffRepository = StaffParam;

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
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Create(int staffID=0)
        {
            GetData();
            return PartialView(new responsibility { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", staffID = staffID });
        } 


        //
        // POST: /Responisbility/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(responsibility responsibility)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.responsibilities.Add(responsibility);
                    db.SaveChanges();
                    ResponsibilityRepository.AddRecord(responsibility);
                    TempData["Message2"] = "Responsibility added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding responsibility";
            }
            GetData();

            return PartialView(responsibility);
        }
        
        //
        // GET: /Responisbility/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int ResponsibilityID)
        {
            GetData();
            responsibility responsibility = ResponsibilityRepository.GetResponsibilityByID(ResponsibilityID);
            staff staff = StaffRepository.GetStaffByID(responsibility.staffID);
            ViewBag.Heading = string.Format("Edit {0} Dutes", staff.FullNameTitle);
            return PartialView(responsibility);
        }

        //
        // POST: /Responisbility/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(responsibility responsibility)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(responsibility).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Responsible record update successfully.");
                    GetData();
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", responsibility.Title);
            }
            GetData();
            return PartialView(responsibility);
        }

        //
        // GET: /Responisbility/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Delete(int ResponsibilityID)
        {
            ViewBag.ResponsibilityID = ResponsibilityID;
            responsibility responsibility = ResponsibilityRepository.GetResponsibilityByID(ResponsibilityID);
            return View(responsibility);
        }

        //
        // POST: /Responisbility/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ResponsibilityID)
        {
            responsibility responsibility = ResponsibilityRepository.GetResponsibilityByID(ResponsibilityID);
            ResponsibilityRepository.DeleteRecord(responsibility);
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

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> StaffList;
            StaffList = StaffRepository.GetStaffList();
            ViewBag.StaffList = new SelectList(StaffList, "Key", "Value", id);
        }


        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<responsibility> ResponsibilityList;

            if (SearchType == "StaffSearch")
            {
                ResponsibilityList = ResponsibilityRepository.GetResponsibilityByStaff(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                ResponsibilityList = ResponsibilityRepository.GetResponsibilityByStatus(code);
            }
            else
            {
                ResponsibilityList = ResponsibilityRepository.GetResponsiblityByDateRange(bDate,eDate);
            }

            ViewBag.RecordCount = ResponsibilityList.Count();

            return PartialView(ResponsibilityList);
        }

        public ActionResult StaffDuties(int staffID)
        {
            IEnumerable<responsibility> ResponsibilityList = ResponsibilityRepository.GetResponsibilityByStaff(staffID);
            return PartialView(ResponsibilityList);
        }
    }
}