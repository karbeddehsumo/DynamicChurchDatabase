using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMinistryExpenseRepository
    {
        void AddRecord(ministryexpense Record);
        Dictionary<int, string> GetExpenseList();
        ministryexpense GetExpenseByID(int expenseID);
        IEnumerable<ministryexpense> GetExpenseByCategory(int ministryID, int categoryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<ministryexpense> GetExpenseByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<ministryexpense> GetExpenseByDateRange(DateTime BeginDate, DateTime EndDate);
        IEnumerable<ministryexpense> GetReconciledExpense(int ministryID, DateTime BeginDate, DateTime EndDate, bool Reconcile);
        void DeleteRecord(ministryexpense record);
    }
}
