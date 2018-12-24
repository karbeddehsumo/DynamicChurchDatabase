using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPropertyRepository
    {
        void AddRecord(property Record);
        Dictionary<int, string> GetPropertyList();
        property GetPropertyByID(int propertyID);
        IEnumerable<property> GetPropertyByCategory(int categoryID);
        IEnumerable<property> GetPropertyByStatus(string status);
        IEnumerable<property> GetAllProperty();
        void DeleteRecord(property record);
    }
}
