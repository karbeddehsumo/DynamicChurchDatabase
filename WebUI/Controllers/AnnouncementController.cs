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
using System.IO;
using System.Text;
using WebUI.Filters;

namespace WebUI.Controllers
{
   //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
    public class AnnouncementController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IAnnouncementRepository AnnouncementRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

       
        public AnnouncementController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IAnnouncementRepository AnnouncementParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            AnnouncementRepository = AnnouncementParam;

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
        // GET: /Announcement/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Announcement/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.AddDays(30).ToShortDateString();
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /Announcement/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Create(int ministryID = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            GetData(ministryID);
            ViewBag.Ministry = "";
            if (ministryID > 0)
            {
                ViewBag.Ministry = "Ministry";
                return PartialView(new announcement { ministryID = ministryID, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive" });
            }
            else
            {
                return PartialView(new announcement { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive" });
            }
        } 

        //
        // POST: /Announcement/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(announcement announcement, string CallerType = "")
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }

                if (ModelState.IsValid)
                {
                    //document
                    foreach (var file in announcement.files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            int documentTypeID = ConstantRepository.GetConstantByName("Property Document").constantID;
                            // Get file info
                            document document = new document();
                            document.Title = string.Format("{0} document", announcement.Title);
                            document.DocumentTypeID = documentTypeID;
                            document.Status = "Active";
                            document.EnteredBy = User.Identity.Name.ToString();
                            document.DateEntered = System.DateTime.Today;
                            document.FileName = Path.GetFileName(file.FileName);
                            document.ContentLength = file.ContentLength;
                            document.ContentType = file.ContentType;
                            document.Author = "Announcement Document";
                            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                           // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                            file.SaveAs(path);
                            db.documents.Add(document);
                            db.SaveChanges();
                            announcement.DocumentID = document.documentID;
                        }

                    }

                    db.announcements.Add(announcement);
                    db.SaveChanges();
                    AnnouncementRepository.AddRecord(announcement);
                    TempData["Message2"] = "Announcement record added successfully.";
                    GetData(announcement.ministryID);
                    /*
                    if (CallerType == "Ministry")
                    {
                        return Redirect(ReturnUrl);
                        //return Redirect(string.Format("/Home/Admin?Page=Announcement&MinistryID={0}&CallerType={1}", announcement.ministryID, "Ministry"));
                    }
                    else
                    {
                        return Redirect(ReturnUrl);
                        //return Redirect("/Home/Admin?Page=Announcement");
                    }
                     */
                    if (CallerType == "Officer")
                    {
                        return RedirectToAction("MinistryGeneralAnnouncements", "Announcement", new {ministryID=announcement.ministryID, bDate=announcement.BeginDate, eDate=announcement.EndDate, Requestor = "Officer"});
                    }
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding announcement";
            }
            GetData();

            return PartialView(announcement);

        }
        
        //
        // GET: /Announcement/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Edit(int AnnouncementID, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            announcement announcement = AnnouncementRepository.GetAnnouncementByID(AnnouncementID);
            GetData(announcement.ministryID);
            return PartialView(announcement);
        }

        //
        // POST: /Announcement/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(announcement announcement, string CallerType = "")
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    //document
                    foreach (var file in announcement.files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            int documentTypeID = ConstantRepository.GetConstantByName("Property Document").constantID;
                            // Get file info
                            document document = new document();
                            document.Title = string.Format("{0} document", announcement.Title);
                            document.DocumentTypeID = documentTypeID;
                            document.Status = "Active";
                            document.EnteredBy = User.Identity.Name.ToString();
                            document.DateEntered = System.DateTime.Today;
                            document.FileName = Path.GetFileName(file.FileName);
                            document.ContentLength = file.ContentLength;
                            document.ContentType = file.ContentType;
                            document.Author = "Announcement Document";
                            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                           // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
    
                            file.SaveAs(path);
                            db.documents.Add(document);
                            db.SaveChanges();
                            announcement.DocumentID = document.documentID;
                        }

                    }

                    db.Entry(announcement).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Announcement update successfully.");
                    GetData(announcement.ministryID);
                /*    if (CallerType == "Officer")
                    {
                        return RedirectToAction("MinistryGeneralAnnouncements", "Announcement", new { ministryID = announcement.ministryID, bDate = announcement.BeginDate, eDate = announcement.EndDate, Requestor = "Officer" });
                    }
                 * */
                    return Redirect(ReturnUrl);
                    /*
                    if (CallerType == "Ministry")
                    {
                        return Redirect(string.Format("/Home/Admin?Page=Announcement&MinistryID={0}&CallerType={1}", announcement.ministryID, "Ministry"));
                    }
                    else
                    {
                        return Redirect("/Home/Admin?Page=Announcement");
                    }
                     */
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing announcement.");
            }
            GetData(announcement.ministryID);
            return PartialView(announcement);
        }

        //
        // GET: /Announcement/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Delete(int AnnouncementID)
        {
            ViewBag.AnnouncementID = AnnouncementID;
            announcement announcement = AnnouncementRepository.GetAnnouncementByID(AnnouncementID);
            return PartialView(announcement);
        }

        //
        // POST: /Announcement/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int AnnouncementID, string CallerType = "")
        {
            announcement announcement = AnnouncementRepository.GetAnnouncementByID(AnnouncementID);
            AnnouncementRepository.DeleteRecord(announcement);
            if (CallerType == "Ministry")
            {
                return RedirectToAction("List", new { bdate = "1/1/2015", edate = "1/1/2015", SearchType = "MinistrySearch", MinistryID = announcement.ministryID, CallerType = "Ministry" });
            }
            else
            {
                return RedirectToAction("List", new { bDate = DateTime.Today, eDate = DateTime.Today.AddDays(30) });
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int ministryID = 0)
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            if (ministryID > 0)
            {
                MinistryList = new Dictionary<int, string>();
                string MinistryName = MinistryRepository.GetMinistryByID(ministryID).MinistryName;
                MinistryList.Add(ministryID, MinistryName);
            }
            else
            {
                MinistryList = MinistryRepository.GetMinistryList();
            }
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);
        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchCode = "", int codeID = 0, string CallerType = "", string ReportType = "")
        {
            ViewBag.CallerType = CallerType;
            ViewBag.ReportType = ReportType;
            if (CallerType == "Ministry")
            {
                GetData(codeID);
            }
            else
            {
              GetData();
            }
            IEnumerable<announcement> AnnouncementList;

            if (SearchCode == "MinistrySearch")
            {
                    AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(codeID, bDate, eDate, CallerType);
            }
            else
            {
                    AnnouncementList = AnnouncementRepository.GetAnnouncementByDateRange(bDate, eDate, CallerType);
            }

            foreach (announcement a in AnnouncementList)
            {
                a.ministry = MinistryRepository.GetMinistryByID(a.ministryID);
            }
               ViewBag.RecordCount = AnnouncementList.Count();


            return PartialView(AnnouncementList);
        }

        public ActionResult MyAnnouncement(DateTime bDate, DateTime eDate, string SearchCode = "", int codeID = 0)
        {
            GetData();
            IEnumerable<announcement> AnnouncementList;

            if (SearchCode == "MinistrySearch")
            {
                ViewBag.Heading = "Ministry Announcement";
                AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(codeID, bDate, eDate);
            }
            else
            {
                ViewBag.Heading = "General Announcement";
                AnnouncementList = AnnouncementRepository.GetAnnouncementByDateRange(bDate, eDate);
            }

            foreach (announcement a in AnnouncementList)
            {
                a.ministry = MinistryRepository.GetMinistryByID(a.ministryID);
            }

            ViewBag.RecordCount = AnnouncementList.Count();

            return PartialView(AnnouncementList);
        }

        public ActionResult AdminMisc(string Page = "")
        {
            ViewBag.Page = Page;
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();

            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
            }

            return PartialView();
        }

        public ActionResult MinistryGeneralAnnouncements(int ministryID, DateTime bDate, DateTime eDate, string Requestor = "")
        {
            ViewBag.Requestor = Requestor;
            ViewBag.MinistryID = ministryID;
            GetData();
            IEnumerable<announcement> AnnouncementList; 
           // if ((ViewBag.Supervisor == true) || (ViewBag.Supervisor2 == true))
            if (Requestor == "Officer")
            {
                AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, bDate, eDate, Requestor).OrderByDescending(e => e.BeginDate);
            }
            else
            {
                AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, bDate, eDate).OrderByDescending(e => e.BeginDate);
            }

            foreach (announcement a in AnnouncementList)
            {
                a.ministry = MinistryRepository.GetMinistryByID(a.ministryID);
            }

            ViewBag.RecordCount = AnnouncementList.Count();
            return PartialView(AnnouncementList);
        }

        public ActionResult GeneralAnnouncement()
        {
            DateTime eDate = DateTime.Now.Date;
            DateTime bDate = eDate.AddDays(-60);
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;

            ministry ministry = MinistryRepository.GetMinistryByCodeDesc("Church");
            ViewBag.MinistryID = ministry.ministryID;
            return PartialView();
           // return RedirectToAction("MinistryGeneralAnnouncements","Announcement", new { ministryID = ministry.ministryID, bDate = bDate, eDate = eDate });
        }
    }
}