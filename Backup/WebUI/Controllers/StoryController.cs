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
     //[RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
    public class StoryController : Controller
    {
      private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IPictureRepository  PictureRepository;
        private IStoryRepository StoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public StoryController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IPictureRepository PictureParam, IStoryRepository StoryParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            PictureRepository = PictureParam;
            StoryRepository = StoryParam;

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
        // GET: /Story/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /Story/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Details()
        {
            return PartialView();
        }

        //
        // GET: /Story/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Create(int MinistryID)
        {
            GetData(MinistryID);
            return PartialView(new story { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), MinistryID = MinistryID, Status = "Inactive" });
 
        } 

        //
        // POST: /Story/Create

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(story story)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    story.PictureID = 1;
                    if (story.StoryLine.Length > 2000)
                    {
                        return Content("Error saving: Story more than 2000 string long."); 
                    }
                    db.stories.Add(story);
                    db.SaveChanges();
                    StoryRepository.AddRecord(story);
                    return Content("Story added successfully"); 
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding story";
            }
            GetData(story.MinistryID);
            return PartialView(story);
        }
        
        //
        // GET: /Story/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Edit(int StoryID)
        {
            story story = StoryRepository.GetStoryByID(StoryID);
            GetData(story.MinistryID);
            return PartialView(story);
        }

        //
        // POST: /Story/Edit/5

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(story story)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    //add main picture
                   
                    foreach (var image in story.MainPic)
                    {
                        if (image != null)
                        {
                            picture pic = new picture();
                            pic.ImageMimeType = image.ContentType;
                            pic.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(pic.ImageData, 0, image.ContentLength);
                       
                            pic.ministryID = 0;
                            pic.PictureDate = System.DateTime.Today;
                            pic.Status = "Active";
                            pic.Description = string.Format("Picture:{0}", story.Header);
                            pic.DateEntered = System.DateTime.Today;
                            pic.EnteredBy = User.Identity.Name.ToString();

                            db.pictures.Add(pic);
                            db.SaveChanges();

                            story.PictureID = pic.pictureID;
                        }

                    }
                    
                    db.Entry(story).State = EntityState.Modified;
                    db.SaveChanges();

                    //add related picture                
                    foreach (var image in story.GroupPic)
                    {
                        if (image != null)
                        {
                            picture picture = new picture();
                            picture.ImageMimeType = image.ContentType;
                            picture.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(picture.ImageData, 0, image.ContentLength);

                            picture.ministryID = story.MinistryID;
                            picture.PictureDate = System.DateTime.Today;
                            picture.Status = "Active";
                            picture.GroupID = story.StoryID;
                            picture.Description = "";//string.Format("Picture:{0}", story.Header);
                            picture.DateEntered = System.DateTime.Today;
                            picture.EnteredBy = User.Identity.Name.ToString();
                            db.pictures.Add(picture);
                            db.SaveChanges();
                        }
                    }
                    GetData(story.MinistryID);
                   // return Redirect("/Home/Admin?Page=Ministry");
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing story.");
            }
            GetData(story.MinistryID);
            return View(story);
        }

        //
        // GET: /Story/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Delete(int StoryID)
        {
            ViewBag.StoryID = StoryID;
            story story = StoryRepository.GetStoryByID(StoryID);
            return PartialView(story);
        }

        //
        // POST: /Story/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            story story = db.stories.Find(id);
            db.stories.Remove(story);
            db.SaveChanges();
            return RedirectToAction("Index");
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

            Dictionary<int, string> StoryTypeList;
            StoryTypeList = ConstantRepository.GetConstantByCategoryID("StoryType");
            ViewBag.StoryTypeList = new SelectList(StoryTypeList, "Key", "Value", id);

            Dictionary<int, string> StoryList;
            
            if (ViewBag.Supervisor == true)
            {
                StoryList = StoryRepository.GetStoryList(ministryID);
            }
            else
            {
                StoryList = StoryRepository.GetStoryStatusList(ministryID);
            }
            ViewBag.StoryList = new SelectList(StoryList, "Key", "Value", id);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int MinistryID = 0, int codeID = 0)
        {
            IEnumerable<story> StoryList;
            GetData(MinistryID);
            ViewBag.MinistryID = MinistryID;
            if (SearchType == "MinistrySearch")
            {
                if (ViewBag.Supervisor == true)
                {
                    StoryList = StoryRepository.GetStoryByMinistry(MinistryID).Take(20);
                }
                else
                {
                    StoryList = StoryRepository.GetStoryByMinistryStatus(MinistryID, "Active").Take(7);
                }
            }
            else if (SearchType == "StoryTypeSearch")
            {
                StoryList = StoryRepository.GetStoryByType(MinistryID, codeID);
            }
            else if(SearchType == "MinistryDateRangeSearch")
            {
                StoryList = StoryRepository.GetStoryByMinistryDateRange(MinistryID, bDate, eDate);
            }
            else
            {
                StoryList = StoryRepository.GetStoryByDateRange(bDate, eDate);
            }

            foreach(var s in StoryList)
            {
                s.picture = PictureRepository.GetPictureByID((int)s.PictureID);
                s.StoryType = ConstantRepository.GetConstantID((int)s.StoryTypeID).Value1;
                int GroupPictureCount = PictureRepository.GetPictureByGroup(s.MinistryID, s.StoryID).Count();
                s.HasGroupPictures = false;
                if (GroupPictureCount > 0)
                {
                    s.HasGroupPictures = true;
                }

            }

            ViewBag.RecordCount = StoryList.Count();

            return PartialView(StoryList);
        }

        public ActionResult ChurchNewsPaper()
        {
            ViewBag.NewsPaperTitle = ConstantRepository.GetConstantByName("NewsPaperTitle").Value1;
            DateTime eDate = DateTime.Now.Date;
            DateTime bDate = eDate.AddDays(-60);
            DateTime bDate2 = DateTime.Now.Date;
            DateTime eDate2 = bDate2.AddDays(60);
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.BeginDate2 = bDate2;
            ViewBag.EndDate2 = eDate2;

            IEnumerable<story> stories = StoryRepository.GetStory();
            ViewBag.TodayDate = eDate.ToString("D");
            ministry ministry = MinistryRepository.GetMinistryByCodeDesc("Church");
            ViewBag.MinistryID = ministry.ministryID;
            foreach (story s in stories)
            {
                s.picture = PictureRepository.GetPictureByID((int)s.PictureID);
            }
            return View(stories);
        }

        public ActionResult ChurchNewsPaper2()
        {

            ViewBag.NewsPaperTitle = ConstantRepository.GetConstantByName("NewsPaperTitle").Value1;
            DateTime eDate = DateTime.Now.Date;
            DateTime bDate = eDate.AddDays(-60);
            DateTime bDate2 = DateTime.Now.Date;
            DateTime eDate2 = bDate2.AddDays(60);
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.BeginDate2 = bDate2;
            ViewBag.EndDate2 = eDate2;

            IEnumerable<story> stories = StoryRepository.GetStory();
            ViewBag.TodayDate = eDate.ToString("D");
            ministry ministry = MinistryRepository.GetMinistryByCodeDesc("Church");
            ViewBag.MinistryID = ministry.ministryID;
            foreach (story s in stories)
            {
                s.picture = PictureRepository.GetPictureByID((int)s.PictureID);
            }
            return View(stories);
        }

        public ActionResult DisplayStory(int storyID)
        {
            story story = StoryRepository.GetStoryByID(storyID);
            story.picture = PictureRepository.GetPictureByID((int)story.PictureID);
            story.StoryType = ConstantRepository.GetConstantID((int)story.StoryTypeID).Value1;
            int GroupPictureCount = PictureRepository.GetPictureByGroup(story.MinistryID, story.StoryID).Count();
            story.HasGroupPictures = false;
            if (GroupPictureCount > 0)
            {
                story.HasGroupPictures = true;
            }
            return PartialView(story);
        }

        public ActionResult LatestStories(int ministryID = 0, string Requestor = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.Requestor = Requestor;
            IEnumerable<story> StoryList;
            GetData(ministryID);
            if (Requestor == "Officer")
            {
               StoryList = StoryRepository.GetStoryByMinistry(ministryID).Take(7);
            }
            else
            {
                StoryList = StoryRepository.GetStoryByMinistryStatus(ministryID,"Active").Take(7);
            }

            foreach(var s in StoryList)
            {
                s.picture = PictureRepository.GetPictureByID((int)s.PictureID);
                s.StoryType = ConstantRepository.GetConstantID((int)s.StoryTypeID).Value1;
                int GroupPictureCount = PictureRepository.GetPictureByGroup(s.MinistryID, s.StoryID).Count();
                s.HasGroupPictures = false;
                if (GroupPictureCount > 0)
                {
                    s.HasGroupPictures = true;
                }

            }

            ViewBag.RecordCount = StoryList.Count();

            return PartialView(StoryList);
        }

    }
}