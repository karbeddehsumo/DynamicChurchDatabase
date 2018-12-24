using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFGoalRepository : IGoalRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private goal record;
        private IEnumerable<goal> list;

        private List<goal> myRecords = new List<goal>();

        public EFGoalRepository()
        {
            myRecords = context.goals.Where(e => e.Status == "Active").ToList(); 
        }

        public void AddRecord(goal Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetGoalList(int ministryID)
        {
            Dictionary<int, string> GoalList;
            GoalList = myRecords
            .Where(e => e.ministryID == ministryID && e.Status == "Active")
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.goalID, e => (string)e.Title);

            return (GoalList);
        }

        public goal GetGoalByID(int goalID)
        {
            record = myRecords.FirstOrDefault(e => e.goalID == goalID);
            return (record);
        }

        public IEnumerable<goal> GetGoalByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == "Active");
            return (list.OrderBy(e => e.EndDate));
        }

        public IEnumerable<goal> GetGoalAssignedTo(int memberID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.AssignedTo == memberID && e.BeginDate == bDate.Date && e.EndDate == eDate.Date);
            return (list.OrderBy(e => e.BeginDate));
        }

        public IEnumerable<goal> GetGoalBeginning(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.BeginDate == bDate.Date && e.EndDate == eDate.Date);
            return (list.OrderBy(e => e.BeginDate));
        }

        public IEnumerable<goal> GetGoalEnding(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.BeginDate == bDate.Date && e.EndDate == eDate.Date);
            return (list.OrderBy(e => e.EndDate));
        }

        public IEnumerable<goal> GetGoalByCompletionRatio(string Ratio)
        {
            if (Ratio.Contains("Quarter"))
            {
                list = myRecords.Where(e => e.CompletionRatio >= 0 && e.CompletionRatio <= 24);
            }
            else if (Ratio.IndexOf("Half") > 0)
            {
                list = myRecords.Where(e => e.CompletionRatio >= 25 && e.CompletionRatio <= 49);
            }
            else if (Ratio.Contains("Third"))
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


            return (list.OrderBy(e => e.CompletionRatio));
        }

        public IEnumerable<goal> GetGoalByStatus(string status)
        {
            list = context.goals.Where(e => e.Status == status);
            return (list.OrderBy(e => e.Status));
        }

        public IEnumerable<goal> GetGoalByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.BeginDate >= bDate.Date && e.BeginDate <= eDate.Date);
            return (list.OrderBy(e => e.ministryID).ThenBy(e => e.BeginDate));
        }

        public void DeleteRecord(goal record)
        {
            myRecords.Remove(record);
            context.goals.Remove(record);
            context.SaveChanges();
        }

    }
}
