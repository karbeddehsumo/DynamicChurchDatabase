using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IConstantRepository
    {
        bool AddRecord(constant Record);
        Dictionary<int,string> GetConstantList();
        constant GetConstantID(int ConstantID);
        constant GetConstantByName(string Name);
        Dictionary<string, string> GetConstantByCategory(string Category);
        Dictionary<int, string> GetConstantByCategoryID(string Category);
        IEnumerable<constant> GetAllConstantList();
        IEnumerable<constant> GetConstantListByCategory(string Category);
        void UpdateRecord(constant constant);
        IEnumerable<constant> Constants { get; }
        Dictionary<string, string> GetRegistrationQuestions();
        Dictionary<string, string> GetConstantByMinistryActivity(string MinistryName = "");
        Dictionary<int, string> GetConstantByMinistryIncomeType(int ministryID = 0);
        Dictionary<int, string> GetConstantByMinistryExpenseType(int ministryID = 0);
        bool isMinistryActivity(int ministryID, string EventType);
        Dictionary<string, string> GetConstantCategoryGroup();
        IEnumerable<constant> GetConstantByStatus(string Status);
        constant GetConstantByCategoryName(string Category, string Name);
        void DeleteRedord(constant constant);
    }
}
