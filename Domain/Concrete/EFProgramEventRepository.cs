using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFProgramEventRepository : IProgramEventRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private programevent record;
        private IEnumerable<programevent> list;

        private List<programevent> myRecords = new List<programevent>();

         public EFProgramEventRepository()
        {
            myRecords = context.programevents.ToList(); 
        }

         public void AddRecord(programevent Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetEventList()
        {
            Dictionary<int, string> EventList;
            EventList = context.programevents.Where(e => e.Status == "Active")
          //  .OrderBy(e => (string)e.Title + " (" + (string)e.When.ToShortDateString() + " "+ (string)e.Where + ")")
          //  .ToDictionary(e => (string)e.Title + " (" + (string)e.When.ToShortDateString() + " " + (string)e.Where + ")", e => (int)e.programEventID);
              .ToDictionary( e => (int)e.programEventID, e => string.Format("{0} ({1} {2})", e.Title,e.C_When.ToShortDateString(), e.C_Where));

            return (EventList);
        }

        public programevent GetEventByID(int eventID)
        {
            record = myRecords.FirstOrDefault(e => e.programEventID == eventID);
            return (record);
        }

        public IEnumerable<programevent> GetEventByGoal(int goalID)
        {
            list = myRecords.Where(e => e.goalID == goalID);
            return (list);
        }


        public IEnumerable<programevent> GetEventByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.C_When >= bDate.Date && e.C_When <= eDate.Date);
            return (list);
        }

        public IEnumerable<programevent> GetEventByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.C_When == aDate.Date);
            return (list);
        }

        public IEnumerable<programevent> GetEventByStatus(string status, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.Status == status && e.C_When >= bDate.Date && e.C_When <= eDate.Date);
            return (list);
        }

        public IEnumerable<programevent> GetEventByMinistry(int ministryID)
        {/*
            var ministryGoals = from g in context.ministries
                               join c in context.goals
                                on g.ministryID equals c.ministryID
                               where g.ministryID == ministryID
                               select c.goalID;

            list = from g in ministryGoals
                                join c in myRecords
                                 on g equals c.goalID
                                select c;

            */
            //Unable to create a constant value of type 'Domain.ministry'. Only primitive types or enumeration types are supported in this context.
            var goals = context.goals.Where(e => e.ministryID == ministryID && e.Status == "Active").Select(e => e.goalID).ToList();
            var events = context.programevents.Where(e => goals.Contains(e.goalID)).ToList();
            return (events);
        }


        public void DeleteRecord(programevent record)
        {
            myRecords.Remove(record);
            context.programevents.Remove(record);
            context.SaveChanges();
        }

    }
}
