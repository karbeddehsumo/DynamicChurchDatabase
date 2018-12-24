using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;
using WebUI.Filters;

namespace WebUI.Controllers
{
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
    public class SubCategoryController : Controller
    {
         private churchdatabaseEntities db = new churchdatabaseEntities();
        private ISubCategoryRepository SubCategoryRepository;
        private ICategoryRepository CategoryRepository;
        private IBankAccountRepository BankAccountRepository;
        private IConstantRepository ConstantRepository;
        private ISubCategoryItemRepository SubCategoryItemRepository;
        private IIncomeRepository IncomeRepository;
        private IExpenseRepository ExpenseRepository;
        private IBudgetRepository BudgetRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public SubCategoryController(ISubCategoryRepository subCategoryParam, ICategoryRepository categoryParam, IBankAccountRepository bankAccountParam,
            IConstantRepository ConstantParam, ISubCategoryItemRepository SubCategoryItemParam, IIncomeRepository IncomeParam, IExpenseRepository ExpenseParam,
            IBudgetRepository BudgetParam)
        {
            SubCategoryRepository = subCategoryParam;
            CategoryRepository = categoryParam;
            BankAccountRepository = bankAccountParam;
            ConstantRepository = ConstantParam;
            SubCategoryItemRepository = SubCategoryItemParam;
            IncomeRepository = IncomeParam;
            ExpenseRepository = ExpenseParam;
            BudgetRepository = BudgetParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "FinanceStaff") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }
        //
        // GET: /SubCategory/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Index()
        {
            
            return PartialView();
        }

        //
        // GET: /SubCategory/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Details()
        {
            ViewBag.Year = System.DateTime.Now.Year.ToString();
            int id = 0;
            Dictionary<string, string> BudgetYearCreateList = new Dictionary<string, string>();
            int byear = DateTime.Now.Year;
            for (int i = byear-2; i <= byear + 1; i += 1)
            {
                BudgetYearCreateList.Add(i.ToString(), i.ToString());
            }
            ViewBag.BudgetYearCreateList = new SelectList(BudgetYearCreateList, "Key", "Value", id);
            return PartialView();
        }

        //
        // GET: /SubCategory/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Create(string CategoryType)
        {
            GetData(CategoryType);
            int categoryID = CategoryRepository.GetByCategoryName(CategoryType);
            return PartialView(new subcategory { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", CategoryType = CategoryType, categoryID = categoryID });
        } 

        //
        // POST: /SubCategory/Create
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(subcategory subcategory, int SubCategoryParentID = 0)
        {
            subcategory subcat = db.subcategories.FirstOrDefault(e => e.SubCategoryName == subcategory.SubCategoryName && e.categoryID == subcategory.categoryID);
            if (subcat != null)
            {
                TempData["Message2"] = string.Format("Category <{0}> already exist",subcat.SubCategoryName);
                return RedirectToAction("Index");
            }

            try
            {
                

                if (SubCategoryParentID > 0)
                {
                    subcategory ParentCategory = db.subcategories.Find(SubCategoryParentID);
                    subcategory.bankAccountID = ParentCategory.bankAccountID;
                    subcategory.categoryID = ParentCategory.categoryID;

                }

                if (ModelState.IsValid)
                {
                    db.subcategories.Add(subcategory);
                    db.SaveChanges();
                    SubCategoryRepository.AddRecord(subcategory);
                    TempData["Message2"] = "Category created successfully.";
                    if (SubCategoryParentID > 0)
                    {
                        subcategoryitem item = new subcategoryitem();
                        item.ParentCategoryID = SubCategoryParentID;
                        item.ChildCategoryID = subcategory.subCategoryID;
                        db.subcategoryitems.Add(item);
                        db.SaveChanges();
                    }

                    return RedirectToAction("Create", new { CategoryType = subcategory.CategoryType});
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error creating category";
            }
            subcategory.category = CategoryRepository.GetCategoryByID(subcategory.categoryID);
            GetData(subcategory.category.CategoryName);
            return PartialView(subcategory);
        }
        
        //
        // GET: /SubCategory/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Edit(int SubCategoryID,int BudgetYear = 0)
        {
           
            subcategory subcategory = SubCategoryRepository.GetBySubCategoryID(SubCategoryID);
            subcategory.category = CategoryRepository.GetCategoryByID(subcategory.categoryID);
            subcategory.BudgetYear = BudgetYear;
            GetData(subcategory.category.CategoryName);
            ViewBag.ParentCategoryName = "";
             bool HasParent = SubCategoryItemRepository.HasParent(subcategory.subCategoryID);
             if (HasParent)
             {
                 int parentCategoryID = SubCategoryItemRepository.GetCategoryByChild(subcategory.subCategoryID).ParentCategoryID;
                 ViewBag.ParentCategoryName = SubCategoryRepository.GetBySubCategoryID(parentCategoryID).SubCategoryName;
             }

            return PartialView(subcategory);
        }

        //
        // POST: /SubCategory/Edit/5
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(subcategory subcategory, int SubCategoryParentID = 0)
        {
            try
            {
                if (SubCategoryParentID > 0)
                {
                    subcategory ParentCategory = db.subcategories.Find(SubCategoryParentID);
                    subcategory.bankAccountID = ParentCategory.bankAccountID;
                    subcategory.categoryID = ParentCategory.categoryID;

                }

                IEnumerable<subcategoryitem> oldItems = SubCategoryItemRepository.GetCategoryByChildren(subcategory.subCategoryID);
                //Remove old subcatergory item listing
                if (oldItems != null)
                {
                    foreach (var i in oldItems.ToList())
                    {
                        SubCategoryItemRepository.DeleteRecord(i);
                    }
                }
                if (ModelState.IsValid)
                {
                    db.Entry(subcategory).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("record update successfully.");
                    if (SubCategoryParentID > 0)
                    {

                        subcategoryitem item = new subcategoryitem();
                        item.ParentCategoryID = SubCategoryParentID;
                        item.ChildCategoryID = subcategory.subCategoryID;
                        db.subcategoryitems.Add(item);
                        db.SaveChanges();


                    } 
                    
                    return RedirectToAction("List", new { CategoryType = subcategory.category.CategoryName, budgetYear = subcategory.BudgetYear });
                }
            }
             catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing record.");
            }
            return PartialView(subcategory);
        }

        //
        // GET: /SubCategory/Delete/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Delete(int SubCategoryID)
        {
            ViewBag.SubCategoryID = SubCategoryID;
            subcategory subcategory = SubCategoryRepository.GetBySubCategoryID(SubCategoryID);
            return PartialView(subcategory);
        }

        //
        // POST: /SubCategory/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int SubCategoryID, int BudgetYear = 0)
        {
            subcategory subcategory = SubCategoryRepository.GetBySubCategoryID(SubCategoryID);
            subcategory.category = CategoryRepository.GetCategoryByID(subcategory.categoryID);
            string CategoryName = subcategory.category.CategoryName;
            budget budget = BudgetRepository.GetBudgetByCategory(SubCategoryID);
            subcategoryitem child = SubCategoryItemRepository.GetCategoryByChild(SubCategoryID);
            int parentCount = SubCategoryItemRepository.GetCategoryByParent(SubCategoryID).Count();
            if ((budget != null) || (child != null) || (parentCount > 0))
            {
                TempData["Message2"] = string.Format("Can not delete record! Budget items attached to this category");
            }
            else
            {
                SubCategoryRepository.DeleteRecord(subcategory);
            }
            return RedirectToAction("List", new { CategoryType = CategoryName, budgetYear = BudgetYear });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult List(string CategoryType = "Income", int  budgetYear = 0, string SortType = "")
        {
            ViewBag.CategoryType = CategoryType;
            ViewBag.budgetYear = budgetYear;

            IEnumerable<subcategory> subCategoryList;
            subCategoryList = SubCategoryRepository.GetCategoryByCategoryType(CategoryType);
            ViewBag.RecordCount = subCategoryList.Count();
            int parentCategoryID;

            //calendar dates
            int year = budgetYear;
            string bDateString = "1/1/" + year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;
            ViewBag.CalendarBeginDate = bDate;

            string eDateString = "12/31/" + year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;
            ViewBag.CalendarEndDate = eDate;

            foreach (var i in subCategoryList)
            {
                i.category = CategoryRepository.GetCategoryByID(i.categoryID);
                i.bankaccount = BankAccountRepository.GetBankAccountByID(i.bankAccountID);
                if(CategoryType == "Income")
                {
                    i.TotalYTDAmount = IncomeRepository.GetTotalIncomeByCategory(i.subCategoryID, bDate.Date, eDate.Date);
                }
                else
                {
                    i.TotalYTDAmount = ExpenseRepository.GetTotalExpenseByCategoryIncludeChildren(i.subCategoryID, bDate.Date, eDate.Date);
                }

                bool HasParent = SubCategoryItemRepository.HasParent(i.subCategoryID);
                if (HasParent)
                {
                    parentCategoryID = SubCategoryItemRepository.GetCategoryByChild(i.subCategoryID).ParentCategoryID;
                    i.ParentCategoryName = SubCategoryRepository.GetBySubCategoryID(parentCategoryID).SubCategoryName;
                }
                else
                {
                    i.ParentCategoryName = "";
                }
            }
            if (SortType == "Category")
            {
                return PartialView(subCategoryList.OrderBy(e => e.SubCategoryName).ThenBy(e => e.ParentCategoryName));
            }
            else if (SortType == "Parent")
            {
                return PartialView(subCategoryList.OrderBy(e => e.ParentCategoryName).ThenBy(e => e.ShortTitle));
            }
            else if (SortType == "Bank Name")
            {
                return PartialView(subCategoryList.OrderBy(e => e.bankAccountID).ThenBy(e => e.SubCategoryName));
            }
            else if (SortType == "Short Title")
            {
                return PartialView(subCategoryList.OrderBy(e => e.ShortTitle).ThenBy(e => e.ParentCategoryName));
            }
            else
            {
                return PartialView(subCategoryList.OrderBy(e => e.ShortTitle).ThenBy(e => e.ParentCategoryName));
            }
            
        }

        public void GetData(string CategoryType)
        {
            int id = 0;

            Dictionary<int, string> BankAccountList;
            BankAccountList = BankAccountRepository.GetBankAccountList();
            ViewBag.BankAccountList = new SelectList(BankAccountList, "Key", "Value", id);

            Dictionary<int, string> CategoryList;
            CategoryList = CategoryRepository.GetCategoryList();
            ViewBag.CategoryList = new SelectList(CategoryList, "Key", "Value", id);


            Dictionary<int, string> SubCategoryList;
            if (CategoryType == "Expense")
            {
                SubCategoryList = SubCategoryRepository.GetExpenseCategoryList();
            }
            else
            {
                SubCategoryList = SubCategoryRepository.GetIncomeCategoryList();
            }
            ViewBag.SubCategoryList = new SelectList(SubCategoryList, "Key", "Value", id);

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

           

        }

    }
}