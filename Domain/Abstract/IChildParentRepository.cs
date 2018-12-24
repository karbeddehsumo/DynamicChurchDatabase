using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IChildParentRepository
    {
        bool AddRecord(childparent Record);
        member GetParent(int childID, int parentID);
        IEnumerable<member> GetParents(int childID);
        IEnumerable<member> Getchildren(int parentID);
        IEnumerable<childparent> GetParentRecords(int childID);
        bool HasParent(int childID, int parentID);
        void DeleteRecord(childparent record);
    }
}
