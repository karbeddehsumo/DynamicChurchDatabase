using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IBankBalanceRepository
    {
        Dictionary<int,string> BankBalanceList();
        bankbalance GetByBankBalanceID(int BankBalanceID);
        IEnumerable<bankbalance> GetByBankAccount(int bankAccountID);
        IEnumerable<bankbalance> GetByDateRange(DateTime BeginDate, DateTime EndDate);
        bankbalance GetLastBankBlance(int bankAccountID);
        bool AddRecord(bankbalance record);
        void DeleteRecord(bankbalance Record);
    }
}
