using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFProductRepository : IProductRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<product> list;
        private product record;

         private List<product> myRecords = new List<product>();

         public EFProductRepository()
        {
            myRecords = context.products.ToList(); 
        }

         public void AddRecord(product Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetProductList()
        {
            Dictionary<int, string> ProductList;
            ProductList = context.products
            .OrderBy(e => (string)e.Name)
            .ToDictionary(e => (int)e.productID, e => (string)e.Name);

            return (ProductList);
        }

        public product GetProductByID(int productID)
        {
            record = context.products.FirstOrDefault(e => e.productID == productID);
            return(record);
        }

        public IEnumerable<product> GetProductByPriceRange(decimal lowPrice, decimal highPrice)
        {
            list = context.products.Where(e => e.Price >= lowPrice && e.Price <= highPrice);
            return (list);
        }

        public IEnumerable<product> GetProductByQuantityRange(int lowQuantity, int highQuantity)
        {
            list = context.products.Where(e => e.Quantity >= lowQuantity && e.Quantity <= highQuantity);
            return (list);
        }

        public IEnumerable<product> GetProductByCategory(int categoryID)
        {
            list = context.products.Where(e => e.categoryID >= categoryID);
            return (list);
        }

        public IEnumerable<product> GetAllProduct()
        {
            list = context.products;
            return (list);
        }

        public void DeleteRecord(product record)
        {
            myRecords.Remove(record);
            context.products.Remove(record);
            context.SaveChanges();
        }

    }
}
