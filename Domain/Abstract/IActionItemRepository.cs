using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IActionItemRepository
    {
        void AddRecord(actionitem Record);
        actionitem GetActionItemParent(int childID);
        IEnumerable<actionitem> GetActionItemChild(int parentID);
        IEnumerable<task> GetActionItemTaskList(int parentID);
        IEnumerable<task> GetActionItemTaskList(string code);
        void DeleteRecord(actionitem record);
    }
}
