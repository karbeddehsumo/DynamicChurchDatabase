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
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
    public class ListTableController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IListtableRepository ListTableRepository;
        private IListHeaderRepository ListHeaderRepository;
        private IListItemRepository ListItemRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ListTableController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IListtableRepository ListTableParam,
            IMinistryMemberRepository MinistryMemberParam, IListHeaderRepository ListHeaderParam, IListItemRepository ListItemParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            ListTableRepository = ListTableParam;
            ListHeaderRepository = ListHeaderParam;
            ListItemRepository = ListItemParam;
            MinistryMemberRepository = MinistryMemberParam;

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
        // GET: /ListTable/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /ListTable/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Details()
        {
            DateTime aDate = DateTime.Now;
            ViewBag.BeginDate = aDate.AddDays(-60).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();

            GetData();
            return PartialView();
        }

        //
        // GET: /ListTable/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Create(int ministryID)
        {
            GetData(ministryID);
            return PartialView(new listtable { DateCreated = System.DateTime.Today, DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", ministryID = ministryID, DefaultOpen = false });
        } 

        //
        // POST: /ListTable/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(listtable listtable)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }

                if (ModelState.IsValid)
                {
                    db.listtables.Add(listtable);
                    db.SaveChanges();
                    //ListTableRepository.AddRecord(listtable);
                    TempData["Message2"] = "Table record added successfully.";
                    GetData();
                    return Content("Table record added successfully");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding table";
            }
            GetData();
            return PartialView(listtable);
        }
        
        //
        // GET: /ListTable/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer")]
        public ActionResult Edit(int ListTableID)
        {
            
            listtable listtable = ListTableRepository.GetListByID(ListTableID);
            GetData(listtable.ministryID);
            ViewBag.TableHeader = string.Format("Edit Table ({0})",listtable.Title);
            return PartialView(listtable);
        }

        //
        // POST: /ListTable/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(listtable listtable)
        {
            try
            {
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(listtable).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Table update successfully.");
                    GetData(listtable.ministryID);
                    return RedirectToAction("Create", "ListItem", new { ListTableID = listtable.listTableID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} table data.", listtable.Title);
            }
            GetData(listtable.ministryID);
            return PartialView(listtable);
        }


        [HttpPost]
        public ActionResult DisableTable(int ListTableID)
        {
            listtable listtable = ListTableRepository.GetListByID(ListTableID);
            try
            { 
                listtable.Status = "Deleted";
                //if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(listtable).State = EntityState.Modified;
                    db.SaveChanges();
                    return Content("Table deleted successfully");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error deleteing {0} table.", listtable.Title);
            }
            GetData();
            return PartialView(listtable);
        }
        //
        // GET: /ListTable/Delete/5

        public ActionResult Delete(int ListTableID)
        {
            ViewBag.ListTableID = ListTableID;
            listtable listtable = ListTableRepository.GetListByID(ListTableID);
            return PartialView(listtable);
        }

        //
        // POST: /ListTable/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ListTableID)
        {
            listtable listtable = ListTableRepository.GetListByID(ListTableID);
            IEnumerable<listitem> litems = ListItemRepository.GetAllListItemByListTable(ListTableID);
            ListItemRepository.DeleteAllRecords(litems);
            listheader header = ListHeaderRepository.GetByListTableID(ListTableID);
            ListHeaderRepository.DeleteRecord(header);
            ListTableRepository.DeleteRecord(listtable);

            //return RedirectToAction("List");
            return Content("Table deleted successfully");
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

            Dictionary<int, string> ListTypeTable;
            ListTypeTable = ListTableRepository.GetList(ministryID);
            ViewBag.ListTypeTable = new SelectList(ListTypeTable, "Key", "Value", id);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "")
        {
            IEnumerable<listtable> ListTable;
            ViewBag.MinistryID = codeID;

            if (SearchType == "MinistrySearch")
            {
                ListTable = ListTableRepository.GetListByMinistry(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                ListTable = ListTableRepository.GetListByStatus(code);
            }
            else
            {
                ListTable = ListTableRepository.GetListByDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = ListTable.Count();
            GetData(codeID);
            return PartialView(ListTable);
        }

        public ActionResult Display(int ListTableID, bool canEdit = true)
        {
            ViewBag.CanEdit = canEdit;
            ViewBag.ListTableID = ListTableID;
            listtable table = ListTableRepository.GetListByID(ListTableID);
            listheader header = ListHeaderRepository.GetByListTableID(ListTableID);
            IEnumerable<listitem> items = ListItemRepository.GetAllListItemByListTable(ListTableID);

            ViewBag.HasHeader = false;
            if (header != null) 
            {
                ViewBag.HasHeader = true;
            }

           

            return PartialView(table);
        }

        public ActionResult DefaultOpenTables(int ministryID)
        {
            DateTime aDate = DateTime.Now;
            int days = aDate.DayOfYear;
            ViewBag.BeginDate = aDate.AddDays(-days + 1).ToShortDateString();
            ViewBag.EndDate = aDate.ToShortDateString();

            ViewBag.ministryID = ministryID;
            IEnumerable<listtable> DefaultOpenTables = ListTableRepository.GetListByDefaultOpen(ministryID);

            
            

            return PartialView(DefaultOpenTables);
        }

        public ActionResult GetMinistryGeneralTables(int ministryID = 0, string Requestor = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.Requestor = Requestor;
            IEnumerable<listtable> ListTable = ListTableRepository.GetListByMinistry(ministryID).OrderByDescending(e => e.DateCreated).Take(10);

            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);

            ViewBag.IsMember = false;


            ministrymember mm = MinistryMemberRepository.GetMinistryMemberByID(ministryID, memberID);
            if (mm != null)
            {
                ViewBag.IsMember = true;
            }

            ViewBag.RecordCount = ListTable.Count();
            GetData(ministryID);
            return PartialView(ListTable);
        }


        public void HideListTable(int ListTableID = 0)
        {
             listtable table = ListTableRepository.GetListByID(ListTableID);
             if (table != null)
             {

                 table.Status = "Inactive";
                 table.EnteredBy = User.Identity.Name.ToString();
                 table.DateEntered = DateTime.Now;
                 db.Entry(table).State = EntityState.Modified;
                 db.SaveChanges();
             }
        }
    }
}