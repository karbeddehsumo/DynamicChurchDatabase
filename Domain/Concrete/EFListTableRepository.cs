using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFListTableRepository : IListtableRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private listtable record;
        private IEnumerable<listtable> list;

        private List<listtable> myRecords = new List<listtable>();

        public EFListTableRepository()
        {
            myRecords = context.listtables.ToList(); 
        }

        public bool AddRecord(listtable Record)
        {
            myRecords.Add(Record);
            context.listtables.Add(Record);
            context.SaveChanges();
            return true;
        }

        public Dictionary<int, string> GetList(int ministryID)
        {
            Dictionary<int, string> aList;
            
            aList = context.listtables.Where(e => e.Status == "Active")
            .Where(e => e.ministryID == ministryID)
           // .OrderBy(e => (string)e.Title)
           .OrderByDescending(e => e.DateCreated)
            .ToDictionary(e => (int)e.listTableID,e => (string)e.Title);

            return (aList);
        }
 /*

        public Lookup<int, listtable> GetList(int ministryID)
        {
          //  Dictionary<string, int> aList;

            var aList = myRecords
            .Where(e => e.ministryID == ministryID)
            .OrderBy(e => (string)e.Title)
            .ToLookup(e =>e.listTableID,e => e);

            return (aList);
        }
*/
        public listtable GetListByID(int listID)
        {
            record = myRecords.FirstOrDefault(e => e.listTableID == listID);
            return (record);
        }

        public IEnumerable<listtable> GetListByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == "Active");
            return (list);
        }

        public IEnumerable<listtable> GetListByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<listtable> GetListByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.DateCreated >= BeginDate.Date && e.DateCreated <= EndDate.Date);
            return (list);
        }


        public IEnumerable<listtable> GetListByDefaultOpen(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == "Active" && e.DefaultOpen == true);
            return (list);
        }

        public void DeleteRecord(listtable record)
        {
            myRecords.Remove(record);
            context.listtables.Remove(record);
            context.SaveChanges();
        }

    }
}
