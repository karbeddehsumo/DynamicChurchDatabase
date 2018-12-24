using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFListItemRepository : IListItemRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private listitem record;
        private IEnumerable<listitem> list;

        private List<listitem> myRecords = new List<listitem>();

        public EFListItemRepository()
        {
            myRecords = context.listitems.ToList(); 
        }

        public void AddRecord(listitem Record)
        {
            myRecords.Add(record);
        }


        public listitem GetListItemByID(int itemID)
        {
            record = myRecords.FirstOrDefault(e => e.listItemID == itemID);
            return (record);
        }

        public IEnumerable<listitem> GetActiveListItemByListTable(int listTableID)
        {
            list = myRecords.Where(e => e.listTableID == listTableID && e.Status == "Active");
            return (list);
        }

        public IEnumerable<listitem> GetListItemByListStatus(int listID, string Status)
        {
            list = myRecords.Where(e => e.listTableID == listID && e.Status == Status);
            return (list);
        }

        public IEnumerable<listitem> GetAllListItemByListTable(int ListTableID)
        {
            list = myRecords.Where(e => e.listTableID == ListTableID);
            return list;
        }

        public void DeleteRecord(listitem record)
        {
            myRecords.Remove(record);
            context.listitems.Remove(record);
            context.SaveChanges();
        }

        public void DeleteAllRecords(IEnumerable<listitem> items)
        {
            foreach (listitem i in items)
            {
                context.listitems.Remove(i);          
            }
            context.SaveChanges();
        }

    }
}
