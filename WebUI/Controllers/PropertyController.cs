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
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class PropertyController : Controller
    {
         private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IPropertyRepository PropertyRepository;
        private IMinistryRepository MinistryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public PropertyController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IPropertyRepository PropertyParam, IMinistryRepository MinistryParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            PropertyRepository = PropertyParam;
            MinistryRepository = MinistryParam;

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
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Index()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return View();
        }

        //
        // GET: /Pledge/Details/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Create()
        {
            GetData();
            return PartialView(new property { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 

        //
        // POST: /Property/Create
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(property property)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.properties.Add(property);
                    db.SaveChanges();
                    PropertyRepository.AddRecord(property);
                    TempData["Message2"] = "Property added successfully.";
                    GetData();
                    return Content("Property created successfully.");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding property";
            }

            return PartialView(property);
        }
        
        //
        // GET: /Property/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int PropertyID)
        {
            property property = PropertyRepository.GetPropertyByID(PropertyID);
            return PartialView(property);
        }

        //
        // POST: /Property/Edit/5
         [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(property property)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    //add property picture
                    picture pic = new picture();
                    foreach (var image in property.files)
                    {
                        if (image != null)
                        {                    
                            pic.ImageMimeType = image.ContentType;
                            pic.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(pic.ImageData, 0, image.ContentLength);
                        }

                        pic.ministryID = 0;
                        pic.PictureDate = System.DateTime.Today;
                        pic.Status = "Active";
                        pic.Description = string.Format("Picture:{0}", property.Title);
                        pic.DateEntered = System.DateTime.Today;
                        pic.EnteredBy = User.Identity.Name.ToString();

                        db.pictures.Add(pic);
                        db.SaveChanges();
                    }
                    property.PictureID = pic.pictureID;
                    db.Entry(property).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", property.Title);
                    GetData();
                    //return RedirectToAction("Edit", new { PropertyID = property.propertyID });
                   // return Content(string.Format("{0} record update successfully.", property.Title));
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", property.Title);
            }
            GetData();
            return PartialView(property);
        }

        //
        // GET: /Property/Delete/5

         public ActionResult Delete(int PropertyID)
        {
            ViewBag.PropertyID = PropertyID;
            property property = PropertyRepository.GetPropertyByID(PropertyID);
            return PartialView(property);
        }

        //
        // POST: /Property/Delete/5

        [HttpPost, ActionName("Delete")]
         public ActionResult DeleteConfirmed(int PropertyID)
        {
            property property = PropertyRepository.GetPropertyByID(PropertyID);
            PropertyRepository.DeleteRecord(property);
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
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> PropertyCategoryList;
            PropertyCategoryList = ConstantRepository.GetConstantByCategoryID("Property Category");
            ViewBag.PropertyCategoryList = new SelectList(PropertyCategoryList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<string, string> PledgeList;
            PledgeList = ConstantRepository.GetConstantByCategory("Pledge");
            ViewBag.PledgeList = new SelectList(PledgeList, "Key", "Value", id);

            Dictionary<string, string> MemberContributionList;
            MemberContributionList = ConstantRepository.GetConstantByCategory("Member Contribution");
            ViewBag.MemberContributionList = new SelectList(MemberContributionList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0)
        {
            IEnumerable<property> PropertyList;

            if (SearchType == "CategorySearch")
            {
                PropertyList = PropertyRepository.GetPropertyByCategory(codeID);
            }
            else if (SearchType == "StatusSearch")
            {
                PropertyList = PropertyRepository.GetPropertyByStatus(code);
            }
            else
            {
                PropertyList = PropertyRepository.GetAllProperty();
            }

            ViewBag.RecordCount = PropertyList.Count();

            return PartialView(PropertyList);
        }

       
    }
}