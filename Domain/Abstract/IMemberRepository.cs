using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMemberRepository
    {
        void AddRecord(member Record);
        Dictionary<int, string> GetMemberList();
        Dictionary<int, string> GetMwmberByFamilyList(int memberID);
        member GetMemberByID(int memberID);
        IEnumerable<member> GetMemberByDOB(DateTime DOB);
        IEnumerable<member> GetMemberByDOBRange(DateTime bDate, DateTime eDate);
        IEnumerable<member> GetMemberByGender(string Gender);
        IEnumerable<member> GetMemberByMembershipDate(DateTime aDate);
        IEnumerable<member> GetMemberByMembershipDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<member> GetMemberByChurchTitle(string churchTitle);
        IEnumerable<member> GetMemberByFamily(int familyID);
        IEnumerable<member> GetMemberByStatus(string status);
        IEnumerable<member> GetMemberByAge(int BeginningYear, int EndingYear);
        IEnumerable<member> GetWeddingAnniversary(DateTime TodayDate);
        IEnumerable<member> GetMemberListCategory(string MemberGroup);
        IEnumerable<member> GetChildParent(int childID);
        Dictionary<int, string> GetAllMembers();
        void DeleteRecord(member record);



    }
}
