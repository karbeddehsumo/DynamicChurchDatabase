using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IListItemRepository
    {
        void AddRecord(listitem Record);
        listitem GetListItemByID(int itemID);
        IEnumerable<listitem> GetActiveListItemByListTable(int listTableID);
        IEnumerable<listitem> GetListItemByListStatus(int listID, string Status);
        IEnumerable<listitem> GetAllListItemByListTable(int ListTableID);
        void DeleteRecord(listitem record);
        void DeleteAllRecords(IEnumerable<listitem> items);
    }
}
