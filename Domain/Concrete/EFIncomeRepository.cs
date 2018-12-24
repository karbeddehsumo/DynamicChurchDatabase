using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFIncomeRepository : IIncomeRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private income record;
        private IEnumerable<income> list;

        private List<income> myRecords = new List<income>();

        private List<subcategoryitem> MoreSubcategoryChildren = new List<subcategoryitem>();
        private List<income> IncomeList = new List<income>();
        //private IEnumerable<subcategoryitem> MoreSubcategoryChildren = null;
        private IEnumerable<subcategoryitem> children = null;
        private IEnumerable<subcategoryitem> parent = null;



        public EFIncomeRepository()
        {
            myRecords = context.incomes.ToList(); 
        }

        public void AddRecord(income Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetIncomeList()
        {
            Dictionary<int, string> IncomeList;
            IncomeList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.incomeID, e => (string)e.Title);

            return (IncomeList);
        }

        public income GetIncomeByID(int incomeID)
        {
            record = myRecords.FirstOrDefault(e => e.incomeID == incomeID);
            return (record);
        }

        public IEnumerable<income> GetIncomeByCategory(int categoryID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.subCategoryID == categoryID && e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<income> GetIncomeByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<income> GetIncomeByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.IncomeDate == aDate.Date);
            return (list);
        }

        public IEnumerable<income> GetIncomeByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            return (list);
        }

        
        public IEnumerable<income> GetBudgetIncomeByDateRange(DateTime bDate, DateTime eDate)
        {

           var incomes =  from b in context.bankaccounts.Where(e => e.IsBudgeted == true)
                          join i in context.incomes.Where(e => e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date)
                         on b.bankAccountID equals i.bankAccountID
                        select i;

           return (incomes);
        }

        public IEnumerable<income> GetIncomeByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public decimal GetIncomeTotalByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            decimal cashTotal = (decimal)list.Sum(e => e.CashAmount);
            decimal checkTotal = (decimal)list.Sum(e => e.CheckAmount);
            decimal cointotal = (decimal)list.Sum(e => e.CoinAmount);
            decimal total = cashTotal + checkTotal + cointotal;
            return (total);

        }

        public decimal GetIncomeTotalByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            decimal cashTotal = (decimal) list.Sum(e => e.CashAmount);
            decimal checkTotal = (decimal)list.Sum(e => e.CheckAmount);
            decimal cointotal = (decimal)list.Sum(e => e.CoinAmount);
            decimal total = cashTotal + checkTotal + cointotal;
            return (total);
        }

        public decimal GetPendingIncomeTotalByBankAccount(int BankAccountID)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.Status == "Inactive");
            decimal cashTotal = (decimal)list.Sum(e => e.CashAmount);
            decimal checkTotal = (decimal)list.Sum(e => e.CheckAmount);
            decimal cointotal = (decimal)list.Sum(e => e.CoinAmount);
            decimal total = cashTotal + checkTotal + cointotal;
            return (total);
        }

        public decimal GetTotalIncomeByCategory(int categoryID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.subCategoryID == categoryID && e.IncomeDate >= bDate.Date && e.IncomeDate <= eDate.Date);
            decimal cashTotal = (decimal)list.Sum(e => e.CashAmount);
            decimal checkTotal = (decimal)list.Sum(e => e.CheckAmount);
            decimal cointotal = (decimal)list.Sum(e => e.CoinAmount);
            decimal total = cashTotal + checkTotal + cointotal;
            return (total);
        }


        public int GetLastIncomeRecordID()
        {
            record = myRecords.OrderByDescending(e => e.incomeID).Take(1).SingleOrDefault();
            return (record.incomeID);
        }

        public bool IncomeByBankAccount(int BankAccountID)
        {
            int i = myRecords.Where(e => e.bankAccountID == BankAccountID).Count();
            if (i > 0)
            {
                return true;
            }
            return false;

        }

        public IEnumerable<income> GetIncomeChildren(int categoryID, DateTime bDate, DateTime eDate)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                IEnumerable<subcategoryitem> children = context.subcategoryitems.Where(e => e.ParentCategoryID == categoryID).ToList();
                if(children.Count() > 0)
                {
                foreach (var child in children)
                {
                    parent = null;
                    using (churchdatabaseEntities context2 = new churchdatabaseEntities())
                    {
                        int i = context2.subcategoryitems.Where(e => e.ParentCategoryID == child.ChildCategoryID).Count();
                        if (i == 0)
                        {
                            var income = GetIncomeByCategory(child.ChildCategoryID, bDate, eDate);
                            if (income.Count() > 0)
                            {
                                if (IncomeList.Count() == 0)
                                {
                                    IncomeList = income.ToList();
                                }
                                else
                                {
                                    IncomeList.AddRange(income);
                                }

                            }
                        }
                        else
                        {
                            var extraChildren = context2.subcategoryitems.Where(e => e.ParentCategoryID == child.ChildCategoryID);
                            foreach (var item in extraChildren)
                            {
                                MoreSubcategoryChildren.Add(item); //store all subcategory with children
                            }
                        }
                    }
                }
                if (MoreSubcategoryChildren == null)
                {
                    return IncomeList;
                }
                var MoreSubcategoryChildren2 = MoreSubcategoryChildren;
                MoreSubcategoryChildren = null;
                foreach (var child2 in MoreSubcategoryChildren2)
                {
                    GetIncomeChildren(child2.ChildCategoryID, bDate, eDate);  //re-iterate subcategory with children
                }
                }
                else
                {
                    var income = GetIncomeByCategory(categoryID, bDate, eDate);
                    IncomeList.AddRange(income);
                }
            }
            return IncomeList.OrderBy(e => e.subCategoryID).ThenBy(e => e.IncomeDate);


        }

        public void InitializeVariable()
        {
            MoreSubcategoryChildren = new List<subcategoryitem>();
            IncomeList = new List<income>();
        }

        public void DeleteRecord(income record)
        {
            myRecords.Remove(record);
            context.incomes.Remove(record);
            context.SaveChanges();
        }

    }
}
