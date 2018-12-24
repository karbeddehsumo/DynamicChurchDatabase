using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IVisitorRepository
    {
        void AddRecord(visitor Record);
        Dictionary<int, string> GetVisitorList();
        visitor GetVisitorByID(int visitorID);
        IEnumerable<visitor> GetVisitorByCategory(int categoryID);
        IEnumerable<visitor> GetVisitorByEventDate(DateTime bDate, DateTime eDate);
        IEnumerable<visitor> GetVisitorByBestContact(int contactTypeID);
        IEnumerable<visitor> GetVisitorByCity(string city);
        IEnumerable<visitor> GetVisitorByState(string state);
        IEnumerable<visitor> GetVisitorByZip(string zip);
        IEnumerable<visitor> GetVisitorByStatus(string status);
        IEnumerable<visitor> GetVisitorByLastEventAttended(int EventID);
        void DeleteRecord(visitor record);
    }
}
