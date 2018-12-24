using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMeetingNotesRepository : IMeetingNotesRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<meetingnote> list;
        private meetingnote record;

        private List<meetingnote> myRecords = new List<meetingnote>();

        public EFMeetingNotesRepository()
        {
            myRecords = context.meetingnotes.ToList(); 
        }

        public void AddRecord(meetingnote Record)
        {
            myRecords.Add(record);
        }

        public meetingnote GetMeetingNotesByID(int noteID)
        {
            record = myRecords.FirstOrDefault(e => e.meetingNoteID == noteID);
            return (record);
        }

        public meetingnote GetMeetingNotesByAgenda(int meetingAgendaID)
        {
            record = myRecords.FirstOrDefault(e => e.MeetingAgendaID == meetingAgendaID);
            return(record);
        }

        public IEnumerable<meetingnote> GetMeetingNotesByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<meetingnote> GetMeetingNotesByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            var meetings = context.meetings.Where(e => e.ministryID == ministryID && e.meetingDate >= BeginDate.Date && e.meetingDate <= EndDate.Date);
            var IncludeMeetingIds = new HashSet<int>(meetings.Select(x => x.meetingID));
            var agendas = context.meetingagendas.Where(x => IncludeMeetingIds.Contains(x.meetingID)).ToList();

            var IncludeAgendaIds = new HashSet<int>(agendas.Select(x => x.meetingAgendaID));
            var notes = myRecords.Where(x => IncludeAgendaIds.Contains(x.MeetingAgendaID)).ToList();

            return (notes.OrderByDescending(e => e.meetingNoteID));
        }

        public IEnumerable<meetingnote> GetMeetingNotesByMeeting(int MeetingID)
        {
            var agendas = context.meetingagendas.Where(x => x.meetingID == MeetingID).ToList();

            var IncludeAgendaIds = new HashSet<int>(agendas.Select(x => x.meetingAgendaID));
            var notes = myRecords.Where(x => IncludeAgendaIds.Contains(x.MeetingAgendaID)).ToList();

            return (notes.OrderByDescending(e => e.meetingNoteID));
        }

        public IEnumerable<meetingnote> GetMeetingNotesByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            var meetings = context.meetings.Where(e => e.meetingDate >= BeginDate.Date && e.meetingDate <= EndDate.Date);
            var IncludeMeetingIds = new HashSet<int>(meetings.Select(x => x.meetingID));
            var agendas = context.meetingagendas.Where(x => IncludeMeetingIds.Contains(x.meetingID)).ToList();

            var IncludeAgendaIds = new HashSet<int>(agendas.Select(x => x.meetingAgendaID));
            var notes = myRecords.Where(x => IncludeAgendaIds.Contains(x.MeetingAgendaID)).ToList();

            return (notes.OrderByDescending(e => e.MeetingAgendaID));
        }

        public void DeleteRecord(meetingnote record)
        {
            myRecords.Remove(record);
            context.meetingnotes.Remove(record);
            context.SaveChanges();
        }
    }
}
