using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMinistryExpenseRepository : IMinistryExpenseRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private ministryexpense record;
        private IEnumerable<ministryexpense> list;

        private List<ministryexpense> myRecords = new List<ministryexpense>();

        public EFMinistryExpenseRepository()
        {
            myRecords = context.ministryexpenses.ToList(); 
        }

        public void AddRecord(ministryexpense Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int, string> GetExpenseList()
        {
            Dictionary<int, string> ExpenseList;
            ExpenseList = myRecords
            .OrderBy(e => e.ExpenseDate)
            .ToDictionary(e => (int)e.ministryExpenseID, e => (string)e.Title);

            return (ExpenseList);
        }

        public ministryexpense GetExpenseByID(int expenseID)
        {
            record = myRecords.FirstOrDefault(e => e.ministryExpenseID == expenseID);
            return (record);
        }

        public IEnumerable<ministryexpense> GetExpenseByCategory(int ministryID, int categoryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.subCategoryID == categoryID && e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<ministryexpense> GetExpenseByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date);
            return (list);
        }
        public IEnumerable<ministryexpense> GetExpenseByTitle(string Title, int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.Title == Title && e.ministryID == ministryID && e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date);
            return (list);
        }
        public IEnumerable<ministryexpense> GetExpenseByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<ministryexpense> GetReconciledExpense(int ministryID, DateTime BeginDate, DateTime EndDate, bool Reconcile)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date && e.Reconcile == Reconcile);
            return (list);
        }

        public void DeleteRecord(ministryexpense record)
        {
            myRecords.Remove(record);
            context.ministryexpenses.Remove(record);
            context.SaveChanges();
        }
    }
}
