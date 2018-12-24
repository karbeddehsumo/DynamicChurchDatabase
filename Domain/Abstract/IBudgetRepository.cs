using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IBudgetRepository
    {
        void AddRecord(budget Record);
      //  Dictionary<string, int> GetBudgetList();
        budget GetBudgetByID(int budgetID);
        budget GetBudgetByCategory(int subCategoryID);
        IEnumerable<budget> GetBudgetByStatus(string status);
        IEnumerable<budget> GetBudgetByYear(int BudgetYear);
        IEnumerable<budget> GetBudgetByTypeYear(int BudgetYear, string Type);
        void DeleteRecord(budget record);

    }
}
