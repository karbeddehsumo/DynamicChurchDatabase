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
using System.Text.RegularExpressions;
using WebUI.Filters;

namespace WebUI.Controllers
{
   // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class MemberController : Controller
    {
        public ERoleProviderRepository Roles { get; set; }

        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IFamilyRepository FamilyRepository;
        private IContributionRepository ContributionRepository;
        private ISpouseRepository SpouseRepository;
        private IChildParentRepository ChildParentRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMinistryRepository MinistryRepository;
        private IPictureRepository PictureRepository;
        private IVisitorRepository VisitorRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public MemberController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IFamilyRepository FamilyParam, IContributionRepository ContributionParam, IVisitorRepository VisitorParam,
                                ISpouseRepository SpouseParam, IChildParentRepository ChildParentParam, IMinistryMemberRepository MinistryMemberParam, IMinistryRepository MinistryParam, IPictureRepository PictureParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            FamilyRepository = FamilyParam;
            ContributionRepository = ContributionParam;
            SpouseRepository = SpouseParam;;
            ChildParentRepository = ChildParentParam;
            MinistryMemberRepository = MinistryMemberParam;
            MinistryRepository = MinistryParam;
            PictureRepository = PictureParam;
            VisitorRepository = VisitorParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Admin2")) //creator access
                {
                    ViewBag.Supervisor = true;
                }

                if ((user.role.Name == "Officer") || (user.role.Name == "FinanceLead")) //creator access
                {
                    ViewBag.Supervisor2 = true;
                }

                if (user.role.Name == "FinanceStaff") //creator access
                {
                    ViewBag.Supervisor3 = true;
                }
            }
        }
        //
        // GET: /Member/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Index()
        {
            GetData();
            return PartialView();
        }

        //
        // GET: /Member/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Details()
        {
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            GetData();
            return PartialView();
        }

        //
        // GET: /Member/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Create()
        {
            GetData();
            return PartialView(new member { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active"});
        } 

        //
        // POST: /Member/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(member member)
        {
            try
            {
                if (member.MiddleName == null) { member.MiddleName = ""; }
                if (member.ChurchTitle == null) { member.ChurchTitle = ""; }
                string aContactType = ConstantRepository.GetConstantID(member.ContactTypeID).Value1;
                if (((aContactType == "None") || ((aContactType == "Email Only"))) && (member.PhoneNumber == null))
                {
                    member.PhoneNumber = "999-999-9999";
                }

                if ((member.familyID == null)||(member.familyID == 0))
                {
                    family family = new family();
                    family.FamilyName = member.LastName;
                    family.Address = member.Address;
                    //family.Address2 = member.Address2;
                    family.City = member.City;
                    family.State =member.State;
                    family.Zip = member.Zip;
                    family.EnteredBy = member.EnteredBy;
                    family.DateEntered = member.DateEntered;
                    family.Status = member.Status;

                    db.families.Add(family);
                    db.SaveChanges();
                    MemberRepository.AddRecord(member);
                    member.familyID = family.familyID;
                }

                if (ModelState.IsValid)
                {

                    member.ContactType = ConstantRepository.GetConstantID(member.ContactTypeID).Value1;
                    if ((member.ContactType.Contains("Email")||((member.EmailAddress != null)&&(member.EmailAddress.Length > 0))))
                    {
                        if (MembershipRepositroy.IsUser(member.EmailAddress))
                        {
                            user user = MembershipRepositroy.GetUser(member.EmailAddress);
                            int UserMemberID = user.PersonID;
                            member UserMember = MemberRepository.GetMemberByID(UserMemberID);
                            if (UserMember.familyID == member.familyID)
                            {
                                member.ContactType = "None"; //enter member data without creating membership user record (Family sharing email )
                            }
                            else
                            {
                                TempData["Message2"] = string.Format("Member with email address <{0}> already exist.", member.EmailAddress);
                                GetData();
                                return PartialView(member);
                            }
                        }
                    }
                    else if (member.ContactType.Contains("Text"))
                    {
                        string ProviderEmail = ConstantRepository.GetConstantID((int)member.PhoneProviderID).Value2;
                        string phoneEmail = string.Format("{0}@{1}", member.PhoneNumber, ProviderEmail);

                        if (MembershipRepositroy.IsUser(phoneEmail))
                        {
                            user user = MembershipRepositroy.GetUser(phoneEmail);
                            int UserMemberID = user.PersonID;
                            member UserMember = MemberRepository.GetMemberByID(UserMemberID);
                            if (UserMember.familyID == member.familyID)
                            {
                                member.ContactType = "None"; //enter member data without creating membership user record (family sharing phone number)
                            }
                            else
                            {
                                TempData["Message2"] = string.Format("Member with phone number <{0}> already exist. Can not use for texting.", member.PhoneNumber);
                                GetData();
                                return PartialView(member);
                            }
                        }
                    }
                    else
                    {

                    }

                    db.members.Add(member);
                    db.SaveChanges();

                    //create user record
                    if (member.ContactType.Contains("None"))
                    {
                        TempData["Message2"] = "Member record added successfully.";
                        GetData();
                        return RedirectToAction("Create");
                    }
                    CreateUserAccount(member);
                                     
                    TempData["Message2"] = "Member record added successfully.";
                    GetData();

                    return RedirectToAction("Create");
                   // return Content("Member record added successfully");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error adding member";
            }
            GetData();
            return PartialView(member);
        }
        
        //
        // GET: /Member/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int MemberID)
        {
            GetData();
            ViewBag.MemberID = MemberID;
            ViewBag.TodayDate = DateTime.Now.ToShortDateString();
            member member = MemberRepository.GetMemberByID(MemberID);
            family family = FamilyRepository.GetFamilyByID((int)member.familyID);
            ViewBag.family = family;
            member.Address = family.Address;
            member.Address2 = family.Address2;
            member.City = family.City;
            member.State = family.State;
            member.Zip = family.Zip;
 
            int familySize = MemberRepository.GetMemberByFamily((int)member.familyID).Count();
            ViewBag.MultipleFamilyMembers = false;
            if (familySize > 1)
            {
                ViewBag.MultipleFamilyMembers = true;
            }



            return PartialView(member);
        }

        //
        // POST: /Member/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(member member)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                picture pic = new picture();
                foreach (var image in member.files)
                {
                    if (image != null)
                    {
                        pic.ImageMimeType = image.ContentType;
                        pic.ImageData = new byte[image.ContentLength];
                        image.InputStream.Read(pic.ImageData, 0, image.ContentLength);

                        pic.ministryID = 0;
                        pic.PictureDate = System.DateTime.Today;
                        pic.Status = "Active";
                        pic.Description = string.Format("MP&{0}", member.memberID);
                        pic.PictureType = "Member Picture";
                        pic.IsPublic = false;
                        pic.DateEntered = System.DateTime.Today;
                        pic.EnteredBy = User.Identity.Name.ToString();

                        db.pictures.Add(pic);
                        db.SaveChanges();
                        PictureRepository.AddRecord(pic);
                        member.PictureID = pic.pictureID;
                    }

                }

                if (member.MiddleName == null) { member.MiddleName = ""; }
                if (ModelState.IsValid)
                {
                    //update user membership email or text email
                    if(MembershipRepositroy.IsUser(member.memberID) &&(member.EmailAddress != null))
                    {
                        MembershipRepositroy.UpdateEmail(member.memberID,member.EmailAddress);
                    }
                    member.ContactType = ConstantRepository.GetConstantID(member.ContactTypeID).Value1;
                    if ((member.ContactType.Contains("Email") || ((member.EmailAddress != null) && (member.EmailAddress.Length > 0))))
                    {
                        if (MembershipRepositroy.IsUser(member.memberID))
                        {
                            user user = MembershipRepositroy.GetUser(member.EmailAddress);
                            if (user.PersonID != member.memberID)
                            {
                               // member OriginalMember = MemberRepository.GetMemberByID(user.PersonID);
                                //email used by anoter member
                                //errorMsg1 = string.Format("Error - Email currently used by {0}.", OriginalMember.FullNameTitle);
                            }
                            else if (user.Email != member.EmailAddress)
                            {

                              MembershipRepositroy.UpdateEmail(member.memberID, member.EmailAddress);
                            }
                        }
                    }
                    else if (member.ContactType.Contains("Text"))
                    {
                        string ProviderEmail = ConstantRepository.GetConstantID((int)member.PhoneProviderID).Value2;
                        string phoneEmail = string.Format("{0}@{1}", member.PhoneNumber, ProviderEmail);

                        user user = MembershipRepositroy.GetUser(phoneEmail);
                        if (user.PersonID != member.memberID)
                        {
                            //member OriginalMember = MemberRepository.GetMemberByID(user.PersonID);
                            //email used by anoter member
                            //errorMsg2 = string.Format("Error - Email currently used by {0}.", OriginalMember.FullNameTitle);
                        }
                        if (MembershipRepositroy.IsUser(member.memberID))
                        {
                            MembershipRepositroy.UpdateEmail(member.memberID, phoneEmail);
                        }
                    }

                    //update family data changes
                    family family = db.families.Find(member.familyID);
                    family.Address = member.Address;
                    family.Address2 = member.Address2;
                    family.City = member.City;
                    family.State = member.State;
                    family.Zip = member.Zip;
                    db.SaveChanges();

                    db.Entry(member).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", member.FirstName);
                    GetData();

                    //return Content(string.Format("{0} record update successfully.", member.FirstName));
                    return Redirect(ReturnUrl);
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", member.FirstName);
            }
            GetData();
            return PartialView(member);
        }

        public ActionResult MyPageEdit(int MemberID)
        {
            GetData();
            ViewBag.MemberID = MemberID;
            ViewBag.TodayDate = DateTime.Now.ToShortDateString();
            member member = MemberRepository.GetMemberByID(MemberID);
            family family = FamilyRepository.GetFamilyByID((int)member.familyID);
            ViewBag.family = family;
            ViewBag.FamilyID = member.familyID;
            member.Address = family.Address;
            member.Address2 = family.Address2;
            member.City = family.City;
            member.State = family.State;
            member.Zip = family.Zip;

            int familySize = MemberRepository.GetMemberByFamily((int)member.familyID).Count();
            ViewBag.MultipleFamilyMembers = false;
            if (familySize > 1)
            {
                ViewBag.MultipleFamilyMembers = true;
            }

            ViewBag.UpdateFamilyBirthdate = false;
            constant UpdateFamilyBirthdates = ConstantRepository.GetConstantByName("UpdateFamilyBirthdate");
            if (UpdateFamilyBirthdates.Value1 == "Yes")
            {
                ViewBag.UpdateFamilyBirthdate = true;
            }

            return PartialView(member);
        }

        //
        // POST: /Member/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult MyPageEdit(member member)
        {
            try
            {
                if (member.MiddleName == null) { member.MiddleName = ""; }
                if (ModelState.IsValid)
                {
                    //update family data changes
                    family family = db.families.Find(member.familyID);
                    family.Address = member.Address;
                    family.Address2 = member.Address2;
                    family.City = member.City;
                    family.State = member.State;
                    family.Zip = member.Zip;
                    db.SaveChanges();

                    db.Entry(member).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", member.FirstName);
                    GetData();

                    //return Content(string.Format("{0} record update successfully.", member.FirstName));
                    return RedirectToAction("MemberDataView", "Member");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", member.FirstName);
            }
            GetData();
            return PartialView(member);
        }

        //
        // GET: /Member/Delete/5
 
        public ActionResult Delete(int MemberID)
        {
            ViewBag.MemberID = MemberID;
            member member = MemberRepository.GetMemberByID(MemberID);
          /*  int contributionCount = ContributionRepository.GetContributionByMember(MemberID);
            int MinistryMemberCount = MinistryMemberRepository.GetMinistryMemberByMember(MemberID).Count();
            int FamilyMemberCount = MemberRepository.GetMemberByFamily((int)member.familyID).Count();
            spouse spouse1 = SpouseRepository.GetSpouseByID1(MemberID);
            spouse spouse2 = SpouseRepository.GetSpouseByID2(MemberID);
            if (contributionCount > 0)
            {
                return Content("Members has contribution records. Can not delete.");
            } 
            else if (MinistryMemberCount >0)
            {
                return Content("Members belongs to ministries. Remove before deleting.");
            }
            else if ((spouse1 != null) || (spouse2 != null))
            {
                return Content("Members is listed as married; Remove before deleting.");
            }
            else if (FamilyMemberCount == 1)
            {
                family family = FamilyRepository.GetFamilyByID((int)member.familyID);
                FamilyRepository.DeleteRecord(family);
            }
            else
            {
                user user = MembershipRepositroy.GetUserByID(MemberID);
                MembershipRepositroy.DeleteUser(user);
            }
            */
            return Content(string.Format("{0} record deleted successfully.",member.FullName));
        }

        //
        // POST: /Member/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int MemberID)
        {
            //member member = MemberRepository.GetMemberByID(MemberID);

            ViewBag.MemberID = MemberID;
            
            member member = MemberRepository.GetMemberByID(MemberID);
            int contributionCount = ContributionRepository.GetContributionByMember(MemberID);
            int MinistryMemberCount = MinistryMemberRepository.GetMinistryMemberByMember(MemberID).Count();
            int FamilyMemberCount = MemberRepository.GetMemberByFamily((int)member.familyID).Count();
            spouse spouse1 = SpouseRepository.GetSpouseByID1(MemberID);
            spouse spouse2 = SpouseRepository.GetSpouseByID2(MemberID);
            if (contributionCount > 0)
            {
                return Content("Members has contribution records. Can not delete.");
            }
            else if (MinistryMemberCount > 0)
            {
                return Content("Members belongs to ministries. Remove before deleting.");
            }
            else if ((spouse1 != null) || (spouse2 != null))
            {
                return Content("Members is listed as married; Remove before deleting.");
            }
            else if (FamilyMemberCount == 1)
            {
                family family = FamilyRepository.GetFamilyByID((int)member.familyID);
                FamilyRepository.DeleteRecord(family);
                if (MembershipRepositroy.IsUser(MemberID))
                {
                    user user = MembershipRepositroy.GetUserByID(MemberID);
                    MembershipRepositroy.DeleteUser(user);
                }
            }
            else
            {
                if (MembershipRepositroy.IsUser(MemberID))
                {
                    user user = MembershipRepositroy.GetUserByID(MemberID);
                    MembershipRepositroy.DeleteUser(user);
                }
            }
            MemberRepository.DeleteRecord(member);
           /// return RedirectToAction("Index");
            return Content(string.Format("{0} record deleted successfully",member.FullName));
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

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> MemberListAll;
            MemberListAll = MemberRepository.GetAllMembers();
            ViewBag.MemberListAll = new SelectList(MemberListAll, "Key", "Value", id);

            Dictionary<int, string> FamilyList;
            FamilyList = FamilyRepository.GetFamilyList();
            ViewBag.FamilyList = new SelectList(FamilyList, "Key", "Value", id);

            Dictionary<string, string> ChurchTitleList;
            ChurchTitleList = ConstantRepository.GetConstantByCategory("Church Title");
            ViewBag.ChurchTitleList = new SelectList(ChurchTitleList, "Key", "Value", id);

            Dictionary<int, string> MemberCategoryList;
            MemberCategoryList = ConstantRepository.GetConstantByCategoryID("Member Category");
            Dictionary<int, string> MemberCategoryList2;
            MemberCategoryList2 = ConstantRepository.GetConstantByCategoryID("Member Category List");
            foreach (var item in MemberCategoryList2)
            {
                MemberCategoryList.Add(item.Key, item.Value);
            }
            ViewBag.MemberCategoryList = new SelectList(MemberCategoryList, "Key", "Value", id);

            Dictionary<int, string> CellProviderList;
            CellProviderList = ConstantRepository.GetConstantByCategoryID("Cell Phone Provider");
            ViewBag.CellProviderList = new SelectList(CellProviderList, "Key", "Value", id);

            Dictionary<int, string> ContactTypeList;
            ContactTypeList = ConstantRepository.GetConstantByCategoryID("Contact Type");
            ViewBag.ContactTypeList = new SelectList(ContactTypeList, "Key", "Value", id);

            Dictionary<int, string> RoleList;
            RoleList = MembershipRepositroy.GetAllRoles();
            ViewBag.RoleList = new SelectList(RoleList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            MinistryList = MinistryRepository.GetMinistryList();
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

            Dictionary<string, string> SuffixList;
            SuffixList = ConstantRepository.GetConstantByCategory("Suffix");
            ViewBag.SuffixList = new SelectList(SuffixList, "Key", "Value", id);

            
        }

        public ActionResult List(DateTime Date, int codeID = 0,string categoryType = "")
        {
            Date = DateTime.Today;
            IEnumerable<member> memberList;
            ViewBag.FieldName = "Phone #";
            ViewBag.Heading = string.Format("{0} List", categoryType); 
            if (categoryType == "Men")
            {
               // memberList = MemberRepository.GetMemberByGender("Male");
                memberList = MemberRepository.GetMemberListCategory("Men");
            }
            else if (categoryType == "Women")
            {
               // memberList = MemberRepository.GetMemberByGender("Female");
                memberList = MemberRepository.GetMemberListCategory("Women");
            }
                /*
            else if ((categoryType == "Children") || (categoryType == "Youths") || (categoryType == "Young Adults") || (categoryType == "Adults") || (categoryType == "Seniors") || (categoryType == "Babies"))
            {
                
                constant cont = ConstantRepository.GetConstantID(codeID);
                memberList = MemberRepository.GetMemberByAge(Convert.ToInt16(cont.Value2), Convert.ToInt16(cont.Value3));
            }*/
            else if (categoryType == "Children")
            {
                memberList = MemberRepository.GetMemberListCategory("Children");
            }
            else if (categoryType == "Youths")
            {
                memberList = MemberRepository.GetMemberListCategory("Youth");
            }
            else if (categoryType == "Young Adults")
            {
                memberList = MemberRepository.GetMemberListCategory("Young Adults");
            }
            else if (categoryType == "Adults")
            {
                memberList = MemberRepository.GetMemberListCategory("Adults");
            }
            else if (categoryType == "Babies")
            {
                memberList = MemberRepository.GetMemberListCategory("Babies");
            }
            else if (categoryType == "Seniors")
            {
                memberList = MemberRepository.GetMemberListCategory("Seniors");
            }
            else if (categoryType == "Singles")
            {
                memberList = MemberRepository.GetMemberListCategory("Singles");
            }
            else if (categoryType == "Married Couples")
            {
                ViewBag.Heading = "Married Couple List";
                ViewBag.FieldName = "Married Couple";
                string Phone1, phone2;
                //memberList = MemberRepository.GetMemberByStatus("Active");
                memberList = MemberRepository.GetMemberListCategory("Marriage");
                foreach (var i in memberList)
                {
                    spouse spouse = SpouseRepository.GetSpouseByID1(i.memberID);
                    if (spouse == null)
                    {
                        spouse = SpouseRepository.GetSpouseByID2(i.memberID);

                    }
                    i.spouse = spouse;
                    Phone1 = MemberRepository.GetMemberByID(i.memberID).PhoneNumber;
                    phone2 = MemberRepository.GetMemberByID(i.spouse.spouse2ID).PhoneNumber;
                    if (((Phone1 != null) && (Phone1.Length > 0)) && ((Phone1 != null) && (Phone1.Length > 0)))
                    {
                        i.PhoneList = string.Format("{0} / {1}", Phone1, phone2);
                    }
                    else if ((Phone1 != null) && (Phone1.Length > 0))
                    {
                        i.PhoneList = Phone1;
                    }
                    else
                    {
                        i.PhoneList = phone2;
                    }

               }
            }
            else if (categoryType == "Upcoming Birthdays")
            {
                ViewBag.Heading = "Upcoming Birthday List";
                ViewBag.FieldName = "Birthdate";
                memberList = MemberRepository.GetMemberByDOB(Date);
            }
            else if (categoryType == "Wedding Anniversary")
            {
                IEnumerable<member> memberList2;
                ViewBag.FieldName = "Wedding Anniversary";
                ViewBag.Heading = "Upcoming Wedding Anniversary List";
                memberList2 = MemberRepository.GetWeddingAnniversary(Date);
                foreach (var i in memberList2)
                {
                    spouse spouse = SpouseRepository.GetSpouseByID1(i.memberID);
                    if (spouse == null)
                    {
                        spouse = SpouseRepository.GetSpouseByID2(i.memberID);
                    }
                    i.spouse = spouse;
                }
                memberList = memberList2.OrderBy(e => (string.Format("{0}/{1}", e.spouse.AnniversaryDate.Month, e.spouse.AnniversaryDate.Date)));
            }
            else if (categoryType == "Email List")
            {
                ViewBag.FieldName = "Email";
                ViewBag.Heading = "Email List";
                memberList = MemberRepository.GetMemberByStatus("Active");
            }
            else if (categoryType == "Address List")
            {
                ViewBag.FieldName = "Address";
                ViewBag.Heading = "Address List";
                memberList = MemberRepository.GetMemberByStatus("Active").GroupBy(e => e.familyID).ToList().Select(e => e.First());
                foreach (var i in memberList)
                {
                    i.family = FamilyRepository.GetFamilyByID((int)i.familyID);
                }

            }
            else
            {
                memberList = MemberRepository.GetMemberByStatus("Active");
            }
            ViewBag.RecordCount = 0;
            if (memberList.Count() > 0)
            {
                ViewBag.RecordCount = memberList.Count();
            }
            /*
                        if (ViewBag.FieldName == "Phone #")
                        {
                            foreach(var i in memberList)
                            {
                                i.PhoneList += PhoneRepository.GetPhoneByMemberString(i.memberID);                                           
                            }
                        }
             * */
            return PartialView(memberList.OrderBy(e => e.LastName).ThenBy(e => e.FirstName));
        }

        public ActionResult BirthdayAnniversaryList(DateTime Date, string categoryType = "")
        {
            IEnumerable<member> memberList = null;
            if (categoryType == "Upcoming Birthdays")
            {
                ViewBag.Heading = "Upcoming Birthday List";
                ViewBag.FieldName = "Birthdate";
                memberList = MemberRepository.GetMemberByDOB(Date);
            }
            else if (categoryType == "Wedding Anniversary")
            {
                IEnumerable<member> memberList2;
                ViewBag.FieldName = "Wedding Anniversary";
                ViewBag.Heading = "Upcoming Wedding Anniversary List";
                memberList2 = MemberRepository.GetWeddingAnniversary(Date);
                foreach (var i in memberList2)
                {
                    spouse spouse = SpouseRepository.GetSpouseByID1(i.memberID);
                    if (spouse == null)
                    {
                        spouse = SpouseRepository.GetSpouseByID2(i.memberID);
                    }
                    i.spouse = spouse;
                }
                memberList = memberList2.OrderBy(e => (string.Format("{0}/{1}", e.spouse.AnniversaryDate.Month, e.spouse.AnniversaryDate.Date)));
            }
            else
            {
            }
            ViewBag.RecordCount = memberList.Count();
            return PartialView(memberList);
        }

        public ActionResult MyPage()
        {
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
           //member data
            ViewBag.MemberID = memberID;
            member member = MemberRepository.GetMemberByID(memberID);
            family family = FamilyRepository.GetFamilyByID((int)member.familyID);
            ViewBag.FamilyAddress = family.Address;
            if ((family.Address2 != null) && (family.Address2.Length > 0))
            {
                ViewBag.FamilyAddress = string.Format("{0},{1}",family.Address,family.Address2);
            }
            
            ViewBag.FamilyCityStateZip = string.Format("{0}, {1} {2}",family.City,family.State,family.Zip);
            ViewBag.FamilyName = family.FamilyName;

            //Calendar dates
            DateTime edate = DateTime.Now;
            int dateCount = edate.DayOfYear-1;
            DateTime bdate = edate.AddDays(-dateCount);
            ViewBag.BDate = bdate;
            ViewBag.EDate = edate;

            DateTime aDate = DateTime.Now.Date;
            ViewBag.CalendarStartDate = aDate;
            ViewBag.CalendarEndDate = aDate.AddDays(60); //60 days of calendar

            ViewBag.Supervisor = false;
            //memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Admin")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
            }

            constant pledgeEntry = ConstantRepository.GetConstantByName("Member Pledge Entry");
            ViewBag.IsMemberPledgeEntry = pledgeEntry.Value1;
            ViewBag.MemberPledgeYear = pledgeEntry.Value2;
            return PartialView(member);
        }

        public ActionResult MyPage2(int memberID = 0)
        {
            if (memberID == 0)
            {
                memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            }
            else
            {
                user _user = MembershipRepositroy.GetUserByID(memberID);
                System.Web.HttpContext.Current.Session["personID"] = memberID;
                Roles = new ERoleProviderRepository(_user.Email);
                System.Web.HttpContext.Current.Session["personID"] = _user.PersonID;
                System.Web.HttpContext.Current.Session["email"] = _user.Email;
                System.Web.HttpContext.Current.Session["OldPassword"] = _user.Password;
                _user.role = Roles.GetRole(_user.RoleID);

                System.Web.HttpContext.Current.Session["memberRole"] = _user.role.Name;
            }
            //member data
            ViewBag.MemberID = memberID;
            member member = MemberRepository.GetMemberByID(memberID);
            family family = FamilyRepository.GetFamilyByID((int)member.familyID);
            ViewBag.FamilyAddress = family.Address;
            if ((family.Address2 != null) && (family.Address2.Length > 0))
            {
                ViewBag.FamilyAddress = string.Format("{0},{1}", family.Address, family.Address2);
            }

            ViewBag.FamilyCityStateZip = string.Format("{0}, {1} {2}", family.City, family.State, family.Zip);
            ViewBag.FamilyName = family.FamilyName;

            //Calendar dates
            DateTime edate = DateTime.Now;
            int dateCount = edate.DayOfYear - 1;
            DateTime bdate = edate.AddDays(-dateCount);
            ViewBag.BDate = bdate;
            ViewBag.EDate = edate;

            DateTime aDate = DateTime.Now.Date;
            ViewBag.CalendarStartDate = aDate;
            ViewBag.CalendarEndDate = aDate.AddDays(60); //60 days of calendar

            ViewBag.Supervisor = false;
            //memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            user user = MembershipRepositroy.GetUserByID(memberID);
            if ((user.role.Name == "WebMaster") || (user.role.Name == "Admin")) //creator access
            {
                ViewBag.Supervisor = true;
            }

            constant pledgeEntry = ConstantRepository.GetConstantByName("Member Pledge Entry");
            ViewBag.IsMemberPledgeEntry = pledgeEntry.Value1;
            ViewBag.MemberPledgeYear = pledgeEntry.Value2;
            return View(member);
        }


        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult AdminPage(int memberID = 0)
        {
            if (memberID > 0)
            {
                user _user = MembershipRepositroy.GetUserByID(memberID);
                System.Web.HttpContext.Current.Session["personID"] = memberID;
                Roles = new ERoleProviderRepository(_user.Email);
                System.Web.HttpContext.Current.Session["personID"] = _user.PersonID;
                System.Web.HttpContext.Current.Session["email"] = _user.Email;
                System.Web.HttpContext.Current.Session["OldPassword"] = _user.Password;
                _user.role = Roles.GetRole(_user.RoleID);

                System.Web.HttpContext.Current.Session["memberRole"] = _user.role.Name;
            }

            GetData();
            return PartialView();
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult AdminMemberPage(int memberID)
        {
            member member = MemberRepository.GetMemberByID(memberID);
            return PartialView(member);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public void AddSpouse(int spouse1ID, int spouse2ID, bool JointTithe, DateTime AnniversaryDate)
        {
            spouse spouse = SpouseRepository.GetSpouseByID1(spouse1ID);
            spouse spouse2 = SpouseRepository.GetSpouseByID1(spouse2ID);

            if (spouse != null)
            {
                SpouseRepository.DeleteRecord(spouse);
            }
            if (spouse2 != null)
            {
                SpouseRepository.DeleteRecord(spouse2);
            }

            member member1 = MemberRepository.GetMemberByID(spouse1ID);
            member member2 = MemberRepository.GetMemberByID(spouse2ID);
            string JointTitheTitle;
            if ((member1.Gender == "Male") && (member2.Gender == "Female"))
            {
                JointTitheTitle = string.Format("{0} & {1} {2}", member1.FirstNameTitle, member2.FirstNameTitle,member1.LastName);
            }
            else if ((member2.Gender == "Male") && (member1.Gender == "Female"))
            {
                JointTitheTitle = string.Format("{0} & {1} {2}", member2.FirstNameTitle, member1.FirstNameTitle,member2.LastName);
            }
            else
            {
                return;
            }

                spouse s = new spouse();
                s.spouse1ID = spouse1ID;
                s.spouse2ID = spouse2ID;
                s.JointTithe = JointTithe;
                s.JointTitheTitle = JointTitheTitle;
                s.AnniversaryDate = AnniversaryDate;
                db.spouses.Add(s);
                db.SaveChanges();
                SpouseRepository.AddRecord(s);
            

        }

        public string DeleteSpouse(int spouseID)
        {
            string returnMsg;

            spouse spouse = SpouseRepository.GetSpouseByID(spouseID);
            member member1 = MemberRepository.GetMemberByID(spouse.spouse1ID);
            member member2 = MemberRepository.GetMemberByID(spouse.spouse2ID);

            family oldFamily = FamilyRepository.GetFamilyByID((int)member1.familyID);
            family newFamily = new family();
            newFamily.FamilyName = oldFamily.FamilyName;
            newFamily.Address = oldFamily.Address;
            newFamily.Address2 = oldFamily.Address2;
            newFamily.City = oldFamily.City;
            newFamily.State = oldFamily.State;
            newFamily.Zip = oldFamily.Zip;
            newFamily.EnteredBy = User.Identity.Name.ToString();
            newFamily.DateEntered = DateTime.Today.Date;
            newFamily.Status = "Active";

            db.families.Add(newFamily);
            db.SaveChanges();

            //update member2 new family record
            member2.familyID = newFamily.familyID;
            db.Entry(member2).State = EntityState.Modified;
            db.SaveChanges();


            if (spouse.JointTithe == true)
            {
                returnMsg = "Couples nullified. Family was jointly tithing. Separate contributions accordingly.";
            }
            else
            {
                returnMsg = "Couples nullified.";
            }

            SpouseRepository.DeleteRecord(spouse);
            return returnMsg;
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public string JoinMaritalFamily(int memberID, int MaritalspouseID, DateTime AnniversaryDate, bool moveEntireFamily = false, bool JointTithe = false)
        {
            //add spouse here
            AddSpouse(memberID, MaritalspouseID, JointTithe, AnniversaryDate);

            int memberFamilyID = (int)MemberRepository.GetMemberByID(memberID).familyID;
            member spouse = db.members.Find(MaritalspouseID);
            int spouseFamilyID = (int)spouse.familyID;
            spouse.familyID = memberFamilyID;
            db.SaveChanges();
            family spousefamily = FamilyRepository.GetFamilyByID(spouseFamilyID);
            IEnumerable<member> familyMembers = MemberRepository.GetMemberByFamily(spouseFamilyID);
            if (familyMembers == null)
            {
                FamilyRepository.DeleteRecord(spousefamily);
            }
            else
            {
                if (moveEntireFamily)
                {

                    foreach (member m in familyMembers)
                    {
                        member member = db.members.Find(m.memberID);
                        member.familyID = memberFamilyID;
                        db.SaveChanges();
                    }

                }
            }

            return ("Spouse data added successfully.");
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        private string CreateUserAccount(member member)
        {
           // member member = MemberRepository.GetMemberByID(memberID);
            //create random pasword
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var password = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            member.ContactType = ConstantRepository.GetConstantID(member.ContactTypeID).Value1;
            if ((member.ContactType.Contains("Email") || ((member.EmailAddress != null) && (member.EmailAddress.Length > 0))))
            {
                if (MembershipRepositroy.IsUser(member.EmailAddress))
                {
                    user originalUser = MembershipRepositroy.GetUser(member.EmailAddress);
                    member OriginalMember = MemberRepository.GetMemberByID(originalUser.PersonID);
                    return (string.Format("Error - Email address ({0}) already used by {1}.",OriginalMember.EmailAddress, OriginalMember.FullNameTitle));

                }

                MembershipRepositroy.CreateUser(member.EmailAddress.Trim(), member.FullName.Trim(), password.Trim(), member.EmailAddress.Trim(), "Register", "Pending", "Pending", member.memberID, "Email");
                SendEmail(member,password);
            }
            else if (member.ContactType.Contains("Text"))
            {
                string PhoneNumber = Regex.Replace(member.PhoneNumber, @"[^\d]", String.Empty);
                string ProviderEmail = ConstantRepository.GetConstantID((int)member.PhoneProviderID).Value2;
                string phoneEmail = string.Format("{0}@{1}", PhoneNumber, ProviderEmail);

                if (MembershipRepositroy.IsUser(phoneEmail))
                {
                    user  originalUser = MembershipRepositroy.GetUser(phoneEmail);
                    member OriginalMember = MemberRepository.GetMemberByID(originalUser.PersonID);
                    return (string.Format("Error - phone number ({0}) already used by {1}.", PhoneNumber, OriginalMember.FullNameTitle));
                }
                MembershipRepositroy.CreateUser(phoneEmail, member.FullName, password, phoneEmail, "Register", "Pending", "Pending", member.memberID, "Phone");
                SendEmail(member,password);
            }
            else
            {
                if ((member.EmailAddress != null)&&(member.EmailAddress.Length > 0))
                {
                    MembershipRepositroy.CreateUser(member.EmailAddress, member.FullName, password, member.EmailAddress, "Register", "Pending", "Pending", member.memberID, "Email");
                    SendEmail(member, password);
                }
                //no user record created
            }
            return password;
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        private string SendEmail(member member,string password)
        {
            user user = MembershipRepositroy.GetUserByID(member.memberID);
            if (user.EmailType == "Email")
            {
                EmailController EmailServer = new EmailController(ConstantRepository, MemberRepository, MinistryMemberRepository, MinistryRepository, VisitorRepository, ChildParentRepository);
               // EmailServer.Index(member.FirstNameTitle, member.LastName, member.Email, password);
                EmailServer.Index(member.FirstNameTitle, member.LastName, user.Email, password);
            }
            else
            {
                EmailController EmailServer = new EmailController(ConstantRepository, MemberRepository, MinistryMemberRepository, MinistryRepository, VisitorRepository, ChildParentRepository);
               // EmailServer.Index(member.FirstNameTitle, member.LastName, member.Email, password);
                EmailServer.IndexText(member.FirstNameTitle, member.LastName, user.Email, password);
            }

            
            TempData["Message2"] = string.Format("{0} record edited successfully.", member.FirstName);
            return "Email sent!";

        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult ChildParentList(string MinistryType = "Children")
        {
            return PartialView();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public JsonResult GetFamilyMemberList(int MemberID)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            FamilyMemberModel listBox = new FamilyMemberModel();

            member member = MemberRepository.GetMemberByID(MemberID);

            Dictionary<int, string> Familylist = MemberRepository.GetMwmberByFamilyList(MemberID);

            foreach (var familyMember in Familylist)
            {
                listBox.key = familyMember.Key;
                listBox.value = familyMember.Value;
                listItem.Add(new SelectListItem() { Text = listBox.value, Value = listBox.key.ToString() });
            }

            return Json(listItem, JsonRequestBehavior.AllowGet); 
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult AddChildParent(int childID = 0, int parentID = 0, int ministryID3 = 0)
        {
            if (ChildParentRepository.HasParent(childID, parentID) == false)
            {
                childparent minor = new childparent();
                minor.ChildID = childID;
                minor.ParentID = parentID;
                db.childparents.Add(minor);
                db.SaveChanges();
                ChildParentRepository.AddRecord(minor);
            }

            //get parent list
            int id = 0;
            ministry ministry = MinistryRepository.GetMinistryByID(ministryID3);
            IEnumerable<member> minorList = MemberRepository.GetMemberListCategory(ministry.CodeDesc);
                Dictionary<int, string> ParentChildList = new Dictionary<int, string>();
                member child, parent;
                string ParentName = "",parentChildName = "",FamilyName="";
                foreach (member c in minorList)
                {
                    child = MemberRepository.GetMemberByID(c.memberID);
                    //IEnumerable<member> parents = MemberRepository.GetChildParent(c.memberID);
                    IEnumerable<member> parents = ChildParentRepository.GetParents(c.memberID);
                    if (parents == null)
                    {
                        continue;
                    }
                    ParentName = "";
                    foreach (member p in parents)
                    {
                        parent = MemberRepository.GetMemberByID(p.memberID);
                        if (ParentName == "")
                        {
                            ParentName = string.Format("{0}", parent.FirstNameTitle);
                            FamilyName = parent.LastName;
                        }
                        else
                        {
                            ParentName += string.Format(" & {0}", parent.FirstNameTitle);
                            FamilyName = parent.LastName;
                        }

                    }
                    if (parents.Count() > 0)
                    {
                        parentChildName = string.Format("{0} ({1})", c.FullName, ParentName + " " + FamilyName);
                        ParentChildList.Add(c.memberID, parentChildName);
                    }
                    ViewBag.ParentChildList = new SelectList(ParentChildList, "Key", "Value", id);
                }

            return PartialView();
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult MemberSecurity()
        {
            GetData();

            return PartialView();
        }

        public bool UpdateUserRole(int memberID, int roleID)
        {
            MembershipRepositroy.UpdateRole(memberID, roleID);
            return true;
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult GetMemberRoleList(int roleID)
        {
            List<string> userList = MembershipRepositroy.GetAllUsers(roleID);
            ViewBag.RecordCount = userList.Count();
            return PartialView(userList);
        }

        public ActionResult MemberDataView()
        {
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            //member data
            ViewBag.MemberID = memberID;
            member member = MemberRepository.GetMemberByID(memberID);
            family family = FamilyRepository.GetFamilyByID((int)member.familyID);
            ViewBag.FamilyAddress = family.Address;
            if ((family.Address2 != null) && (family.Address2.Length > 0))
            {
                ViewBag.FamilyAddress = string.Format("{0},{1}", family.Address, family.Address2);
            }

            ViewBag.FamilyCityStateZip = string.Format("{0}, {1} {2}", family.City, family.State, family.Zip);
            ViewBag.FamilyName = family.FamilyName;
            return PartialView(member);

        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public string UpdateStatus(int memberID, string status)
        {
            member member = db.members.Find(memberID);
            string returnString;
            if (status == "Active")
            {
                member.Status = "Active";
                returnString = string.Format("{0} record activated.", member.FullName);
            }
            else{
                member.Status = "Inactive";
                returnString = string.Format("{0} record deactivated.", member.FullName);
            }

            db.Entry(member).State = EntityState.Modified;
            db.SaveChanges();

            return (returnString);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public string ResendEmailInvite(int memberID = 0)
        {
            string returnString = ""; ;
            //get new password
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var password = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            
            member member = MemberRepository.GetMemberByID(memberID);
           if (MembershipRepositroy.IsUser(memberID) == false)
           {
               returnString = CreateUserAccount(member);
           }
           else
           {
               user user = MembershipRepositroy.GetUserByID(member.memberID);
               MembershipRepositroy.ChangePassword2(user.UserName, user.Password,password);
               user = MembershipRepositroy.GetUserByID(member.memberID);
               if (user.IsRegister)
               {
                   returnString = SendEmail(member, password);
               }
           }
           //return Redirect(ReturnUrl);
           return returnString;
        }

      //  public ActionResult EmailDialog(int[] ministryID = null, member[] memberList = null, string attachment = null)
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer,FinanceLead")]
        public ActionResult EmailDialog(int ministryID = 0, int memberID = 0, string RecipientType = "", bool SimpleForm = false)
        {
            ViewBag.SimpleForm = SimpleForm;
            string ReturnUrl = Request.UrlReferrer.ToString();
            ViewBag.ReturnUrl = ReturnUrl;
            GetData();
            ViewBag.MinistryID = ministryID;
            ViewBag.MemberID = memberID;
            ViewBag.RecipientType = RecipientType;
            int senderID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            member sender = MemberRepository.GetMemberByID(senderID);
            DateTime aDate = DateTime.Now;


            EmailModel EmailRecord = new EmailModel();
            EmailRecord.SenderCode = string.Format("{0}{1}{2}{3}", senderID,aDate.Year,aDate.Month,aDate.Day);
            

            EmailRecord.FromEmail = ConstantRepository.GetConstantByName("Church Email").Value1;
            if (sender.EmailAddress != null)
            {
                EmailRecord.FromName = string.Format("{0}", sender.FullName);
            }
            else
            {
                string PhoneProvideEmailString = ConstantRepository.GetConstantID((int)sender.PhoneProviderID).Value2;
                string PhoneEmailAddress = string.Format("{0}@{1}", sender.PhoneNumber, PhoneProvideEmailString);
                if ((PhoneProvideEmailString != null) && (sender.PhoneNumber != null))
                {
                    EmailRecord.FromName = string.Format("{0}", sender.FullName);
                }
            }

            if (ministryID > 0)
            {
                EmailRecord.To = MinistryRepository.GetMinistryByID(ministryID).MinistryName;
                ViewBag.SelectedType = "Ministry";
                EmailRecord.MinistryID = ministryID;
            }
            else if (memberID > 0)
            {
                ViewBag.SelectedType = "Member";
                EmailRecord.To = MemberRepository.GetMemberByID(memberID).FullName; ;
                EmailRecord.MemberID = memberID;
            }
            else
            {
                //EmailRecord.To = "";
               // ViewBag.SelectedType = "";
            }

            return PartialView(EmailRecord);
        }

         [ValidateAntiForgeryToken]
         [HttpPost]
         public ActionResult EmailDialog(EmailModel EmailRecord )
         {
             string ReturnUrl = Request.UrlReferrer.ToString();
             try
             {

                 var result = new EmailController(ConstantRepository, MemberRepository, MinistryMemberRepository, MinistryRepository, VisitorRepository, ChildParentRepository).EmailMinistry(EmailRecord); //call controller
                 TempData["Message2"] = result;
                 GetData();
                 return Redirect(ReturnUrl);
             }
             catch (Exception ex)
             {
                 TempData["Message2"] = "Error sending email";
             }
             GetData();
             return PartialView(EmailRecord);
         }


        [HttpPost]
        public ActionResult SendEmailDialog(EmailModel model)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            var image = model.files;
              
            if (image != null)
            {

            }
            else
            {

            }
                
            return Redirect(ReturnUrl);

        }


        public ActionResult RemoveMeFromFamilyView(int memberID)
        {
            GetData();
            ViewBag.MemberID = memberID;
            return PartialView();
        }

        public ActionResult AddSpouseView(int memberID)
        {
            GetData();
            ViewBag.MemberID = memberID;
            member member = MemberRepository.GetMemberByID(memberID);
            ViewBag.Header = string.Format("Select{0} spouse",member.FullNameTitle);
            return PartialView();
        }

        public ActionResult EditActiveMember()
        {
            GetData();
            ViewBag.DefaultmemberID = MemberRepository.GetMemberByStatus("Active").FirstOrDefault().memberID;
            return PartialView();
        }

        public ActionResult EditAllMember()
        {
            GetData();
            return PartialView();
        }

        public ActionResult UpdateMemberStatus()
        {
            GetData();
            return PartialView();
        }

        public ActionResult FamilyDOBDisplay(int familyID)
        {
            IEnumerable<member> familyMembers = MemberRepository.GetMemberByFamily(familyID);
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            ViewBag.MemberID = memberID;
            return PartialView(familyMembers);
        }

        public string UpdateFamilyDOB(int memberID, DateTime DOB)
        {
            member member = db.members.Find(memberID);
            member.DOB = DOB;
            string returnStr = string.Format("{0} birthdate updated.",member.FullNameTitle);
            db.SaveChanges();
            return returnStr;
        }

    }
}