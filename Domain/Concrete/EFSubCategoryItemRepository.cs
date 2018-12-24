using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSubCategoryItemRepository : ISubCategoryItemRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private subcategoryitem record;
        private IEnumerable<subcategoryitem> list;

        private List<subcategoryitem> myRecords = new List<subcategoryitem>();

        public EFSubCategoryItemRepository()
        {
            myRecords = context.subcategoryitems.ToList(); 
        }

        public void AddRecord(subcategoryitem Record)
        {
            myRecords.Add(record);
        }


        public subcategoryitem GetCategoryByChild(int childID)
        {
            record = myRecords.FirstOrDefault(e => e.ChildCategoryID == childID);
            return (record);
        }

        public IEnumerable<subcategoryitem> GetCategoryByChildren(int childID)
        {
            list = myRecords.Where(e => e.ChildCategoryID == childID);
            return (list);
        }

        public IEnumerable<subcategoryitem> GetCategoryByParent(int parentID)
        {
            list = myRecords.Where(e => e.ParentCategoryID == parentID);
            return (list);
        }

        public bool HasParent(int childID)
        {
            if (myRecords.Count(e => e.ChildCategoryID == childID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void DeleteRecord(subcategoryitem record)
        {
            myRecords.Remove(record);
            context.subcategoryitems.Remove(record);
            context.SaveChanges();
        }
    }
}
