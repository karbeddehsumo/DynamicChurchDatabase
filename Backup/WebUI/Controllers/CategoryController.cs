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
  //   [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
    public class CategoryController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private ICategoryRepository CategoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public CategoryController(ICategoryRepository categoryParam)
        {
            CategoryRepository = categoryParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }

                    if ((user.role.Name == "Officer") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }

                    if ((user.role.Name == "FinanceStaff") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor3 = true;
                    }
                }
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Category/Details/5

        public ActionResult Details()
        {
            return View();
        }

        //
        // GET: /Category/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Create()
        {
            return View(new category() {Status = "Active" });
        } 

        //
        // POST: /Category/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(category category)
        {
            category cat = CategoryRepository.GetAllCategory().FirstOrDefault(e => e.CategoryName == category.CategoryName);
            if (cat != null)
            {
                TempData["Message2"] = string.Format("Category <{0}> already exist", cat.CategoryName);
                return RedirectToAction("Create");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    db.categories.Add(category);
                    db.SaveChanges();
                    CategoryRepository.AddRecord(category);
                    TempData["Message2"] = "Category created successfully.";
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error ocurred creating category.";
            }

            return View(category);
        }
        
        //
        // GET: /Category/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Edit(int CategoryID)
        {
            category category = CategoryRepository.GetCategoryByID(CategoryID);
            return View(category);
        }

        //
        // POST: /Category/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", category.CategoryName);
                    return RedirectToAction("Edit", new { CategoryID = category.categoryID });
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", category.CategoryName);
            }
            return View(category);
        }

        //
        // GET: /Category/Delete/5
 
        public ActionResult Delete(int CategoryID)
        {
            ViewBag.CategoryID = CategoryID;
            category category = CategoryRepository.GetCategoryByID(CategoryID);
            return View(category);
        }

        //
        // POST: /Category/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int CategoryID)
        {
            category category = CategoryRepository.GetCategoryByID(CategoryID);
            CategoryRepository.DeleteCategory(category);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult List()
        {
            IEnumerable<category> categoryList = CategoryRepository.GetAllCategory();
            ViewBag.RecordCount = categoryList.Count();
            return View(categoryList);
        }

    }
}