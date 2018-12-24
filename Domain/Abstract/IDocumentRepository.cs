using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IDocumentRepository
    {
        void AddRecord(document Record);
        Dictionary<int, string> GetDocumentList();
        document GetDocumentByID(int documentID);
        IEnumerable<document> GetDocumentByStatus(int ministryID, string status);
        IEnumerable<document> GetDocumentByStatusOnly(string status);
        IEnumerable<document> GetDocumentByAuthor(string Author);
        IEnumerable<document> GetDocumentByDateCreatedRange(DateTime bDate, DateTime eDate);
        IEnumerable<document> GetDocumentByMinistry(int ministryID);
        IEnumerable<document> GetDocumentByDocumentType(int DocumentTypeID);
        document GetDocumentByTitle(string Title);
        IEnumerable<document> GetAllDocument();
        void DeleteRecord(document record);
    }
}
