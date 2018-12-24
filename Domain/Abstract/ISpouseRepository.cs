using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISpouseRepository
    {
        spouse GetSpouseByID(int spouseID);
        spouse GetSpouseByID1(int spouseID);
        spouse GetSpouseByID2(int spouseID);
        IEnumerable<spouse> GetSpouseByJointTithe(bool joint);
        bool IsJointTitle(int memberID);
        string JointTitheTitle(int memberID);
        bool AddRecord(spouse Record);
        void DeleteRecord(spouse record);
    }
}
