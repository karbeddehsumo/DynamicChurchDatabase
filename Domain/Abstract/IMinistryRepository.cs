using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMinistryRepository
    {
        void AddRecord(ministry Record);
        Dictionary<int, string> GetMinistryList();
        Dictionary<int, string> GetPublicMinistryList();
        Dictionary<int, string> GetGroupMinistryList();
        ministry GetMinistryByID(int ministryID);
        ministry GetParentMinistryByID(int ministryID);
        IEnumerable<ministry> GetMyMinistries(int memberID);
        IEnumerable<ministry> GetMyDefaultMinistries(int memberID);
        IEnumerable<ministry> GetMinistryByStatus(string status);
        ministry GetMinistryByCodeDesc(string CodeDesc);
        ministry GetMainChurchMinistry();
        IEnumerable<ministry> GetPublicMinistry();
        IEnumerable<ministry> GetDisplayMinistry();
        void DeleteRecord(ministry record);
    }
}
