using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFBillRepository : IBillRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private bill record;
        private IEnumerable<bill> list;

        private List<bill> myRecords = new List<bill>();

        public EFBillRepository()
        {
            myRecords = context.bills.ToList(); 
        }

        public void AddRecord(bill Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int,string> GetBillList()
        {
            Dictionary<int,string> BillList;
            BillList = context.bills
            .OrderBy(e => (string)e.PayeeName + " (" + (string)e.AccountNumber + ")")
            .ToDictionary( e => (int)e.billID, e => (string)e.PayeeName + " (" + (string)e.AccountNumber + ")");

            return (BillList);
        }

        public bill GetBillByID(int billID)
        {
            record = myRecords.FirstOrDefault(e => e.billID == billID);
            return (record);
        }

        public IEnumerable<bill> GetBillByPayeeDateRange(int payeeID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.payeeID == payeeID && e.DateEntered >= bDate.Date && e.DateEntered <= eDate.Date);
            return (list);
        }

        public IEnumerable<bill> GetBillByDueDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.DateEntered >= bDate.Date && e.DateEntered <= eDate.Date);
            return (list);
        }

        public IEnumerable<bill> GetAllBill()
        {  
            list = myRecords.Where(e => e.Status == "New");
            return (list);
        }

        public IEnumerable<bill> GetBillByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public void DeleteRecord(bill record)
        {
            myRecords.Remove(record);
            context.bills.Remove(record);
            context.SaveChanges();
        }

    }
}
