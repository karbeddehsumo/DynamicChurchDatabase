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
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class MinistryController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IGoalRepository GoalRepository;
        private IMeetingRepository MeetingRepository;
        private IAnnouncementRepository AnnouncementRepository;
        private ICalendarRepository CalendarRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMemberRepository MemberRepository;
        private IChildParentRepository ChildParentRepository;
        private IStoryRepository StoryRepository;
        private IPictureRepository PictureRepository;
        private IListtableRepository ListTableRepository;
        private IMinistryGroupRepository MinistryGroupRepository;
        private IDocumentRepository DocumentRepository;
        private IVideoRepository VideoRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MinistryController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IGoalRepository GoalParam, IMeetingRepository MeetingParam,
            IAnnouncementRepository AnnouncementParam, ICalendarRepository CalendarParam, IMinistryMemberRepository MinistryMemberParam, IMemberRepository MemberParam,
            IChildParentRepository ChildParentParam, IStoryRepository StoryParam, IPictureRepository PictureParam, IListtableRepository ListTableParam, IMinistryGroupRepository MinistryGroupParam,
            IDocumentRepository DocumentParam, IVideoRepository VideoParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            GoalRepository = GoalParam;
            MeetingRepository = MeetingParam;
            AnnouncementRepository =  AnnouncementParam;
            CalendarRepository = CalendarParam;
            MinistryMemberRepository = MinistryMemberParam;
            MemberRepository = MemberParam;
            ChildParentRepository = ChildParentParam;
            StoryRepository = StoryParam;
            PictureRepository = PictureParam;
            ListTableRepository = ListTableParam;
            MinistryGroupRepository = MinistryGroupParam;
            DocumentRepository = DocumentParam;
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

                    if ((user.role.Name == "Officer")) //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }

                    if ((user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor3 = true;
                    }
                }
            }
        }


        // GET: /Ministry/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Index()
        {
            GetData();
            return PartialView();
        }

        //
        // GET: /Ministry/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Details()
        {
            GetData();
            return PartialView();
        }

        //
        // GET: /Ministry/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Create()
        {
            GetData();
            return PartialView(new ministry { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", BriefDescription = "A brief description of the ministry goes here.", IsPublic = false, StoryCreateAccess = false, PictureCreateAccess = false, VideoCreateAccess = false, IsGroupMinistry = false, GroupMinistryID=0 });
        } 

        //
        // POST: /Ministry/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(ministry ministry)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }

                if (ModelState.IsValid)
                {

                    db.ministries.Add(ministry);
                    db.SaveChanges();
                    MinistryRepository.AddRecord(ministry);
                    TempData["Message2"] = "Ministry record added successfully.";
                    GetData();

                    if (ministry.GroupMinistryID > 0)
                    {
                        ministrygroup group = new ministrygroup();
                        group.ChildID = ministry.ministryID;
                        group.ParentID = (int) ministry.GroupMinistryID;
                        db.ministrygroups.Add(group);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding ministry";
            }
            GetData();
            return PartialView(ministry);
        }
        
        //
        // GET: /Ministry/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Edit(int MinistryID, string Code = "")
        {
            ViewBag.ReturnPage = Code;
            GetData();
            ministry ministry = MinistryRepository.GetMinistryByID(MinistryID);
            return PartialView(ministry);
        }

        //
        // POST: /Ministry/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ministry ministry, string ReturnCode)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                picture pic = new picture();
                foreach (var image in ministry.files)
                {
                    if (image != null)
                    {
                        pic.ImageMimeType = image.ContentType;
                        pic.ImageData = new byte[image.ContentLength];
                        image.InputStream.Read(pic.ImageData, 0, image.ContentLength);

                        pic.ministryID = 0;
                        pic.PictureDate = System.DateTime.Today;
                        pic.Status = "Active";
                        pic.Description = string.Format("Picture:{0}", ministry.MinistryName);
                        pic.PictureType = "Ministry Banner";
                        pic.DateEntered = System.DateTime.Today;
                        pic.EnteredBy = User.Identity.Name.ToString();

                        db.pictures.Add(pic);
                        db.SaveChanges();
                        PictureRepository.AddRecord(pic);
                        int NewPicID = pic.pictureID;
                         //delete old picture
                        if (ministry.PictureID != null)
                        {
                            pic = PictureRepository.GetPictureByID((int)ministry.PictureID);
                            if (pic != null)
                            {
                              PictureRepository.DeleteRecord(pic);
                            }
                        }
                        ministry.PictureID = NewPicID;
                    }

                }

                if (ministry.StoryCreateAccess == null) { ministry.StoryCreateAccess = false; }
                if (ministry.PictureCreateAccess == null) { ministry.PictureCreateAccess = false; }
                if (ministry.VideoCreateAccess == null) { ministry.VideoCreateAccess = false; }
                if (ministry.PageStyleID == null) { ministry.PageStyleID = 0; }

                if (ModelState.IsValid)
                {
                    if (ministry.GroupMinistryID > 0)
                    {
                        ministrygroup gMinistry = MinistryGroupRepository.GetMinistryGroupParent(ministry.ministryID);
                        if ((gMinistry != null) && (gMinistry.ParentID != ministry.GroupMinistryID))
                        {
                            MinistryGroupRepository.DeleteRecord(gMinistry);
                            ministrygroup group = new ministrygroup();
                            group.ChildID = ministry.ministryID;
                            group.ParentID = (int) ministry.GroupMinistryID;
                            db.ministrygroups.Add(group);
                            db.SaveChanges();
                        }
                        else
                        {
                            ministrygroup group = new ministrygroup();
                            group.ChildID = ministry.ministryID;
                            group.ParentID = (int) ministry.GroupMinistryID;
                            db.ministrygroups.Add(group);
                            db.SaveChanges();
                        }
                    }

                    db.Entry(ministry).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} Ministry data update successfully.", ministry.MinistryName);
                    GetData();
                    /*
                    if (ReturnCode == "MinistryPage")
                    {
                        return Content("Ministry data update successfully.");
                    }
                    else{
                    return RedirectToAction("Details");
                    }
                     */
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                //TempData["Message2"] = string.Format("Error editing {0} ministry data.", ministry.MinistryName);
                TempData["Message2"] = ex.Message.ToString();
               // return (ex.Message);
            }
            GetData();
            return PartialView(ministry);
        }

        //
        // GET: /Ministry/Delete/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Delete(int MinistryID)
        {
            ViewBag.MinistryID = MinistryID;
            ministry ministry = MinistryRepository.GetMinistryByID(MinistryID);
            return View(ministry);
        }

        //
        // POST: /Ministry/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MinistryID)
        {
            ministry ministry = MinistryRepository.GetMinistryByID(MinistryID);
            int goalCt = GoalRepository.GetGoalByMinistry(ministry.ministryID).Count();
            int meetingCt = MeetingRepository.GetMeetingByMinistry(ministry.ministryID).Count();
            int calendarCt = CalendarRepository.GetCalendarByMinistry(ministry.ministryID).Count();
            int storyCt = StoryRepository.GetStoryByMinistry(ministry.ministryID).Count();
            int pictureCt = PictureRepository.GetPictureByMinistry(ministry.ministryID).Count();
            int ListTableCt = ListTableRepository.GetListByMinistry(ministry.ministryID).Count();

            if ((goalCt > 0) || (meetingCt > 0) || (calendarCt > 0) || (storyCt > 0) || (pictureCt > 0) || (ListTableCt>0))
            {
                return RedirectToAction("Details");
            }
            MinistryRepository.DeleteRecord(ministry);
            return RedirectToAction("Details");
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

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> PublicMinistryList;
            PublicMinistryList = MinistryRepository.GetPublicMinistryList();
            ViewBag.PublicMinistryList = new SelectList(PublicMinistryList, "Key", "Value", id);

            Dictionary<string, string> DefaultMemberType;
            DefaultMemberType = ConstantRepository.GetConstantByCategory("Member Category");
            ViewBag.DefaultMemberType = new SelectList(DefaultMemberType, "Key", "Value", id);

            Dictionary<int, string> MinistryPageStyle;
            MinistryPageStyle = ConstantRepository.GetConstantByCategoryID("Ministry Page Style");
            ViewBag.MinistryPageStyle = new SelectList(MinistryPageStyle, "Key", "Value", id);

            Dictionary<int, string> GroupMinistryList;
            GroupMinistryList = MinistryRepository.GetGroupMinistryList();
            ViewBag.GroupMinistryList = new SelectList(GroupMinistryList, "Key", "Value", id);

        }

        public ActionResult List(string Status = "Active")
        {
            IEnumerable<ministry> MinistryList = MinistryRepository.GetMinistryByStatus(Status);
            ministrygroup GroupMinistry;
            ViewBag.RecordCount = MinistryList.Count();
            ministry ministry;
            foreach (ministry m in MinistryList)
            {
                GroupMinistry = MinistryGroupRepository.GetMinistryGroupParent(m.ministryID);
                if(GroupMinistry != null)
                {
                    ministry = MinistryRepository.GetParentMinistryByID(GroupMinistry.ParentID);
                    if (ministry != null)
                    {
                    m.ParentMinistryName = (string) ministry.MinistryName;               
                    }
                }
                if (m.PageStyleID > 0)
                {
                 m.PageStyle = ConstantRepository.GetConstantID((int)m.PageStyleID).Value1;
                }

            }
            return PartialView(MinistryList);
        }

        public ActionResult MyMinistry()
        {
           // ministry ministry = MinistryRepository.GetMinistryByID(ministryID);
           // ViewBag.MinistryName = ministry.MinistryName;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            IEnumerable<ministry> MyMinistryList = MinistryRepository.GetMyMinistries(memberID);
            IEnumerable<ministry> MyDefaultMinistryList = MinistryRepository.GetMyDefaultMinistries(memberID);
            IEnumerable<ministry> myMinistries = MyMinistryList.Concat(MyDefaultMinistryList).Distinct();

            return PartialView(myMinistries);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult ListAdmin()
        {
            IEnumerable<ministry> MinistryList = MinistryRepository.GetMinistryByStatus("Active");
            ViewBag.RecordCount = MinistryList.Count();
            return View(MinistryList);
        }

        public ActionResult MinistryView(int ministryID)
        {
            DateTime BeginDate = DateTime.Now;
            DateTime EndDate = DateTime.Today.AddDays(365);
            ViewBag.BDate = BeginDate;
            ViewBag.EDate = EndDate;
            ministry ministry = MinistryRepository.GetMinistryByID(ministryID);
            ministry.Goals =  GoalRepository.GetGoalByMinistry(ministryID).ToList();
            ministry.Meetings = MeetingRepository.GetMeetingByMinistry(ministryID).ToList();
            ministry.Announcements = AnnouncementRepository.GetAnnouncementByMinistry(ministryID, BeginDate, EndDate).ToList();
            ministry.Calendars = CalendarRepository.GetCalendarByMinistry(ministryID).ToList();
            return PartialView(ministry);
        }

        public ActionResult AdminPage(string Page = "")
        {
            ViewBag.Page = Page;
            GetData();
            return PartialView();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult MinistryPageAdmin(int ministryID)
        {
            GetData();
            DateTime aDate = DateTime.Now;
            int days = aDate.DayOfYear;
            ViewBag.BeginDate = aDate.AddDays(-days + 1).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();
            ministry ministry = MinistryRepository.GetMinistryByID(ministryID);

            int id= 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            //calendar dates
            int year = DateTime.Now.Year;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;
            

            return PartialView(ministry);
        }

        public ActionResult MinistryPage(int ministryID, string code = "")
        {
            GetData();
            DateTime aDate = DateTime.Now;
            int days = aDate.DayOfYear;
            ViewBag.BeginDate = aDate.AddDays(-days + 1).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();
            ministry ministry = MinistryRepository.GetMinistryByID(ministryID);

            int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            //calendar dates
            int year = DateTime.Now.Year;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;

            ViewBag.Superviosr = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "Admin") || (user.role.Name == "Officer") || (user.role.Name == "WebMaster"))
                {
                    ViewBag.Superviosr = true;
                }
            }
             
             ViewBag.MinistryType = "";
             if ((ministry.CodeDesc == "Children") || (ministry.CodeDesc == "Youth"))
             {
                 ViewBag.MinistryType = "Minor";

             }
 
            ViewBag.StoryAccess = ministry.StoryCreateAccess;
            ViewBag.PictureAccess = ministry.PictureCreateAccess;
            ViewBag.StoryAccess = ministry.StoryCreateAccess;
            if (code != "MyMinistry")
            {
                ViewBag.StoryAccess = true;
                ViewBag.PictureAccess = true;
                ViewBag.StoryAccess = true;
            }

            return View(ministry);
           // return PartialView(ministry);
        }

        public ActionResult MinistryMember(int ministryID, int memberID,string officeTitle, DateTime membershipDate)
        {
            ministrymember member = new ministrymember();
            member.memberID = memberID;
            member.ministryID = ministryID;
            if (officeTitle.Length == 0)
            {
                member.OfficeTitle = " ";
            }
            else
            {
              member.OfficeTitle = officeTitle;
            }
            member.MembershipDate = membershipDate.Date;
            member.EnteredBy = User.Identity.Name.ToString();
            member.DateEntered = DateTime.Now.Date;
            member.SortOrder = 1;
            db.ministrymembers.Add(member);
            db.SaveChanges();
            MinistryMemberRepository.AddRecord(member);

            int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            return PartialView();

        }

        public ActionResult StoryCenteredPageLayout()
        {
            return View();
        }

        public ActionResult PictureCenteredPageLayout()
        {
            return View();
        }

        public ActionResult VideoCenteredPageLayout()
        {
            return View();
        }

        public ActionResult DefaultPageLayout()
        {
            return View();
        }

        public ActionResult MinistryIndexView(int ministryID)
        {
            ministry ministry = MinistryRepository.GetMinistryByID(ministryID);
            return PartialView(ministry);
        }

        public ActionResult MemberView(int ministryID,string Requestor ="")
        {
            GetData();
            ViewBag.MinistryID = ministryID;
            ViewBag.Requestor = Requestor;

            DateTime aDate = DateTime.Now;
            ViewBag.aDate = aDate.ToShortDateString();

             int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            ViewBag.MinistryName = MinistryRepository.GetMinistryByID(ministryID).MinistryName;

            IEnumerable<ministrymember> MinistryMembers = MinistryMemberRepository.GetMemintryMemberByMinistry(ministryID).OrderByDescending(e=>e.SortOrder);
            foreach (ministrymember mm in MinistryMembers)
            {
                mm.member = MemberRepository.GetMemberByID(mm.memberID);
            }
            ViewBag.Members = MinistryMembers;

            return PartialView();
        }

        public ActionResult ParentView(int ministryID)
        {
            GetData();
            ViewBag.MinistryID = ministryID;

            DateTime aDate = DateTime.Now;
            ViewBag.aDate = aDate.ToShortDateString();

            ministry ministry = MinistryRepository.GetMinistryByID(ministryID);

            int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            ViewBag.MinistryType = "";
            if ((ministry.CodeDesc == "Children") || (ministry.CodeDesc == "Youth"))
            {
                ViewBag.MinistryType = "Minor";
                IEnumerable<member> minorList = MemberRepository.GetMemberListCategory(ministry.CodeDesc);
                Dictionary<int, string> ParentChildList = new Dictionary<int, string>();
                member child, parent;
                string ParentName = "", parentChildName = "", FamilyName = "";
                foreach (member c in minorList)
                {
                    child = MemberRepository.GetMemberByID(c.memberID);
                    //IEnumerable<member> parents = MemberRepository.GetChildParent(c.memberID);
                    IEnumerable<member> parents = ChildParentRepository.GetParents(c.memberID);
                    if (parents == null)
                    {
                        continue;
                    }
                    ParentName = "";
                    foreach (member p in parents)
                    {
                        parent = MemberRepository.GetMemberByID(p.memberID);
                        if (ParentName == "")
                        {
                            ParentName = string.Format("{0}", parent.FirstNameTitle);
                            FamilyName = parent.LastName;
                        }
                        else
                        {
                            ParentName += string.Format(" & {0}", parent.FirstNameTitle);
                            FamilyName = parent.LastName;
                        }

                    }
                    if (parents.Count() > 0)
                    {
                        parentChildName = string.Format("{0} ({1})", c.FullName, ParentName + " " + FamilyName);
                        ParentChildList.Add(c.memberID, parentChildName);
                    }
                    ViewBag.ParentChildList = new SelectList(ParentChildList, "Key", "Value", id);
                }
            }


            return PartialView();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult RemoveParent(int childID = 0, int MinistryID = 0)
        {
            ViewBag.MinistryID = MinistryID;
            IEnumerable<childparent> ChildParentList = ChildParentRepository.GetParentRecords(childID);
            foreach (childparent m in ChildParentList)
            {
                ChildParentRepository.DeleteRecord(m);
            }

            return PartialView();
        }

        public ActionResult MinistryHomePage()
        {
            return PartialView();
        }

        public ActionResult MinistryViewAll(int ministryID = 0, bool isMember = false, bool isPublic = false)
        {
            ViewBag.IsMember = isMember;
            ViewBag.IsPublic = isPublic;
            ministry ministry;
            if (ministryID == 0)
            {
                ministry = MinistryRepository.GetMainChurchMinistry();
            }
            else
            {
                ministry = MinistryRepository.GetMinistryByID(ministryID);
            }
            DateTime beginDate = DateTime.Now;
            DateTime endDate = beginDate.AddDays(60);
            IEnumerable<calendar> currentCalendar = CalendarRepository.GetCalendarByDateRange(beginDate, endDate);
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

        public ActionResult GetMinistryPage(int ministryID = 0, bool isPublic = false)
        {
            ViewBag.IsPublic = isPublic;
            ministry ministry;
            if (ministryID == 0)
            {
                ministry = MinistryRepository.GetMainChurchMinistry();
            }
            else
            {
                ministry = MinistryRepository.GetMinistryByID(ministryID);
            }
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

            if ((ministry.PageStyleID != 0)&&(ministry.PageStyleID != null))
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

        public void DeleteMinistryMember(int ministryID,int memberID)
        {
            ministrymember member =  MinistryMemberRepository.GetMinistryMemberByID(ministryID, memberID);
            if (member != null)
            {
               MinistryMemberRepository.DeleteRecord(member);
            }
        }

        public ActionResult AddMinistryMember(int ministryID)
        {
            GetData();
            ViewBag.MinistryID = ministryID;

            DateTime aDate = DateTime.Now;
            ViewBag.aDate = aDate.ToShortDateString();

            int id = 0;
            Dictionary<int, string> MemberRoster;
            MemberRoster = MinistryMemberRepository.GetMinistryMemberList(ministryID);
            ViewBag.MemberRoster = new SelectList(MemberRoster, "Key", "Value", id);

            ViewBag.MinistryName = MinistryRepository.GetMinistryByID(ministryID).MinistryName;



            return PartialView();
        }

        public ActionResult MinistryMemberList(int ministryID, string Requestor = "")
        {
            ViewBag.Requestor = Requestor;
            IEnumerable<ministrymember> MinistryMembers = MinistryMemberRepository.GetMemintryMemberByMinistry(ministryID).OrderByDescending(e => e.SortOrder);
            foreach (ministrymember mm in MinistryMembers)
            {
                mm.member = MemberRepository.GetMemberByID(mm.memberID);
            }
            ViewBag.Members = MinistryMembers;

            return PartialView();
        }



    }

    
}