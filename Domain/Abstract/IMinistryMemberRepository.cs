using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMinistryMemberRepository
    {
        bool AddRecord(ministrymember Record);
        Dictionary<int, string> GetMinistryMemberList(int ministryID);
        IEnumerable<member> GetMinistryRoster(int ministryID);
        IEnumerable<ministrymember> GetMemintryMemberByMinistry(int ministryID);
        IEnumerable<ministrymember> GetMinistryMemberByMember(int memberID);
        IEnumerable<ministrymember> GetMinistryMemberByOfficer(int ministryID);
        IEnumerable<ministrymember> GetMinistryMemberByNonOfficer(int ministryID);
        ministrymember GetMinistryMemberByID(int ministryID, int memberID);
        void DeleteRecord(ministrymember record);

    }
}
