using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFProductDiscountRepository : IProductDiscountRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<productdiscount> list;
        private productdiscount record;

        private List<productdiscount> myRecords = new List<productdiscount>();

         public EFProductDiscountRepository()
        {
            myRecords = context.productdiscounts.ToList(); 
        }

         public void AddRecord(productdiscount Record)
        {
            myRecords.Add(record);
        }

        /*
                public Dictionary<int,string> GetDiscountList()
                {
                    Dictionary<int,string> DiscountList;
                    DiscountList = myRecords
                    .OrderBy(e => (string)e.)
                    .ToDictionary(e => (int)e.meetingID, e => (string)e.Title);
            
                    return (DiscountList);
                }
        */
        public productdiscount GetDiscountByID(int discountID)
        {
            record = myRecords.FirstOrDefault(e => e.ProductDiscountID == discountID);
            return (record);
        }

        public IEnumerable<productdiscount> GetDiscountByProduct(int productID)
        {
            list = myRecords.Where(e => e.productID == productID);
            return (list);
        }

        public IEnumerable<productdiscount> GetDiscountByDateRange(DateTime sDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.StartDate >= sDate.Date && e.StartDate < eDate.Date);
            return (list);
        }

        public void DeleteRecord(productdiscount record)
        {
            myRecords.Remove(record);
            context.productdiscounts.Remove(record);
            context.SaveChanges();
        }

    }
}
