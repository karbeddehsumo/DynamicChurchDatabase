using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IFamilyRepository
    {
        Dictionary<int, string> GetFamilyList();
        family GetFamilyByID(int familyID);
        IEnumerable<family> GetFamilyByCity(string city);
        IEnumerable<family> GetFamilyByState(string state);
        IEnumerable<family> GetFamilyByZip(string zip);
        IEnumerable<family> GetFamilyByStatus(string status);
        void DeleteRecord(family record);
        bool AddRecord(family record);
    }
}
