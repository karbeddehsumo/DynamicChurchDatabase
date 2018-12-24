using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IListtableRepository
    {
        Dictionary<int, string> GetList(int ministryID);
       // Lookup<int, listtable> GetList(int ministryID);
        listtable GetListByID(int listID);
        IEnumerable<listtable> GetListByMinistry(int ministryID);
        IEnumerable<listtable> GetListByStatus(string status);
        IEnumerable<listtable> GetListByDateRange(DateTime BeginDate, DateTime EndDate);
        IEnumerable<listtable> GetListByDefaultOpen(int ministryID);
        void DeleteRecord(listtable record);
        bool AddRecord(listtable record);
    }
}
