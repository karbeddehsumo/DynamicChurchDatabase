using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPledgeRepository
    {
        void AddRecord(pledge Record);
        pledge GetPledgeByID(int pledgeID);
        IEnumerable<pledge> GetPledgeByMember(int memberID, int PledgeYear);
        IEnumerable<pledge> GetPledgeByType(string PledgeType, int PledgeYear);
        IEnumerable<pledge> GetPledgeByFund(int fundID, int PledgeYear);
        IEnumerable<pledge> GetPledgeByYear(int pledgeYear);
        void DeleteRecord(pledge record);
    }
}
