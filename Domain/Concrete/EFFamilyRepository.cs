using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFFamilyRepository : IFamilyRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private family record;
        private IEnumerable<family> list;

        private List<family> myRecords = new List<family>();

         public EFFamilyRepository()
        {
            myRecords = context.families.ToList(); 
        }

         public bool AddRecord(family Record)
        {
            myRecords.Add(record);
            return true;
        }

        public Dictionary<int, string> GetFamilyList()
        {
            Dictionary<int, string> FamilyList;
            FamilyList = myRecords
            .OrderBy(e => (string)e.FamilyName)
            .ToDictionary(e => (int)e.familyID, e => (string)e.FamilyName + " - " + e.Address + "," + e.City + "," + e.State);

            return (FamilyList);
        }

        public family GetFamilyByID(int familyID)
        {
            record = myRecords.FirstOrDefault(e => e.familyID == familyID);
            return (record);
        }

        public IEnumerable<family> GetFamilyByCity(string city)
        {
            list = myRecords.Where(e => e.City == city);
            return (list);
        }

        public IEnumerable<family> GetFamilyByState(string state)
        {
            list = myRecords.Where(e => e.State == state);
            return (list);
        }

        public IEnumerable<family> GetFamilyByZip(string zip)
        {
            list = myRecords.Where(e => e.Zip == zip);
            return (list);
        }

        public IEnumerable<family> GetFamilyByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        /*
        public bool  AddRecord(family record)
        {
            myRecords.Add(record);
            context.SaveChanges();
            return true;
        }
        */
        public void DeleteRecord(family record)
        {
            myRecords.Remove(record);
            context.families.Remove(record);
            context.SaveChanges();
        }
    }
}
