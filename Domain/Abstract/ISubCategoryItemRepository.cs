using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISubCategoryItemRepository
    {
        void AddRecord(subcategoryitem Record);
        subcategoryitem GetCategoryByChild(int childID);
        IEnumerable<subcategoryitem> GetCategoryByParent(int parentID);
        IEnumerable<subcategoryitem> GetCategoryByChildren(int childID);
        bool HasParent(int childID);
        void DeleteRecord(subcategoryitem record);


    }
}
