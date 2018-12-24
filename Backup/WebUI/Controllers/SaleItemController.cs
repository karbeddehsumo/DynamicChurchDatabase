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
    //[RoleAuthentication()]
    public class SaleItemController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
       private IConstantRepository ConstantRepository;
       private IProductRepository ProductRepository;
       private ISaleRepository SaleRepository;
       private ISaleItemRepository SaleItemRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public SaleItemController(IConstantRepository ConstantParam, IProductRepository ProductParam, ISaleRepository SaleParam, ISaleItemRepository SaleItemParam)
        {
            ConstantRepository = ConstantParam;
            ProductRepository = ProductParam;
            SaleRepository = SaleParam;
            SaleItemRepository = SaleItemParam;
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

        public ActionResult Create(int saleID = 0,int productID=0)
        {
            GetData();
            return View(new saleitem {DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", saleID = saleID, productID=productID});
        } 

        //
        // POST: /SaleItem/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(saleitem saleitem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.saleitems.Add(saleitem);
                    db.SaveChanges();
                    SaleItemRepository.AddRecord(saleitem);
                    TempData["Message2"] = "Sales item added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding sale item";
            }
            GetData();

            return View(saleitem);
        }
        
        //
        // GET: /SaleItem/Edit/5
 
        public ActionResult Edit(int SaleItemID)
        {
            saleitem saleitem = SaleItemRepository.GetSaleItemByID(SaleItemID);
            return View(saleitem);
        }

        //
        // POST: /SaleItem/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(saleitem saleitem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(saleitem).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Sale item record update successfully.");
                    GetData();
                    return RedirectToAction("Edit", new { SaleItemID = saleitem.SaleItemID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing sale item record.");
            }
            GetData();
            return View(saleitem);
        }

        //
        // GET: /SaleItem/Delete/5

        public ActionResult Delete(int SaleItemID)
        {
            saleitem saleitem = SaleItemRepository.GetSaleItemByID(SaleItemID);
            return View(saleitem);
        }

        //
        // POST: /SaleItem/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int SaleItemID)
        {
            saleitem saleitem = SaleItemRepository.GetSaleItemByID(SaleItemID);
            db.saleitems.Remove(saleitem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData()
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Sales Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> ProductList;
            ProductList = ProductRepository.GetProductList();
            ViewBag.ProductList = new SelectList(ProductList, "Key", "Value", id);

            Dictionary<int, string> SaleList;
            SaleList = ProductRepository.GetProductList();
            ViewBag.SaleList = new SelectList(SaleList, "Key", "Value", id);
        }

        public ActionResult List(int saleID)
        {
            IEnumerable<saleitem> SaleItemList;
            SaleItemList = SaleItemRepository.GetSalesItem(saleID);
            ViewBag.RecordCount = SaleItemList.Count();

            return View(SaleItemList);
        }
    }
}