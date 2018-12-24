using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IProductDiscountRepository
    {
        void AddRecord(productdiscount Record);
        //  Dictionary<int,string> GetDiscountList();
        productdiscount GetDiscountByID(int discountID);
        IEnumerable<productdiscount> GetDiscountByProduct(int productID);
        IEnumerable<productdiscount> GetDiscountByDateRange(DateTime sDate, DateTime eDate);
        void DeleteRecord(productdiscount record);
    }
}
