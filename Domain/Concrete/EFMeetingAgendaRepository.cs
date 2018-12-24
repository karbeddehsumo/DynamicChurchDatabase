using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMeetingAgendaRepository : IMeetingAgendaRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<meetingagenda> list;
        private meetingagenda record;

        private List<meetingagenda> myRecords = new List<meetingagenda>();

        public EFMeetingAgendaRepository()
        {
            myRecords = context.meetingagendas.ToList(); 
        }

        public void AddRecord(meetingagenda Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetAgendaList(int meetingID)
        {
            Dictionary<int, string> AgendaList;
            AgendaList = context.meetingagendas
            .Where(e => e.meetingID == meetingID)
            .OrderBy(e => (string)e.Description)
            .ToDictionary(e => (int)e.meetingAgendaID, e => (string)e.Description);

            return (AgendaList);
        }

        public meetingagenda GetAgendaByID(int agendaID)
        {
            record = myRecords.FirstOrDefault(e => e.meetingAgendaID == agendaID);
            return (record);
        }

        public IEnumerable<meetingagenda> GetAgendaByMeeting(int meetingID)
        {
            list = myRecords.Where(e => e.meetingID == meetingID);
            return (list);
        }

        public IEnumerable<meetingagenda> GetAgendaByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<meetingagenda> GetAgendaByMinistry(int MinistryID, DateTime BeginDate, DateTime EndDate)
        {
            var meetings = context.meetings.Where(e => e.meetingID == MinistryID && e.meetingDate >= BeginDate.Date && e.meetingDate <= EndDate.Date);
            var IncludeIds = new HashSet<int>(meetings.Select(x => x.meetingID));
            var targetList = myRecords.Where(x =>IncludeIds.Contains(x.meetingID)).ToList();
            return (targetList.OrderByDescending(e => e.meetingAgendaID));
        }

        public IEnumerable<meetingagenda> GetAgendaByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            var meetings = context.meetings.Where(e => e.meetingDate >= BeginDate.Date && e.meetingDate <= EndDate.Date);
            var IncludeIds = new HashSet<int>(meetings.Select(x => x.meetingID));
            var targetList = myRecords.Where(x => IncludeIds.Contains(x.meetingID)).ToList();

            return (targetList.OrderByDescending(e => e.meetingAgendaID));
        }


        public void DeleteRecord(meetingagenda record)
        {
            myRecords.Remove(record);
            context.meetingagendas.Remove(record);
            context.SaveChanges();
        }
    }
}
