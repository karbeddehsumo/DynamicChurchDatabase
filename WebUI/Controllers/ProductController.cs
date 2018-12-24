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
   // [RoleAuthentication()]
    public class ProductController : Controller
    {
         private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IProductRepository ProductRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ProductController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IProductRepository ProductParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            ProductRepository = ProductParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "Officer") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }
        //
        // GET: /Pledge/

        public ViewResult Index()
        {
            GetData();
            return View();
        }

        //
        // GET: /Pledge/Details/5

        public ViewResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return View();
        }

        //
        // GET: /Pledge/Create

        public ActionResult Create()
        {
            GetData();
            return View(new product {DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString()});
        } 

        //
        // POST: /Product/Create
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.products.Add(product);
                    db.SaveChanges();
                    ProductRepository.AddRecord(product);
                    TempData["Message2"] = "Product added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding product";
            }

            return View(product);
        }
        
        //
        // GET: /Product/Edit/5
 
        public ActionResult Edit(int ProductID)
        {
            product product = ProductRepository.GetProductByID(ProductID);
            return View(product);
        }

        //
        // POST: /Product/Edit/5
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", product.Name);
                    GetData();
                    return RedirectToAction("Edit", new { ProductID = product.productID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", product.Name);
            }
            GetData();
            return View(product);
        }

        //
        // GET: /Product/Delete/5

         public ActionResult Delete(int ProductID)
        {
            ViewBag.ProductID = ProductID;
            product product = ProductRepository.GetProductByID(ProductID);
            return View(product);
        }

        //
        // POST: /Product/Delete/5

        [HttpPost, ActionName("Delete")]
         public ActionResult DeleteConfirmed(int ProductID)
        {
            product product = ProductRepository.GetProductByID(ProductID);
            ProductRepository.DeleteRecord(product);
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData()
        {
            int id = 0;

            Dictionary<int, string> ProductCategoryList;
            ProductCategoryList = ConstantRepository.GetConstantByCategoryID("Product Category");
            ViewBag.ProductCategoryList = new SelectList(ProductCategoryList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> ProductList;
            ProductList = ProductRepository.GetProductList();
            ViewBag.ProductList = new SelectList(ProductList, "Key", "Value", id);

        }

        public ActionResult List(decimal low = 0, decimal high = 0, string SearchType = "", int codeID = 0)
        {
            IEnumerable<product> ProductList;

            if (SearchType == "CategorySearch")
            {
                ProductList = ProductRepository.GetProductByCategory(codeID);
            }
            else if (SearchType == "Price Search")
            {
                ProductList = ProductRepository.GetProductByPriceRange(low, high);
            }
            else if (SearchType == "Quantity Search")
            {
                ProductList = ProductRepository.GetProductByQuantityRange(Convert.ToInt16(low), Convert.ToInt16(high));
            }
            else
            {
                ProductList = ProductRepository.GetAllProduct();
            }

            ViewBag.RecordCount = ProductList.Count();

            return View(ProductList);
        }
    }
}