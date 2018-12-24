using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IStaffRepository
    {
        void AddRecord(staff Record);
        Dictionary<int, string> GetStaffList();
        staff GetStaffByID(int staffId);
        IEnumerable<staff> GetStaffByHireDate(DateTime bDate, DateTime eDate);
        IEnumerable<staff> GetStaffByStatus(string status);
        void DeleteRecord(staff record);
    }
}
