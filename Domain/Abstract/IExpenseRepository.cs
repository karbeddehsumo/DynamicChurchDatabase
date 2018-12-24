using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IExpenseRepository
    {
        void AddRecord(expense Record);
        Dictionary<int, string> GetExpenseList();
        Dictionary<string, string> GetRecentPayeeList();
        IEnumerable<expense> GetMostRecentPayeeList();
        expense GetExpenseByID(int expenseID);
        IEnumerable<expense> GetExpenseByCategory(int categoryID, DateTime bDate, DateTime eDate);
        IEnumerable<subcategoryitem> GetSubCategoryChildren(int subcategoryID);
        IEnumerable<expense> GetExpenseByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate);
        IEnumerable<expense> GetExpenseByDate(DateTime aDate);
        IEnumerable<expense> GetExpenseByDateRange(DateTime bDate, DateTime eDate);
        expense GetExpenseByCheckNumber(string checkNumber);
        IEnumerable<expense> GetUnReconcile(int bankID);
        IEnumerable<expense> GetExpenseByStatus(string status);
        decimal GetExpenseTotalByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate);
        int GetLastExpenseRecordID();
        decimal GetExpenseTotalByDateRange(DateTime bDate, DateTime eDate);
        decimal GetPendingExpenseTotalByBankAccount(int BankAccountID);
        bool ExpenseByBankAccount(int BankAccountID);
        decimal GetTotalExpenseByCategoryIncludeChildren(int categoryID, DateTime bDate, DateTime eDate);
        IEnumerable<expense> GetExpenseChildren(int categoryID, DateTime bDate, DateTime eDate);
        void InitializeVariable();
        void DeleteRecord(expense record);

    }
}
