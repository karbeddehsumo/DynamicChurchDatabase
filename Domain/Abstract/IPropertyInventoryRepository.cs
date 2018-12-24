using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPropertyInventoryRepository
    {
        void AddRecord(propertyinventory Record);
        Dictionary<int, string> GetInventoryList();
        propertyinventory GetInventoryByID(int inventoryID);
        IEnumerable<propertyinventory> GetInventoryByProperty(int propertyID);
        IEnumerable<propertyinventory> GetInventoryByPerchaseDate(DateTime aDate);
        IEnumerable<propertyinventory> GetInventoryByPerchaseDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<propertyinventory> GetInventoryByValueRange(decimal lowValue, decimal highValue);
        IEnumerable<propertyinventory> GetInventoryByQuantityRange(int lowQuantity, int highQuantity);
        IEnumerable<propertyinventory> GetInventoryByLocation(string location);
        IEnumerable<propertyinventory> GetInventoryByAssignTo(string AssignTo);
        IEnumerable<propertyinventory> GetInventoryByStatus(string status);
        IEnumerable<propertyinventory> GetInventoryByTag(string tagID);
        IEnumerable<propertyinventory> GetInventoryByCondition(string condition);
        IEnumerable<propertyinventory> GetAllPropertyInventory();
        List<string> GetAssignedToName();
        List<string> GetPropertyLocation();
        
        void DeleteRecord(propertyinventory record);
    }
}
