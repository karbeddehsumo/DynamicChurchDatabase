using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IGoalRepository
    {
        void AddRecord(goal Record);
        Dictionary<int,string> GetGoalList(int ministryID);
        goal GetGoalByID(int goalID);
        IEnumerable<goal> GetGoalByMinistry(int ministryID);
        IEnumerable<goal> GetGoalAssignedTo(int memberID, DateTime bDate, DateTime eDate);
        IEnumerable<goal> GetGoalBeginning(DateTime bDate, DateTime eDate);
        IEnumerable<goal> GetGoalEnding(DateTime bDate, DateTime eDate);
        IEnumerable<goal> GetGoalByCompletionRatio(string Ratio);
        IEnumerable<goal> GetGoalByStatus(string status);
        IEnumerable<goal> GetGoalByDateRange(DateTime bDate, DateTime eDate);
        void DeleteRecord(goal record);

    }
}
