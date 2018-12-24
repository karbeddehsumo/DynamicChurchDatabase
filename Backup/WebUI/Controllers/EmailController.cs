using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;
using WebUI.Mailers;
using Mvc.Mailer;
using Domain;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WebUI.Filters;




namespace WebUI.Controllers
{
    //[RoleAuthentication]
    public class EmailController : Controller
    {
        private IConstantRepository ConstantRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMemberRepository MemberRepository;
        private IMinistryRepository MinistryRepository;
        private IVisitorRepository VisitorRepository;
        private IChildParentRepository ChildParentRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
        private string ChurchName;
        private string ChurchURL;

        public EmailController(IConstantRepository ConstantParam, IMemberRepository MemberParam, IMinistryMemberRepository MinistryMemberParam, IMinistryRepository MinistryParam,
                                IVisitorRepository VisitorParam, IChildParentRepository ChildParentParam)
        {
            ConstantRepository = ConstantParam;
            MemberRepository = MemberParam;
            MinistryMemberRepository = MinistryMemberParam;
            MinistryRepository = MinistryParam;
            VisitorRepository = VisitorParam;
            ChildParentRepository = ChildParentParam;

            ChurchName = ConstantRepository.GetConstantByName("Church Name").Value1;
            ChurchURL = ConstantRepository.GetConstantByName("Church URL").Value1;
            if (ChurchURL.Substring(0,4) != "www.")
            {
                ChurchURL = string.Format("www.{0}", ChurchURL);
            }
        }

        public EmailController(IConstantRepository ConstantParam)
        {
            ConstantRepository = ConstantParam;
            ChurchName = ConstantRepository.GetConstantByName("Church Name").Value1;
            ChurchURL = ConstantRepository.GetConstantByName("Church URL").Value1;
            if (ChurchURL.Substring(0, 4) != "www.")
            {
                ChurchURL = string.Format("www.{0}", ChurchURL);
            }
        }

        public EmailController()
        {

        }

        //
        // GET: /Email/


        private IUserMailer _mailer = new UserMailer();

        public IUserMailer Mailer
        {
            get { return _mailer; }
            set { _mailer = value; }
          

        }

        

        public ActionResult Index(string firstName = "", string familyName = "", string email = "", string password = "")
        {
            //Mailer.ChurchName = ChurchName;
            //Mailer.Church_URL = ChurchURL;

            Mailer.Welcome(firstName, familyName, email, password, ChurchName, ChurchURL).Send();
            return View();
        }

        public ActionResult IndexText(string firstName = "", string familyName = "", string email = "", string password = "")
        {
            Mailer.WelcomeText(firstName, familyName, email, password, ChurchName, ChurchURL).Send();
            return View();
        }
/*
        public ActionResult ContactUs(string fullName = "", string email = "", string message = "", int personID = 0, string subject = "", string sender = "")
        {
            string ReturnUrl = Request.UrlReferrer.ToString();

            TempData["Message"] = "Error sending message!";

            try
            {

                if (message.Trim().Length == 0)
                {
                    TempData["Message"] = "Message is missing";
                    throw new System.InvalidOperationException("Message is missing");
                }
                else if (fullName.Trim().Length == 0)
                {
                    TempData["Message"] = "Please enter your name.";
                    throw new System.InvalidOperationException("Please enter your name.");
                }

                
                user user = MembershipRepositroy.GetUser(User.Identity.Name);
                email = user.Email;
                string webmasterEmail = "gfumbah@gmail.com";//ConstantRepository.GetWebMasterEmail();
                Mailer.ContactUs(fullName, email, message, personID, webmasterEmail, subject, sender).Send();
                TempData["Message"] = "Message send successfully!";
                
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error sending message. Must be login!";
            }
            return Redirect(ReturnUrl);
        }
*/
        public ActionResult ContactMember(string firstName = "", string familyName = "", string memberID = "", string message = "")
        {
            return View();
        }

        public ActionResult PassWordReset(string FullName = "", string email = "", string password = "")
        {
            Mailer.PasswordReset(FullName, email, password, ChurchName, ChurchURL).Send();
            return View();
        }

        public ActionResult ReplyMember(string firstname = "", string familyname = "", int personID = 0)
        {

            return View();
        }

        //public bool EmailMinistry(string message = "", string subject = "", string sender = "", int MinistryID = 0, string attachmentPath = "")
        //public async Task EmailMinistry(EmailModel model)
        public string EmailMinistry(EmailModel model, string ReturnUrl = "")
        {
            //Text message is the first 160 characters
            if (model.EmailMessage.Length > 160)
            {
                model.TextMessage = model.EmailMessage.Substring(0, 160);
            }
            else
            {
                model.TextMessage = model.EmailMessage;
            }

            if(model.EmailMessage != null)
            {
                if (SendMinistryEmail(model, ReturnUrl) == false)
                {
                    return "Error sending email";
                    TempData["Message2"] = "Error sending email.";
                }

            }
            if (model.TextMessage != null)
             {
                 if(SendMinistryText(model, ReturnUrl) == false)
                 {
                     return "Error sending text";
                     TempData["Message2"] = "Error sending text.";
                 }

             }

            return "Email/Text sent successfully!";
            /*
            if(model.EmailMessage != null)
            {
                await SendMinistryEmail(model, ReturnUrl);
            }
            if (model.TextMessage != null)
             {
                 await SendMinistryText(model, ReturnUrl);
             }
             */
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    //    public async Task SendMinistryEmail(EmailModel model, string returnURL)
    public bool SendMinistryEmail(EmailModel model, string returnURL)
        {

            try
            {
                //string ReturnUrl = Request.UrlReferrer.ToString();
                int ContactType1 = ConstantRepository.GetConstantByName("Email & Text").constantID;
                int ContactType2 = ConstantRepository.GetConstantByName("Email Only").constantID;

                // var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                string[] EmailGroup;
                int i = 0;
                int k = 0;

                int[] VisitorIDGroup;
                VisitorIDGroup = new int[500];


                member m;
                if ((model.To == "Ministry") && (model.MinistryID > 0))
                {
                    k = MinistryMemberRepository.GetMemintryMemberByMinistry(model.MinistryID).Count();
                    EmailGroup = new string[k];
                    foreach (ministrymember mm in MinistryMemberRepository.GetMemintryMemberByMinistry(model.MinistryID))
                    {
                        m = MemberRepository.GetMemberByID(mm.memberID);
                        if ((m != null) && (m.Status == "Active") && (m.EmailAddress != null) && (m.EmailAddress != "") && ((m.ContactTypeID == ContactType1) || ((m.ContactTypeID == ContactType2))))
                        {
                            EmailGroup[i++] = m.EmailAddress;
                        }
                    }
                }
                else if (model.To == "Visitor")
                {
                    visitor v;

                    int ContactTypeID = ConstantRepository.GetConstantByCategoryName("Best Contact", "Email").constantID;
                    k = VisitorRepository.GetVisitorByBestContact(ContactTypeID).Count();
                    EmailGroup = new string[k];
                    VisitorIDGroup = new int[k];
                    foreach (visitor vv in VisitorRepository.GetVisitorByBestContact(ContactTypeID))
                    {
                        v = VisitorRepository.GetVisitorByID(vv.visitorID);
                        if ((v != null) && (v.Status == "Active") && (v.Email != null) && (v.Email != ""))
                        {
                            EmailGroup[i] = v.Email;
                            VisitorIDGroup[i++] = v.visitorID;
                        }
                    }
                }
                else if (model.To == "ChildrenParents")
                {
                    k = MemberRepository.GetMemberListCategory("Children").Count();
                    EmailGroup = new string[k];
                    foreach (member c in MemberRepository.GetMemberListCategory("Children"))
                    {
                        IEnumerable<member> cp = ChildParentRepository.GetParents(c.memberID);
                        foreach (member mm in cp)
                        {
                            if ((mm != null) && (mm.Status == "Active") && (mm.EmailAddress != null) && (mm.EmailAddress != "") && ((mm.ContactTypeID == ContactType1) || ((mm.ContactTypeID == ContactType2))))
                            {
                                EmailGroup[i++] = mm.EmailAddress;
                            }
                        }
                    }
                }
                else if (model.To == "YouthParents")
                {
                    k = MemberRepository.GetMemberListCategory("Youth").Count();
                    EmailGroup = new string[k];
                    foreach (member c in MemberRepository.GetMemberListCategory("Youth"))
                    {
                        IEnumerable<member> cp = ChildParentRepository.GetParents(c.memberID);
                        foreach (member mm in cp)
                        {
                            if ((mm != null) && (mm.Status == "Active") && (mm.EmailAddress != null) && (mm.EmailAddress != "") && ((mm.ContactTypeID == ContactType1) || ((mm.ContactTypeID == ContactType2))))
                            {
                                EmailGroup[i++] = mm.EmailAddress;
                            }
                        }
                    }
                }
                else
                {
                    EmailGroup = new string[1];
                    m = MemberRepository.GetMemberByID(model.MemberID);
                    if ((m.Status == "Active") && (m.EmailAddress != null) && (m.EmailAddress != "") && ((m.ContactTypeID == ContactType1) || ((m.ContactTypeID == ContactType2))))
                    {
                        EmailGroup[i++] = m.EmailAddress;
                    }
                }

                if (model.To == "Visitor")
                {
                    visitor v;

                    foreach (int x in VisitorIDGroup)
                    {
                        v = VisitorRepository.GetVisitorByID(x);
                        if (v.Title != null)
                        {
                            v.Title2 = ConstantRepository.GetConstantID((int)v.Title).Value1;
                        }
                        model.MemberID = x;
                        model.MemberName = string.Format("{0} {1}", v.Title2, v.FullName);
                        model.MemberEmail = v.Email;
                        //Get senior pastor name
                        string SeniorPastor = ConstantRepository.GetConstantByCategoryName("Misc Category", "Senior Pastor").Value1;
                        if ((SeniorPastor != null) && (SeniorPastor != ""))
                        {
                            model.FromName = SeniorPastor;
                        }
                        Mailer.VisitorEmailOrText(model,ChurchName,ChurchURL, "Text").Send();

                    }
                }
                else
                {

                    model.EmailString = EmailGroup;
                    model.MinistryName = MinistryRepository.GetMinistryByID(model.MinistryID).MinistryName;
                    Mailer.EmailOrText(model, "Email").Send();
                }

            }
            catch(Exception ex)
            {
                return false;
            }
            //return Redirect(returnURL);
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    //    public async Task SendMinistryText(EmailModel model, string returnURL)
        public bool SendMinistryText(EmailModel model, string returnURL)
        {

            try{
            int ContactType1 = ConstantRepository.GetConstantByName("Email & Text").constantID;
            int ContactType2 = ConstantRepository.GetConstantByName("Text Only").constantID;
            string PhoneProvideEmailString = "";
            string PhoneEmailAddress = "";

            //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();
            member m;

            string[] EmailGroup;
            int[] VisitorIDGroup;
            VisitorIDGroup = new int[500];
            int i = 0;
            int k = 0;

            if ((model.To == "Ministry") && (model.MinistryID > 0))
            {
                k = MinistryMemberRepository.GetMemintryMemberByMinistry(model.MinistryID).Count();
                EmailGroup = new string[k];
                foreach (ministrymember mm in MinistryMemberRepository.GetMemintryMemberByMinistry(model.MinistryID))
                {
                    m = MemberRepository.GetMemberByID(mm.memberID);
                    if ((m.Status == "Active") && (m.PhoneNumber != null) && (m.PhoneNumber != "") && ((m.ContactTypeID == ContactType1) || ((m.ContactTypeID == ContactType2))))
                    {
                        string PhoneNumber = Regex.Replace(m.PhoneNumber, @"[^\d]", String.Empty);
                        PhoneProvideEmailString = ConstantRepository.GetConstantID((int)m.PhoneProviderID).Value2;
                        PhoneEmailAddress = string.Format("{0}@{1}", PhoneNumber, PhoneProvideEmailString);
                        if ((PhoneProvideEmailString != null) && (PhoneNumber != null))
                        {
                           //message.To.Add(new MailAddress(PhoneEmailAddress));
                            EmailGroup[i++] = PhoneEmailAddress;
                        }
                    }
                }
            }
            else if (model.To == "Visitor")
            {
                visitor v;
                int ContactTypeID = ConstantRepository.GetConstantByCategoryName("Best Contact", "Text").constantID;
                int ContactTypeID2 = ConstantRepository.GetConstantByCategoryName("Best Contact", "Email & Text").constantID;
                IEnumerable<visitor> TextOnly = VisitorRepository.GetVisitorByBestContact(ContactTypeID);
                IEnumerable<visitor> EmailAndText = VisitorRepository.GetVisitorByBestContact(ContactTypeID2);
                var TextingVisitor = TextOnly.Concat(EmailAndText);

                k = TextingVisitor.Count();
                EmailGroup = new string[k];
                VisitorIDGroup = new int[k];
                
                foreach (visitor vv in TextingVisitor)
                {
                    v = VisitorRepository.GetVisitorByID(vv.visitorID);


                    if ((v != null) && (v.Status == "Active") && (v.phoneNumber != null) && (v.phoneNumber != "") && (v.PhoneProviderID != null))
                    {
                        string PhoneNumber = Regex.Replace(v.phoneNumber, @"[^\d]", String.Empty);
                        PhoneProvideEmailString = ConstantRepository.GetConstantID((int)v.PhoneProviderID).Value2;
                        PhoneEmailAddress = string.Format("{0}@{1}", PhoneNumber, PhoneProvideEmailString);
                        if ((PhoneProvideEmailString != null) && (PhoneNumber != null))
                        {
                            //message.To.Add(new MailAddress(PhoneEmailAddress));
                            EmailGroup[i] = PhoneEmailAddress;
                            VisitorIDGroup[i++] = v.visitorID;
                        }
                    }
               
                }
            }
            else if (model.To == "ChildrenParents")
            {
                k = MemberRepository.GetMemberListCategory("Children").Count();
                EmailGroup = new string[k*2];
                foreach (member c in MemberRepository.GetMemberListCategory("Children"))
                {
                    IEnumerable<member> cp = ChildParentRepository.GetParents(c.memberID);
                    foreach (member mm in cp)
                    {
                        if ((mm.Status == "Active") && (mm.PhoneNumber != null) && (mm.PhoneNumber != "") && ((mm.ContactTypeID == ContactType1) || ((mm.ContactTypeID == ContactType2))))
                        {
                            string PhoneNumber = Regex.Replace(mm.PhoneNumber, @"[^\d]", String.Empty);
                            PhoneProvideEmailString = ConstantRepository.GetConstantID((int)mm.PhoneProviderID).Value2;
                            PhoneEmailAddress = string.Format("{0}@{1}", PhoneNumber, PhoneProvideEmailString);
                            if ((PhoneProvideEmailString != null) && (PhoneNumber != null))
                            {
                               // message.To.Add(new MailAddress(PhoneEmailAddress));
                               // message.To.Add(new MailAddress;PhoneEmailAddress));
                                EmailGroup[i] = PhoneEmailAddress;
                            }
                        }
                    }
                }
            }
            else if (model.To == "YouthParents")
            {
                k = MemberRepository.GetMemberListCategory("Children").Count();
                EmailGroup = new string[k * 2];
                foreach (member c in MemberRepository.GetMemberListCategory("Youth"))
                {
                    IEnumerable<member> cp = ChildParentRepository.GetParents(c.memberID);
                    foreach (member mm in cp)
                    {
                        if ((mm.Status == "Active") && (mm.PhoneNumber != null) && (mm.PhoneNumber != "") && ((mm.ContactTypeID == ContactType1) || ((mm.ContactTypeID == ContactType2))))
                        {
                            string PhoneNumber = Regex.Replace(mm.PhoneNumber, @"[^\d]", String.Empty);
                            PhoneProvideEmailString = ConstantRepository.GetConstantID((int)mm.PhoneProviderID).Value2;
                            PhoneEmailAddress = string.Format("{0}@{1}", PhoneNumber, PhoneProvideEmailString);
                            if ((PhoneProvideEmailString != null) && (PhoneNumber != null))
                            {
                                //message.To.Add(new MailAddress(PhoneEmailAddress));
                                EmailGroup[i] = PhoneEmailAddress;
                            }
                        }
                    }
                }
            }
            else
            {
                EmailGroup = new string[1];
                m = MemberRepository.GetMemberByID(model.MemberID);
                if ((m.Status == "Active") && ((m.ContactTypeID == ContactType1) || ((m.ContactTypeID == ContactType2))))
                {
                    PhoneProvideEmailString = ConstantRepository.GetConstantID((int)m.PhoneProviderID).Value2;
                    PhoneEmailAddress = string.Format("{0}@{1}", m.PhoneNumber, PhoneProvideEmailString);
                    if ((PhoneProvideEmailString != null) && (m.PhoneNumber != null))
                    {
                       // message.To.Add(new MailAddress(PhoneEmailAddress));
                        EmailGroup[0] = PhoneEmailAddress;
                    }
                }
            }

            if (model.To == "Visitor")
            {
                visitor v;

                foreach (int x in VisitorIDGroup)
                {
                    v = VisitorRepository.GetVisitorByID(x);
                    if (v.Title != null)
                    {
                        v.Title2 = ConstantRepository.GetConstantID((int)v.Title).Value1;
                    }
                    string PhoneNumber = Regex.Replace(v.phoneNumber, @"[^\d]", String.Empty);
                    PhoneProvideEmailString = ConstantRepository.GetConstantID((int)v.PhoneProviderID).Value2;
                    PhoneEmailAddress = string.Format("{0}@{1}", PhoneNumber, PhoneProvideEmailString);

                    model.MemberID = x;
                    model.MemberName = string.Format("{0} {1}", v.Title2, v.FullName);
                    model.MemberEmail = PhoneEmailAddress;
                    //Get senior pastor name
                    string SeniorPastor = ConstantRepository.GetConstantByCategoryName("Misc Category", "Senior Pastor").Value1;
                    if ((SeniorPastor != null) && (SeniorPastor != ""))
                    {
                        model.FromName = SeniorPastor;
                    }
                    string ChurchName = ConstantRepository.GetConstantByCategoryName("Misc Category", "Church Name").Value1;
                    model.ChurchName = ChurchName;
                    Mailer.VisitorEmailOrText(model, "Text", ChurchName, ChurchURL).Send();
                }
            }
            else
            {
                model.EmailString = EmailGroup;
                model.MinistryName = MinistryRepository.GetMinistryByID(model.MinistryID).MinistryName;
                Mailer.EmailOrText(model, "Text").Send();
            }

            //return Redirect(returnURL);
                /*
            if (model.To == "Ministry")
            {
                return RedirectToAction("Details", "Visitor");
            }
            else if (model.To == "Visitor")
            {
                return RedirectToAction("Details", "Visitor");
            }
            else if (model.To == "ChildrenParents")
            {
                return RedirectToAction("Details", "Visitor");
            }
            else if (model.To == "YouthParents")
            {
                return RedirectToAction("Details", "Visitor");
            }
            else
            {
                return RedirectToAction("Details", "Visitor");
            }
                */
            }
            catch (Exception ex)
            {
                return false;
            }
            //return Redirect(returnURL);
            return true;
        }

        public bool EmailMember(string message = "", string subject = "", string sender = "", int MemberID = 0, string attachmentPath = "")
        {
            return true;
        }

        public void RentalEmail(RentProperty RentalRequest)
        {
            string recipient = ConstantRepository.GetConstantByName("Rental_email_Recipient").Value1;
            string[] emailRecipient = recipient.Split(new string[] { "," },StringSplitOptions.None);

            Mailer.RentalEmail(RentalRequest, emailRecipient).Send();
        }

        public void ContactUs(ContactUs ContactUs)
        {
            string recipient = ConstantRepository.GetConstantByCategoryName("Contact Us Email",ContactUs.Subject).Value2;
            string[] emailRecipient = recipient.Split(new string[] { "," }, StringSplitOptions.None);
            ContactUs.FromEmail = ConstantRepository.GetConstantByName("Church Email From").Value1;
            Mailer.ContactUs(ContactUs, emailRecipient).Send();
        }
    }
}
