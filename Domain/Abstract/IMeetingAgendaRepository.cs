using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMeetingAgendaRepository
    {
        void AddRecord(meetingagenda Record);
        Dictionary<int, string> GetAgendaList(int meetingID);
        meetingagenda GetAgendaByID(int agendaID);
        IEnumerable<meetingagenda> GetAgendaByMeeting(int meetingID);
        IEnumerable<meetingagenda> GetAgendaByStatus(string status);
        IEnumerable<meetingagenda> GetAgendaByMinistry(int MinistryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<meetingagenda> GetAgendaByDateRange(DateTime BeginDate, DateTime EndDate);
        void DeleteRecord(meetingagenda record);
    }
}
