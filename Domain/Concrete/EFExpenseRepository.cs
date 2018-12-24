using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFExpenseRepository : IExpenseRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private expense record;
        private IEnumerable<expense> list;

        private List<expense> myRecords = new List<expense>();

        private List<subcategoryitem> MoreSubcategoryChildren = new List<subcategoryitem>();
        private List<expense> ExpenseList = new List<expense>();
        //private IEnumerable<subcategoryitem> MoreSubcategoryChildren = null;
        private IEnumerable<subcategoryitem> children = null;
        private IEnumerable<subcategoryitem> parent = null;
        
        public EFExpenseRepository()
        {
            myRecords = context.expenses.ToList(); 
        }

        public void AddRecord(expense Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetExpenseList()
        {
            Dictionary<int, string> ExpenseList;
            ExpenseList = myRecords
            .OrderBy(e => (string)e.Payee)
            .ToDictionary(e => (int)e.expenseID, e => (string)e.Payee);

            return (ExpenseList);
        }


        public IEnumerable<expense> GetMostRecentPayeeList()
        {
            DateTime EndDate = DateTime.Now;
            DateTime BeginDate = EndDate.AddDays(-365);

            var PayeeList = myRecords.Where(e => e.ExpenseDate.Date >= BeginDate.Date && e.ExpenseDate.Date <= EndDate.Date)
                .GroupBy(e => e.Payee).ToList().Select(e => e.First())
            .OrderBy(e => (string)e.Payee);

            return (PayeeList);
        }

        public Dictionary<string, string> GetRecentPayeeList()
        {
            DateTime EndDate = DateTime.Now;
            DateTime BeginDate = EndDate.AddDays(-365);

            var PayeeList = context.expenses.Where(e => e.ExpenseDate >= BeginDate.Date && e.ExpenseDate <= EndDate.Date)
                            .GroupBy(e => e.Payee).ToList().Select(e => e.First()); //distinct


            var payeelist2 = PayeeList
            .OrderBy(e => (string)e.Payee)
            .ToDictionary(e => e.Payee, e => e.Payee);

            return (payeelist2);
        }

        public expense GetExpenseByID(int expenseID)
        {
            record = myRecords.FirstOrDefault(e => e.expenseID == expenseID);
            return (record);
        }

        public IEnumerable<expense> GetExpenseByCategory(int categoryID, DateTime bDate, DateTime eDate)
        {
            IList<expense> mylist;
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {

                mylist = myRecords.Where(e => e.subCategoryID == categoryID && e.ExpenseDate >= bDate.Date && e.ExpenseDate <= eDate.Date).ToList();
            }
            return (mylist.ToList());
        }

        public IEnumerable<subcategoryitem> GetSubCategoryChildren(int subcategoryID)
        {
            IList<subcategoryitem> mylist;
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {

                mylist = context.subcategoryitems.Where(e => e.ParentCategoryID == subcategoryID).ToList();
            }
            return (mylist.ToList());
        }

        public decimal GetTotalExpenseByCategoryIncludeChildren(int categoryID, DateTime bDate, DateTime eDate)
        {
            decimal totalExpense = 0;
            int i = 1;
            List<subcategoryitem> RemainingChildren = new List<subcategoryitem>(); 

            IEnumerable<subcategoryitem> children = context.subcategoryitems.Where(e => e.ParentCategoryID == categoryID);
            i = children.Count();
            if(i == 0)
            {
                totalExpense = GetExpenseByCategory(categoryID, bDate, eDate).Sum(e => e.Amount);
            }
            else{
                while (i > 0)
                {
                    foreach (var child in children)
                    {
                       // totalExpense += myRecords.Where(e => e.subCategoryID == child.ChildCategoryID && e.ExpenseDate >= bDate && e.ExpenseDate <= eDate).ToList().Sum(e => e.Amount);
                        totalExpense += GetExpenseByCategory(child.ChildCategoryID, bDate, eDate).Sum(e => e.Amount);
                        IEnumerable<subcategoryitem> gradnchildren = GetSubCategoryChildren(child.ParentCategoryID).ToList();
                        RemainingChildren.AddRange(gradnchildren);
                    }
                    children = RemainingChildren.ToList();
                    RemainingChildren.Clear();
                    i = RemainingChildren.Count();
                }
            }
            return (totalExpense);
        }

        public IEnumerable<expense> GetExpenseByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.ExpenseDate >= bDate.Date && e.ExpenseDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<expense> GetExpenseByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.ExpenseDate == aDate.Date);
            return (list);
        }

        public IEnumerable<expense> GetExpenseByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.ExpenseDate >= bDate.Date && e.ExpenseDate <= eDate.Date);
            return (list);
        }

        public expense GetExpenseByCheckNumber(string checkNumber)
        {
            record = myRecords.FirstOrDefault(e => e.CheckNumber == checkNumber);
            return (record);
        }

        public IEnumerable<expense> GetUnReconcile(int bankID)
        {
            list = myRecords.Where(e => e.bankAccountID == bankID && e.Reconcile == false || e.Reconcile == null && e.CheckNumber != null).OrderBy(e => e.CheckNumber);
            return (list);
        }

        public IEnumerable<expense> GetExpenseByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public decimal GetExpenseTotalByBankAccount(int BankAccountID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.ExpenseDate >= bDate.Date && e.ExpenseDate <= eDate.Date);
            return (list.Sum(e => e.Amount));
        }

        public decimal GetExpenseTotalByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.ExpenseDate >= bDate.Date && e.ExpenseDate <= eDate.Date);
            return (list.Sum(e => e.Amount));
        }

        public int GetLastExpenseRecordID()
        {
            record = myRecords.OrderByDescending(e => e.expenseID).Take(1).SingleOrDefault();
            return (record.expenseID);
        }

        public decimal GetPendingExpenseTotalByBankAccount(int BankAccountID)
        {
            list = myRecords.Where(e => e.bankAccountID == BankAccountID && e.Status == "Unpaid");
            return (list.Sum(e => e.Amount));
        }

        public bool ExpenseByBankAccount(int BankAccountID)
        {
            int i = myRecords.Where(e => e.bankAccountID == BankAccountID).Count();
            if(i>0)
            {
                return true;
            }
            return false;

        }

        public IEnumerable<expense> GetExpenseChildren(int categoryID, DateTime bDate, DateTime eDate)
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
                            var expense = GetExpenseByCategory(child.ChildCategoryID, bDate, eDate);
                            if (expense.Count() > 0)
                            {
                                if (ExpenseList.Count() == 0)
                                {
                                    ExpenseList = expense.ToList();
                                }
                                else
                                {
                                    ExpenseList.AddRange(expense);
                                }
                                
                            }
                        }
                        else
                        {
                            var extraChildren = context2.subcategoryitems.Where(e => e.ParentCategoryID == child.ChildCategoryID);
                            foreach (var item in extraChildren)
                            {
                                if(MoreSubcategoryChildren == null)
                                {
                                   MoreSubcategoryChildren = new List<subcategoryitem>();
                                }
                                MoreSubcategoryChildren.Add(item); //store all subcategory with children
                            }
                        }
                    }
                }
                if (MoreSubcategoryChildren == null)
                {
                    return ExpenseList;
                }
                var MoreSubcategoryChildren2 = MoreSubcategoryChildren;
                MoreSubcategoryChildren = null;
                foreach (var child2 in MoreSubcategoryChildren2)
                {
                    GetExpenseChildren(child2.ChildCategoryID, bDate, eDate);  //re-iterate subcategory with children
                }
                }
                else
                {
                    var expense = GetExpenseByCategory(categoryID, bDate, eDate);
                    ExpenseList.AddRange(expense);
                }
            }
            return ExpenseList.OrderBy(e => e.subCategoryID).ThenBy(e => e.ExpenseDate);

           
        }

        public void InitializeVariable()
        {
            MoreSubcategoryChildren = new List<subcategoryitem>();
            ExpenseList = new List<expense>();
        }


        public void DeleteRecord(expense record)
        {
            myRecords.Remove(record);
            context.expenses.Remove(record);
            context.SaveChanges();
        }
    }
}
