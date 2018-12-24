using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSaleRepository : ISaleRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<sale> list;
        private sale record;

        private List<sale> myRecords = new List<sale>();

        public EFSaleRepository()
        {
            myRecords = context.sales.ToList(); 
        }

        public void AddRecord(sale Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int, string> GetSalesList()
        {
            Dictionary<int, string> SalesList;
            SalesList = myRecords
            .OrderBy(e => (DateTime)e.saleDate)
            .ToDictionary(e => (int)e.saleID, e => string.Format("D{0}ID{1}${2}#{3}",e.saleDate,e.saleID,e.saleAmount,e.NumberProduct));
            
            return (SalesList);
        }

        public sale GetSaleByID(int saleID)
        {
            record = myRecords.FirstOrDefault(e => e.saleID == saleID);
            return (record);
        }

        public IEnumerable<sale> GetSalesByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.saleDate == aDate.Date);
            return (list);
        }

        public IEnumerable<sale> GetSalesByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.saleDate >= bDate.Date && e.saleDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<sale> GetSalesByCashier(string cashierName, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.Cashier == cashierName && e.saleDate >= bDate.Date && e.saleDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<sale> GetSalesByProductCountRange(int lowCount, int highCount, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.NumberProduct >= lowCount && e.NumberProduct <= highCount && e.saleDate >= bDate.Date && e.saleDate <= eDate.Date);
            return (list);
        }

        public void DeleteRecord(sale record)
        {
            myRecords.Remove(record);
            context.sales.Remove(record);
            context.SaveChanges();
        }

    }
}
