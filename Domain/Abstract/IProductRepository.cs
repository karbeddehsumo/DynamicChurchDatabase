using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IProductRepository
    {
        void AddRecord(product Record);
        Dictionary<int, string> GetProductList();
        product GetProductByID(int productID);
        IEnumerable<product> GetProductByPriceRange(decimal lowPrice, decimal highPrice);
        IEnumerable<product> GetProductByQuantityRange(int lowQuantity, int highQuantity);
        IEnumerable<product> GetProductByCategory(int categoryID);
        IEnumerable<product> GetAllProduct();
        void DeleteRecord(product record);
    }
}
