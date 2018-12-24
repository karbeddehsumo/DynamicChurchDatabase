using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPropertyRepository : IPropertyRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private property record;
        private IEnumerable<property> list;

        private List<property> myRecords = new List<property>();

         public EFPropertyRepository()
        {
            myRecords = context.properties.ToList(); 
        }

         public void AddRecord(property Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetPropertyList()
        {
            Dictionary<int, string> PropertyList;
            PropertyList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.propertyID, e => (string)e.Title);

            return (PropertyList);
        }

        public property GetPropertyByID(int propertyID)
        {
            record = myRecords.FirstOrDefault(e => e.propertyID == propertyID);
            return (record);
        }

        public IEnumerable<property> GetPropertyByCategory(int categoryID)
        {
            list = myRecords.Where(e => e.subCategoryID == categoryID);
            return (list);
        }

        public IEnumerable<property> GetPropertyByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<property> GetAllProperty()
        {
            list = myRecords;
            return (list);
        }

        public void DeleteRecord(property record)
        {
            myRecords.Remove(record);
            context.properties.Remove(record);
            context.SaveChanges();
        }
    }
}
