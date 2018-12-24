using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IBudgetItemRepository
    {
        void AddRecord(budgetitem Record);
        budgetitem GetBudgetItemParent(int childID);
        IEnumerable<budgetitem> GetBudgetItemChild(int parentID);
        void DeleteRecord(budgetitem record);
    }
}
