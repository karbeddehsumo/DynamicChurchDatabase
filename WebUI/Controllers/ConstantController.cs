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
  
    public class ConstantController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
     
        //
        // GET: /Member/

        public ConstantController(IConstantRepository constantParam, ISubCategoryRepository SubCategoryParam)
        {
            ConstantRepository = constantParam;
            SubCategoryRepository = SubCategoryParam;

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

        // GET: /Constant/

        public ActionResult Index()
        {
            return PartialView();
        }

        //
        // GET: /Constant/Details/5
        [RoleAuthentication(Roles = "WebMaster")]
        public ActionResult Details()
        {
            GetData();
            return PartialView();
        }

        //
        // GET: /Constant/Create
        [RoleAuthentication(Roles = "WebMaster")]
        public ActionResult Create()
        {
            GetData();
            return PartialView(new constant() { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 

        //
        // POST: /Constant/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(constant constant)
        {
            constant c = ConstantRepository.GetAllConstantList().FirstOrDefault(e => e.Category == constant.Category && e.ConstantName == constant.ConstantName);
            if (c != null)
            {
                TempData["Message2"] = string.Format("Constant <{0}> already exist for category <{1}>.", c.ConstantName,c.Category);
                return RedirectToAction("Create");
            }

            try
            {
                if (constant.Value2 == null) { constant.Value2 = "Null"; }
                if (constant.Value3 == null) { constant.Value3 = "Null"; }

                if (ModelState.IsValid)
                {
                    db.constants.Add(constant);
                    db.SaveChanges();
                    ConstantRepository.AddRecord(constant);
                    TempData["Message2"] = "Constant created successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error ocurred creating constant.";
            }
            return PartialView(constant);
        }
        
        //
        // GET: /Constant/Edit/5
 
        public ActionResult Edit(int ConstantID)
        {
            constant constant = ConstantRepository.GetConstantID(ConstantID);
            return PartialView(constant);
        }

        //
        // POST: /Constant/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(constant constant)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(constant).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", constant.ConstantName);
                    return RedirectToAction("List", new {});
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", constant.ConstantName);
            }
            return PartialView(constant);
        }

        //
        // GET: /Constant/Delete/5
 
        public ActionResult Delete(int ConstantID)
        {
            ViewBag.ConstantID = ConstantID;
            constant constant = ConstantRepository.GetConstantID(ConstantID);
            return PartialView(constant);
        }

        //
        // POST: /Constant/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ConstantID)
        {
            constant constant = ConstantRepository.GetConstantID(ConstantID);
            ConstantRepository.DeleteRedord(constant);
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [RoleAuthentication(Roles = "WebMaster")]
        public ActionResult List(string SearchType = "", string codeName = "")
        {
            IEnumerable<constant> constantList;

            if(SearchType == "CategorySearch")
            {
                constantList = ConstantRepository.GetConstantListByCategory(codeName);
            }
            else if (SearchType == "StatusSearch")
            {
                constantList = ConstantRepository.GetConstantByStatus(codeName);
            }
            else
            {
                constantList = ConstantRepository.GetAllConstantList();            
            }

            ViewBag.RecordCount = constantList.Count();
            return PartialView(constantList);
        }

        public void GetData()
        {
            int id = 0;
            List<string> ConstantList = db.constants.Select(e => e.Category).Distinct().ToList();
            ViewBag.ConstantList = new SelectList(ConstantList);

            Dictionary<string, string> CategoryList;
            CategoryList = ConstantRepository.GetConstantCategoryGroup();
            ViewBag.CategoryList = new SelectList(CategoryList, "Key", "Value", id);


            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

        }

        public void AddConstant(string aCategory, string value1, string ConstantName = "", int SortOrder = 0, string value2 = "", string value3 = "")
        {
           constant cont = new constant();
           cont.Category = aCategory;
           if (ConstantName == "")
           {
               cont.ConstantName = value1;
           }
           else
           {
               cont.ConstantName = ConstantName;
           }
           cont.Value1 = value1;
           if (SortOrder == 0)
            {
                cont.SortOrder = 1;
            }
            else
            {
                cont.SortOrder = SortOrder;
            }
           cont.Status = "Active";
           cont.Value2 = value2;
           cont.Value3 = value3;
           cont.EnteredBy = User.Identity.Name.ToString();
           cont.DateEntered = DateTime.Now;
           db.constants.Add(cont);
           db.SaveChanges();
           ConstantRepository.AddRecord(cont);

        }

        public void UpConstant(string aCategory, string value1, string ConstantName = "", int SortOrder = 0, string value2 = "", string value3 = "")
        {
            constant cont = ConstantRepository.GetConstantByName(ConstantName);
            if (cont != null)
            {
                cont.Category = aCategory;
                if (ConstantName == "")
                {
                    cont.ConstantName = value1;
                }
                else
                {
                    cont.ConstantName = ConstantName;
                }
                cont.Value1 = value1;
                if (SortOrder == 0)
                {
                    cont.SortOrder = 1;
                }
                else
                {
                    cont.SortOrder = SortOrder;
                }
                cont.Status = "Active";
                cont.Value2 = value2;
                cont.Value3 = value3;
                cont.EnteredBy = User.Identity.Name.ToString();
                cont.DateEntered = DateTime.Now;
                db.Entry(cont).State = EntityState.Modified;
                db.SaveChanges();
            }

        }

        public JsonResult RefreshViewBag(string SearchString)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            ConstantIntModel ListBox_Int = new ConstantIntModel();
            ConstantStringModel ListBox_String = new ConstantStringModel();
            

            int i = SearchString.IndexOf("&");
            string ViewBagTitle =  SearchString.Substring(0,i);
            string SearchingType = SearchString.Substring(i+1,1);
            if (SearchingType == "S") /*Dictionary<string,string>*/
            {
                Dictionary<string, string> list = ConstantRepository.GetConstantByCategory(ViewBagTitle);
                foreach (var item in list.OrderBy(e => e.Value))
                {
                    ListBox_String.key = item.Key;
                    ListBox_String.value = item.Value;
                    listItem.Add(new SelectListItem() { Text = ListBox_String.value, Value = ListBox_String.key.ToString() });
                }

                return Json(listItem, JsonRequestBehavior.AllowGet);
            }
            else if (SearchingType == "E") //Ministry Expense
            {
                Dictionary<int, string> list = ConstantRepository.GetConstantByMinistryExpenseType(Convert.ToInt16(ViewBagTitle));
                foreach (var item in list.OrderBy(e => e.Value))
                {
                    ListBox_Int.key = item.Key;
                    ListBox_Int.value = item.Value;
                    listItem.Add(new SelectListItem() { Text = ListBox_Int.value, Value = ListBox_Int.key.ToString() });
                }

                return Json(listItem, JsonRequestBehavior.AllowGet);
            }
            else if (SearchingType == "I") //Ministry Income
            {
                Dictionary<int, string> list = ConstantRepository.GetConstantByMinistryIncomeType(Convert.ToInt16(ViewBagTitle));
                foreach (var item in list.OrderBy(e => e.Value))
                {
                    ListBox_Int.key = item.Key;
                    ListBox_Int.value = item.Value;
                    listItem.Add(new SelectListItem() { Text = ListBox_Int.value, Value = ListBox_Int.key.ToString() });
                }

                return Json(listItem, JsonRequestBehavior.AllowGet);
            }
            else if (SearchingType == "K") // Income
            {
                Dictionary<int, string> list = SubCategoryRepository.GetIncomeCategoryList(); ;
                foreach (var item in list.OrderBy(e => e.Value))
                {
                    ListBox_Int.key = item.Key;
                    ListBox_Int.value = item.Value;
                    listItem.Add(new SelectListItem() { Text = ListBox_Int.value, Value = ListBox_Int.key.ToString() });
                }

                return Json(listItem, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Dictionary<int, string> list = ConstantRepository.GetConstantByCategoryID(ViewBagTitle);
                foreach (var item in list.OrderBy(e => e.Value))
                {
                    ListBox_Int.key = item.Key;
                    ListBox_Int.value = item.Value;
                    listItem.Add(new SelectListItem() { Text = ListBox_Int.value, Value = ListBox_Int.key.ToString() });
                }

                return Json(listItem, JsonRequestBehavior.AllowGet);
            }

           
        }

        public ActionResult HowToVideos()
        {
           

            return PartialView();
        }

        public ActionResult GetVideos(string VideoType)
        {
            ViewBag.VideoType = VideoType;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                ViewBag.UserType = user.role.Name;
            }
            return PartialView();
        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult MiscUpdate(string category, string ConstantName, string UpdateField)
        {
            ViewBag.Value1 = false;
            ViewBag.Value2 = false;
            ViewBag.Value3 = false;

            if (UpdateField == "Value1")
            {
                ViewBag.Value1 = true;
            }
            else if (UpdateField == "Value2")
            {
                ViewBag.Value2 = true;

            }
            else if (UpdateField == "Value3")
            {
                ViewBag.Value3 = true;

            }
            else
            {

            }
            constant constant = ConstantRepository.GetConstantByCategoryName(category,ConstantName);
            return PartialView(constant);
        }
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult GetConstantMisc()
        {
            return PartialView();
        }
    }
}