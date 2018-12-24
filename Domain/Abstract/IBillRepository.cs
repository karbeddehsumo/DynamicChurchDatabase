using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IBillRepository
    {
        void AddRecord(bill Record);
        Dictionary<int,string> GetBillList();
        bill GetBillByID(int billID);
        IEnumerable<bill> GetAllBill();
        IEnumerable<bill> GetBillByPayeeDateRange(int payeeID, DateTime bDate, DateTime eDate);
        IEnumerable<bill> GetBillByDueDateRange(DateTime bDate, DateTime eDate);
        void DeleteRecord(bill record);
        IEnumerable<bill> GetBillByStatus(string status);
    }
}
