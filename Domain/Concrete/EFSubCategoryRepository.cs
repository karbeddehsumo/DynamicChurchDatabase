using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSubCategoryRepository : ISubCategoryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private subcategory record;
        private IEnumerable<subcategory> list;

        private List<subcategory> myRecords = new List<subcategory>();

         public EFSubCategoryRepository()
        {
            myRecords = context.subcategories.ToList(); 
        }

         public void AddRecord(subcategory Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetIncomeCategoryList()
        {
            int categoryID = context.categories.FirstOrDefault(e => e.CategoryName == "Income").categoryID;
            Dictionary<int, string> SubCategoryList;
/*
            var parentCategory = from g in myRecords.Where(e => e.Status == "Active")
                   join c in context.subcategoryitems
                    on g.subCategoryID equals c.ParentCategoryID
                   select g;

            //exclude parent category
            var excludeIds = new HashSet<int>(parentCategory.Select(x => x.subCategoryID));
            var targetList = myRecords.Where(x => !excludeIds.Contains(x.subCategoryID)).ToList();

            SubCategoryList = targetList
            .Where(e => e.categoryID == categoryID)
            .OrderBy(e => (string)e.SubCategoryName)
            .ToDictionary(e => (string)e.SubCategoryName, e => (int)e.subCategoryID);
*/
            SubCategoryList = myRecords.Where(e => e.Status == "Active")
           .Where(e => e.categoryID == categoryID)
           .OrderBy(e => (string)e.SubCategoryName)
           .ToDictionary(e => (int)e.subCategoryID, e => (string)e.SubCategoryName);

            return (SubCategoryList);
        }

        public Dictionary<int, string> GetExpenseCategoryList()
        {
            int categoryID = context.categories.FirstOrDefault(e => e.CategoryName == "Expense").categoryID;
            Dictionary<int, string> SubCategoryList;
            /*
                        var parentCategory = from g in myRecords.Where(e => e.Status == "Active")
                                             join c in context.subcategoryitems
                                              on g.subCategoryID equals c.ParentCategoryID
                                             select g;

                        //exclude parent category
                        var excludeIds = new HashSet<int>(parentCategory.Select(x => x.subCategoryID));
                        var targetList = myRecords.Where(x => !excludeIds.Contains(x.subCategoryID)).ToList();

                        SubCategoryList = targetList
                        .Where(e => e.categoryID == categoryID)
                        .OrderBy(e => (string)e.SubCategoryName)
                        .ToDictionary(e => (string)e.SubCategoryName, e => (int)e.subCategoryID);
             
                                     SubCategoryList = myRecords.Where(e => e.Status == "Active")
                        .Where(e => e.categoryID == categoryID)
                        .OrderBy(e => (string)e.SubCategoryName)
                        .ToDictionary(e => (int)e.subCategoryID, e => (string)e.SubCategoryName);


*/


             SubCategoryList = myRecords.Where(e => e.Status == "Active")
                        .Where(e => e.categoryID == categoryID)
                        .OrderBy(e => (string)e.SubCategoryName)
                      .ToDictionary(e => (int)e.subCategoryID, e => (string)e.SubCategoryName);
          

            return (SubCategoryList);
        }

        public subcategory GetByCategoryID(int categoryID)
        {
            record = myRecords.FirstOrDefault(e => e.categoryID == categoryID);
            return(record);
        }

        public subcategory GetBySubCategoryID(int subCategoryID)
        {
            record = myRecords.FirstOrDefault(e => e.subCategoryID == subCategoryID);
            return (record);
        }

        public IEnumerable<subcategory> GetCategoryByBankAccount(int bankAccountID)
             {
                 list = myRecords.Where(e => e.bankAccountID == bankAccountID);
                 return (list);
        }

        public IEnumerable<subcategory> GetCategoryByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return(list);
        }

        public IEnumerable<subcategory> GetAllCategory()
        {
            list = myRecords;
            return (list);
        }

        public IEnumerable<subcategory> GetCategoryByCategoryType(string Type = "Income")
        {
            if (Type == "")
            {
                Type = "Income";
            }
            int categoryID = context.categories.FirstOrDefault(e => e.CategoryName == Type).categoryID;
            list = myRecords.Where(e => e.categoryID == categoryID);
            return (list.ToList());
        }

        public string GetDisplayName(int subCategoryID)
        {
            subcategory s = myRecords.FirstOrDefault(e => e.subCategoryID == subCategoryID);
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                subcategoryitem item = context.subcategoryitems.FirstOrDefault(e => e.ChildCategoryID == s.subCategoryID);
                if (item != null)
                {
                    subcategory child = myRecords.FirstOrDefault(e => e.subCategoryID == item.ChildCategoryID);
                    subcategory parent = myRecords.FirstOrDefault(e => e.subCategoryID == item.ParentCategoryID);
                    if (parent.ShortTitle == "")
                    {
                        s.DisplayName = string.Format("{0}-{1}", parent.SubCategoryName, child.SubCategoryName);
                    }
                    else
                    {
                        s.DisplayName = string.Format("{0}-{1}", parent.ShortTitle, child.SubCategoryName);
                    }
                }
                else
                {
                    //s.DisplayName = s.SubCategoryName;
                    if (s.ShortTitle != "")
                    {
                    s.DisplayName = string.Format("{0}-{1}", s.ShortTitle, s.SubCategoryName);
                    }
                    else
                    {
                        s.DisplayName = s.SubCategoryName;
                    }
                }
            }
            return (s.DisplayName);
        }


        public void DeleteRecord(subcategory record)
        {
            myRecords.Remove(record);
            context.subcategories.Remove(record);
            context.SaveChanges();
        }

        public Dictionary<int, string> GetIncomeCategoryNoParentList()
        {
            Dictionary<int, string> SubCategoryList = new Dictionary<int, string>(); 
           int categoryID = context.categories.FirstOrDefault(e => e.CategoryName == "Income").categoryID;
           int i = 0;
            list = myRecords.Where(e => e.Status == "Active")
           .Where(e => e.categoryID == categoryID)
           .OrderBy(e => (string)e.SubCategoryName);

            foreach (subcategory s in list)
            {
                i = context.subcategoryitems.Count(e => e.ParentCategoryID == s.subCategoryID);
                if(i==0)
                {
                    SubCategoryList.Add((int)s.subCategoryID, (string)s.SubCategoryName);
                }
            }

            return SubCategoryList;
        }
        public Dictionary<int, string> GetExpenseCategoryNoParentList()
        {
            Dictionary<int, string> SubCategoryList = new Dictionary<int, string>(); 
            int categoryID = context.categories.FirstOrDefault(e => e.CategoryName == "Expense").categoryID;
            int i = 0;
            list = myRecords.Where(e => e.Status == "Active")
           .Where(e => e.categoryID == categoryID)
           .OrderBy(e => (string)e.DisplayName);

            foreach (subcategory s in list)
            {
                i = context.subcategoryitems.Count(e => e.ParentCategoryID == s.subCategoryID);
                if (i == 0)
                {
                    
                   // SubCategoryList.Add((int)s.subCategoryID, (string)s.DisplayName);
                    SubCategoryList.Add((int)s.subCategoryID, (string)GetDisplayName(s.subCategoryID));
                }
            }

            return SubCategoryList;
        }
    }
}
