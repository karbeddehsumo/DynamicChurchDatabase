using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Abstract
{
    public interface IMeetingNotesRepository 
    {
        void AddRecord(meetingnote Record);
        meetingnote GetMeetingNotesByID(int noteID);
        meetingnote GetMeetingNotesByAgenda(int meetingAgendaID);
        IEnumerable<meetingnote> GetMeetingNotesByStatus(string status);
        IEnumerable<meetingnote> GetMeetingNotesByMinistry(int ministryID,DateTime BeginDate, DateTime EndDate);
        IEnumerable<meetingnote> GetMeetingNotesByMeeting(int MeetingID);
        IEnumerable<meetingnote> GetMeetingNotesByDateRange(DateTime BeginDate, DateTime EndDate);
        void DeleteRecord(meetingnote record);
    }
}
