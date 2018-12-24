using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IIncomeRepository
    {
        void AddRecord(income Record);
        Dictionary<int, string> GetIncomeList();
        income GetIncomeByID(int incomeID);
        IEnumerable<income> GetIncomeByCategory(int categoryID, DateTime bDate, DateTime eDate);
        IEnumerable<income> GetIncomeByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate);
        IEnumerable<income> GetIncomeByDate(DateTime aDate);
        IEnumerable<income> GetIncomeByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<income> GetBudgetIncomeByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<income> GetIncomeByStatus(string status);
        decimal GetIncomeTotalByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate);
        decimal GetIncomeTotalByDateRange(DateTime bDate, DateTime eDate);
        int GetLastIncomeRecordID();
        decimal GetPendingIncomeTotalByBankAccount(int BankAccountID);
        bool IncomeByBankAccount(int BankAccountID);
        decimal GetTotalIncomeByCategory(int categoryID, DateTime bDate, DateTime eDate);
        IEnumerable<income> GetIncomeChildren(int categoryID, DateTime bDate, DateTime eDate);
        void InitializeVariable();
        void DeleteRecord(income record);
        

    }
}
