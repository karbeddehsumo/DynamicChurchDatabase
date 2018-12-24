using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFBankAccountRepository : IBankAccountRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private bankaccount record;
        private IEnumerable<bankaccount> list;

        private List<bankaccount> myRecords = new List<bankaccount>();

        public EFBankAccountRepository()
        {
            myRecords = context.bankaccounts.ToList(); 
        }

        public void AddRecord(bankaccount Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int,string> GetBankAccountList()
        {
            Dictionary<int,string> BankAccountList;
            BankAccountList = myRecords
            .OrderBy(e => (string)e.BankName + " (" + (string)e.AccountNumber + ")")
            .ToDictionary(e => (int)e.bankAccountID, e => (string)e.BankName + " (" + (string)e.AccountNumber + ")");

            return (BankAccountList);
        }

        public bankaccount GetBankAccountByID(int bankAccountID)
        {
            record = myRecords.FirstOrDefault(e => e.bankAccountID == bankAccountID);
            return (record);
        }

        public bankaccount GetBankAccountByNumber(string AccountNumber)
        {
            record = myRecords.FirstOrDefault(e => e.AccountNumber == AccountNumber);
            return (record);
        }

        public IEnumerable<bankaccount> GetBankAccountByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<bankaccount> GetAllBankAccount()
        {
            return (myRecords);
        }

        public void DeleteRedord(bankaccount bankaccount)
        {
            myRecords.Remove(record);
            context.bankaccounts.Remove(bankaccount);
            context.SaveChanges();
        }
    }
}
