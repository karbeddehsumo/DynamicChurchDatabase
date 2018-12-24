using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;


namespace Domain.Concrete
{
    public class EFBankBalanceRepository : IBankBalanceRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private bankbalance record;
        IEnumerable<bankbalance> List;

        private List<bankbalance> myRecords = new List<bankbalance>();

        public EFBankBalanceRepository()
        {
            myRecords = context.bankbalances.ToList(); 
        }

        public bool AddRecord(bankbalance Record)
        {
            //myRecords.Add(record);
            context.bankbalances.Add(Record);
            context.SaveChanges();
            return true;
        }


        public Dictionary<int, string> BankBalanceList()
        {
            Dictionary<int, string> BankAccountList;
            BankAccountList = context.bankaccounts
            .OrderBy(e => (string)e.BankName + " (" + (string)e.AccountNumber + ")")
            .ToDictionary(e => (int)e.bankAccountID, e => (string)e.BankName + " (" + (string)e.AccountNumber + ")");

            return (BankAccountList);
        }

        public bankbalance GetByBankBalanceID(int BankBalanceID)
        {
            record = myRecords.FirstOrDefault(e => e.BankBalanceID == BankBalanceID);
            return (record);
        }

        public IEnumerable<bankbalance> GetByBankAccount(int bankAccountID)
        {
            List = myRecords.Where(e => e.BankAccountID == bankAccountID).Take(10).OrderByDescending(e => e.BankBalanceID);
            return (List);
        }

        public IEnumerable<bankbalance> GetByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            List = myRecords.Where(e => e.BeginDate <= BeginDate.Date && e.EndDate <= EndDate.Date).OrderByDescending(e => e.BankBalanceID);
            return (List);
        }

        public bankbalance GetLastBankBlance(int bankAccountID)
        {
            record = myRecords.Where(e => e.BankAccountID == bankAccountID).OrderByDescending(e => e.EndDate).Take(1).SingleOrDefault();
            return (record);
        }
        /*
        public bool AddRecord(bankbalance record)
        {
            myRecords.Add(record);
            context.SaveChanges();
            return true;
        }
        */
        public void DeleteRecord(bankbalance Record)
        {
            myRecords.Remove(record);
            context.bankbalances.Remove(record);
            context.SaveChanges();
        }

    }
}
