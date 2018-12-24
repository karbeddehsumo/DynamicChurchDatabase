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
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
    public class CalendarController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private ICalendarRepository CalendarRepository;
        private IDocumentRepository DocumentRepository;
        private IPictureRepository PictureRepository;
        private IProgramEventRepository ProgramEventRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public CalendarController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, 
            ICalendarRepository CalendarParam, IDocumentRepository DocumentParam, IPictureRepository PictureParam, IProgramEventRepository ProgramEventParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            CalendarRepository = CalendarParam;
            DocumentRepository = DocumentParam;
            PictureRepository = PictureParam;
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
        // GET: /Calendar/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            DateTime aDate = DateTime.Now.Date;
            ViewBag.BeginDate = aDate.ToShortDateString();
            ViewBag.EndDate = aDate.AddDays(30);
            return PartialView();
        }

        //
        // GET: /Calendar/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;

            DateTime aDate = DateTime.Now;
            ViewBag.BeginDate = aDate.ToShortDateString();
            ViewBag.EndDate = aDate.AddDays(60).ToShortDateString();
            GetData(ministryID);
            return PartialView();
        }

        //
        // GET: /Calendar/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Create(int ministryID = 0, string CallerType = "")
        {
            
            GetData(ministryID);
            if (ministryID > 0)
            {
                ViewBag.Ministry = "Ministry";
                return PartialView(new calendar { ministryID = ministryID, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive", PublicAccess = false });
            }
            else
            {
                return PartialView(new calendar { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Inactive", PublicAccess = false });

            }
        }

        //
        // POST: /Calendar/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(calendar calendar, string CallerType = "")
        {

            try
            {

                if (ModelState.IsValid)
                {


                    db.calendars.Add(calendar);
                    db.SaveChanges();
                    TempData["Message2"] = "New calendar record added.";
                    GetData();

                    if (CallerType == "Ministry")
                    {
                        return RedirectToAction("Details", new { ministryID = calendar.ministryID, CallerType = "Ministry" });
                    }
                    if (CallerType == "Officer")
                    {
                        DateTime aDate = DateTime.Now.Date;
                        ViewBag.BeginDate = aDate.ToShortDateString();
                        ViewBag.EndDate = aDate.AddDays(30);
                        return RedirectToAction("List", new { bDate = @ViewBag.BeginDate, eDate = @ViewBag.EndDate, codeID = calendar.ministryID, SearchCode = "Ministry" });
                    }
                    else
                    {
                        return RedirectToAction("Details", new { });
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding record to calendar";
            }
            GetData();

            return PartialView(calendar);
        }

        //
        // GET: /Calendar/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Edit(int CalendarID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnMethod = "", string ReturnSearchType = "", int ReturnCodeID = 0, string ReturnCodeName = "", string ReturnCallerType = "")
        {
            ViewBag.ReturnMethod = ReturnMethod;
            ViewBag.ReturnBeginDate = ReturnBeginDate;
            ViewBag.ReturnEndDate = ReturnEndDate;
            ViewBag.ReturnSearchType = ReturnSearchType;
            ViewBag.ReturnCodeID = ReturnCodeID;
            ViewBag.ReturnCodeName = ReturnCodeName;
            ViewBag.ReturnCallerType = ReturnCallerType;

            calendar calendar = CalendarRepository.GetCalendarByID(CalendarID);
            GetData(calendar.ministryID);
            ViewBag.StartTime = calendar.StartTime;
            ViewBag.EndTime = calendar.EndTime;
            return PartialView(calendar);
        }

        //
        // POST: /Calendar/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(calendar calendar)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();

            try
            {
                if (ModelState.IsValid)
                {
                    picture pic = new picture();
                    foreach (var image in calendar.files)
                    {
                        if (image != null)
                        {
                            pic.ImageMimeType = image.ContentType;
                            pic.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(pic.ImageData, 0, image.ContentLength);

                            pic.ministryID = 0;
                            pic.PictureDate = System.DateTime.Today;
                            pic.Status = "Active";
                            pic.Description = string.Format("Picture:{0}", calendar.Title);
                            pic.DateEntered = System.DateTime.Today;
                            pic.EnteredBy = User.Identity.Name.ToString();

                            db.pictures.Add(pic);
                            db.SaveChanges();
                            PictureRepository.AddRecord(pic);
                            calendar.PictureID = pic.pictureID;
                        }

                    }

                    if (calendar.Conductor == null) { calendar.Conductor = ""; }
                    if (calendar.Description == null) { calendar.Description = ""; }
                    db.Entry(calendar).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Calendar record update successfully.");
                    /*
                    if (calendar.ReturnMethod == "Ministry")
                    {
                        // return RedirectToAction("Details", new { ministryID = calendar.ministryID, CallerType = "Ministry" });
                    }
                    else if (calendar.ReturnMethod == "AdminList")
                    {
                        return RedirectToAction("ListAdmin", new { bDate = calendar.ReturnBeginDate, eDate = calendar.ReturnEndDate, SearchType = calendar.ReturnSearchType, codeID = calendar.ReturnCodeID, codeName = calendar.ReturnCodeName, CallerType = calendar.ReturnCallerType });
                    }
                    else
                    {
                        return RedirectToAction("Details", new { });
                    }
                     */
                    return Redirect(ReturnUrl); 

                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", calendar.Title);
            }
            GetData(calendar.ministryID);
            return PartialView(calendar);
        }

        //
        // GET: /Calendar/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Delete(int CalendarID)
        {
            ViewBag.CalendarID = CalendarID;
            calendar calendar = CalendarRepository.GetCalendarByID(CalendarID);
            return PartialView(calendar);
        }

        //
        // POST: /Calendar/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int CalendarID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnSearchType = "", int ReturnCodeID = 0, string ReturnCodeName = "", string ReturnCallerType = "", string ReturnMethod = "")
        {
            calendar calendar = CalendarRepository.GetCalendarByID(CalendarID);
            if (calendar.PictureID != null)
            {
                picture pic = PictureRepository.GetPictureByID((int)calendar.PictureID);
                PictureRepository.DeleteRecord(pic);
            }
            CalendarRepository.DeleteRecord(calendar);
            TempData["Message2"] = string.Format("Calendar record deleted successfully.");
            if (ReturnMethod == "Ministry")
            {
                return RedirectToAction("Details", new { ministryID = calendar.ministryID, CallerType = "Ministry" });
            }
            else if (calendar.ReturnMethod == "AdminList")
            {
                return RedirectToAction("ListAdmin", new { bDate = ReturnBeginDate, eDate = ReturnEndDate, SearchType = ReturnSearchType, codeID = ReturnCodeID, codeName = ReturnCodeName, CallerType = ReturnCallerType });
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

            /*
            Dictionary<int, string> EventList;
            EventList = ProgramEventRepository.GetEventList();
            ViewBag.EventList = new SelectList(EventList, "Key", "Value", id);
            */

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

            Dictionary<string, string> MeetingVenueList;
            MeetingVenueList = ConstantRepository.GetConstantByCategory("Church Venue");
            ViewBag.MeetingVenueList = new SelectList(MeetingVenueList, "Key", "Value", id);

            Dictionary<int, string> MinistryActivityList;
            MinistryActivityList = ConstantRepository.GetConstantByCategoryID("Ministry Activity");//GetConstantByMinistryActivity("None");
            ViewBag.MinistryActivityList = new SelectList(MinistryActivityList, "Key", "Value", id);

            Dictionary<string, string> timeIntervals = new Dictionary<string, string>();
            TimeSpan startTime = new TimeSpan(0, 0, 0);
            DateTime startDate = new DateTime(DateTime.MinValue.Ticks); // Date to be used to get shortTime format.
            for (int i = 0; i < 48; i++)
            {
                int minutesToBeAdded = 30 * i;      // Increasing minutes by 30 minutes interval
                TimeSpan timeToBeAdded = new TimeSpan(0, minutesToBeAdded, 0);
                TimeSpan t = startTime.Add(timeToBeAdded);
                DateTime result = startDate + t;
                timeIntervals.Add(result.ToShortTimeString(), result.ToShortTimeString());      // Use Date.ToShortTimeString() method to get the desired format                
            }
            ViewBag.TimeIncrementList = new SelectList(timeIntervals, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult ListAdmin(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string codeName = "", string CallerType = "")
        {
            GetData();
            ViewBag.ReturnBeginDate = bDate; //.ToShortDateString();
            ViewBag.ReturnEndDate = eDate; //.ToShortDateString();
            ViewBag.ReturnSearchType = SearchType;
            ViewBag.ReturnCodeID = codeID;
            ViewBag.ReturnCodeName = codeName;
            ViewBag.ReturnCallerType = CallerType;


            ViewBag.Heading = "Ministry Calendar";

            IEnumerable<calendar> CalendarList;

            if (SearchType == "MinistrySearch")
            {
                GetData(codeID);
                CalendarList = CalendarRepository.GetCalendarByMinistryDate(codeID, bDate, eDate);
                string ministryName = MinistryRepository.GetMinistryByID(codeID).MinistryName;
                ViewBag.Heading = string.Format("{0} Calendar", ministryName);
            }
            else if (SearchType == "StatusSearch")
            {
                CalendarList = CalendarRepository.GetCalendarByStatus(codeName, bDate, eDate);
            }
            else if (SearchType == "EventTypeSearch")
            {
                CalendarList = CalendarRepository.GetCalendarByEvent(codeID, bDate, eDate);
                ViewBag.Heading = "Event Calendar";
            }
            else if (SearchType == "LocationTypeSearch")
            {
                CalendarList = CalendarRepository.GetCalendarByLocation(codeName, bDate, eDate);
                ViewBag.Heading = "Event Calendar";
            }
            else
            {
                CalendarList = CalendarRepository.GetCalendarByDateRange(bDate.Date, eDate.Date);
            }

            ViewBag.RecordCount = CalendarList.Count();

            foreach (calendar c in CalendarList)
            {
                c.ministry = MinistryRepository.GetMinistryByID(c.ministryID);
            }

            return PartialView(CalendarList.OrderBy(e => e.CalendarDate));
        }

        public ActionResult List(DateTime bDate, DateTime eDate, int codeID = 0, string SearchCode = "", string Requestor = "")
        {
            ViewBag.MinistryID = codeID;
            ViewBag.Requestor = Requestor;
            IEnumerable<calendar> CalendarList;
            if (SearchCode == "Ministry")
            {
                CalendarList = CalendarRepository.GetCalendarByMinistryDate(codeID, bDate.Date, eDate.Date);
            }
            else
            {
                CalendarList = CalendarRepository.GetCalendarByDateRangeActive(bDate.Date, eDate.Date);
            }
            foreach (calendar c in CalendarList)
            {
                c.EventTypeDesc = ConstantRepository.GetConstantID(c.EventType).Value1;
                if (c.ministryID != 0)
                {
                c.ministry = MinistryRepository.GetMinistryByID(c.ministryID);
                }
            }

            ViewBag.RecordCount = CalendarList.Count();

            return PartialView(CalendarList);
        }


        /*
                public void MinistryActivity(string MinistryName)
                {
                    int id = 0;
                    Dictionary<string, string> MinistryActivityList;
                    MinistryActivityList = ConstantRepository.GetConstantByMinistryActivity(MinistryName);
                    MinistryActivityList.Add("Meeting","Meeting");
                    ViewBag.MinistryActivityList = new SelectList(MinistryActivityList, "Key", "Value", id);
                }
        */
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MinistryActivity(string MinistryName)
        {
            var activity = ConstantRepository.GetConstantByMinistryActivity(MinistryName).ToList();

            // return Json(ConstantRepository.GetConstantByMinistryActivity(MinistryName));
            return Json(activity);

        }

        public bool IsContantMinistryActivity(int ministryID, string EventType)
        {
            return (ConstantRepository.isMinistryActivity(ministryID, EventType));
        }

        public ActionResult DisplayCalendar(int calendarID)
        {
            calendar cal = CalendarRepository.GetCalendarByID(calendarID);
            cal.ministry = MinistryRepository.GetMinistryByID(cal.ministryID);
            if (cal.DocumentID != null)
            {
                cal.document = DocumentRepository.GetDocumentByID((int)cal.DocumentID);
            }
            return PartialView(cal);
        }

        public ActionResult GetBannerCalendarEvent(DateTime bDate, DateTime eDate)
        {
            IEnumerable<calendar> BannerCalendarEvents = CalendarRepository.GetBannerCalendarEvent(bDate,eDate);
            foreach (calendar c in BannerCalendarEvents)
            {
                if(c.DocumentID != null)
                {
                   c.document = DocumentRepository.GetDocumentByID((int)c.DocumentID);
                }
                c.picture = PictureRepository.GetPictureByID((int)c.PictureID);
            }
            return PartialView(BannerCalendarEvents);
        }



        public ActionResult FullCalendar()
        {

            return PartialView();
        }

        //[WebMethod]
        /*       public List<CalendarEvent> GetEvents()
               {
                   List<CalendarEvent> events = new List<CalendarEvent>();
                   events.Add(new CalendarEvent()
                   {
                       EventID = 1,
                       EventName = "EventName 1",
                       StartDate = DateTime.Now.ToString("MM-dd-yyyy"),
                       EndDate = DateTime.Now.AddDays(2).ToString("MM-dd-yyyy")
                   });
                   events.Add(new CalendarEvent()
                   {
                       EventID = 2,
                       EventName = "EventName 2",
                       StartDate = DateTime.Now.AddDays(4).ToString("MM-dd-yyyy"),
                       EndDate = DateTime.Now.AddDays(5).ToString("MM-dd-yyyy")
                   });
                   events.Add(new CalendarEvent()
                   {
                       EventID = 3,
                       EventName = "EventName 3",
                       StartDate = DateTime.Now.AddDays(10).ToString("MM-dd-yyyy"),
                       EndDate = DateTime.Now.AddDays(11).ToString("MM-dd-yyyy")
                   });
                   events.Add(new CalendarEvent()
                   {
                       EventID = 4,
                       EventName = "EventName 4",
                       StartDate = DateTime.Now.AddDays(22).ToString("MM-dd-yyyy"),
                       EndDate = DateTime.Now.AddDays(25).ToString("MM-dd-yyyy")
                   });
                   return events;
               }

       */
        //   [WebMethod(EnableSession = true)]
        /*
        public static string GetEvents(string userID)
        {
          
            IEnumerable<calendar> cal = CalendarRepository.
            List<calevent> eventList = db.calevents.ToList();

            // Select events and return datetime as sortable XML Schema style.
            var events = from ev in eventList
                         select new
                         {
                             id = ev.event_id,
                             title = ev.title,
                             info = ev.description,
                             start = ev.event_start.ToString("s"),
                             end = ev.event_end.ToString("s"),
                             user = ev.user_id
                         };

            // Serialize to JSON string.
            JavaScriptSerializer jss = new JavaScriptSerializer();
            String json = jss.Serialize(events);
            return json;
        }
       */
/*
        public JsonResult GetEvents()
        {
            //  List<SelectListItem> listItem = new List<SelectListItem>();
            DateTime bdate = Convert.ToDateTime("1/1/2015");
            DateTime edate = Convert.ToDateTime("12/31/2015");
            IEnumerable<calendar> cal = CalendarRepository.GetCalendarByDateRange(bdate, edate);
            var events = from ev in cal
                         select new
                         {
                             id = ev.calendarID,
                             title = ev.Title,
                             info = ev.Description,
                             start = ev.CalendarDate.ToString("s"),
                             end = ev.CalendarDate.ToString("s"),
                             user = ev.EnteredBy
                         };
            return Json(events, JsonRequestBehavior.AllowGet);
        }
*/
        public ActionResult GetEvents(double start, double end)
        {
            var fromDate = ConvertFromUnixTimestamp(start);
            var toDate = ConvertFromUnixTimestamp(end);

            //Get the events
            //You may get from the repository also
            var eventList = GetEventsList();

            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        private List<Events> GetEventsList()
        {
            List<Events> eventList = new List<Events>();

            Events newEvent = new Events
            {
                id = "1",
                title = "Event 1",
                start = DateTime.Now.AddDays(1).ToString("s"),
                end = DateTime.Now.AddDays(1).ToString("s"),
                allDay = false
            };


            eventList.Add(newEvent);

            newEvent = new Events
            {
                id = "1",
                title = "Event 3",
                start = DateTime.Now.AddDays(2).ToString("s"),
                end = DateTime.Now.AddDays(3).ToString("s"),
                allDay = false
            };

            eventList.Add(newEvent);

            return eventList;
        }
        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public ActionResult UpdateFixedBanner(string Description)
        {
             calendar c = CalendarRepository.GetCalendarByDescription(Description);
             return RedirectToAction("Edit", new { calendarID = c.calendarID, ReturnBeginDate = DateTime.Today.Date, ReturnEndDate = DateTime.Today.Date, ReturnCallerType = "FixedBanner" });

        }



    }  
}