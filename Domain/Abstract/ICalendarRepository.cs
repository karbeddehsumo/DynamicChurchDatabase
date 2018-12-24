using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ICalendarRepository
    {
        Dictionary<int, string> GetCalendarList();
        Dictionary<int, string> GetMinistryCalendarList(int ministryID);
        calendar GetCalendarByID(int calendarID);
        IEnumerable<calendar> GetCalendarByMinistry(int ministryID);
        IEnumerable<calendar> GetCalendarByMinistryDate(int ministryID, DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByDate(DateTime aDate);
        IEnumerable<calendar> GetCalendarByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByDateRangeActive(DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByEvent(int eventType, DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByStatus(string status, DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByLocation(string location, DateTime bDate, DateTime eDate);
        IEnumerable<calendar> GetCalendarByMinistryLocation(int ministryID, string location, DateTime bDate, DateTime eDate);
        bool AddRecord(calendar record);
        IEnumerable<calendar> GetBannerCalendarEvent(DateTime bDate, DateTime eDate);
        calendar GetCalendarByDescription(string Description);
        void DeleteRecord(calendar record);
    }
}
