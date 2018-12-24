using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IListHeaderRepository
    {
        void AddRecord(listheader Record);
        listheader GetListHeaderByID(int ListHeaderID);
        listheader GetByListTableID(int ListTableID);
        void DeleteRecord(listheader record);
    }
}
