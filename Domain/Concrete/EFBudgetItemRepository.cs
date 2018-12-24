using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFBudgetItemRepository : IBudgetItemRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private budgetitem record;
        private IEnumerable<budgetitem> list; 
            
        private List<budgetitem> myRecords = new List<budgetitem>();

        public EFBudgetItemRepository()
        {
            myRecords = context.budgetitems.ToList(); 
        }

        public void AddRecord(budgetitem Record)
        {
            myRecords.Add(record);
        }



        public budgetitem GetBudgetItemParent(int childID)
        {
            record = myRecords.FirstOrDefault(e => e.childItemID == childID);
            return (record);
        }

        public IEnumerable<budgetitem> GetBudgetItemChild(int parentID)
        {
            list = myRecords.Where(e => e.parentItemID == parentID);
            return (list);
        }

        public void DeleteRecord(budgetitem record)
        {
            myRecords.Remove(record);
            context.budgetitems.Remove(record);
            context.SaveChanges();
        }

    }
}
