using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFResponsibilityRepository : IResponsibilityRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private responsibility record;
        private IEnumerable<responsibility> list;

        private List<responsibility> myRecords = new List<responsibility>();

        public EFResponsibilityRepository()
        {
            myRecords = context.responsibilities.ToList(); 
        }

        public void AddRecord(responsibility Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetResponsibilityList(int staffID = 0)
        {
            Dictionary<int, string> ResponsibilityList;
            ResponsibilityList = myRecords
            .Where(e => e.staffID == staffID)
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.responsibilityID, e => (string)e.Title);

            return (ResponsibilityList);
        }

        public responsibility GetResponsibilityByID(int responsibilityID)
        {
            record = myRecords.FirstOrDefault(e => e.responsibilityID == responsibilityID);
            return (record);
        }

        public responsibility GetStaffPrimaryResponsibility(int staffID)
        {
            record = myRecords.FirstOrDefault(e => e.staffID == staffID /* && e.Type == "Primary"*/);
            return (record);
        }

        public IEnumerable<responsibility> GetResponsibilityByStaff(int staffID)
        {
            list = myRecords.Where(e => e.staffID == staffID);
            return (list);
        }

        public IEnumerable<responsibility> GetResponsiblityByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.DateCreated >= bDate.Date && e.DateCreated <= eDate.Date);
            return (list);
        }
        public IEnumerable<responsibility> GetResponsibilityByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public void DeleteRecord(responsibility record)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
               // myRecords.Remove(record);
                responsibility aRecord = context.responsibilities.FirstOrDefault(e => e.responsibilityID == record.responsibilityID);
                context.responsibilities.Remove(aRecord);
                context.SaveChanges();
            }
        }

    }
}
