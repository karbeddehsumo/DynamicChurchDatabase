using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFVisitorRepository : IVisitorRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private visitor record;
        private IEnumerable<visitor> list;

        private List<visitor> myRecords = new List<visitor>();

        public EFVisitorRepository()
        {
            myRecords = context.visitors.ToList(); 
        }

        public void AddRecord(visitor Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetVisitorList()
         {
             Dictionary<int, string> VisitorList;
             VisitorList = myRecords
             .OrderBy(e => e.Title + " " +  (string)e.FirstName + " " + e.LastName)
             .ToDictionary(e => (int)e.visitorID, e => e.Title + " " + (string)e.FirstName + " " + e.LastName);

             return (VisitorList);
         }

        public visitor GetVisitorByID(int visitorID)
        {
            record = myRecords.FirstOrDefault(e => e.visitorID == visitorID);
            return (record);
        }

        public IEnumerable<visitor> GetVisitorByCategory(int categoryID)
        {
            list = myRecords.Where(e => e.subCategoryID == categoryID);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByEventDate(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.EventDate >= bDate.Date && e.EventDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByBestContact(int contactTypeID)
        {
            list = myRecords.Where(e => e.BestContact == contactTypeID);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByCity(string city)
        {
            list = myRecords.Where(e => e.City == city);
            return (list);

        }

        public IEnumerable<visitor> GetVisitorByState(string state)
        {
            list = myRecords.Where(e => e.State == state);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByZip(string zip)
        {
            list = myRecords.Where(e => e.Zip == zip);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<visitor> GetVisitorByLastEventAttended(int EventID)
        {
            list = myRecords.Where(e => e.LastEventAttended == EventID);
            return (list);
        }

        public void DeleteRecord(visitor record)
        {
            context.visitors.Remove(record);
            context.SaveChanges();
        }
    }
}
