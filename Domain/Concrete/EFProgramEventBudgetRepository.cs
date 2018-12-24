using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFProgramEventBudgetRepository : IProgramEventBudgetRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private programeventbudget record;
        private IEnumerable<programeventbudget> list;

        private List<programeventbudget> myRecords = new List<programeventbudget>();

        public EFProgramEventBudgetRepository()
        {
            myRecords = context.programeventbudgets.ToList(); 
        }

        public void AddRecord(programeventbudget Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetEventBudgetList(string status)
        {
            Dictionary<int, string> EventBudgetList;
            var parentBudget = from g in context.programeventbudgets.Where(e => e.Status == status)
                                 join c in context.programeventbudgetitems
                                  on g.ProgramEventBudgetID equals c.ParentItemID
                                 select g;

            //get budget parent only
            var IncludeIds = new HashSet<int>(parentBudget.Select(x => x.ProgramEventBudgetID));
            var targetList = myRecords.Where(x => IncludeIds.Contains(x.ProgramEventBudgetID)).ToList();

            EventBudgetList = targetList
            .OrderBy(e => (e.DueDate))
            .ToDictionary(e => (int)e.ProgramEventBudgetID, e => (string)e.Title);


            return (EventBudgetList);
        }

        public IEnumerable<programeventbudget> GetEventBudgetByEventID(int eventID)
        {
            list = myRecords.Where(e => e.ProgramEventID == eventID);
            return (list);
        }

        public programeventbudget GetEventBudgetByID(int ProgramEventBudgetID)
        {
            record = myRecords.FirstOrDefault(e => e.ProgramEventBudgetID == ProgramEventBudgetID);
            return (record);
        }

        public IEnumerable<programeventbudget> GetEventBudgetByDueDateRange(DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.DueDate >= BeginDate.Date && e.DueDate <= EndDate.Date);
            return (list);
        }

        public programeventbudget GetEventBudgetByCheckNumber(string checkNumber)
        {
            record = myRecords.FirstOrDefault(e => e.CheckNumber == checkNumber);
            return (record);
        }

        public IEnumerable<programeventbudget> GetEventBudgetByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public void DeleteRecord(programeventbudget record)
        {
            myRecords.Remove(record);
            context.programeventbudgets.Remove(record);
            context.SaveChanges();
        }

    }
}
