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
    //[RoleAuthentication]
    public class FamilyController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IFamilyRepository FamilyRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public FamilyController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IFamilyRepository FamilyParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            FamilyRepository = FamilyParam;

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
        // GET: /Family/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Family/Details/5

        public ActionResult Details()
        {
            GetData();
            return PartialView();
        }

        //
        // GET: /Family/Create

        public ActionResult Create()
        {
            GetData();
            return PartialView(new family { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active" });
        } 

        //
        // POST: /Family/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(family family)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.families.Add(family);
                    db.SaveChanges();
                    TempData["Message2"] = "family record added successfully.";
                    GetData();
                    FamilyRepository.AddRecord(family);
                    //return Content("Fami!ly record created successfully");
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding family";
            }
            GetData();
            return View(family);
        }
        
        //
        // GET: /Family/Edit/5
 
        public ActionResult Edit(int FamilyID)
        {
            family family = FamilyRepository.GetFamilyByID(FamilyID);
            GetData();
            return PartialView(family);
        }

        //
        // POST: /Family/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(family family)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(family).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", family.FamilyName);
                    GetData();
                    return RedirectToAction("List");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", family.FamilyName);
            }
            GetData();
            return View(family);
        }

        //
        // GET: /Family/Delete/5
 
        public ActionResult Delete(int FamilyID)
        {
            ViewBag.FamilyID = FamilyID;
            family family = FamilyRepository.GetFamilyByID(FamilyID);
            return PartialView(family);
        }

        //
        // POST: /Family/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int FamilyID)
        {
            family family = FamilyRepository.GetFamilyByID(FamilyID);
            int count = MemberRepository.GetMemberByFamily(FamilyID).Count();
            if (count == 0)
            {
                FamilyRepository.DeleteRecord(family);
                TempData["Message2"] = string.Format("{0} successfully deleted.", family.FamilyName);
            }
            else
            {
                TempData["Message2"] = string.Format("Can not delete {0} record. Has member(s) attached.", family.FamilyName);
            }

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

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> FamilyList;
            FamilyList = FamilyRepository.GetFamilyList();
            ViewBag.FamilyList = new SelectList(FamilyList, "Key", "Value", id);

            Dictionary<string, string> ChurchTitleList;
            ChurchTitleList = ConstantRepository.GetConstantByCategory("Church Title");
            ViewBag.ChurchTitleList = new SelectList(ChurchTitleList, "Key", "Value", id);
        }

        public ActionResult List(string FamilySearchType = "Active")
        {
            IEnumerable<family> familyList;

            if (FamilySearchType == "Active")
            {
                familyList = FamilyRepository.GetFamilyByStatus("Active");
            }
            else
            {
                familyList = FamilyRepository.GetFamilyByStatus("Inactive");
            }
            ViewBag.RecordCount = familyList.Count();

            return PartialView(familyList.OrderBy(e => e.FamilyName));
        }

        public ActionResult FamilyMemberList(int familyID)
        {
            IEnumerable<member> FamilyMembers = MemberRepository.GetMemberByFamily(familyID);
            family family = FamilyRepository.GetFamilyByID(familyID);
            ViewBag.Heading = string.Format("{0} Family Member", family.FamilyName);
            ViewBag.RecordCount = FamilyMembers.Count();
            return PartialView(FamilyMembers);
        }

        public void RemoveMeFromFamily(int memberID, string Address, string Address2, string City, string State, string Zip)
        {
            member member = db.members.Find(memberID);
            int originalFamilyID = (int)member.familyID;
            IEnumerable<member> familyMembers = MemberRepository.GetMemberByFamily(originalFamilyID);
            if (familyMembers.Count() > 1)
            {
                string originalFamilyName = FamilyRepository.GetFamilyByID(originalFamilyID).FamilyName;
                family family = new family();
                family.FamilyName = originalFamilyName;
                family.Address = Address;
                //family.Address2 = Address2;
                family.City = City;
                family.State = State;
                family.Zip = Zip;
                family.EnteredBy = User.Identity.Name.ToString();
                family.DateEntered = System.DateTime.Today;
                family.Status = "Active";
                FamilyRepository.AddRecord(family);
                //set member family to new familyid
                member.familyID = family.familyID;
                db.SaveChanges();
            }
        }


    }
}