using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;


using Domain;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;

namespace WebUI.Controllers
{
    public class HomeController : Controller
    {
        private IMinistryRepository MinistryRepository;
        private IConstantRepository ConstantRepository;

        private IMeetingRepository MeetingRepository;
        private IAnnouncementRepository AnnouncementRepository;
        private ICalendarRepository CalendarRepository;
        private IStoryRepository StoryRepository;
        private IPictureRepository PictureRepository;
        private IListtableRepository ListTableRepository;
        private IDocumentRepository DocumentRepository;
        private IVideoRepository VideoRepository;
        private IVisitorRepository VisitorRepository;
        private churchdatabaseEntities db = new churchdatabaseEntities();

        public HomeController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, ICalendarRepository CalendarParam,IMeetingRepository MeetingParam,
                               IAnnouncementRepository AnnouncementParam, IStoryRepository StoryParam, IPictureRepository PictureParam, IListtableRepository ListTableParam,
                                IDocumentRepository DocumentParam, IVideoRepository VideoParam, IVisitorRepository VisitorParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            CalendarRepository = CalendarParam;
            MeetingRepository = MeetingParam;
            AnnouncementRepository = AnnouncementParam;
            StoryRepository = StoryParam;
            PictureRepository = PictureParam;
            ListTableRepository = ListTableParam;
            DocumentRepository = DocumentParam;
            VideoRepository = VideoParam;
            VisitorRepository = VisitorParam;
        }

        public ActionResult Index()
        {
            string role = Convert.ToString(System.Web.HttpContext.Current.Session["memberRole"]);
            ViewBag.Log = true;
            if (role.Trim() == "")
            {
                ViewBag.Log = false;
            }

            //front page postal
            ViewBag.HasFixedBanner1 = false;
            ViewBag.HasFixedBanner2 = false;
            ViewBag.HasFixedBanner3 = false;

            ViewBag.FixedBanner1 =  CalendarRepository.GetCalendarByDescription("FixedBanner1");
            ViewBag.FixedBanner2 = CalendarRepository.GetCalendarByDescription("FixedBanner2");
            ViewBag.FixedBanner3 = CalendarRepository.GetCalendarByDescription("FixedBanner3");

            if (ViewBag.FixedBanner1 != null)
            {
                ViewBag.HasFixedBanner1 = true;
            }

            if (ViewBag.FixedBanner2 != null)
            {
                ViewBag.HasFixedBanner2 = true;
            }

            if (ViewBag.FixedBanner3 != null)
            {
                ViewBag.HasFixedBanner3 = true;
            }
            ViewBag.DisplayFixedBanner = false;
            constant DisplayFixedBanner = ConstantRepository.GetConstantByName("DisplayFixedBanner");
            if (DisplayFixedBanner.Value1 == "Yes")
            {
                ViewBag.DisplayFixedBanner = true;
            }


            ViewBag.Role = role;
            return PartialView();
        }

        public void GetData()
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetPublicMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<string, string> DefaultMemberType;
            DefaultMemberType = ConstantRepository.GetConstantByCategory("Member Category");
            ViewBag.DefaultMemberType = new SelectList(DefaultMemberType, "Key", "Value", id);

            Dictionary<int, string> MinistryPageStyle;
            MinistryPageStyle = ConstantRepository.GetConstantByCategoryID("Ministry Page Style");
            ViewBag.MinistryPageStyle = new SelectList(MinistryPageStyle, "Key", "Value", id);

            Dictionary<int, string> GroupMinistryList;
            GroupMinistryList = MinistryRepository.GetGroupMinistryList();
            ViewBag.GroupMinistryList = new SelectList(GroupMinistryList, "Key", "Value", id);

            Dictionary<int, string> CellProviderList;
            CellProviderList = ConstantRepository.GetConstantByCategoryID("Cell Phone Provider");
            ViewBag.CellProviderList = new SelectList(CellProviderList, "Key", "Value", id);

            Dictionary<string, string> ContactUsList;
            ContactUsList = ConstantRepository.GetConstantByCategory("Contact Us Email");
            ViewBag.ContactUsList = new SelectList(ContactUsList, "Key", "Value", id);

        }

        
        public ActionResult About()
        {
           // return View();
            return PartialView();
        }

        public ActionResult Details()
        {
            return View();
        }

        public ActionResult Events()
        {
            DateTime beginDate = DateTime.Now;
            //ViewBag.BeginDate = beginDate.ToShortDateString();
            //ViewBag.EndDate = beginDate.AddDays(60).ToShortDateString();
            DateTime endDate = beginDate.AddDays(60);
            IEnumerable<calendar> currentCalendar = CalendarRepository.GetCalendarByDateRange(beginDate, endDate);
            foreach (calendar m in currentCalendar)
            {
                m.ministry = MinistryRepository.GetMinistryByID(m.ministryID);
            }
            ViewBag.CurrentCalendar = currentCalendar;
            GetData();
            return PartialView();
        }

        public ActionResult Contact()
        {
            GetData();
            return PartialView();
        }

        public ActionResult Ministries()
        {
            DateTime beginDate = DateTime.Now;
            //ViewBag.BeginDate = beginDate.ToShortDateString();
            //ViewBag.EndDate = beginDate.AddDays(60).ToShortDateString();
            DateTime endDate = beginDate.AddDays(60);
            IEnumerable<calendar> currentCalendar = CalendarRepository.GetCalendarByDateRange(beginDate, endDate);
            foreach (calendar m in currentCalendar)
            {
                m.ministry = MinistryRepository.GetMinistryByID(m.ministryID);
            }
            ViewBag.CurrentCalendar = currentCalendar;
            GetData();


            return PartialView();
        }

        public ActionResult WebMaster()
        {
           // return PartialView();
            return View();
        }

        public ActionResult Member()
        {
           // return View();
            return PartialView();
        }

        public ActionResult Admin(string Page = "")
        {
            ViewBag.Page = Page;
            return PartialView();
        }

        public ActionResult Finance()
        {
            //return View();
            return PartialView();
        }

        public ActionResult Ministry()
        {
           // return View();
            return PartialView();
        }

        public ActionResult SlideShow()
        {
            DateTime beginDate = DateTime.Now;
            DateTime endDate = beginDate.AddDays(60);
            ViewBag.BeginDate = beginDate;
            ViewBag.EndDate = endDate;


            //get public ministries for banner post
            IEnumerable<ministry> PublicMinistry = MinistryRepository.GetDisplayMinistry();
            ViewBag.PublcMinistry = PublicMinistry;
            ViewBag.PublicMinistryCount = PublicMinistry.Count();

            //get banner calendar events
            //check if available banner events
            int bannerCount = CalendarRepository.GetBannerCalendarEvent(beginDate, endDate).Count();
            ViewBag.HasCalendarBanner = false;
            if (bannerCount > 0)
            {
                ViewBag.HasCalendarBanner = true;
            }
            IEnumerable<calendar> BannerCalendarEvents = CalendarRepository.GetBannerCalendarEvent(beginDate, endDate);
            foreach (calendar c in BannerCalendarEvents)
            {
                if(c.DocumentID != null)
                {
                  c.document = DocumentRepository.GetDocumentByID((int)c.DocumentID);
                }
                c.picture = PictureRepository.GetPictureByID((int)c.PictureID);
            }
            ViewBag.CalendarBannerEvent = BannerCalendarEvents;

            return PartialView();
        }

        public ActionResult Donation()
        {
            return PartialView();
        }

        public ActionResult GetMinistryPage(int ministryID = 0)
        {
            
            ministry ministry;
            if (ministryID == 0)
            {
                ministry = MinistryRepository.GetMainChurchMinistry();
            }
            else
            {
                ministry = MinistryRepository.GetMinistryByID(ministryID);
            }
            ViewBag.MinistryID = ministry.ministryID;
            DateTime beginDate = DateTime.Now;
            DateTime endDate = beginDate.AddDays(60);
            IEnumerable<calendar> currentCalendar = CalendarRepository.GetCalendarByMinistryDate(ministryID,beginDate, endDate);
            foreach (calendar m in currentCalendar)
            {
                m.ministry = MinistryRepository.GetMinistryByID(m.ministryID);
            }
            ViewBag.CurrentCalendar = currentCalendar;
            GetData();

            IEnumerable<announcement> announcements = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, beginDate, endDate);
            ViewBag.Announcements = announcements;

            IEnumerable<document> documents = DocumentRepository.GetDocumentByMinistry(ministryID);
            ViewBag.Document = documents;

            IEnumerable<listtable> tables = ListTableRepository.GetListByMinistry(ministryID);
            ViewBag.Tables = tables;

            //banner
            ViewBag.BannerID = null;
            if (ministry.PictureID != null)
            {
                picture banner = PictureRepository.GetPictureByID((int)ministry.PictureID);
                ViewBag.BannerID = banner.pictureID;
            }
            else
            {
                picture banner = PictureRepository.GetMinistryDefaultBanner();
                ViewBag.BannerID = banner.pictureID;
            }

            //pictures
            IEnumerable<picture> pictures = PictureRepository.GetPictureByMinistry(ministryID).Take(30);
            ViewBag.Pictures = pictures;
            //videos
            IEnumerable<video> videos = VideoRepository.GetVideoByMinistry(ministryID, beginDate, endDate);
            ViewBag.Videos = videos;

            if (ministry.PageStyleID != 0)
            {
                ministry.PageStyle = ConstantRepository.GetConstantID((int)ministry.PageStyleID).Value2;
            }

            //calendar dates
            int year = DateTime.Now.Year;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;

            //announcement dates
            DateTime AnnEndDate = DateTime.Now;
            DateTime AnnBeginDate = beginDate.AddDays(-90);
            ViewBag.AnnouncementBeginDate = AnnBeginDate;
            ViewBag.AnnouncementEndDate = AnnEndDate;
            IEnumerable<announcement> AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, AnnBeginDate, AnnEndDate);
            ViewBag.AnnouncementRecordCount = AnnouncementList.Count();
            //ministry tables
            IEnumerable<listtable> ListTable = ListTableRepository.GetListByMinistry(ministryID);
            ViewBag.TableRecordCount = ListTable.Count();
            ViewBag.Tables = ListTable;

            ViewBag.IsRental = false;
            if (ministry.CodeDesc == "Rental")
            {
                ViewBag.IsRental = true;
            }

            return PartialView(ministry);
        }

        public ActionResult GetMinistryPage2(int ministryID = 0, int CalendarBannerID = 0)
        {
            ViewBag.BannerEvent = null;
            ViewBag.isCalendarBannerEvent = false;
            if (CalendarBannerID > 0)
            {
                ViewBag.isCalendarBannerEvent = true;
                calendar c = CalendarRepository.GetCalendarByID(CalendarBannerID);
                if (c.DocumentID != null)
                {
                    c.document = DocumentRepository.GetDocumentByID((int)c.DocumentID);
                }
                c.picture = PictureRepository.GetPictureByID((int)c.PictureID);
                ViewBag.BannerEvent = c;
                ministryID = c.ministryID;
            }

            ministry ministry;
            if (ministryID == 0)
            {
                ministry = MinistryRepository.GetMainChurchMinistry();
            }
            else
            {
                ministry = MinistryRepository.GetMinistryByID(ministryID);
            }
            ViewBag.MinistryID = ministry.ministryID;
            DateTime beginDate = DateTime.Now;
            DateTime endDate = beginDate.AddDays(60);
            IEnumerable<calendar> currentCalendar = CalendarRepository.GetCalendarByMinistryDate(ministryID,beginDate, endDate);
            foreach (calendar m in currentCalendar)
            {
                m.ministry = MinistryRepository.GetMinistryByID(m.ministryID);
            }
            ViewBag.CurrentCalendar = currentCalendar;
            GetData();

            IEnumerable<announcement> announcements = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, beginDate, endDate);
            ViewBag.Announcements = announcements;

            IEnumerable<document> documents = DocumentRepository.GetDocumentByMinistry(ministryID);
            ViewBag.Document = documents;

            IEnumerable<listtable> tables = ListTableRepository.GetListByMinistry(ministryID);
            ViewBag.Tables = tables;

            //banner
            ViewBag.BannerID = null;
            if (ministry.PictureID != null)
            {
                picture banner = PictureRepository.GetPictureByID((int)ministry.PictureID);
                ViewBag.BannerID = banner.pictureID;
            }
            else
            {
                picture banner = PictureRepository.GetMinistryDefaultBanner();
                ViewBag.BannerID = banner.pictureID;
            }

            //pictures
            IEnumerable<picture> pictures = PictureRepository.GetPictureByMinistry(ministryID).Take(30);
            ViewBag.Pictures = pictures;
            //videos
            IEnumerable<video> videos = VideoRepository.GetVideoByMinistry(ministryID, beginDate, endDate);
            ViewBag.Videos = videos;

            if (ministry.PageStyleID != 0)
            {
                ministry.PageStyle = ConstantRepository.GetConstantID((int)ministry.PageStyleID).Value2;
            }

            //calendar dates
            int year = DateTime.Now.Year;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;

            //announcement dates
            DateTime AnnEndDate = DateTime.Now;
            DateTime AnnBeginDate = beginDate.AddDays(-90);
            ViewBag.AnnouncementBeginDate = AnnBeginDate;
            ViewBag.AnnouncementEndDate = AnnEndDate;
            IEnumerable<announcement> AnnouncementList = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, AnnBeginDate, AnnEndDate);
            ViewBag.AnnouncementRecordCount = AnnouncementList.Count();
            //ministry tables
            IEnumerable<listtable> ListTable = ListTableRepository.GetListByMinistry(ministryID);
            ViewBag.TableRecordCount = ListTable.Count();
            ViewBag.Tables = ListTable;

            ViewBag.IsRental = false;
            if (ministry.CodeDesc == "Rental")
            {
                ViewBag.IsRental = true;
            }

            return View(ministry);
        }

        public ActionResult GetMinistryByName(string ministryName)
        {
            ministry ministry = MinistryRepository.GetMinistryByCodeDesc(ministryName);
            return RedirectToAction("GetMinistryPage", new {ministry.ministryID});
        }

        public ActionResult DisplayPublicMinistryBanner()
        {
            GetData();
            IEnumerable<ministry> PublicMinistry = MinistryRepository.GetPublicMinistry();
            return PartialView(PublicMinistry);
        }

        public ActionResult DisplayBannerCalendarEvent(int calendarID)
        {
            calendar c = CalendarRepository.GetCalendarByID(calendarID);
            if (c.DocumentID != null)
            {
                c.document = DocumentRepository.GetDocumentByID((int)c.DocumentID);
            }
            c.picture = PictureRepository.GetPictureByID((int)c.PictureID);

            int year = DateTime.Now.Year;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;

            return View(c);
        }

 
        public ActionResult GetHomePageFull()
        {
            return PartialView();
        }

        public ActionResult GetHomePage3BoxTop()
        {
            return PartialView();
        }

        public ActionResult GetHomePage3BoxBottom()
        {
            return PartialView();
        }

        public ActionResult GetHomePage3BoxLeft()
        {
            return PartialView();
        }

        public ActionResult GetHomePage3BoxRight()
        {
            return PartialView();
        }

        public ActionResult HowToVideos()
        {
            return PartialView();
        }

        public ActionResult HowToInstructions()
        {
            return PartialView();
        }

        public string ContactUs(string Name, string Email, string Phone, string Subject, string comment, bool ContactMe)
        {
            try
            {
                ContactUs ContactUs = new ContactUs();
                ContactUs.Comment = comment;
                ContactUs.ContactMe = ContactMe;
                ContactUs.EmailAddress = Email;
                ContactUs.FullName = Name;
                ContactUs.PhoneNumber = Phone;
                ContactUs.Subject = Subject;

                EmailController EmailServer = new EmailController(ConstantRepository);
                EmailServer.ContactUs(ContactUs);
                //TempData["ContactUsEmailReturnMsg"] = "Email sent successfully";
                return "Email sent successfully";
            }
            catch (Exception ex)
            {
               // TempData["ContactUsEmailReturnMsg"] = "Error sending email";
                return "Error sending email";
            }
       
             
        }


    }
}
