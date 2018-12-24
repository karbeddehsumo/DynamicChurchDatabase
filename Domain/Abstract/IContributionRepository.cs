using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IContributionRepository
    {
        void AddRecord(contribution Record);
        contribution GetContributionByID(int contributionID);
        contribution GetContributionByCheckNumber(string CheckNumber);
        IEnumerable<contribution> GetContributionByDate(DateTime aDate);
        IEnumerable<contribution> GetContributionByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<contribution> GetContributionByMemberDate(int memberID, DateTime aDate);
        IEnumerable<contribution> GetContributionByMemberDateRange(int memberID, DateTime bDate, DateTime eDate);
        IEnumerable<contribution> GetContributionByMemberCategoryRange(int memberID, int categoryID, DateTime bDate, DateTime eDate);
        IEnumerable<contribution> GetContributionByCategoryRange(int categoryID, DateTime bDate, DateTime eDate);
        int GetContributionByMember(int memberID);
        void DeleteRecord(contribution record);
    }
}
