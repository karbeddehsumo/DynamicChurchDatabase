using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPayeeRepository : IPayeeRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private payee record;
        private IEnumerable<payee> list;

        private List<payee> myRecords = new List<payee>();

         public EFPayeeRepository()
        {
            myRecords = context.payees.ToList(); 
        }

         public void AddRecord(payee Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetPayeeList()
        {
            Dictionary<int, string> PayeeList;
            PayeeList = myRecords
            .OrderBy(e => (string)e.PayeeName)
            .ToDictionary(e => (int)e.payeeID, e => (string)string.Format("{0} - ({1})",e.PayeeName,e.AccountNumber));

            return (PayeeList);
        }

        public Dictionary<int, string> GetPayeeListByType(string type)
        {
            int i = context.constants.FirstOrDefault(e => e.Category == "Payee Type" && e.Value1 == type).constantID;
            Dictionary<int, string> PayeeList;
            PayeeList = myRecords.Where(e => e.PayeeTypeID == i)
            .OrderBy(e => (string)e.PayeeName)
            .ToDictionary(e => (int)e.payeeID, e => (string)string.Format("{0} - ({1})", e.PayeeName, e.AccountNumber));

            return (PayeeList);
        }

        public Dictionary<int, string> GetPayeeListNonStaff()
        {
            int i = context.constants.FirstOrDefault(e => e.Category == "Payee Type" && e.Value1 != "Staff").constantID;
            Dictionary<int, string> PayeeList;
            PayeeList = myRecords.Where(e => e.PayeeTypeID == i)
            .OrderBy(e => (string)e.PayeeName)
            .ToDictionary(e => (int)e.payeeID, e => (string)string.Format("{0} - ({1})", e.PayeeName, e.AccountNumber));

            return (PayeeList);
        }
        public Dictionary<int, string> GetPayeeListStaff()
        {
            int i = context.constants.FirstOrDefault(e => e.Category == "Payee Type" && e.Value1 == "Staff").constantID;
            Dictionary<int, string> PayeeList;
            PayeeList = myRecords.Where(e => e.PayeeTypeID == i)
            .OrderBy(e => (string)e.PayeeName)
            .ToDictionary(e => (int)e.payeeID, e => (string)string.Format("{0} - ({1})", e.PayeeName, e.AccountNumber));

            return (PayeeList);
        }

        public Dictionary<string, string> GetPayeeFrequency()
        {
            Dictionary<string, string> PayeeList;
            PayeeList = myRecords
            .OrderBy(e => (string)e.PayeeName)
            .ToDictionary(e => (string)e.PayeeName, e => (string)e.Frequency);

            return (PayeeList);
        }

        public payee GetPayeeByID(int payeeID)
        {
            record = myRecords.FirstOrDefault(e => e.payeeID == payeeID);
            return (record);
        }

        public IEnumerable<payee> GetPayeeByBankAccount(int bankAccountID)
        {
            list = myRecords.Where(e => e.BankAccountID == bankAccountID);
            return (list);
        }

        public IEnumerable<payee> GetPayeeByCategory(int categoryID)
        {
            list = myRecords.Where(e => e.SubCategoryID == categoryID);
            return (list);
        }

        public payee GetPayeeByAccountNumber(string accountNumber)
        {
            record = myRecords.FirstOrDefault(e => e.AccountNumber == accountNumber);
            return (record);
        }

        public IEnumerable<payee> GetPayeeByFrequency()
        {
            int i = context.constants.FirstOrDefault(e => e.Category == "Payee Type" && e.Value1 == "Utility").constantID;
            list = myRecords.Where(e => e.Status == "Active" && e.PayeeTypeID == i);
            foreach (var j in list)
            {
                j.LastPaymentDate = GetPayeeLastPayment(j.SubCategoryID);
            }

            return (list);
        }

        private DateTime GetPayeeLastPayment(int subCategoryID)
        {
            var aDate = DateTime.Today ;
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                if (context.expenses.Where(e => e.subCategoryID == subCategoryID).Count() == 0)
                {
                    return aDate;
                }
                else
                {
                   aDate = context.expenses.Where(e => e.subCategoryID == subCategoryID).OrderByDescending(e => e.ExpenseDate).Take(1).SingleOrDefault().ExpenseDate;
                }
            }
            return (aDate);
        }

        public IEnumerable<payee> GetPayeeByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<payee> GetAllPayee()
        {
            return (myRecords);
        }

        public payee GetPayeeByName(string payee)
        {
            record = myRecords.FirstOrDefault(e => e.PayeeName == payee);
            return (record);
        }

        public void DeleteRecord(payee record)
        {
            myRecords.Remove(record);
            context.payees.Remove(record);
            context.SaveChanges();
        }

    }
}
