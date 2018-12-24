using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFListHeaderRepository : IListHeaderRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private listheader record;
        IEnumerable<listheader> List;

        private List<listheader> myRecords = new List<listheader>();

         public EFListHeaderRepository()
        {
            myRecords = context.listheaders.ToList(); 
        }

         public void AddRecord(listheader Record)
        {
            myRecords.Add(record);
        }

        public listheader GetListHeaderByID(int ListHeaderID)
        {
            record = myRecords.FirstOrDefault(e => e.ListHeaderID == ListHeaderID);
            return record;        
        }

        public listheader  GetByListTableID(int ListTableID)
        {
            record = myRecords.FirstOrDefault(e => e.ListTableID == ListTableID);
            return record;   
        }

        public void DeleteRecord(listheader record)
        {
            myRecords.Remove(record);
            context.listheaders.Remove(record);
            context.SaveChanges();
        }

    }
}
