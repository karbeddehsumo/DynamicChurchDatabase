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
  //  [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
    public class PledgeController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMemberRepository MemberRepository;
        private IPledgeRepository PledgeRepository;
        private IContributionRepository ContributionRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public PledgeController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IPledgeRepository PledgeParam, IContributionRepository ContributributionParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            PledgeRepository = PledgeParam;
            ContributionRepository = ContributributionParam;

            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
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
        }
        //
        // GET: /Pledge/
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Index()
        {
            GetData();
            
            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);
                if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "FinanceLead")) //creator access
                {
                    ViewBag.Supervisor = true;
                }
            }
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5
        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult Details()
        {
            GetData();
            ViewBag.PledgeYear = DateTime.Now.Year;
            return PartialView();
        }

        //
        // GET: /Pledge/Create

        public ActionResult Create(int Year = 0)
        {
            ViewBag.Year = Year;
            GetData();
            return PartialView(new pledge { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), PledgeYear =(short) Year });
        } 

        //
        // POST: /Pledge/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(pledge pledge)
        {
            if (pledge.memberID == 0)
            {
                TempData["Message2"] = "Error adding pledge. Please select member.";
                GetData();
                ViewBag.Year = 0;
                return PartialView(pledge);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    pledge.FundName = ConstantRepository.GetConstantID(pledge.FundID).Value1;
                    db.pledges.Add(pledge);
                    db.SaveChanges();
                    PledgeRepository.AddRecord(pledge);
                    TempData["Message2"] = "Pledge added successfully.";
                    GetData();
                    return RedirectToAction("Create");
                }
            }
            catch(Exception ex)
            {
                TempData["Message2"] = "Error adding pledge";
            }
            GetData();
            ViewBag.Year = 0;
            return PartialView(pledge);
        }
        
        //
        // GET: /Pledge/Edit/5

        public ActionResult Edit(int PledgeID,bool returnCode, int ReturnPledgeYear = 0, string ReturnSearchType = "", int ReturnCodeID = 0)
        {
            GetData();
            pledge pledge = PledgeRepository.GetPledgeByID(PledgeID);
            if (returnCode == true)
            {
                pledge.ReturnPledgeYear = ReturnPledgeYear;
                pledge.ReturnSearchType = ReturnSearchType;
                pledge.ReturnCodeID = ReturnCodeID;
                pledge.ReturnCode = pledge.Type; ;
            }
            return PartialView(pledge);
        }

        //
        // POST: /Pledge/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(pledge pledge)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    pledge.FundName = ConstantRepository.GetConstantID(pledge.FundID).Value1;
                    db.Entry(pledge).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("{0} record update successfully.", pledge.FundName);
                    GetData();
                    return RedirectToAction("Details");
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing {0} record.", pledge.FundName);
            }
            GetData();

            return PartialView(pledge);
        }

        //
        // GET: /Pledge/Delete/5
 
        public ActionResult Delete(int PledgeID)
        {
            ViewBag.PledgeID = PledgeID;
            pledge pledge = PledgeRepository.GetPledgeByID(PledgeID);
            return View(pledge);
        }

        //
        // POST: /Pledge/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int PledgeID)
        {
            pledge pledge = PledgeRepository.GetPledgeByID(PledgeID);
            PledgeRepository.DeleteRecord(pledge);
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

            Dictionary<string, string> ConstantList;
            ConstantList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.ConstantList = new SelectList(ConstantList, "Key", "Value", id);

            Dictionary<int, string> MemberList;
            MemberList = MemberRepository.GetMemberList();
            ViewBag.MemberList = new SelectList(MemberList, "Key", "Value", id);

            Dictionary<string, string> PledgeTypeList;
            PledgeTypeList = ConstantRepository.GetConstantByCategory("Pledge Type");
            ViewBag.PledgeTypeList = new SelectList(PledgeTypeList, "Key", "Value", id);

            Dictionary<int, string> MemberContributionTypeList;
            MemberContributionTypeList = ConstantRepository.GetConstantByCategoryID("Member Contribution");
            ViewBag.MemberContributionTypeList = new SelectList(MemberContributionTypeList, "Key", "Value", id);

            Dictionary<string, string> PledgeYearList = new Dictionary<string, string>();
            int byear = DateTime.Now.Year;
            for (int i = byear; i <= byear + 1; i += 1)
            {
                PledgeYearList.Add(i.ToString(), i.ToString());
            }
            ViewBag.PledgeYearList = new SelectList(PledgeYearList, "Key", "Value", id);
        }

        public ActionResult ListMember(int memberID)
        {
            int year = DateTime.Now.Year;
            string bDateString = "1/1/"+year.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;

            string eDateString = "12/31/"+year.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;

            decimal AnnualpledgeAmount;
            IEnumerable<pledge> pledge = PledgeRepository.GetPledgeByMember(memberID, bDate.Year).Where(e => e.PledgeYear == year);
            foreach (var i in pledge)
            {
                //categoryID = ConstantRepository.GetConstantListByCategory("Member Contribution").FirstOrDefault(e => e.ConstantName == i.FundName).constantID;
                 i.TotalAmountPaid = ContributionRepository.GetContributionByMemberCategoryRange(memberID,i.FundID,bDate,eDate).Sum(e => e.Amount);
                if(i.Type == "Weekly")
                {
                    AnnualpledgeAmount = i.Amount * 52;
                }
                else if (i.Type == "Monthly")
                {
                    AnnualpledgeAmount = i.Amount * 12;
                }
                else if (i.Type == "Semi-Monthly")
                {
                    AnnualpledgeAmount = i.Amount * 24;
                }
                else if (i.Type == "Quarterly")
                {
                    AnnualpledgeAmount = i.Amount * 4;
                }
                else if (i.Type == "Annually")
                {
                    AnnualpledgeAmount = i.Amount;
                }
                else if (i.Type == "Bi-Annually")
                {
                    AnnualpledgeAmount = i.Amount * 2;
                }
                else
                {
                    AnnualpledgeAmount = i.Amount;
                }
                i.RatioCompleted = (i.TotalAmountPaid / AnnualpledgeAmount);
            }
            return PartialView(pledge);
        }

        public ActionResult List()
        {

            return PartialView();
        }

        [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,FinanceLead")]
        public ActionResult ListDetail(int pledgeYear = 0, int codeID = 0, string SearchType = "", string code = "Tithes")
        {
            IEnumerable<pledge> PledgeList;
            decimal AnnualpledgeAmount;
            decimal TotalPledgeAmount = 0;
            ViewBag.PledgeYear = pledgeYear;
            ViewBag.SearchType = SearchType;
            ViewBag.CodeID = codeID;
            ViewBag.Code = code;

            string bDateString = "1/1/" + pledgeYear.ToString();
            DateTime bDate = Convert.ToDateTime(bDateString).Date;

            string eDateString = "12/31/" + pledgeYear.ToString();
            DateTime eDate = Convert.ToDateTime(eDateString).Date;

            if (SearchType == "CategorySearch")
            {
                PledgeList = PledgeRepository.GetPledgeByFund(codeID, pledgeYear);
            }
            else if (SearchType == "MemberSearch")
            {
                PledgeList = PledgeRepository.GetPledgeByMember(codeID, pledgeYear);
            }
            else if (SearchType == "DateRangeSearch")
            {
                PledgeList = PledgeRepository.GetPledgeByYear(pledgeYear);
            }
            else if (SearchType == "TypeSearch")
            {
                PledgeList = PledgeRepository.GetPledgeByType(code, pledgeYear);
            }
            else
            {
                PledgeList = PledgeRepository.GetPledgeByYear(pledgeYear);
            }
            
            foreach (var i in PledgeList)
            {
                i.member = MemberRepository.GetMemberByID(i.memberID);
                i.TotalAmountPaid = ContributionRepository.GetContributionByMemberCategoryRange(i.memberID, i.FundID, bDate, eDate).Sum(e => e.Amount);

                if (i.Type == "Weekly")
                {
                    AnnualpledgeAmount = i.Amount * 52;
                }
                else if (i.Type == "Monthly")
                {
                    AnnualpledgeAmount = i.Amount * 12;
                }
                else if (i.Type == "Semi-Monthly")
                {
                    AnnualpledgeAmount = i.Amount * 24;
                }
                else if (i.Type == "Quarterly")
                {
                    AnnualpledgeAmount = i.Amount * 4;
                }
                else if (i.Type == "Annually")
                {
                    AnnualpledgeAmount = i.Amount;
                }
                else if (i.Type == "Bi-Annually")
                {
                    AnnualpledgeAmount = i.Amount * 2;
                }
                else
                {
                    AnnualpledgeAmount = i.Amount;
                }
                i.RatioCompleted = (i.TotalAmountPaid / AnnualpledgeAmount);
                TotalPledgeAmount += AnnualpledgeAmount;
            }
            
            ViewBag.RecordCount = PledgeList.Count();
            ViewBag.TotalAmount = TotalPledgeAmount;
            return PartialView(PledgeList.OrderBy(e => e.member.FullName));
        }

        public ActionResult MemberPledgeEnable()
        {
            GetData();
            //Member pledge entry status
            constant MemberPledgeEntry = ConstantRepository.GetConstantByName("Member Pledge Entry");
            ViewBag.MemberPledgeEntry = "";
            if (MemberPledgeEntry.Value1 == "Enable")
            {
                ViewBag.MemberPledgeEntry = string.Format("Note: Member Pledge Entry is currently Active for {0}.", MemberPledgeEntry.Value2);
            }
            else
            {
                ViewBag.MemberPledgeEntry = "Note: Member Pledge Entry is Not Active.";
            }
            return PartialView();
        }

        public ActionResult CreatePledge()
        {
            return PartialView();
        }

    }
}