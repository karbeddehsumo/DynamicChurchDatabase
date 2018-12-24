using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;
using System.Data.Entity;
using Domain;
using WebUI.Filters;


namespace WebUI.Controllers
{
    public class AccountController : Controller
    {
        private churchdatabaseEntities db = new churchdatabaseEntities();
        public EMembershipProviderRepository MembershipService { get; set; }
        //public EMembershipProviderRepository membershipServ { get; set; }
        public ERoleProviderRepository AuthorizationService { get; set; }
        //public EFEmailRepository EmailService { get; set; }
        public ERoleProviderRepository Roles { get; set; }
        private EFConstantRepository ConstantRepository = new EFConstantRepository();
        private churchdatabase_userEntities entities = new churchdatabase_userEntities();
       

        private int memberID;


        protected override void Initialize(RequestContext requestContext)
        {
            if (MembershipService == null)
                MembershipService = new EMembershipProviderRepository();
            if (AuthorizationService == null)
                AuthorizationService = new ERoleProviderRepository();
            if (Roles == null)
                Roles = new ERoleProviderRepository();


            base.Initialize(requestContext);
        }
        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
           // int i = 1;
            return PartialView();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            string user = model.Email;
            double myNum = 0;

            if (Double.TryParse(model.Email, out myNum)) //Add phone provider to email login
            {
                var pd = entities.users.SingleOrDefault(e => e.Email.Contains(model.Email));
                if (pd != null)
                {
                    int len = pd.Email.Length;
                    int i = pd.Email.IndexOf("@");
                    string provider = pd.Email.Substring(i, len - i);
                    string fullEmail = string.Format("{0}{1}",model.Email,provider);
                    model.Email = fullEmail;
                    user = fullEmail;
                }
            }
            else
            {
                // it is not a number
            }

            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.Email, model.Password))
                {
                    Roles = new ERoleProviderRepository(model.Email);
                    user _user = MembershipService.GetUser(model.Email);
                    System.Web.HttpContext.Current.Session["personID"] = _user.PersonID;
                    System.Web.HttpContext.Current.Session["email"] = model.Email;
                    System.Web.HttpContext.Current.Session["OldPassword"] = model.Password;
                    _user.role = Roles.GetRole(_user.RoleID);

                    System.Web.HttpContext.Current.Session["memberRole"] = _user.role.Name;

                    //FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);

                    Session["MyMenu"] = null;

                    member member = db.members.Find(_user.PersonID);


                    FormsAuthentication.SetAuthCookie(model.Email, true);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else if (Roles.IsUserInRole(user, "WebMaster"))
                    {
                        
                        return RedirectToAction("WebMaster", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Register"))
                    {
                        return RedirectToAction("Register", "Account", new {});
                    }
                    else if (Roles.IsUserInRole(user, "Member")||Roles.IsUserInRole(user, "Officer"))
                    {
                        return RedirectToAction("MyPage2", "Member");
                    }
                    else if (Roles.IsUserInRole(user, "Admin") || Roles.IsUserInRole(user, "Admin2"))
                    {
                        return RedirectToAction("WebMaster", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Staff"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Pastor"))
                    {
                        return RedirectToAction("PastorDashBoard", "Income");
                    }
                    else if (Roles.IsUserInRole(user, "FinanceStaff") || Roles.IsUserInRole(user, "FinanceLead"))
                    {
                        return RedirectToAction("Finance", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {

                    TempData["Message5"] = "Incorrect user name or password.";
                    return RedirectToAction("LogOn", "Account");
                }
            }

            // If we got this far, something failed, redisplay form
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult LogOnModal(string UserEmail, string UserPassword, string returnUrl)
        {
            LogOnModel model = new LogOnModel();
            model.Email = UserEmail;
            model.Password = UserPassword;
            string user = model.Email;

            double myNum = 0;

            if (Double.TryParse(model.Email, out myNum)) //Add phone provider to email login
            {
                var pd = entities.users.SingleOrDefault(e => e.Email.Contains(model.Email));
                if (pd != null)
                {
                    int len = pd.Email.Length;
                    int i = pd.Email.IndexOf("@");
                    string provider = pd.Email.Substring(i, len - i);
                    string fullEmail = string.Format("{0}{1}", model.Email, provider);
                    model.Email = fullEmail;
                    user = fullEmail;
                }
            }
            else
            {
                // it is not a number
            }

            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.Email, model.Password))
                {
                    Roles = new ERoleProviderRepository(model.Email);
                    user _user = MembershipService.GetUser(model.Email);
                    System.Web.HttpContext.Current.Session["personID"] = _user.PersonID;
                    System.Web.HttpContext.Current.Session["email"] = model.Email;
                    System.Web.HttpContext.Current.Session["OldPassword"] = model.Password;
                    _user.role = Roles.GetRole(_user.RoleID);

                    System.Web.HttpContext.Current.Session["memberRole"] = _user.role.Name;

                    //FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);

                    Session["MyMenu"] = null;

                    member member = db.members.Find(_user.PersonID);


                    FormsAuthentication.SetAuthCookie(model.Email, true);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else if (Roles.IsUserInRole(user, "WebMaster"))
                    {

                        return RedirectToAction("WebMaster", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Register"))
                    {
                        return RedirectToAction("Register", "Account", new { });
                    }
                    else if (Roles.IsUserInRole(user, "Member") || Roles.IsUserInRole(user, "Officer"))
                    {
                        return RedirectToAction("MyPage2", "Member");
                    }
                    else if (Roles.IsUserInRole(user, "Admin") || Roles.IsUserInRole(user, "Admin2"))
                    {
                        return RedirectToAction("WebMaster", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Staff"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (Roles.IsUserInRole(user, "Pastor"))
                    {
                        return RedirectToAction("PastorDashBoard", "Income");
                    }
                    else if (Roles.IsUserInRole(user, "FinanceStaff") || Roles.IsUserInRole(user, "FinanceLead"))
                    {
                        return RedirectToAction("Finance", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    TempData["Message3"] = "Incorrect user name or password.";
                    //return PartialView(model);
                    return RedirectToAction("Index", "Home");
                }
            }

            // If we got this far, something failed, redisplay form
            return PartialView(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session["MyMenu"] = null;

            return RedirectToAction("Index", "Home");
            //return PartialView();
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            user user = MembershipService.GetUserByID(memberID);
            GetData();
            RegisterModel newUser = new RegisterModel();
            newUser.Email = user.Email;
            return View(newUser);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.Email, model.OldPassword))
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false /* createPersistentCookie */);
                    MembershipService.ChangePasswordQuestionAndAnswer(model.Email, model.OldPassword, model.RegistrationQuestion, model.RegistrationAnswer);
                    MembershipService.ChangePassword2(model.Email, model.OldPassword, model.NewPassword);
                    user user = MembershipService.GetUser(model.Email);
                    role role = Roles.GetRole("Member");
                    MembershipService.UpdateRole(user.PersonID, role.roleID);
                    return RedirectToAction("MyPage2", "Member");
                }
                else
                {
                    //ModelState.AddModelError("", ErrorCodeToString("Invalid password"));
                   // return Content("Password not valid. Please re-enter password.");
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        
        public ActionResult ChangePassword()
        {
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            user user = MembershipService.GetUserByID(memberID);
            ChangePasswordModel model = new ChangePasswordModel();
            model.Email = user.Email;
            return View(model);
        }

        //
        // POST: /Account/ChangePassword

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
                user user = MembershipService.GetUserByID(memberID);

                if (MembershipService.ValidateUser(model.Email, model.OldPassword))
                {
                    MembershipService.ChangePassword2(model.Email, model.OldPassword, model.NewPassword);
                    if (user.role.Name == "Member")
                    {
                         return RedirectToAction("MyPage2", "Member");
                    }
                    else
                    {
                        return RedirectToAction("MyPage", "Member");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public ActionResult RecoveryPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RecoverySecurityQuestion(string UserName = "")
        {
            try
            {
                double myNum = 0;
                string actualUserName = UserName;
                string MessageType = "Email";

                if (Double.TryParse(UserName, out myNum)) //Add phone provider to email login
                {
                    var pd = entities.users.SingleOrDefault(e => e.Email.Contains(UserName));
                    if (pd != null)
                    {
                        int len = pd.Email.Length;
                        int i = pd.Email.IndexOf("@");
                        string provider = pd.Email.Substring(i, len - i);
                        actualUserName = string.Format("{0}{1}", UserName, provider);
                        MessageType = "Text";
                    }
                }

                user _user = MembershipService.GetUser(actualUserName);
                ViewBag.SecurityQuestion = _user.PasswordQuestion;
                ViewBag.UserName = actualUserName;
                ViewBag.MessageType = MessageType;
                TempData["Message2"] = "";
                return View();

            }
            catch (Exception ex)
            {
                TempData["Message4"] = string.Format("Incorrect user name {0}.", UserName);
            }
            return RedirectToAction("RecoveryPassword", "Account");
        }
         [HttpPost]
        public ActionResult RecoveryResetPasswrd(string UserName, string UserSecurityAnwer, string MessageType)
        {
            string ReturnUrl = Request.UrlReferrer.ToString();
            user _user = MembershipService.GetUser(UserName);
            if (_user.PasswordAnswer == UserSecurityAnwer)
            {
                RedirectToAction("ResendEmailInvite", "Member", new { memberID = _user.PersonID});
                ResendEmailInvite(_user.PersonID); //call controller

                if (MessageType == "Email")
                {
                    TempData["Message2"] = "Your Password has been reset. A new password message was sent to your email.";
                }
                else
                {
                    TempData["Message2"] = "Your Password has been reset. A new password message was text to you.";
                }
                
            }
            else
            {
               TempData["Message2"] = "Invalid security answer.";
            }
           // string ReturnUrl2 = ReturnUrl + string.Format("?UserName={0}", UserName);
            return Content(TempData["Message2"].ToString());

        }

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

                 user user = MembershipService.GetUserByID(memberID);
                 MembershipService.ChangePassword2(user.UserName, user.Password, password);
                 user = MembershipService.GetUserByID(memberID);
                 if (user.IsRegister)
                 {
                     returnString = SendEmail(memberID, password);
                 }
             
             //return Redirect(ReturnUrl);
             return returnString;
         }

         private string SendEmail(int memberID, string password)
         {
             member member = db.members.Find(memberID);
             user user = MembershipService.GetUserByID(memberID);
             if (user.EmailType == "Email")
             {
                 EmailController EmailServer = new EmailController(ConstantRepository);
                 EmailServer.Index(member.FirstNameTitle, member.LastName, user.Email, password);
             }
             else
             {
                 EmailController EmailServer = new EmailController(ConstantRepository);
                 EmailServer.IndexText(member.FirstNameTitle, member.LastName, user.Email, password);
             }


             TempData["Message2"] = string.Format("{0} record edited successfully.", member.FirstName);
             return "Email sent!";

         }


        public void GetData()
        {
            int id = 0;

            //Continent List
            Dictionary<string, string> RegistrationQuestions;
            RegistrationQuestions = ConstantRepository.GetRegistrationQuestions();
            ViewBag.RegistrationQuestions = new SelectList(RegistrationQuestions, "Value", "Key", id);


        }
        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
