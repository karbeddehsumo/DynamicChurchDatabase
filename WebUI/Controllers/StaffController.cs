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
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class StaffController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IStaffRepository StaffRepository;
        private IResponsibilityRepository  ResponsibilityRepository;
        private IMinistryRepository MinistryRepository;
        private IPictureRepository PictureResponsibility;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public StaffController(IConstantRepository ConstantParam, IStaffRepository StaffParam, IResponsibilityRepository ResponsibilityParam,
                               IMinistryRepository MinistryParam, IPictureRepository PictureParam)
        {
            ConstantRepository = ConstantParam;
            StaffRepository = StaffParam;
            ResponsibilityRepository = ResponsibilityParam;
            MinistryRepository = MinistryParam;
            PictureResponsibility = PictureParam;

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
        public ActionResult Create()
        {
            GetData();
            return PartialView(new staff { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 

        //
        // POST: /Staff/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(staff staff)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.staffs.Add(staff);
                    db.SaveChanges();

                    //create default staff responsibities
                    responsibility duties = new responsibility();
                    duties.staffID = staff.staffID;
                    duties.BriefDescription = "Here is summary of My responsibilities";
                    duties.FullDescription = "Here is full detail of my responsibilities";
                    duties.DateCreated = System.DateTime.Today;
                    duties.Status = "Active";
                    duties.Title = "Church officer";
                    duties.Type = "Primary";
                    duties.EnteredBy = User.Identity.Name.ToString();
                    duties.DateEntered = System.DateTime.Today;
                    db.responsibilities.Add(duties);
                    db.SaveChanges();
                    StaffRepository.AddRecord(staff);
                    TempData["Message2"] = "Staff added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding staff";
            }
            GetData();
            return PartialView(staff);
        }
        
        //
        // GET: /Staff/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int StaffID)
        {
            GetData();
            staff staff = StaffRepository.GetStaffByID(StaffID);
            return PartialView(staff);
        }

        //
        // POST: /Staff/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(staff staff, string returnUrl)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                                                     
                if (ModelState.IsValid)
                {
                    if (staff.files != null)
                    {
                   foreach (var image in staff.files)
                    {

                        if (image != null)
                        {
                            picture pic = new picture();
                            ministry ministry = MinistryRepository.GetMinistryByCodeDesc("Church");
                            if (ministry == null)
                            {
                                GetData();
                                TempData["Message2"] = string.Format("Error saving staff data. No Church default ministry exist.");
                                return Redirect("/Home/Admin?Page=Staff");
                            }

                            pic.ministryID = ministry.ministryID;
                            pic.Description = " ";
                            pic.PictureDate = System.DateTime.Today;
                            pic.PictureType = "Staff Photo";
                            pic.Status = "Active";
                            pic.EnteredBy = User.Identity.Name.ToString();
                            pic.DateEntered = System.DateTime.Today;
                            pic.ImageMimeType = image.ContentType;
                            pic.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(pic.ImageData, 0, image.ContentLength);

                            db.pictures.Add(pic);
                            db.SaveChanges();
                            staff.pictureID = pic.pictureID;
                        }


                    }
                    }

                    if (staff.pictureID == null) { staff.pictureID = 0; }
                    if (staff.SortOrder == null) { staff.SortOrder = 0; }
                    db.Entry(staff).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Staff update successfully.");
                    GetData();

                    //string url = this.Request.UrlReferrer.AbsolutePath;
                    //return Redirect("/Home/Admin?Page=Staff");
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing staff.");
            }
            GetData();
            return PartialView(staff);
        }

        //
        // GET: /Staff/Delete/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Delete(int StaffID)
        {
            ViewBag.StaffID = StaffID;
            staff staff = StaffRepository.GetStaffByID(StaffID);
            return PartialView(staff);
        }

        //
        // POST: /Staff/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int StaffID)
        {
            staff staff = StaffRepository.GetStaffByID(StaffID);
            if (staff.pictureID > 0)
            {
                picture pic = PictureResponsibility.GetPictureByID((int)staff.pictureID);
                PictureResponsibility.DeleteRecord(pic);
            }
           IEnumerable<responsibility> duties = ResponsibilityRepository.GetResponsibilityByStaff(StaffID);
            foreach(responsibility r in duties)
            {
                ResponsibilityRepository.DeleteRecord(r);
            }
            StaffRepository.DeleteRecord(staff);
            return RedirectToAction("Details");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int staffID = 0)
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> StaffList;
            StaffList = StaffRepository.GetStaffList();
            ViewBag.StaffList = new SelectList(StaffList, "Key", "Value", id);

            Dictionary<string, string> ChurchTitleList;
            ChurchTitleList = ConstantRepository.GetConstantByCategory("Church Title");
            ViewBag.ChurchTitleList = new SelectList(ChurchTitleList, "Key", "Value", id);

            if (staffID > 0)
            {
            Dictionary<int, string> ResponsibilityList;
            ResponsibilityList = ResponsibilityRepository.GetResponsibilityList(staffID);
            ViewBag.ResponsibilityList = new SelectList(ResponsibilityList, "Key", "Value", id);
            }

            Dictionary<int, int> SortOrderList = new Dictionary<int, int>();
            for(int i = 1; i < 20; i++)
            {
                SortOrderList.Add(i,i);
            }
            ViewBag.SortOrderList = new SelectList(SortOrderList, "Key", "Value", id);
        }

        public ActionResult List(string code)
        {
            GetData();
            IEnumerable<staff> StaffList;
            StaffList = StaffRepository.GetStaffByStatus(code);
            ViewBag.RecordCount = StaffList.Count();

            return PartialView(StaffList);
        }

        public ActionResult DisplayStaff()
        {
            IEnumerable<staff> StaffList;
            StaffList = StaffRepository.GetStaffByStatus("Active");
            ViewBag.RecordCount = StaffList.Count();

            foreach (staff s in StaffList)
            {
                if (s.pictureID == null)
                {
                    s.pictureID = 0;
                }
                s.picture = PictureResponsibility.GetStaffPicture((int)s.pictureID);
                s.mainResponsibility = ResponsibilityRepository.GetStaffPrimaryResponsibility(s.staffID);
            }

            return PartialView(StaffList);
        }
    }
}