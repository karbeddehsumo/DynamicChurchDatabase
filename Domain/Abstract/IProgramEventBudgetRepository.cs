using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IProgramEventBudgetRepository
    {
        void AddRecord(programeventbudget Record);
        Dictionary<int, string> GetEventBudgetList(string status);
        IEnumerable<programeventbudget> GetEventBudgetByEventID(int eventID);
        programeventbudget GetEventBudgetByID(int ProgramEventBudgetID);
        IEnumerable<programeventbudget> GetEventBudgetByDueDateRange(DateTime BeginDate, DateTime EndDate);
        programeventbudget GetEventBudgetByCheckNumber(string checkNumber);
        IEnumerable<programeventbudget> GetEventBudgetByStatus(string status);
        void DeleteRecord(programeventbudget record);
    }
}
