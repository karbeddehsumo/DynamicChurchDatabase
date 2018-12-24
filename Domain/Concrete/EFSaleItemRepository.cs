using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSaleItemRepository : ISaleItemRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<saleitem> list;
        private saleitem record;

        private List<saleitem> myRecords = new List<saleitem>();

        public EFSaleItemRepository()
        {
            myRecords = context.saleitems.ToList(); 
        }

        public void AddRecord(saleitem Record)
        {
            myRecords.Add(record);
        }


        public saleitem GetSaleItemByID(int saleItemID)
        {
            record = myRecords.FirstOrDefault(e => e.SaleItemID == saleItemID);
            return (record);
        }
         
        public IEnumerable<saleitem> GetSalesItem(int saleID)
        {
            list = myRecords.Where(e => e.SaleItemID == saleID);
            return (list);
        }

        public IEnumerable<saleitem> GetSalesItemByProduct(int productID, DateTime bDate, DateTime eDate)
        {
            list = from s in context.sales.Where(e => e.saleDate >= bDate.Date && e.saleDate <= eDate.Date)
                   join i in context.saleitems.Where(e => e.productID == productID)
                   on s.saleID equals i.saleID
                   select i;
            return (list);
        }

        public void DeleteRecord(saleitem record)
        {
            myRecords.Remove(record);
            context.saleitems.Remove(record);
            context.SaveChanges();
        }

    }
}
