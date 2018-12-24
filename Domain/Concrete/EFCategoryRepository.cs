using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private category record;
        private IEnumerable<category> list;

        private List<category> myRecords = new List<category>();

        public EFCategoryRepository()
        {
            myRecords = context.categories.ToList(); 
        }

        public void AddRecord(category Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetCategoryList()
        {
            Dictionary<int, string> CategoryList;
            CategoryList = myRecords
            .OrderBy(e => (string)e.CategoryName)
            .ToDictionary(e => (int)e.categoryID, e => (string)e.CategoryName);

            return (CategoryList);
        }

        public category GetCategoryByID(int categoryID)
        {
            record = myRecords.FirstOrDefault(e => e.categoryID == categoryID);
            return (record);
        }

        public IEnumerable<category> GetCategoryByStatus(string Status)
        {
            list = myRecords.Where(e => e.Status == Status);
            return (list);
        }


        public IEnumerable<category> GetAllCategory()
        {
            return(myRecords);
        }

        public int GetByCategoryName(string CategoryName)
        {
            record = myRecords.FirstOrDefault(e => e.CategoryName == CategoryName);
            return (record.categoryID);
        }

        public void DeleteCategory(category record)
        {
            myRecords.Remove(record);
            context.categories.Remove(record);
            context.SaveChanges();
        }

    }
}
