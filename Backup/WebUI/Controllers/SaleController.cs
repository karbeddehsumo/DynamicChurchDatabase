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
    public class SaleController : Controller
    {
       private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IProductRepository ProductRepository;
        private ISaleRepository  SaleRepository;
        private ISaleItemRepository SaleItemRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public SaleController(IConstantRepository ConstantParam, IProductRepository ProductParam, ISaleRepository SaleParam, ISaleItemRepository SaleItemParam)
        {
            ConstantRepository = ConstantParam;
            ProductRepository = ProductParam;
            SaleRepository = SaleParam;
            SaleItemRepository = SaleItemParam;

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
            return View(new sale {DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString()});
        } 

        //
        // POST: /Sale/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(sale sale)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.sales.Add(sale);
                    db.SaveChanges();
                    SaleRepository.AddRecord(sale);
                    TempData["Message2"] = "Sale added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding Sale";
            }
            GetData();

            return View(sale);
        }
        
        //
        // GET: /Sale/Edit/5

        public ActionResult Edit(int SaleID)
        {
            sale sale = SaleRepository.GetSaleByID(SaleID);
            return View(sale);
        }

        //
        // POST: /Sale/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(sale sale)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(sale).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Sale update successfully.");
                    GetData();
                    return RedirectToAction("Edit", new { SaleID = sale.saleID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing sale.");
            }
            GetData();
            return View(sale);
        }

        //
        // GET: /Sale/Delete/5

        public ActionResult Delete(int SaleID)
        {
            ViewBag.SaleID = SaleID;
            sale sale = SaleRepository.GetSaleByID(SaleID);
            return View(sale);
        }

        //
        // POST: /Sale/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int SaleID)
        {
            sale sale = SaleRepository.GetSaleByID(SaleID);
            SaleRepository.DeleteRecord(sale);
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

        public ActionResult List(DateTime bDate, DateTime eDate)
        {
            IEnumerable<sale> SaleList;

            SaleList = SaleRepository.GetSalesByDateRange(bDate, eDate);

            ViewBag.RecordCount = SaleList.Count();

            return View(SaleList);
        }
    }
}