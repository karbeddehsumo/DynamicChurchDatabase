using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPayeeCategoryRepository
    {
        void AddRecord(payeecategory Record);
        IEnumerable<payeecategory> GetPayeeCategoryByPayee(int payeeID);
        IEnumerable<payeecategory> GetPayeeCategoryByCategory(int categoryID);
        void DeleteRecord(payeecategory record);
    }
}
