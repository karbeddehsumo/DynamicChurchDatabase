using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ITaskRepository
    {
        void AddRecord(task Record);
        Dictionary<int, string> GetTaskList(int goalID);
        task GetTaskByID(int taskID);
        IEnumerable<task> GetTaskByGoal(int goalID);
        IEnumerable<task> GetTaskByAssignTo(int memberID);
        IEnumerable<task> GetTaskByAssignDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<task> GetTaskByDueDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<task> GetTaskByRatio(string Ratio);
        IEnumerable<task> GetTaskByStatus(string Status);
        IEnumerable<task> GetTaskByMinistryDateRange(int ministryID, DateTime BeginDate, DateTime EndDate);
        void DeleteRecord(task record);
    }
}
