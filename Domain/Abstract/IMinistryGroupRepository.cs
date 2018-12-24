using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMinistryGroupRepository
    {
        void AddRecord(ministrygroup Record);
        ministrygroup GetMinistryGroupParent(int childID);
        IEnumerable<ministrygroup> GetMinistryGrouChild(int parentID);
        void DeleteRecord(ministrygroup record);
    }
}
