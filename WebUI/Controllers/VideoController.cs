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
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
    public class VideoController : Controller
    {
      private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IVideoRepository  VideoRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public VideoController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IVideoRepository VideoParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            VideoRepository = VideoParam;

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
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;

            GetData(ministryID);
            DateTime aDate = DateTime.Now;
            ViewBag.BeginDate = aDate.AddDays(-60).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer,Admin2")]
        public ActionResult Create(int ministryID = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            GetData(ministryID);
            return PartialView(new video { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive", ministryID = ministryID });
        } 

        //
        // POST: /Video/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(video video, string CallerType = "")
        {
            try
            {
                if (video.Description == null) { video.Description = ""; }

                if (ModelState.IsValid)
                {
                    db.videos.Add(video);
                    db.SaveChanges();
                    VideoRepository.AddRecord(video);
                    TempData["Message2"] = "Video added successfully.";
                    GetData(video.ministryID);
                    if (CallerType == "Ministry")
                    {
                        return RedirectToAction("Details", new { ministryID = video.ministryID, CallerType = "Ministry" });
                    }
                    else if (CallerType == "Officer")
                    {
                        return RedirectToAction("LatestVideo", new { ministryID = video.ministryID});
                    }
                    else
                    {
                        return RedirectToAction("Details", new { });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding video";
            }
            GetData(video.ministryID);
            return PartialView(video);
        }
        
        //
        // GET: /Video/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Edit(int VideoID, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            video video = VideoRepository.GetVideoByID(VideoID);
            GetData(video.ministryID);
            return PartialView(video);
        }

        //
        // POST: /Video/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(video video, string CallerType = "")
        {
            if (video.Description == null) { video.Description = ""; }
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(video).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Vodeo update successfully.");
                    GetData(video.ministryID);
                    if (CallerType == "Ministry")
                    {
                        return RedirectToAction("Details", new { ministryID = video.ministryID, CallerType = "Ministry" });
                    }
                    else
                    {
                        return RedirectToAction("Details", new { });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing video.");
            }
            GetData(video.ministryID);
            return PartialView(video);
        }

        //
        // GET: /Video/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Delete(int VideoID)
        {
            ViewBag.VideoID = VideoID;
            video video = VideoRepository.GetVideoByID(VideoID);
            return PartialView(video);
        }

        //
        // POST: /Video/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int VideoID, string CallerType = "")
        {
            video video = VideoRepository.GetVideoByID(VideoID);
            VideoRepository.DeleteRecord(video);
            if (CallerType == "Ministry")
            {
                return RedirectToAction("Details", new { ministryID = video.ministryID, CallerType = "Ministry" });
            }
            else
            {
                return RedirectToAction("Details", new { });
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

            Dictionary<string, string> VideoCategory;
            VideoCategory = ConstantRepository.GetConstantByCategory("Video Category");
            ViewBag.VideoCategory = new SelectList(VideoCategory, "Key", "Value", id);

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

            Dictionary<int, string> VideoList;
            VideoList = VideoRepository.GetVideoList(ministryID);
            ViewBag.VideoList = new SelectList(VideoList, "Key", "Value", id);
        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int MinistryID = 0, string code = "", int codeID2 = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            IEnumerable<video> VideoList;

            if (SearchType == "MinistrySearch")
            {
                VideoList = VideoRepository.GetVideoByMinistry(MinistryID, bDate.Date, eDate.Date);
            }
            else if (SearchType == "MinistryStatusSearch")
            {
                VideoList = VideoRepository.GetVideoByMinistryStatus(MinistryID, code);
            }
            else if (SearchType == "StatusSearch")
            {
                VideoList = VideoRepository.GetVideoByStatus(code);
            }
            else
            {
                VideoList = VideoRepository.GetVideoByDateRange(bDate.Date, eDate.Date);
            }

            ViewBag.RecordCount = VideoList.Count();

            return PartialView(VideoList);
        }

        public ActionResult DisplayVideo(string url)
        {
            /*
            int i = url.IndexOf("=");
            int len = url.Length;
            string videoUrl = url.Substring(i + 1);
            ViewBag.url = string.Format("https://www.youtube.com/embed/{0}?autoplay=1", videoUrl);
             * */
            ViewBag.url = string.Format("{0}?autoplay=1", url); ;
        //    ViewBag.url = string.Format("{0}", url); ;
            return PartialView();
        }

        public ActionResult VideoList(DateTime bDate, DateTime eDate, string SearchType = "", int MinistryID = 0, string code = "", int codeID2 = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            IEnumerable<video> VideoList;

            if (SearchType == "MinistrySearch")
            {
                VideoList = VideoRepository.GetVideoByMinistry(MinistryID, bDate, eDate);
            }
            else if (SearchType == "StatusSearch")
            {
                VideoList = VideoRepository.GetVideoByStatus(code);
            }
            else
            {
                VideoList = VideoRepository.GetVideoByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = VideoList.Count();

            return PartialView(VideoList);
        }

        public ActionResult LatestVideo(int ministryID = 0, string Requestor = "")
        {
            ViewBag.Requestor = Requestor;
            ViewBag.MinistryID = ministryID;
            IEnumerable<video> VideoList;
           // if ((ViewBag.Supervisor == true) || (ViewBag.Supervisor2 == true))
            if (Requestor == "Officer")
            {
                VideoList = VideoRepository.GetMinistryVideo(ministryID).Take(15);
            }
            else
            {
               VideoList = VideoRepository.GetVideoByMinistryStatus(ministryID,"Active").Take(7);
            }

            ViewBag.RecordCount = VideoList.Count();

            return PartialView(VideoList);
        }
    }
}