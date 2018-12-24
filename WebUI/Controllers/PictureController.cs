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
 //   [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class PictureController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IPictureRepository PictureRepository;
        private IStoryRepository StoryRepository;
        private IMemberRepository MemberRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public PictureController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IPictureRepository PictureParam, IStoryRepository StoryParam, IMemberRepository MemberParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            PictureRepository = PictureParam;
            StoryRepository = StoryParam;
            MemberRepository = MemberParam;

            //security
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
        // GET: /Picture/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            ViewBag.BeginDate = DateTime.Now.AddDays(-30).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Picture/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            GetData();
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;
            ViewBag.BeginDate = DateTime.Now.AddDays(-30).ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();  
            return PartialView();
        }

        //
        // GET: /Picture/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Create(int MinistryID)
        {
            GetData();
            return PartialView(new picture { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive", ministryID = MinistryID });
        } 

        //
        // POST: /Picture/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(picture picture)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (picture.Description == null) { picture.Description = ""; }

                if (ModelState.IsValid)
                {
                    foreach(var image in picture.files)
                    {
                        if (image != null)
                        {
                            picture.ImageMimeType = image.ContentType;
                            picture.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(picture.ImageData, 0, image.ContentLength);
                        }

                        db.pictures.Add(picture);
                        db.SaveChanges();
                        PictureRepository.AddRecord(picture);
                    }

                    TempData["Message2"] = "Picture added successfully.";
                    GetData();
                    //return Content("Picture added successfully.");
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding picture";
            }
            GetData();

            return PartialView(picture);
        }
        
        //
        // GET: /Picture/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int PictureID)
        {
            picture picture = PictureRepository.GetPictureByID(PictureID);
            return View(picture);
        }

        //
        // POST: /Picture/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(picture picture)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Picture update successfully.");
                    GetData();
                    return RedirectToAction("Edit", new { PictureID = picture.pictureID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing picture.");
            }
            GetData();
            return PartialView(picture);
        }

        //
        // GET: /Picture/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Delete(int PictureID)
        {
            ViewBag.PictureID = PictureID;
            picture picture = PictureRepository.GetPictureByID(PictureID);
            return PartialView(picture);
        }

        //
        // POST: /Picture/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int PictureID)
        {
            picture picture = PictureRepository.GetPictureByID(PictureID);
            PictureRepository.DeleteRecord(picture);
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

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);
        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<picture> PictureList;
            IEnumerable<picture> GroupPictureList;
            IEnumerable<picture> RecentPictureList;

            ViewBag.HasGroupPictures = false;
            ViewBag.GroupPictures = false;
            ViewBag.HasRecentPictures = false;

            if (SearchType == "MinistrySearch")
            {
                PictureList = PictureRepository.GetPictureByMinistry(codeID);
                GroupPictureList = PictureRepository.GetMinistryPictureGroup(codeID);
                ViewBag.MinistryID = codeID;
                if (GroupPictureList.Count() > 0)
                {
                     ViewBag.HasGroupPictures = true;
                     ViewBag.GroupPictures = true;
                     ViewBag.GroupPictureList = GroupPictureList;

                    foreach(picture p in GroupPictureList)
                    {
                        p.GroupDescription = StoryRepository.GetStoryByID((int)p.GroupID).Header;
                    }
                }

                RecentPictureList = PictureRepository.GetMinistryRecentPictures(codeID);
                if (RecentPictureList.Count() > 0)
                {
                    ViewBag.HasRecentPictures = true;
                }
            }
            else if (SearchType == "StatusSearch")
            {
                PictureList = PictureRepository.GetPictureByStatus(code,codeID2, bDate, eDate);
            }
            else
            {
                PictureList = PictureRepository.GetPictureByDateRange(bDate, eDate);
            }



            ViewBag.RecordCount = PictureList.Count();

            return PartialView(PictureList);
        }

        public FileContentResult GetImageByID(int PictureID = 0)
        {
            picture pic = PictureRepository.GetPictureByID(PictureID);

            if (pic != null)
            {
                return File(pic.ImageData, pic.ImageMimeType);
            }
            return File(pic.ImageData, pic.ImageMimeType);
        }

        public FileContentResult GetMemberImageByID(int memberID = 0)
        {
            member member = MemberRepository.GetMemberByID(memberID);
            picture pic = PictureRepository.GetDefaultPersonPicture(member.memberID);

            return File(pic.ImageData, pic.ImageMimeType);
        } 

        public FileContentResult GetImageByDesc(string Description)
        {
            picture pic = PictureRepository.GetPictureByDescription(Description);

            if (pic != null)
            {
                return File(pic.ImageData, pic.ImageMimeType);
            }
            return File(pic.ImageData, pic.ImageMimeType);
        }
        public ActionResult DisplayPicque(int pictureID)
        {
            picture pic = PictureRepository.GetPictureByID((int)pictureID);
            return PartialView(pic);
        }

        public ActionResult GroupSlider(int ministryID, int GroupID = 0, string Requestor = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.Requestor = Requestor;
            IEnumerable<picture> PictureList;

            if (GroupID == 0)
            {
                PictureList = PictureRepository.GetMinistryRecentPictures(ministryID);
            }
            else
            {
               PictureList = PictureRepository.GetPictureByGroup(ministryID, GroupID);
            }
            return PartialView(PictureList);
        }

        public FileContentResult GetDefaultPeoplePicture()
        {
            picture pic = null;

            if (PictureRepository.IsDefaultPeoplePicture())
            {
                pic = PictureRepository.GetDefaultPeoplePicture();
            }
            return File(pic.ImageData, pic.ImageMimeType);
        }

      
    }
}