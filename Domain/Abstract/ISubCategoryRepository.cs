using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISubCategoryRepository
    {
        void AddRecord(subcategory Record);
        Dictionary<int, string> GetIncomeCategoryList();
        Dictionary<int, string> GetExpenseCategoryList();
        Dictionary<int, string> GetIncomeCategoryNoParentList();
        Dictionary<int, string> GetExpenseCategoryNoParentList();
        subcategory GetByCategoryID(int categoryID);
        subcategory GetBySubCategoryID(int subCategoryID);
        IEnumerable<subcategory> GetCategoryByBankAccount(int bankAccountID);
        IEnumerable<subcategory> GetCategoryByStatus(string status);
        IEnumerable<subcategory> GetAllCategory();
        IEnumerable<subcategory> GetCategoryByCategoryType(string Type = "Income");
        string GetDisplayName(int subCategoryID);
        void DeleteRecord(subcategory record);

    }
}
