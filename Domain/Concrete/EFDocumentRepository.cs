using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFDocumentRepository : IDocumentRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private document record;
        private IEnumerable<document> list;

        private List<document> myRecords = new List<document>();

        public EFDocumentRepository()
        {
            myRecords = context.documents.ToList(); 
        }

        public void AddRecord(document Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetDocumentList()
        {
            Dictionary<int, string> DocumentList;
            DocumentList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.documentID, e => (string)e.Title);

            return (DocumentList);
        }

        public document GetDocumentByID(int documentID)
        {
            record = myRecords.FirstOrDefault(e => e.documentID == documentID);
            return (record);
        }


        public IEnumerable<document> GetDocumentByStatus(int ministryID, string status)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == status);
            return (list);
        }

        public IEnumerable<document> GetDocumentByAuthor(string Author)
        {
            list = myRecords.Where(e => e.Author == Author);
            return (list);
        }

        public IEnumerable<document> GetDocumentByDateCreatedRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.DateCreated >= bDate.Date && e.DateCreated <= eDate.Date);
            return (list);
        }

        public IEnumerable<document> GetDocumentByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == "Active");
            return (list);
        }

        public IEnumerable<document> GetDocumentByDocumentType(int DocumentTypeID)
        {
            list = myRecords.Where(e => e.DocumentTypeID == DocumentTypeID);
            return (list);
        }

        public document GetDocumentByTitle(string Title)
        {
            record = myRecords.FirstOrDefault(e => e.Title == Title);
            return (record);
        }

        public IEnumerable<document> GetAllDocument()
        {
            int documentTypeID = context.constants.FirstOrDefault( e => e.ConstantName == "Property Document").constantID;
            return (myRecords.Where(e => e.DocumentTypeID != documentTypeID));
        }

        public IEnumerable<document> GetDocumentByStatusOnly(string status)
        {
            int documentTypeID = context.constants.FirstOrDefault(e => e.ConstantName == "Property Document").constantID;
            list = myRecords.Where(e => e.Status == status && e.DocumentTypeID != documentTypeID);
            return (list.OrderBy(e => e.DocumentTypeID).ThenBy(e => e.Title));
        }

        public void DeleteRecord(document record)
        {
            context.documents.Remove(record);
            context.SaveChanges();
        }
    }
}
