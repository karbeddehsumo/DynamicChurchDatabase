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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class ListItemController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IListtableRepository ListTableRepository;
        private IListItemRepository ListItemRepository;
        private IListHeaderRepository ListHeaderRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ListItemController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IListtableRepository ListTableParam,
            IListItemRepository ListItemParam, IListHeaderRepository ListHeaderParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            ListTableRepository = ListTableParam;
            ListItemRepository = ListItemParam;
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
        // GET: /ListItem/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /ListItem/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /ListItem/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int ListTableID)
        {
            GetData();
            ViewBag.BeginDate = DateTime.Today;
            ViewBag.ListTableID = ListTableID;
            listtable table = ListTableRepository.GetListByID(ListTableID);
            if (table == null)
            {
                return Content("Table not found");
            }
            ViewBag.TableName = string.Format("Add record to table ({0})",table.Title);
            listheader header = ListHeaderRepository.GetByListTableID(ListTableID);
            ViewBag.TableHeaderID = header.ListHeaderID;
            ViewBag.Type1 = header.Type1;
            ViewBag.Type2 = header.Type2;
            ViewBag.Type3 = header.Type3;
            ViewBag.Type4 = header.Type4;
            ViewBag.Type5 = header.Type5;
            ViewBag.Type6 = header.Type6;
            ViewBag.Type7 = header.Type7;
            ViewBag.Type8 = header.Type8;
            ViewBag.Type9 = header.Type9;
            ViewBag.Type10 = header.Type10;

            return PartialView(new listitem { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), listTableID=ListTableID,Status="Active" });
        } 

        //
        // POST: /ListItem/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(listitem listitem)
        {
            try
            {
                if (listitem.Comment == null) { listitem.Comment = ""; }
                if (listitem.Value1 == null) { listitem.Value1 = ""; }
                if (listitem.Value2 == null) { listitem.Value2 = ""; }
                if (listitem.Value3 == null) { listitem.Value3 = ""; }
                if (listitem.Value4 == null) { listitem.Value4 = ""; }
                if (listitem.Value5 == null) { listitem.Value5 = ""; }
                if (listitem.Value6 == null) { listitem.Value6 = ""; }
               // if (listitem.Value7 == null) { listitem.Value7 = ""; }


                if (ModelState.IsValid)
                {
                    db.listitems.Add(listitem);
                    db.SaveChanges();
                    ListItemRepository.AddRecord(listitem);
                    TempData["Message2"] = "Table record added successfully.";
                    GetData();
                    return RedirectToAction("Create", new { ListTableID = listitem.listTableID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding table record";
            }
            GetData();
            return PartialView(listitem);
        }
        
        //
        // GET: /ListItem/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int ListItemID)
        {
            listitem listitem = ListItemRepository.GetListItemByID(ListItemID);
            GetData();
            listtable table = ListTableRepository.GetListByID(listitem.listTableID);
            ViewBag.TableName = string.Format("Add new record to the{0}", table.Title);
            listheader header = ListHeaderRepository.GetByListTableID(listitem.listTableID);
            ViewBag.Type1 = header.Type1;
            ViewBag.Type2 = header.Type2;
            ViewBag.Type3 = header.Type3;
            ViewBag.Type4 = header.Type4;
            ViewBag.Type5 = header.Type5;
            ViewBag.Type6 = header.Type6;
            ViewBag.Type7 = header.Type7;
            ViewBag.Type8 = header.Type8;
            ViewBag.Type9 = header.Type9;
            ViewBag.Type10 = header.Type10;

            return PartialView(listitem);
        }

        //
        // POST: /ListItem/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(listitem listitem)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (listitem.Comment == null) { listitem.Comment = ""; }
                if (listitem.Value1 == null) { listitem.Value1 = ""; }
                if (listitem.Value2 == null) { listitem.Value2 = ""; }
                if (listitem.Value3 == null) { listitem.Value3 = ""; }
                if (listitem.Value4 == null) { listitem.Value4 = ""; }
                if (listitem.Value5 == null) { listitem.Value5 = ""; }
                if (listitem.Value6 == null) { listitem.Value6 = ""; }
                // if (listitem.Value7 == null) { listitem.Value7 = ""; }

                if (ModelState.IsValid)
                {
                    db.Entry(listitem).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Table data update successfully.");
                    GetData();
                    //return RedirectToAction("Create", new { ListTableID = listitem.listTableID });
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing table data.");
            }
            GetData();
            return PartialView(listitem);
        }

        //
        // GET: /ListItem/Delete/5

        public ActionResult Delete(int ListItemID)
        {
            ViewBag.ListItemID = ListItemID;
            listitem listitem = ListItemRepository.GetListItemByID(ListItemID);
            return PartialView(listitem);
        }

        //
        // POST: /ListItem/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ListItemID)
        {
            listitem listitem = ListItemRepository.GetListItemByID(ListItemID);
            ListItemRepository.DeleteRecord(listitem);
            return RedirectToAction("Create", new { ListTableID = listitem.listTableID });
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

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

        }

        public ActionResult List(int ListTableID,bool canEdit = true)
        {
            ViewBag.CanEdit = canEdit;
            ViewBag.ListTableID = ListTableID;
            listtable table = ListTableRepository.GetListByID(ListTableID);
            ViewBag.TableName = table.Title;
            listheader header = ListHeaderRepository.GetByListTableID(ListTableID);
            ViewBag.Header = header;

            IEnumerable<listitem> ListItem = null;
           // ListItem = ListItemRepository.GetListItemByList(ListTableID);
            //ViewBag.RecordCount = ListItem.Count();

            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Officer") || (user.role.Name == "Admin")) //creator access
                {
                    ViewBag.Supervisor = true;
                    ListItem = ListItemRepository.GetAllListItemByListTable(ListTableID);
                    ViewBag.RecordCount = ListItem.Count();
                }
                else
                {
                    ListItem = ListItemRepository.GetActiveListItemByListTable(ListTableID);
                    ViewBag.RecordCount = ListItem.Count();
                }
            }

            ViewBag.HasComment = false;
            foreach (listitem i in ListItem)
            {
                if (i.Comment.Trim() != "")
                {
                    ViewBag.HasComment = true;
                }
            }
            return PartialView(ListItem);
        }

        public ActionResult ListStatus(int ListTableID, string Status)
        {
            ViewBag.ListTableID = ListTableID;
            listtable table = ListTableRepository.GetListByID(ListTableID);
            listheader header = ListHeaderRepository.GetByListTableID(ListTableID);
            ViewBag.Header = header;

            IEnumerable<listitem> ListItem;
            ListItem = ListItemRepository.GetListItemByListStatus(ListTableID,Status);
            ViewBag.RecordCount = ListItem.Count();

            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Officer") || (user.role.Name == "Admin")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
            }

            return PartialView(ListItem);
        }
    }
}