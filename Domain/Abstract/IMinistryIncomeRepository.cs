using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMinistryIncomeRepository
    {
        void AddRecord(ministryincome Record);
        Dictionary<int, string> GetIncomeList();
        ministryincome GetIncomeByID(int incomeID);
        IEnumerable<ministryincome> GetIncomeByCategory(int ministryID, int categoryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<ministryincome> GetIncomeByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<ministryincome> GetIncomeByDateRange(DateTime BeginDate, DateTime EndDate);
        void DeleteRecord(ministryincome record);
    }
}
