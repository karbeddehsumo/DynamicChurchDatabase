using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFCalendarRepository : ICalendarRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private calendar record;
        private IEnumerable<calendar> list;

        private List<calendar> myRecords = new List<calendar>();

        public EFCalendarRepository()
        {
            myRecords = context.calendars.ToList(); 
        }

        public bool AddRecord(calendar Record)
        {
            myRecords.Add(record);
            return true;
        }

        public Dictionary<int, string> GetCalendarList()
        {
            Dictionary<int, string> CalendarList;
            CalendarList = context.calendars.Where(e => e.Status == "Active")
                
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.calendarID, e => (string)e.Title);

            return (CalendarList);
        }

        public Dictionary<int, string> GetMinistryCalendarList(int ministryID)
        {
            Dictionary<int, string> CalendarList;
            CalendarList = context.calendars.Where(e => e.Status == "Active")
            .Where(e => e.ministryID == ministryID)
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.calendarID, e => (string)e.Title);

            return (CalendarList);
        }


        public calendar GetCalendarByID(int calendarID)
        {
            record = myRecords.FirstOrDefault(e => e.calendarID == calendarID);
            return (record);
        }

        public IEnumerable<calendar> GetCalendarByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID);
            return (list);
        }

        public IEnumerable<calendar> GetCalendarByMinistryDate(int ministryID, DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.ministryID == ministryID && e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date);
            var b = myRecords.Where(e => e.ministryID == ministryID && e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date);
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }



        public IEnumerable<calendar> GetCalendarByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.CalendarDate == aDate.Date);
            return (list);
        }

        public IEnumerable<calendar> GetCalendarByDateRange(DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date);
            var b = myRecords.Where(e => e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date);
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public IEnumerable<calendar> GetCalendarByDateRangeActive(DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == "Active");
            var b = myRecords.Where(e => e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == "Active");
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }


        public IEnumerable<calendar> GetCalendarByEvent(int eventType, DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.EventType == eventType && e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == "Active");
            var b = myRecords.Where(e => e.EventType == eventType && e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == "Active");
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public IEnumerable<calendar> GetCalendarByStatus(string status, DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == status);
            var b = myRecords.Where(e => e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == status);
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public IEnumerable<calendar> GetCalendarByLocation(string location, DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.Location == location && e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == "Active");
            var b = myRecords.Where(e => e.Location == location && e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == "Active");
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public IEnumerable<calendar> GetCalendarByMinistryLocation(int ministryID, string location, DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.ministryID == ministryID && e.Location == location && e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == "Active");
            var b = myRecords.Where(e => e.ministryID == ministryID && e.Location == location && e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == "Active");
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public IEnumerable<calendar> GetBannerCalendarEvent(DateTime bDate, DateTime eDate)
        {
            var a = myRecords.Where(e => e.CalendarDate >= bDate.Date && e.CalendarDate <= eDate.Date && e.Status == "Active" && e.DisplayBanner == true && e.PictureID != null);

            var b = myRecords.Where(e => e.CalendarEndDate >= bDate.Date && e.CalendarEndDate <= eDate.Date && e.Status == "Active" && e.DisplayBanner == true && e.PictureID != null);
            list = a.Concat(b);
            return (list.Distinct().OrderBy(e => e.CalendarDate).ThenBy(e => e.StartTime));
        }

        public calendar GetCalendarByDescription(string Description)
        {
            record = myRecords.Where(e => e.Description == Description).OrderBy(e => e.DateEntered).FirstOrDefault();
            return record;
        }
        
        public void DeleteRecord(calendar record)
        {
            myRecords.Remove(record);
            context.calendars.Remove(record);
            context.SaveChanges();
        }
    }
}
