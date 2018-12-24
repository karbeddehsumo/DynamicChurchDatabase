using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFBudgetRepository : IBudgetRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private budget record;
        private IEnumerable<budget> list;

        private List<budget> myRecords = new List<budget>();

        public EFBudgetRepository()
        {
            myRecords = context.budgets.ToList(); 
        }

        public void AddRecord(budget Record)
        {
            myRecords.Add(record);
        }

   /*     public Dictionary<string, int> GetBudgetList()
        {
            Dictionary<string, int> BudgetList;
            BudgetList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (string)e.Title, e => (int)e.budgetID);

            return (BudgetList);
        }
*/
        public budget GetBudgetByID(int budgetID)
        {
            record = myRecords.FirstOrDefault(e => e.budgetID == budgetID);
            return(record);
        }

        public budget GetBudgetByCategory(int subCategoryID)
        {
            record = myRecords.FirstOrDefault(e => e.SubCategoryID == subCategoryID);
            return (record);
        }

        public IEnumerable<budget> GetBudgetByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<budget> GetBudgetByYear(int BudgetYear)
        {
            list = myRecords.Where(e => e.BudgetYear == BudgetYear);
            return (list);
        }

        public IEnumerable<budget> GetBudgetByTypeYear(int BudgetYear, string Type)
        {
            list = myRecords.Where(e => e.BudgetYear == BudgetYear && e.Type == Type);
            return (list);
        }

        public void DeleteRecord(budget record)
        {
            myRecords.Remove(record);
            context.budgets.Remove(record);
            context.SaveChanges();
        }
    }
}
