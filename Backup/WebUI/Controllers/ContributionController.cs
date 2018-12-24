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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using WebUI.Filters;

namespace WebUI.Controllers
{
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
    public class ContributionController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IFamilyRepository FamilyRepository;
        private IContributionRepository ContributionRepository;
        private ISubCategoryRepository SubCategoryRepository;
        private IPictureRepository PictureRepository;
        private ISpouseRepository SpouseRepository;
        private IMinistryRepository MinistryRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public ContributionController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IFamilyRepository FamilyParam,
            IContributionRepository ContributionParam, ISubCategoryRepository SubCategoryParam, IPictureRepository PictureParam, 
            ISpouseRepository SpouseParam, IMinistryRepository MinistryParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            FamilyRepository = FamilyParam;
            ContributionRepository = ContributionParam;
            SubCategoryRepository = SubCategoryParam;
            PictureRepository = PictureParam;
            SpouseRepository = SpouseParam;
            MinistryRepository = MinistryParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "FinanceLead")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
                if (user.role.Name == "WebMaster") //creator access
                {
                    ViewBag.WebMaster = true;
                }

                if ((user.role.Name == "FinanceStaff") || (user.role.Name == "Admin2")) //creator access
                {
                    ViewBag.Supervisor2 = true;
                }
            }
        }
        //
        // GET: /Default1/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Index()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Default1/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Details()
        {
            GetData();
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Default1/Create
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
      //  public ActionResult Create(DateTime dDate , int memberID = 0)
        public ActionResult Create( DateTime dDate, int memberID = 0)
        {
            ViewBag.DefaultDate = dDate;
            ViewBag.MemberID = memberID;
            ViewBag.MemberName = MemberRepository.GetMemberByID(memberID).FullNameLastFirstMiddle;
            DateTime edate = DateTime.Now;
            int dateCount = edate.DayOfYear; ;
            DateTime bdate = edate.AddDays(-dateCount + 1);

            ViewBag.BeginDate = bdate.Date;
            ViewBag.EndDate = edate.Date;
            GetData();
            //security
            ViewBag.Supervisor = false;
            int memberID2 = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID2 > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID2);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "FinanceLead")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                }
            }
            return PartialView(new contribution { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(),memberID=memberID });
        } 

        //
        // POST: /Default1/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(contribution contribution)
        {
            if (contribution.memberID == 0)
            {
                TempData["Message2"] = "Error adding contribution. Please select member.";
                GetData();
                ViewBag.Year = 0;
                return PartialView(contribution);
            }
            try
            {
                string memberName = db.members.Find(contribution.memberID).MemberName;
                if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }

                if (ModelState.IsValid)
                {
                    db.contributions.Add(contribution);
                    db.SaveChanges();
                    ContributionRepository.AddRecord(contribution);
                    TempData["Message2"] = "contribution record added successfully.";
                    GetData();
                    return RedirectToAction("CreateList", new {dDate=contribution.ContributionDate, memberID = contribution.memberID });
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding contribution";
            }
            GetData();
            return PartialView(contribution);
        }
        
        //
        // GET: /Default1/Edit/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult Edit(int ContributionID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnType = "", string ReturnSearchType = "", int ReturnCodeID = 0)
        {
            contribution contribution = ContributionRepository.GetContributionByID(ContributionID);
            if (ReturnType == "List")
            {
                contribution.ReturnBeginDate = ReturnBeginDate;
                contribution.ReturnEndDate = ReturnEndDate;
                contribution.ReturnCodeID = ReturnCodeID;
                contribution.ReturnSearchType = ReturnSearchType;
                contribution.ReturnType = ReturnType;
            }

            GetData();
            return PartialView(contribution);
        }

        //
        // POST: /Default1/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(contribution contribution)
        {
            member member = MemberRepository.GetMemberByID(contribution.memberID);
            try
            {
                if (contribution.CheckNumber == null) { contribution.CheckNumber = ""; }
                if (ModelState.IsValid)
                {
                    db.Entry(contribution).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} contribution update successfully.", member.FirstName);
                    GetData();
                    if (contribution.ReturnType == "List")
                    {
                        return RedirectToAction("List", new { bDate = contribution.ReturnBeginDate, eDate = contribution.ReturnEndDate, codeID = contribution.ReturnCodeID, SearchType = contribution.ReturnType });
                    }
                    else if (contribution.ReturnType == "ListActive")
                    {
                        //return RedirectToAction("ListActive", new { bDate = contribution.ReturnBeginDate, eDate = contribution.ReturnEndDate, memberID = contribution.ReturnCodeID});
                        return RedirectToAction("CreateList", new { dDate = DateTime.Now.Date,memberID = contribution.memberID });
                    }
                    else
                    {
                        return RedirectToAction("Create");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} contribution.", member.FirstName);
            }
            GetData();
            return PartialView(contribution);
        }

        //
        // GET: /Default1/Delete/5
 
        public ActionResult Delete(int ContributionID)
        {
            ViewBag.ContributionID = ContributionID;
            contribution contribution = ContributionRepository.GetContributionByID(ContributionID);
            return PartialView(contribution);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int ContributionID, DateTime ReturnBeginDate, DateTime ReturnEndDate, string ReturnType = "", string ReturnSearchType = "", int ReturnCodeID = 0)
        {
            contribution contribution = ContributionRepository.GetContributionByID(ContributionID);
            ContributionRepository.DeleteRecord(contribution);
            if (ReturnType == "List")
            {
                return RedirectToAction("List", new { bDate = ReturnBeginDate, eDate = ReturnEndDate, codeID = ReturnCodeID, SearchType = ReturnType });
            }
            else if (ReturnType == "ListActive")
            {
                return RedirectToAction("CreateList", new { dDate = DateTime.Now.Date, memberID = contribution.memberID });
            }
            else
            {
                return RedirectToAction("Create");
            }
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
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<int, string> FamilyList;
            FamilyList = FamilyRepository.GetFamilyList();
            ViewBag.FamilyList = new SelectList(FamilyList, "Key", "Value", id);

            Dictionary<int, string> MemberContrubutionFundList;
            MemberContrubutionFundList = ConstantRepository.GetConstantByCategoryID("Member Contribution");
            ViewBag.MemberContrubutionFundList = new SelectList(MemberContrubutionFundList, "Key", "Value", id);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Member,Admin2,Officer")]
        public ActionResult List(DateTime bDate, DateTime eDate, int codeID = 0, string SearchType = "",string CallingMethod = "Details")
        {
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.CodeID = codeID;
            ViewBag.SearchType = SearchType;
            ViewBag.AllowEdit = false;
            if (ViewBag.CallingMethod == "Details")
            {
                ViewBag.AllowEdit = true;
            }

            GetData();
            IEnumerable<contribution> ContributionList;
            ViewBag.Title = "";
            ViewBag.DefaultSearch = false;
            if (SearchType == "MemberSearch")
            {
                ContributionList = ContributionRepository.GetContributionByMemberDateRange(codeID, bDate, eDate);
                member member = MemberRepository.GetMemberByID(codeID);

                spouse spouse1 = SpouseRepository.GetSpouseByID1(codeID);
                spouse spouse2 = SpouseRepository.GetSpouseByID2(codeID);

                if (spouse1 != null)
                {
                    ViewBag.Title = string.Format("{0} Contributions ({1} - {2})", spouse1.JointTitheTitle, bDate.ToShortDateString(), eDate.ToShortDateString());
                }
                else if (spouse2 != null)
                {
                    ViewBag.Title = string.Format("{0} Contributions ({1} - {2})", spouse2.JointTitheTitle, bDate.ToShortDateString(), eDate.ToShortDateString());
                }
                else
                {
                    ViewBag.Title = string.Format("{0} Contributions ({1} - {2})", member.FullNameTitle, bDate.ToShortDateString(), eDate.ToShortDateString());
                }
            }
            else if (SearchType == "FundSearch")
            {
                ViewBag.DefaultSearch = true;
                ContributionList = ContributionRepository.GetContributionByCategoryRange(codeID, bDate, eDate);
                constant category = ConstantRepository.GetConstantID(codeID);
                ViewBag.Title = string.Format("{0} Contributions ({1} - {2})", category.Value1, bDate.ToShortDateString(), eDate.ToShortDateString());
            }
            else
            {
                ViewBag.DefaultSearch = true;
                ContributionList = ContributionRepository.GetContributionByDateRange(bDate, eDate);
                ViewBag.Title = string.Format("All Contributions ({0} - {1})", bDate.ToShortDateString(), eDate.ToShortDateString());
            }


            ViewBag.RecordCount = ContributionList.Count();
            foreach (var i in ContributionList)
            {
                i.FundName = ConstantRepository.GetConstantID(i.subCategoryID).Value1;
                i.member = MemberRepository.GetMemberByID(i.memberID);
            }
            decimal sum = ContributionList.Sum(e => e.Amount);
            ViewBag.Heading = string.Format("Total: {0:c}",sum);
            return PartialView(ContributionList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult ListActive(DateTime bDate, DateTime eDate, int memberID)
        {
            ViewBag.BeginDate = bDate;
            ViewBag.EndDate = eDate;
            ViewBag.CodeID = memberID;
            ViewBag.SearchType = "";

            GetData();
            IEnumerable<contribution> ContributionList;
            ContributionList = ContributionRepository.GetContributionByMemberDateRange(memberID, bDate.Date, eDate.Date).OrderByDescending(e => e.ContributionDate);
            ViewBag.RecordCount = ContributionList.Count();
            foreach (var i in ContributionList)
            {
                i.FundName = ConstantRepository.GetConstantID(i.subCategoryID).Value1;
                i.member = MemberRepository.GetMemberByID(i.memberID);
            }
            decimal sum = ContributionList.Sum(e => e.Amount);
            ViewBag.Heading = string.Format("Total Contribution: {0:c}", sum);
            return PartialView(ContributionList);
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public void TaxReport(int memberID)
        {
            string FileName = string.Format("{0}","ContributionTaxFile.pdf");
           // var path = Path.Combine(Server.MapPath("~/public_html/ClientFiles"), FileName);
            var path = Path.Combine(Server.MapPath("~/App_Data/ClientFiles"), FileName);
            bool exist = System.IO.File.Exists(string.Format("{0}", path));
            if (exist)
            {
                System.IO.File.Delete(string.Format("{0}", path));
                //System.IO.File.Delete(@"C:\test.txt");
            }

            member member = MemberRepository.GetMemberByID(memberID); 

            Document doc = new Document(iTextSharp.text.PageSize.LETTER, 10, 10, 42, 35);
            
            PdfWriter wri = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();

            //image
            picture icon = PictureRepository.GetLetterHeadImage();
            if (icon != null)
            {
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(icon.ImageData);
                //image.ScalePercent(50f);
                //image.SetAbsolutePosition(doc.PageSize.Width - 36f - 72f, doc.PageSize.Height - 36f - 216.6f);
                image.ScaleToFit(250f, 250f);
                // image.Border = iTextSharp.text.Rectangle.BOX;
                // image.BorderColor = iTextSharp.text.BaseColor.GREEN;
                // image.BorderWidth = 5f;
                doc.Add(image);
            }

            //get contribution data
            //ministry ministry = 
            string ChurchName = ConstantRepository.GetConstantByName("Church Name").Value1;
            string ChurchAddress = ConstantRepository.GetConstantByName("Church Address").Value1;
            string ChurchPhone = ConstantRepository.GetConstantByName("Church Phone").Value1;

            constant signature1 = ConstantRepository.GetConstantByName("TaxSignature1");
            constant signature2 = ConstantRepository.GetConstantByName("TaxSignature2");


            Paragraph data;
            data = new Paragraph(string.Format("{0}\n", ChurchName));
            doc.Add(data);
            data = new Paragraph(string.Format("{0}\n", ChurchAddress));
            doc.Add(data);
            data = new Paragraph(string.Format("{0}\n", ChurchPhone));
            doc.Add(data);

            data = new Paragraph(Environment.NewLine);
            doc.Add(data);

            data = new Paragraph("Member Contribution Tax Statement");
            doc.Add(data);

            data = new Paragraph(Environment.NewLine);
            doc.Add(data);


            string memberTitle = SpouseRepository.JointTitheTitle(memberID);
            data = new Paragraph(string.Format("For: {0}", memberTitle));
            doc.Add(data);

            data = new Paragraph(Environment.NewLine);
            doc.Add(data);

            int ContributionYear = DateTime.Today.Year;
            DateTime BeginDate = Convert.ToDateTime("1/1/" + ContributionYear.ToString());
            DateTime EndDate = Convert.ToDateTime("12/31/" + ContributionYear.ToString());
            int thiteID = ConstantRepository.GetConstantByName("Tithes").constantID;
            int offeringID = ConstantRepository.GetConstantByName("Offering").constantID;


            decimal TithesTotal = ContributionRepository.GetContributionByMemberCategoryRange(memberID, thiteID, BeginDate, EndDate).Sum(e => e.Amount);
            decimal OfferingTotal = ContributionRepository.GetContributionByMemberCategoryRange(memberID, offeringID, BeginDate, EndDate).Sum(e => e.Amount); 
            decimal Totalcontribution = ContributionRepository.GetContributionByMemberDateRange(memberID,BeginDate, EndDate).Sum(e => e.Amount);
            decimal OthersTotal = Totalcontribution - TithesTotal - OfferingTotal;


            data = new Paragraph(string.Format("Tithes: {0:c}", TithesTotal));
            doc.Add(data);

            data = new Paragraph(string.Format("Offerings: {0:c}", OfferingTotal));
            doc.Add(data);

            data = new Paragraph(string.Format("Others: {0:c}", OthersTotal));
            doc.Add(data);

            data = new Paragraph(string.Format("-----------------------------------"));
            doc.Add(data);

            data = new Paragraph(string.Format("Total: {0:c}", Totalcontribution));
            doc.Add(data);

            data = new Paragraph(Environment.NewLine);
            doc.Add(data);


            data = new Paragraph(string.Format("-----------------------------------"));
            doc.Add(data);

            if (signature1.Value1 != "")
            {
                data = new Paragraph(string.Format("Signed: {0} ({1})", signature1.Value2, signature1.Value1));
                doc.Add(data);
            }

            data = new Paragraph(Environment.NewLine);
            doc.Add(data);


            if (signature2.Value1 != "")
            {
                data = new Paragraph(string.Format("-----------------------------------"));
                doc.Add(data);
                data = new Paragraph(string.Format("Signed: {0} ({1})", signature2.Value2, signature2.Value1));
                doc.Add(data);
            }
            doc.Close();

            //document doc = DocumentRepository.GetDocumentByID(DocumentID);
            //string path = AppDomain.CurrentDomain.BaseDirectory + "App_Data/ClientFiles/";

            path = AppDomain.CurrentDomain.BaseDirectory + "App_Data/ClientFiles/";
           // path = AppDomain.CurrentDomain.BaseDirectory + "public_html/ClientFiles/";
            Response.ContentType = "application/pdf";
            
            Response.AddHeader("Content-Disposition", @"filename=""IT Report.xls""");      
            Response.TransmitFile(@path + FileName);
            //Response.TransmitFile(FileName);
        }

         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead,FinanceStaff,Admin2")]
        public ActionResult CreateList(DateTime dDate, int memberID = 0)
        {
            if (memberID==0)
             {
                 memberID = MemberRepository.GetMemberByStatus("Active").FirstOrDefault().memberID;
             }
            ViewBag.MemberID = memberID;
            ViewBag.Date = dDate;
            GetData();
            return PartialView();
        }

    }
}