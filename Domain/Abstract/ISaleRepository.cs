using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISaleRepository
    {
        void AddRecord(sale Record);
        Dictionary<int, string> GetSalesList();
        sale GetSaleByID(int saleID);
        IEnumerable<sale> GetSalesByDate(DateTime aDate);
        IEnumerable<sale> GetSalesByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<sale> GetSalesByCashier(string cashierName, DateTime bDate, DateTime eDate);
        IEnumerable<sale> GetSalesByProductCountRange(int lowCount, int highCount, DateTime bDate, DateTime eDate);
        void DeleteRecord(sale record);
    }
}
