using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISaleItemRepository
    {
        void AddRecord(saleitem Record);
        IEnumerable<saleitem> GetSalesItem(int saleID);
        saleitem GetSaleItemByID(int saleItemID);
        IEnumerable<saleitem> GetSalesItemByProduct(int productID, DateTime bDate, DateTime eDate);
        void DeleteRecord(saleitem record);
    }
}
