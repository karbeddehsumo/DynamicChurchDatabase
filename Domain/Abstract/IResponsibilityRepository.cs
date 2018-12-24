using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IResponsibilityRepository
    {
        void AddRecord(responsibility Record);
        Dictionary<int, string> GetResponsibilityList(int staffID = 0);
        responsibility GetResponsibilityByID(int responsibilityID);
        IEnumerable<responsibility> GetResponsibilityByStaff(int staffID);
        responsibility GetStaffPrimaryResponsibility(int staffID);
        IEnumerable<responsibility> GetResponsiblityByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<responsibility> GetResponsibilityByStatus(string status);
        void DeleteRecord(responsibility record);
    }
}
