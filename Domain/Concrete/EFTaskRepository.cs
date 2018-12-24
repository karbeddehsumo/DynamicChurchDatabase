using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFTaskRepository : ITaskRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private task record;
        private IEnumerable<task> list;

        private List<task> myRecords = new List<task>();

        public EFTaskRepository()
        {
            myRecords = context.tasks.ToList(); 
        }

        public void AddRecord(task Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetTaskList(int goalID)
        {
            Dictionary<int, string> TaskList;
            TaskList = myRecords.Where(e => e.goalID == goalID)
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.taskID, e => (string)e.Title);

            return (TaskList);
        }

        public task GetTaskByID(int taskID)
        {
            record = myRecords.FirstOrDefault(e => e.taskID == taskID);
            return (record);
        }

        public IEnumerable<task> GetTaskByGoal(int goalID)
        {
            list = myRecords.Where(e => e.goalID == goalID);
            return (list);
        }

        public IEnumerable<task> GetTaskByAssignTo(int memberID)
        {
            list = myRecords.Where(e => e.AssignTo == memberID);
            return (list);
        }

        public IEnumerable<task> GetTaskByAssignDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.AssignDate >= bDate && e.AssignDate <= eDate);
            return (list);
        }

        public IEnumerable<task> GetTaskByDueDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.DueDate >= bDate && e.DueDate <= eDate);
            return (list);
        }

        public IEnumerable<task> GetTaskByRatio(string Ratio)
        {

            if (Ratio.Contains("Quarter"))
            {
                list = myRecords.Where(e => e.CompletionRatio >= 0 && e.CompletionRatio <= 24);
            }
            else if (Ratio.IndexOf("Half") > 0)
            {
                list = myRecords.Where(e => e.CompletionRatio >= 25 && e.CompletionRatio <= 49);
            }
            else if (Ratio.Contains("Thirds"))
            {
                list = myRecords.Where(e => e.CompletionRatio >= 50 && e.CompletionRatio <= 74);
            }
            else if (Ratio.Contains("Full"))
            {
                list = myRecords.Where(e => e.CompletionRatio >= 75 && e.CompletionRatio <= 99);
            }
            else if (Ratio.Contains("Complete"))
            {
                list = myRecords.Where(e => e.Status == "Complete");
            }


            return (list);
        }

        public IEnumerable<task> GetTaskByStatus(string Status)
        {
            list = myRecords.Where(e => e.Status == Status);
            return (list);
        }

        public IEnumerable<task> GetTaskByMinistryDateRange(int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            var goals = from m in context.ministries.Where(e => e.Status == "Active")
                        join g in context.goals
                         on m.ministryID equals g.ministryID
                        select g;

            var goalIds = new HashSet<int>(goals.Select(x => x.goalID));
            var taskList = myRecords.Where(x => goalIds.Contains(x.goalID)).ToList();
            return (taskList);
        }

        public void DeleteRecord(task record)
        {
            myRecords.Remove(record);
            context.tasks.Remove(record);
            context.SaveChanges();
        }

    }
}
