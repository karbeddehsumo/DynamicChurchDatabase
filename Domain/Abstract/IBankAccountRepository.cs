using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IBankAccountRepository
    {
        void AddRecord(bankaccount Record);
        Dictionary<int,string> GetBankAccountList();
        bankaccount GetBankAccountByID(int bankAccountID);
        bankaccount GetBankAccountByNumber(string AccountNumber);
        IEnumerable<bankaccount> GetBankAccountByStatus(string status);
        IEnumerable<bankaccount> GetAllBankAccount();
        void DeleteRedord(bankaccount bankaccount);
    }
}
