using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMinistryIncomeRepository : IMinistryIncomeRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private ministryincome record;
        private IEnumerable<ministryincome> list;

        private List<ministryincome> myRecords = new List<ministryincome>();

        public EFMinistryIncomeRepository()
        {
            myRecords = context.ministryincomes.ToList(); 
        }

        public void AddRecord(ministryincome Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int, string> GetIncomeList()
        {
            Dictionary<int, string> MinistryIncomeList;
            MinistryIncomeList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.ministryIncomeID, e => (string)e.Title);

            return (MinistryIncomeList);
        }

        public ministryincome GetIncomeByID(int incomeID)
        {
            record = myRecords.FirstOrDefault(e => e.ministryIncomeID == incomeID);
            return (record);
        }

        public IEnumerable<ministryincome> GetIncomeByCategory(int ministryID, int categoryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.subCategoryID == categoryID && e.ministryID == ministryID && e.IncomeDate >= BeginDate.Date && e.IncomeDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<ministryincome> GetIncomeByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.IncomeDate >= BeginDate.Date && e.IncomeDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<ministryincome> GetIncomeByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.IncomeDate >= BeginDate.Date && e.IncomeDate <= EndDate.Date);
            return (list);
        }

        public void DeleteRecord(ministryincome record)
        {
            myRecords.Remove(record);
            context.ministryincomes.Remove(record);
            context.SaveChanges();
        }

    }
}
