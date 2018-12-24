using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPayeeCategoryRepository : IPayeeCategoryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private payeecategory record;
        private IEnumerable<payeecategory> list;

        private List<payeecategory> myRecords = new List<payeecategory>();

         public EFPayeeCategoryRepository()
        {
            myRecords = context.payeecategories.ToList(); 
        }

         public void AddRecord(payeecategory Record)
        {
            myRecords.Add(record);
        }


        public IEnumerable<payeecategory> GetPayeeCategoryByPayee(int payeeID)
        {
            list = myRecords.Where(e => e.payeeID == payeeID);
            return (list);
        }

        public IEnumerable<payeecategory> GetPayeeCategoryByCategory(int categoryID)
        {
            list = myRecords.Where(e => e.categoryID == categoryID);
            return (list);
        }

        public void DeleteRecord(payeecategory record)
        {
            myRecords.Remove(record);
            context.payeecategories.Remove(record);
            context.SaveChanges();
        }

    }
}
