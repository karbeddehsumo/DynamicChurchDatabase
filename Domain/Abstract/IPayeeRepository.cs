using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPayeeRepository
    {
        void AddRecord(payee Record);
        Dictionary<int, string> GetPayeeList();
        Dictionary<int, string> GetPayeeListByType(string type);
        Dictionary<int, string> GetPayeeListNonStaff();
        Dictionary<int, string> GetPayeeListStaff();
        Dictionary<string, string> GetPayeeFrequency();
        payee GetPayeeByID(int payeeID);
        IEnumerable<payee> GetPayeeByBankAccount(int bankAccountID);
        IEnumerable<payee> GetPayeeByCategory(int categoryID);
        payee GetPayeeByAccountNumber(string accountNumber);
        IEnumerable<payee> GetPayeeByFrequency();
        IEnumerable<payee> GetPayeeByStatus(string status);
        IEnumerable<payee> GetAllPayee();
        void DeleteRecord(payee record);
        payee GetPayeeByName(string payee);
        

    }
}
