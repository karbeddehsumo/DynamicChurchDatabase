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
using System.IO;
using System.Text;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using WebUI.Filters;

namespace WebUI.Controllers
{ 
  //  [RoleAuthentication()]
    public class DocumentController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        IDocumentRepository DocumentRepository;
        private IMinistryRepository MinistryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public DocumentController(IConstantRepository ConstantParam, IDocumentRepository DocumentParam, IMinistryRepository MinistryParam)
        {
            ConstantRepository = ConstantParam;
            DocumentRepository = DocumentParam;
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
                        ViewBag.Supervisor2 = true;
                    }

                    if ((user.role.Name == "Officer") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor3 = true;
                    }
                }

            }
        }

        //
        // GET: /Document/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Index(int ministryID = 0, string CallerType = "")
        {
            ViewBag.MinistryID = ministryID;
            ViewBag.CallerType = CallerType;
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Document/Details/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Details(int ministryID = 0, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = ministryID;
            GetData(ministryID);
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Document/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Create(int MinistryID = 0, string CallerType = "")
        {
            GetData(MinistryID);
             ViewBag.Ministry = "";
             ViewBag.CallerType = CallerType;

            return PartialView(new document { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), ministryID = MinistryID, Status = "Inactive" });

        } 

        //
        // POST: /Document/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(document document, string CallerType = "")
        {
            string ReturnUrl = Request.UrlReferrer.ToString();

            try
            {
                if (document.SortOrder == null) { document.SortOrder = 3; }
                if (document.Version == null) { document.Version = "1.0"; }
                if (document.FileName == null) { document.FileName = document.Title; }
                if (document.TempFileName == null) { document.TempFileName = document.FileName; }


                if (ModelState.IsValid)
                {

                    foreach (var file in document.files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // Get file info
                            document.FileName = Path.GetFileName(file.FileName);
                            document.ContentLength = file.ContentLength;
                            document.ContentType = file.ContentType;

                            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                         //   var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                            file.SaveAs(path);
                            db.documents.Add(document);
                            db.SaveChanges();
                            DocumentRepository.AddRecord(document);

                        }
                        else
                        {
                            TempData["Message2"] = "File missing; Please select file.";
                            GetData((int)document.ministryID);
                            return Redirect("/Home/Admin?Page=Document");
                        }


                    }
                   
                    TempData["Message2"] = "Document record added successfully.";
                    GetData((int)document.ministryID);
                    return Redirect(ReturnUrl);
                    /*
                    if (CallerType == "Ministry")
                    {
                        return Redirect(string.Format("/Home/Admin?Page=Ministry&MinistryID={0}&CallerType={1}", document.ministryID, "Ministry"));
                    }
                    else
                    {
                        return Redirect("/Home/Admin?Page=Document");
                    }
                     */
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding document";
            }
            GetData();
            return PartialView(document);
        }
        
        //
        // GET: /Document/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Admin2,Officer")]
        public ActionResult Edit(int DocumentID, string CallerType = "")
        {
            ViewBag.CallerType = CallerType;
            document document = DocumentRepository.GetDocumentByID(DocumentID);
            GetData((int)document.ministryID);
            return PartialView(document);
        }

        //
        // POST: /Document/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(document document, string CallerType = "")
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    if (document.files != null)
                    {
                        foreach (var file in document.files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                // Get file info
                                document.FileName = Path.GetFileName(file.FileName);
                                document.ContentLength = file.ContentLength;
                                document.ContentType = file.ContentType;

                                var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
                               // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
                                file.SaveAs(path);
                                db.documents.Add(document);
                                db.SaveChanges();
                                DocumentRepository.AddRecord(document);

                            }
                        }
                    }

                    db.Entry(document).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Document update successfully.");
                    GetData((int)document.ministryID);
                    return Redirect(ReturnUrl);
                    /*
                    if (CallerType == "Ministry")
                    {
                        return Redirect(string.Format("/Home/Admin?Page=Ministry&MinistryID={0}&CallerType={1}", document.ministryID, "Ministry"));
                    }
                    else
                    {
                        return Redirect("/Home/Admin?Page=Document");
                    }
                     */
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing document.");
            }
            GetData();
            return PartialView(document);
        }

        //
        // GET: /Document/Delete/5

        public ActionResult Delete(int DocumentID)
        {
            ViewBag.DocumentID = DocumentID;
            document document = DocumentRepository.GetDocumentByID(DocumentID);
            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
           // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
            bool exist = System.IO.File.Exists(string.Format("{0}", path));
            if (exist)
            {
                System.IO.File.Delete(string.Format("{0}", path));
                //System.IO.File.Delete(@"C:\test.txt");
            }

            DocumentRepository.DeleteRecord(document);
            return PartialView(document);
        }

        //
        // POST: /Document/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int DocumentID, string CallerType = "")
        {
            document document = DocumentRepository.GetDocumentByID(DocumentID);
            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), document.FileName);
            // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), document.FileName);
            bool exist = System.IO.File.Exists(string.Format("{0}", path));
            if (exist)
            {
                System.IO.File.Delete(string.Format("{0}", path));
                //System.IO.File.Delete(@"C:\test.txt");
            }
            DocumentRepository.DeleteRecord(document);
           return RedirectToAction("Details");
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

            Dictionary<int, string> DocumentTypeList;
            DocumentTypeList = ConstantRepository.GetConstantByCategoryID("Document Type");
            ViewBag.DocumentTypeList = new SelectList(DocumentTypeList, "Key", "Value", id);

            Dictionary<int, string> MinistryList;
            if (ministryID > 0)
            {
                MinistryList = new Dictionary<int, string>();
                string MinistryName = MinistryRepository.GetMinistryByID(ministryID).MinistryName;
                MinistryList.Add(ministryID, MinistryName);
            }
            else
            {
                MinistryList = MinistryRepository.GetMinistryList();
            }
            ViewBag.MinistryList = new SelectList(MinistryList, "Key", "Value", id);

        }


        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int MinistryID = 0, string code = "", int codeID = 0, string CallerType = "", bool canEdit = true, string Requestor = "")
        {
            ViewBag.Requestor = Requestor;
            ViewBag.CanEdit = canEdit;
            ViewBag.CallerType = CallerType;
            ViewBag.MinistryID = MinistryID;

            GetData();
            IEnumerable<document> DocumentList;

            if (SearchType == "MinistrySearch")
            {
                DocumentList = DocumentRepository.GetDocumentByMinistry(MinistryID);
            }
            else if (SearchType == "StatusSearch")
            {
                DocumentList = DocumentRepository.GetDocumentByStatus(MinistryID, code);
            }
            else if (SearchType == "StatusOnlySearch")
            {
                DocumentList = DocumentRepository.GetDocumentByStatusOnly(code);
            }
            else if (SearchType == "DocumentTypeSearch")
            {
                DocumentList = DocumentRepository.GetDocumentByDocumentType(codeID);
            }
            else
            {
                DocumentList = DocumentRepository.GetAllDocument();
            }

            ViewBag.RecordCount = DocumentList.Count();

            return PartialView(DocumentList);
        }
/*
        public FilePathResult GetFileFromDisk(int DocumentID)
        {
            document doc = DocumentRepository.GetDocumentByID(DocumentID);
            string path = AppDomain.CurrentDomain.BaseDirectory + "App_Data/ClientFiles/";
            return File(path + doc.FileName, doc.ContentType, doc.FileName);
        }
*/
        public void GetFileFromDisk(int DocumentID)
        {
            
            document doc = DocumentRepository.GetDocumentByID(DocumentID);
            string path = AppDomain.CurrentDomain.BaseDirectory + "App_Data/ClientFiles/";
           // string path = AppDomain.CurrentDomain.BaseDirectory + "public_html/ClientFiles/";
  //          return File(path + doc.FileName, doc.ContentType, doc.FileName);


            Response.ContentType = doc.ContentType;
            Response.AddHeader("Content-Disposition", @"filename=""IT Report.xls""");
            Response.TransmitFile(@path + doc.FileName);
        }

        public ActionResult LatestDocument(int MinistryID)
        {
            GetData();
            IEnumerable<document> DocumentList;
            DocumentList = DocumentRepository.GetDocumentByMinistry(MinistryID);

            ViewBag.HasDocument = false;
            if (DocumentList.Count() > 0)
            {
                ViewBag.HasDocument = true;
            }
            return PartialView(DocumentList);
        }
    }
}