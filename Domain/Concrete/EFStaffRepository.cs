using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFStaffRepository : IStaffRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private staff record;
        private IEnumerable<staff> list;

        private List<staff> myRecords = new List<staff>();

         public EFStaffRepository()
        {
            myRecords = context.staffs.ToList(); 
        }

         public void AddRecord(staff Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetStaffList()
        {
            Dictionary<int, string> StaffList;
            StaffList = context.staffs.Where(e => e.Status == "Active") 
            .OrderBy(e => (string)e.LastName.Trim() + ", " + e.FirstName.Trim() + " " + e.MiddleName.Trim())
            .ToDictionary(e => (int)e.staffID, e => (string)e.FullNameTitle);

            return (StaffList);
        }

        public staff GetStaffByID(int staffId)
        {
            record = myRecords.FirstOrDefault(e => e.staffID == staffId);
            return (record);
        }

        public IEnumerable<staff> GetStaffByHireDate(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.HireDate >= bDate.Date && e.HireDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<staff> GetStaffByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }


        public void DeleteRecord(staff record)
        {
            myRecords.Remove(record);
            context.staffs.Remove(record);
            context.SaveChanges();

        }

    }
}
