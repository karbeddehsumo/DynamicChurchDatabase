using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPropertyInventoryRepository : IPropertyInventoryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private propertyinventory record;
        private IEnumerable<propertyinventory> list;

        private List<propertyinventory> myRecords = new List<propertyinventory>();

        public EFPropertyInventoryRepository()
        {
            myRecords = context.propertyinventories.Where(e => e.Status == "Active").ToList(); 
        }

        public void AddRecord(propertyinventory Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetInventoryList()
        {
            Dictionary<int, string> InventoryList;
            InventoryList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.propertyInventoryID, e => (string)e.Title);

            return (InventoryList);
        }

        public propertyinventory GetInventoryByID(int inventoryID)
        {
            record = myRecords.FirstOrDefault(e => e.propertyInventoryID == inventoryID);
            return (record);
        }

        public IEnumerable<propertyinventory> GetInventoryByProperty(int propertyID)
        {
            list = myRecords.Where(e => e.propertyID == propertyID);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByPerchaseDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.purchaseDate == aDate.Date);
            return (list);
        }
        public IEnumerable<propertyinventory> GetInventoryByPerchaseDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.purchaseDate >= bDate.Date && e.purchaseDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByValueRange(decimal lowValue, decimal highValue)
        {
            list = myRecords.Where(e => e.Value >= lowValue && e.Value <= highValue);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByQuantityRange(int lowQuantity, int highQuantity)
        {
            list = myRecords.Where(e => e.Quantity >= lowQuantity && e.Quantity <= highQuantity);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByLocation(string location)
        {
            list = myRecords.Where(e => e.Location == location);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByAssignTo(string AssignTo)
        {
            list = myRecords.Where(e => e.AssignedTo == AssignTo);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByStatus(string status)
        {
            list = context.propertyinventories.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByTag(string tagID)
        {
            list = myRecords.Where(e => e.TagNumber == tagID);
            return (list);
        }

        public IEnumerable<propertyinventory> GetInventoryByCondition(string condition)
        {
            list = myRecords.Where(e => e.Condition == condition);
            return (list);
        }

        public IEnumerable<propertyinventory> GetAllPropertyInventory()
        {
            list = myRecords;
            return (list);
        }

        public List<string> GetAssignedToName()
        {
            var list = myRecords.Where(e => e.Status == "Active").Select(e => e.AssignedTo).Distinct().ToList();
            return (list);             
        }

        public List<string> GetPropertyLocation()
        {
            var list = myRecords.Where(e => e.Status == "Active").Select(e => e.Location).Distinct().ToList();
            return (list);
        }


        public void DeleteRecord(propertyinventory record)
        {
            myRecords.Remove(record);
            context.propertyinventories.Remove(record);
            context.SaveChanges();
        }

    }
}
