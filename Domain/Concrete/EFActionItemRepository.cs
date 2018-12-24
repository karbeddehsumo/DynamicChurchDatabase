using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFActionItemRepository : IActionItemRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private actionitem record;
        IEnumerable<actionitem> List;
        private List<actionitem> myRecords = new List<actionitem>();

        public EFActionItemRepository()
        {
            myRecords = context.actionitems.ToList(); 
        }

        public void AddRecord(actionitem Record)
        {
            myRecords.Add(record);
        }


        public actionitem GetActionItemParent(int childID)
        {
            record = myRecords.FirstOrDefault(e => e.childItemID == childID);
            return (record);
        }
        public IEnumerable<actionitem> GetActionItemChild(int parentID)
        {
            List = myRecords.Where(e => e.ParentItemID == parentID);
            return (List);
        }

        public IEnumerable<task> GetActionItemTaskList(int parentID)
        {
            var actionItems = from g in context.tasks
                              join c in context.actionitems
                                  on g.taskID equals c.childItemID
                                  where c.ParentItemID == parentID
                                 select g;
            return (actionItems.ToList());
        }

        public IEnumerable<task> GetActionItemTaskList(string code)
        {
            var actionItems = from g in context.tasks
                              join c in context.actionitems
                                  on g.taskID equals c.childItemID
                              where c.ParentType == code
                              select g;
            return (actionItems.ToList());
        }

        public void DeleteRecord(actionitem record)
        {
            myRecords.Remove(record);
            context.actionitems.Remove(record);
            context.SaveChanges();
        }
    }
}
