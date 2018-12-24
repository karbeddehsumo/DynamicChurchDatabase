using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IProgramEventRepository
    {
        void AddRecord(programevent Record);
        Dictionary<int, string> GetEventList();
        programevent GetEventByID(int eventID);
        IEnumerable<programevent> GetEventByGoal(int goalID);
        IEnumerable<programevent> GetEventByMinistry(int ministryID);
        IEnumerable<programevent> GetEventByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<programevent> GetEventByDate(DateTime aDate);
        IEnumerable<programevent> GetEventByStatus(string status, DateTime bDate, DateTime eDate);
        void DeleteRecord(programevent record);

    }
}
