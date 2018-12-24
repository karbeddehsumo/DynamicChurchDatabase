using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFConstantRepository : IConstantRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private constant record;
        private IEnumerable<constant> list;

        private List<constant> myRecords = new List<constant>();

        public EFConstantRepository()
        {
            myRecords = context.constants.ToList(); 
        }


        public bool AddRecord(constant Record)
        {
            myRecords.Add(record);
            return true;
        }

        public void UpdateRecord(constant constant)
        {
            if (constant.constantID > 0)
            {
                context.Entry(constant).State = System.Data.EntityState.Modified;
            }
            context.SaveChanges();
        }

        public IEnumerable<constant> Constants
        {
            get { return myRecords; }
        }

        public Dictionary<int, string> GetConstantList()
        {
            Dictionary<int,string> ConstantList;
            ConstantList = myRecords
            .OrderBy(e => e.Category)
            .ToDictionary(e => (int)e.constantID, e => (string)e.Category);

            return (ConstantList);
        }


        public constant GetConstantID(int ConstantID)
        {
            var constant = myRecords
                  .FirstOrDefault(e => e.constantID == ConstantID);
            return (constant);
        }

        public constant GetConstantByName(string Name)
        {
            var constant = myRecords
                  .FirstOrDefault(e => e.ConstantName == Name);
            return (constant);
        }

        public Dictionary<string, string> GetConstantByCategory(string Category)
        {
            Dictionary<string, string> ConstantList;
            ConstantList = myRecords
            .Where(e => e.Category == Category)
            .OrderBy(e => e.ConstantName)
            .ToDictionary(e => (string)e.ConstantName, e => (string)e.Value1);

            return (ConstantList);
        }

        public IEnumerable<constant> GetAllConstantList()
        {
            list = myRecords
              .OrderBy(e => e.Category);
            return (list.ToList());
        }

        public IEnumerable<constant> GetConstantListByCategory(string Category)
        {
            list = myRecords
                .Where(e => e.Category == Category)
              .OrderBy(e => e.ConstantName);
            return (list.ToList());
        }

        public Dictionary<int, string> GetConstantByCategoryID(string Category)
        {
            Dictionary<int, string> ConstantList;
            ConstantList = myRecords
            .Where(e => e.Category == Category)
            .OrderBy(e => e.ConstantName)
            .ToDictionary(e => (int)e.constantID, e => (string)e.Value1);

            return (ConstantList);
        }

        public Dictionary<string, string> GetConstantCategoryGroup()
        {
            var GroupList = myRecords
                            .GroupBy(e => e.Category).ToList().Select(e => e.First()); //distinct

            var constantlist2 = GroupList
            .OrderBy(e => (string)e.Category)
            .ToDictionary(e => e.Category, e => e.Category);

            return (constantlist2);

        }


        public Dictionary<string, string> GetRegistrationQuestions()
        {
            Dictionary<string, string> ConstantList = new Dictionary<string, string>();
            ConstantList = myRecords
            .Where(e => e.Category == "RegistrationQuestions")
            .OrderBy(e => e.Category)
            .ToDictionary(e => (string)e.ConstantName, e => (string)e.ConstantName);

            return (ConstantList);
        }

        public Dictionary<string, string> GetConstantByMinistryActivity(string MinistyName = "")
        {

            Dictionary<string, string> ConstantList = new Dictionary<string, string>();
           
            if(MinistyName.Length > 0)
            {
                 ConstantList = myRecords
             .Where(e => e.Category == "Ministry Activity" && e.ConstantName == MinistyName)
              .OrderBy(e => e.Category)
            .ToDictionary(e => (string)e.Value1, e => (string)e.Value1);
            }
            else
            {
                 ConstantList = myRecords
                .Where(e => e.Category == "Ministry Activity")
                 .OrderBy(e => e.Category)
            .ToDictionary(e => (string)e.Value1, e => (string)e.Value1);
            }
           

            return (ConstantList);
        }

        public Dictionary<int, string> GetConstantByMinistryIncomeType(int ministryID = 0)
        {
            Dictionary<int, string> ConstantList = new Dictionary<int, string>();

                ConstantList = myRecords
            .Where(e => e.Category == "Ministry Income Type" && e.SortOrder == ministryID)
             .OrderBy(e => e.ConstantName)
           .ToDictionary(e => e.constantID, e => (string)e.Value1);


            return (ConstantList);
        }
        public Dictionary<int, string> GetConstantByMinistryExpenseType(int ministryID = 0)
        {
            Dictionary<int, string> ConstantList = new Dictionary<int, string>();

            ConstantList = myRecords
           .Where(e => e.Category == "Ministry Expense Type" && e.SortOrder == ministryID)
           .OrderBy(e => e.ConstantName)
           .ToDictionary(e => e.constantID, e => (string)e.Value1);


            return (ConstantList);
        }

        public bool isMinistryActivity(int ministryID, string EventType)
        {
            string ministryName = context.ministries.FirstOrDefault(e => e.ministryID == ministryID).MinistryName;
            list = myRecords.Where(e => e.Category == "Ministry Activity" && e.ConstantName == ministryName && e.Value1 == EventType);
            if (list.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public IEnumerable<constant> GetConstantByStatus(string Status)
        {
            list = context.constants.Where(e => e.Status == Status);
            return list;
        }

        public constant GetConstantByCategoryName(string Category, string Name)
        {
            record = myRecords.FirstOrDefault(e => e.Category == Category && e.ConstantName == Name);
            return record;
        }

        public void DeleteRedord(constant constant)
        {
            myRecords.Remove(record);
            context.constants.Remove(constant);
            context.SaveChanges();
        }

    }
}
