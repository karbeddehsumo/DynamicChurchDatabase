using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMeetingRepository : IMeetingRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<meeting> list;
        private meeting record;

        private List<meeting> myRecords = new List<meeting>();

        public EFMeetingRepository()
        {
            myRecords = context.meetings.ToList(); 
        }

        public void AddRecord(meeting Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetMeetingList()
        {
            Dictionary<int, string> MeetingList;
            MeetingList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.meetingID, e => (string)e.Title);

            return (MeetingList);
        }

        public Dictionary<int, string> GetMinistryMeetingList(int ministryID)
        {
            Dictionary<int, string> MeetingList;
            MeetingList = myRecords
             .Where(e => e.ministryID == ministryID)
            .OrderBy(e => (string)e.Title)
            .ToDictionary( e => (int)e.meetingID, e => (string)e.Title);

            return (MeetingList);
        }


        public meeting GetMeetingByID(int meetingID)
        {
            record = myRecords.FirstOrDefault(e => e.meetingID == meetingID);
            return (record);
        }

        public IEnumerable<meeting> GetMeetingByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.meetingDate == aDate.Date);
            return (list);
        }

        public IEnumerable<meeting> GetMeetingByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.meetingDate >= bDate.Date && e.meetingDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<meeting> GetMeetingByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID);
            return (list);
        }

        public IEnumerable<meeting> GetMeetingByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public void DeleteRecord(meeting record)
        {
            myRecords.Remove(record);
            context.meetings.Remove(record);
            context.SaveChanges();
        }


    }
}
