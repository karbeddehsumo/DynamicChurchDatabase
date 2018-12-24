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
using System.IO;
using System.Text;
using WebUI.Filters;
using WebUI.Models;

namespace WebUI.Controllers
{
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
    public class PropertyInventoryController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IPropertyInventoryRepository PropertyInventoryRepository;
        private IPictureRepository PictureRepository;
        private IDocumentRepository DocumentRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public PropertyInventoryController(IConstantRepository ConstantParam, IMemberRepository MemberParam,
            IPropertyInventoryRepository PropertyInventoryParam, IPictureRepository PictureParam, IDocumentRepository DocumentParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            PropertyInventoryRepository = PropertyInventoryParam;
            PictureRepository = PictureParam;
            DocumentRepository = DocumentParam;

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
            return PartialView();
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
        public ActionResult Create(int propertyID = 0)
        {
            GetData();
            return PartialView(new propertyinventory { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", propertyID = propertyID });
        }         //
        // POST: /Inventory/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(propertyinventory inventory)
        {
            try
            {
                if (inventory.Comment == null) { inventory.Comment = ""; }
                if (ModelState.IsValid)
                {
                    db.propertyinventories.Add(inventory);
                    db.SaveChanges();
                    TempData["Message2"] = "Property inventory added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding property inventory";
            }
            GetData();
            return PartialView(inventory);
        }

        //
        // GET: /Inventory/Edit/5

     //   public ActionResult Edit(int PropertyInventoryID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnSearchType = "", int ReturnCodeID = 0, string ReturnCode = "", int ReturnCodeID2 = 0, decimal ReturnLowValue = 0, decimal ReturnHighValue = 0, string ReturnMethod = "")
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult Edit(int PropertyInventoryID) 
    {
            GetData();
            propertyinventory inventory = PropertyInventoryRepository.GetInventoryByID(PropertyInventoryID);
        /*
            inventory.ReturnBeginDate = ReturnBeginDate;
            inventory.ReturnEndDate = ReturnEndDate;
            inventory.ReturnSearchType = ReturnSearchType;
            inventory.ReturnCodeID = ReturnCodeID;
            inventory.ReturnCode = ReturnCode;
            inventory.ReturnCodeID2 = ReturnCodeID2;
            inventory.ReturnLowValue = ReturnLowValue;
            inventory.ReturnHighValue = ReturnHighValue;
            inventory.ReturnMethod = ReturnMethod;
         * */

            return PartialView(inventory);
        }

        //
        // POST: /Inventory/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(propertyinventory inventory)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (inventory.Comment == null) { inventory.Comment = ""; }
                if (ModelState.IsValid)
                {
                    //add property picture
                    picture pic = new picture();
                    foreach (var image in inventory.files)
                    {
                        if (image != null)
                        {
                            pic.ImageMimeType = image.ContentType;
                            pic.ImageData = new byte[image.ContentLength];
                            image.InputStream.Read(pic.ImageData, 0, image.ContentLength);

                            pic.ministryID = 0;
                            pic.PictureDate = System.DateTime.Today;
                            pic.Status = "Active";
                            pic.Description = string.Format("Picture:{0}", inventory.Title);
                            pic.DateEntered = System.DateTime.Today;
                            pic.EnteredBy = User.Identity.Name.ToString();

                            db.pictures.Add(pic);
                            db.SaveChanges();
                            PictureRepository.AddRecord(pic);
                            inventory.PictureID = pic.pictureID;
                        }
                      
                    }


                    //document
                    foreach (var file in inventory.documentFile)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            int documentTypeID = ConstantRepository.GetConstantByName("Property Document").constantID;
                            // Get file info
                            document document = new document();
                            document.Title = string.Format("{0} document",inventory.Title);
                            document.DocumentTypeID = documentTypeID;
                            document.Status = "Active";
                            document.EnteredBy = User.Identity.Name.ToString();
                            document.DateEntered = System.DateTime.Today;
                            document.FileName = Path.GetFileName(file.FileName);
                            document.ContentLength = file.ContentLength;
                            document.ContentType = file.ContentType;
                            document.Author = "Property Document";
                            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                            //var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                            file.SaveAs(path);
                            db.documents.Add(document);
                            db.SaveChanges();
                            inventory.DocumentID = document.documentID;
                        }

                    }

                    db.Entry(inventory).State = EntityState.Modified;
                    db.SaveChanges();
                   // PropertyInventoryRepository.AddRecord(inventory);
                    TempData["Message2"] = string.Format("Property record update successfully.", inventory.Title);
                    GetData();
                    //return Redirect("/Home/Admin?Page=Property");
                    //return Redirect("/PropertyInventory/PropertyInventoryCategoryList?categoryID=" + inventory.propertyID);
                    return Redirect(ReturnUrl);
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", inventory.Title);
            }
            GetData();
            return PartialView(inventory);
        }

        //
        // GET: /Inventory/Delete/5

        public ActionResult Delete(int PropertyInventoryID)
        {
            ViewBag.PropertyInventoryID = PropertyInventoryID;
            propertyinventory inventory = PropertyInventoryRepository.GetInventoryByID(PropertyInventoryID);
            return View(inventory);
        }

        //
        // POST: /Inventory/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int PropertyInventoryID)
        {
            propertyinventory inventory = PropertyInventoryRepository.GetInventoryByID(PropertyInventoryID);
            if(inventory.PictureID > 0)
            {
                picture picture = PictureRepository.GetPictureByID((int)inventory.PictureID);
                PictureRepository.DeleteRecord(picture);
            }
            if(inventory.DocumentID > 0)
            {
                document document = DocumentRepository.GetDocumentByID((int)inventory.DocumentID);
                DocumentRepository.DeleteRecord(document);
            }
            PropertyInventoryRepository.DeleteRecord(inventory);
            return RedirectToAction("Details");

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

            Dictionary<string, string> PropertyConditionList;
            PropertyConditionList = ConstantRepository.GetConstantByCategory("Property Condition");
            ViewBag.PropertyConditionList = new SelectList(PropertyConditionList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<string, string> AssignedToList;
            AssignedToList = ConstantRepository.GetConstantByCategory("Property AssignedTo");
            ViewBag.AssignedToList = new SelectList(AssignedToList, "Key", "Value", id);

            Dictionary<string, string> PropertyLocationList;
            PropertyLocationList = ConstantRepository.GetConstantByCategory("Property Location");
            ViewBag.PropertyLocationList = new SelectList(PropertyLocationList, "Key", "Value", id);

            Dictionary<int, string> PropertyCategoryList;
            PropertyCategoryList = ConstantRepository.GetConstantByCategoryID("Property Category");
            ViewBag.PropertyCategoryList = new SelectList(PropertyCategoryList, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0, decimal lowValue = 0, decimal highValue = 0)
        {
            /*
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.SearchType = SearchType;
            ViewBag.CodeID = codeID;
            ViewBag.Code = code;
            ViewBag.CodeID2 = codeID2;
            ViewBag.LowValue = lowValue;
            ViewBag.HighValue = highValue;
             * */

            IEnumerable<propertyinventory> InventoryList;

            if (SearchType == "AssignedToSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByAssignTo(code);
            }
            else if (SearchType == "LocationSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByLocation(code);
            }
            else if (SearchType == "PurchaseDateSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByPerchaseDateRange(bDate, eDate);
            }
            else if (SearchType == "QuantitySearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByQuantityRange(codeID, codeID2);
            }
            else if (SearchType == "StatusSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByStatus(code);
            }
            else if (SearchType == "TagSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByTag(code);
            }
            else if (SearchType == "ValueSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByValueRange(lowValue, highValue);
            }
            else if (SearchType == "ConditionSearch")
            {
                InventoryList = PropertyInventoryRepository.GetInventoryByCondition(code);
            }
            else
            {
                InventoryList = PropertyInventoryRepository.GetAllPropertyInventory();
            }

            ViewBag.RecordCount = InventoryList.Count();
            GetData();
            return PartialView(InventoryList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult PropertySummary()
        {


            IEnumerable<constant> summary = ConstantRepository.GetConstantListByCategory("Property Category");
            foreach (constant i in summary)
            {
                i.SortOrder = (int) PropertyInventoryRepository.GetInventoryByProperty(i.constantID).Sum(e => e.Quantity);
            }

            return PartialView(summary);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2")]
        public ActionResult PropertyInventoryCategoryList(int categoryID,string PropertyName = "")
        {
            ViewBag.PropertyName = PropertyName;
            IEnumerable<propertyinventory> categoryList = PropertyInventoryRepository.GetInventoryByProperty(categoryID);
            ViewBag.RecordCount = categoryList.Count();
            foreach (var p in categoryList)
            {
                if (p.PictureID > 0)
                {
                    p.picture = PictureRepository.GetPictureByID((int)p.PictureID);
                }
                if(p.DocumentID > 0)
                {
                    p.document = DocumentRepository.GetDocumentByID((int)p.DocumentID);
                }
            }
            return PartialView(categoryList);
        }

        public ActionResult RentalEmailDisplay(int ministryID)
        {
            ViewBag.TodayDate = DateTime.Today.Date;
            document RentalContract = DocumentRepository.GetDocumentByTitle("Rental Contract");
            document RentalPolicy = DocumentRepository.GetDocumentByTitle("Rental Policy");

            ViewBag.IsRentalContract = false;
            ViewBag.IsRentalPolicy = false;

            if (RentalContract != null)
            {
                ViewBag.IsRentalContract = true;
                ViewBag.RentalContractID = RentalContract.documentID;
            }

            if (RentalPolicy != null)
            {
                ViewBag.IsRentalPolicy = true;
                ViewBag.RentalPolicyID = RentalPolicy.documentID;
            }
            
            

            return PartialView();
        }

        public string SendRentalEmail(DateTime EventDate, string FullName, string PhoneNumber, string EmaiAddress, string BusinessName, string DescribeEvent)
        {
            try
            {
                RentProperty RentalRequest = new RentProperty();
                RentalRequest.EventDate = EventDate;
                RentalRequest.FullName = FullName;
                RentalRequest.PhnoeNumber = PhoneNumber;
                RentalRequest.EmailAddress = EmaiAddress;
                RentalRequest.BusinessName = BusinessName;
                RentalRequest.DescribeEvent = DescribeEvent;

                EmailController EmailServer = new EmailController(ConstantRepository);
                EmailServer.RentalEmail(RentalRequest);
                return "Email sent successfully";
            }
            catch (Exception ex)
            {
                return "Error sending email";
            }
        }
    }
} 