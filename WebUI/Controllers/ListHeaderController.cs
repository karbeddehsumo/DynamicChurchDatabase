using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain;
using WebUI.Models.churchdatabaseEntities;
using Domain.Abstract;
using Domain.Concrete;
using WebUI.Filters;

namespace WebUI.Controllers
{
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class ListHeaderController : Controller
    {
         private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IListtableRepository ListTableRepository;
        private IListHeaderRepository ListHeaderRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ListHeaderController(IConstantRepository ConstantParam, IListtableRepository ListTableParam,
            IListHeaderRepository ListHeaderParam)
        {
            ConstantRepository = ConstantParam;
            ListTableRepository = ListTableParam;
            ListTableRepository = ListTableParam;
            ListHeaderRepository = ListHeaderParam;

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
        // GET: /ListHeader/

        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /ListHeader/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /ListHeader/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int MinistryID)
        {
            GetData();
            return PartialView(new listheader { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), ministryID = MinistryID });
        } 

        //
        // POST: /ListHeader/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(listheader listheader)
        {
            try
            {
               // if (listheader.Type1 == null) { listheader.Type1 = ""; }
                if (listheader.Type2 == null) { listheader.Type2 = ""; }
                if (listheader.Type3 == null) { listheader.Type3 = ""; }
                if (listheader.Type4 == null) { listheader.Type4 = ""; }
                if (listheader.Type5 == null) { listheader.Type5 = ""; }
                if (listheader.Type6 == null) { listheader.Type6 = ""; }
                if (listheader.Type7 == null) { listheader.Type7 = ""; }
                if (listheader.Type8 == null) { listheader.Type8 = ""; }
                if (listheader.Type9 == null) { listheader.Type9 = ""; }
                if (listheader.Type10 == null) { listheader.Type10 = ""; }

               // if (listheader.FieldName1 == null) { listheader.FieldName1 = ""; }
                if (listheader.FieldName2 == null) { listheader.FieldName2 = ""; }
                if (listheader.FieldName3 == null) { listheader.FieldName3 = ""; }
                if (listheader.FieldName4 == null) { listheader.FieldName4 = ""; }
                if (listheader.FieldName5 == null) { listheader.FieldName5 = ""; }
                if (listheader.FieldName6 == null) { listheader.FieldName6 = ""; }
                if (listheader.FieldName7 == null) { listheader.FieldName7 = ""; }
                if (listheader.FieldName8 == null) { listheader.FieldName8 = ""; }
                if (listheader.FieldName9 == null) { listheader.FieldName9 = ""; }
                if (listheader.FieldName10 == null) { listheader.FieldName10 = ""; }

                if (ModelState.IsValid)
                {
                    listtable table = new listtable();
                    table.ministryID = listheader.ministryID;
                    table.Title = listheader.TableName;
                    table.Status = "Active";
                    table.ministryID = listheader.ministryID;
                    table.EnteredBy = User.Identity.Name.ToString();
                    table.DateEntered = System.DateTime.Today;
                    table.DateCreated = System.DateTime.Today;
                    ListTableRepository.AddRecord(table);

                    listheader.ListTableID = table.listTableID;
                    db.listheaders.Add(listheader);
                    db.SaveChanges();
                    ListHeaderRepository.AddRecord(listheader);
                    TempData["Message2"] = "Table header added successfully.";
                    GetData();
                    return RedirectToAction("Create", "ListItem", new { ListTableID = listheader.ListTableID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding table header";
            }
            GetData();

            return PartialView(listheader);
        }
        
        //
        // GET: /ListHeader/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int ListHeaderID)
        {
            listheader listheader = ListHeaderRepository.GetListHeaderByID(ListHeaderID);
            return PartialView(listheader);
        }

        //
        // POST: /ListHeader/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(listheader listheader)
        {
            try
            {
                // if (listheader.Type1 == null) { listheader.Type1 = ""; }
                if (listheader.Type2 == null) { listheader.Type2 = ""; }
                if (listheader.Type3 == null) { listheader.Type3 = ""; }
                if (listheader.Type4 == null) { listheader.Type4 = ""; }
                if (listheader.Type5 == null) { listheader.Type5 = ""; }
                if (listheader.Type6 == null) { listheader.Type6 = ""; }
                if (listheader.Type7 == null) { listheader.Type7 = ""; }
                if (listheader.Type8 == null) { listheader.Type8 = ""; }
                if (listheader.Type9 == null) { listheader.Type9 = ""; }
                if (listheader.Type10 == null) { listheader.Type10 = ""; }

                // if (listheader.FieldName1 == null) { listheader.FieldName1 = ""; }
                if (listheader.FieldName2 == null) { listheader.FieldName2 = ""; }
                if (listheader.FieldName3 == null) { listheader.FieldName3 = ""; }
                if (listheader.FieldName4 == null) { listheader.FieldName4 = ""; }
                if (listheader.FieldName5 == null) { listheader.FieldName5 = ""; }
                if (listheader.FieldName6 == null) { listheader.FieldName6 = ""; }
                if (listheader.FieldName7 == null) { listheader.FieldName7 = ""; }
                if (listheader.FieldName8 == null) { listheader.FieldName8 = ""; }
                if (listheader.FieldName9 == null) { listheader.FieldName9 = ""; }
                if (listheader.FieldName10 == null) { listheader.FieldName10 = ""; }

                if (ModelState.IsValid)
                {
                    db.Entry(listheader).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("List header update successfully.");
                    GetData();
                    return RedirectToAction("Create", "ListItem", new { ListTableID = listheader.ListTableID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing table header.");
            }
            GetData();
            return PartialView(listheader);
        }

        //
        // GET: /ListHeader/Delete/5

        public ActionResult Delete(int ListHeaderID)
        {
            ViewBag.ListHeaderID = ListHeaderID;
            listheader listheader = ListHeaderRepository.GetListHeaderByID(ListHeaderID);
            return PartialView(listheader);
        }

        //
        // POST: /ListHeader/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ListHeaderID)
        {
            listheader listheader = ListHeaderRepository.GetListHeaderByID(ListHeaderID);
            ListHeaderRepository.DeleteRecord(listheader);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int ministryID = 0)
        {
            int id = 0;

            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int,string> ListTypeTable;
            ListTypeTable = ListTableRepository.GetList(ministryID);
            ViewBag.ListTypeTable = new SelectList(ListTypeTable, "Key","Value", id);

        }

        public ActionResult List(int ListTableID)
        {
            listheader ListHeader;
            ListHeader = ListHeaderRepository.GetByListTableID(ListTableID);
            return PartialView(ListHeader);
        }
    }
}