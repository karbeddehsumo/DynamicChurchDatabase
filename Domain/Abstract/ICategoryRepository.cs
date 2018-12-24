using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ICategoryRepository
    {
        void AddRecord(category Record);
        Dictionary<int, string> GetCategoryList();
        category GetCategoryByID(int categoryID);
        IEnumerable<category> GetCategoryByStatus(string Status);
        IEnumerable<category> GetAllCategory();
        int GetByCategoryName(string CategoryName);
        void DeleteCategory(category record);
    }
}
