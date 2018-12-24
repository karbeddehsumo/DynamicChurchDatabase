using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Abstract
{
    public interface IMeetingRepository 
    {
        void AddRecord(meeting Record);
        Dictionary<int, string> GetMeetingList();
        Dictionary<int, string> GetMinistryMeetingList(int ministryID);
        meeting GetMeetingByID(int meetingID);
        IEnumerable<meeting> GetMeetingByDate(DateTime aDate);
        IEnumerable<meeting> GetMeetingByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<meeting> GetMeetingByMinistry(int ministryID);
        IEnumerable<meeting> GetMeetingByStatus(string status);
        void DeleteRecord(meeting record);
    }
}
