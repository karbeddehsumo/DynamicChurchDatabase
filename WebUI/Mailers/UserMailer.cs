using Mvc.Mailer;
using System.Web;
using WebUI.Models;

using System.Net;
using System.Net.Mail;
using System.IO;
using Domain.Abstract;
using Domain.Concrete;

namespace WebUI.Mailers
{
    public class UserMailer : MailerBase, IUserMailer	
	{
        public  UserMailer()
        {
            MasterName = "_Layout";

            
        }


        public virtual MvcMailMessage Welcome(string firstName, string familyName, string email, string password, string ChurchName, string ChurchURL)
        {

            ViewBag.ChurchName = ChurchName;
            ViewBag.Church_URL = ChurchURL;

            ViewBag.Name = firstName;
            ViewBag.FamilyName = familyName;
            ViewBag.Password = password;
            

            return Populate(x =>
            {
                x.Subject = "Welcome";
                x.ViewName = "PasswordReset";
                x.To.Add(email);
                // x.From = "admin@newfamilyreunion.com";
                x.ViewName = "Welcome";
                x.IsBodyHtml = true;

            });
        }

        public virtual MvcMailMessage WelcomeText(string firstName, string familyName, string email, string password, string ChurchName, string ChurchURL)
        {
            ViewBag.ChurchName = ChurchName;
            ViewBag.Church_URL = ChurchURL;

            ViewBag.Name = firstName;
            ViewBag.FamilyName = familyName;
            ViewBag.Password = password;

            return Populate(x =>
            {
                x.Subject = "Welcome";
                x.ViewName = "WelcomeText";
                x.To.Add(email);
                x.ViewName = "WelcomeText";
                x.IsBodyHtml = true;

            });
        }

        public virtual MvcMailMessage PasswordReset(string FullName, string email, string password, string ChurchName, string ChurchURL)
        {
            ViewBag.ChurchName = ChurchName;
            ViewBag.Church_URL = ChurchURL;

            ViewBag.FullName = FullName;
            ViewBag.Password = password;
            //ViewBag.Data = someObject;
            return Populate(x =>
            {
                x.Subject = "PasswordReset";
                x.ViewName = "PasswordReset";
                x.To.Add(email);
                x.IsBodyHtml = true;
            });
        }

        public virtual MvcMailMessage RentalEmail(RentProperty model, string[] Recipients)
        {
            ViewBag.Subject = "Church Property Rental Request";
            ViewBag.EventDate   = model.EventDate;
            ViewBag.FullName    = model.FullName;
            ViewBag.PhoneNumber = model.PhnoeNumber;
            ViewBag.Email        = model.EmailAddress;
            ViewBag.BusinessName = model.BusinessName;
            ViewBag.Message = model.DescribeEvent;

            return Populate(x =>
            {
                x.Subject = "Church Property Rental Request";
                x.From = new System.Net.Mail.MailAddress(model.EmailAddress);
                foreach (string email in Recipients)
                {
                    x.To.Add(new System.Net.Mail.MailAddress(email));
                }
                x.ViewName = "RentalRequest";
               // x.Bcc.Add(model.MemberEmail);
                x.IsBodyHtml = true;

            });
        }

        public virtual MvcMailMessage ContactUs(ContactUs model, string[] Recipients)
        {
            ViewBag.Subject = model.Subject;
            ViewBag.FullNme = model.FullName;
            ViewBag.PhoneNumber = model.PhoneNumber;
            ViewBag.EmailAddress = model.EmailAddress;
            if (model.ContactMe == true)
            {
                ViewBag.ContactMe = "Please contact me!";
            }
            else
            {
                ViewBag.ContactMe = "Do not contact me!";
            }
            ViewBag.Comment = model.Comment;

            return Populate(x =>
            {
                x.Subject = "Church Website: Contact Me";
                x.From = new System.Net.Mail.MailAddress(model.FromEmail);
                foreach (string email in Recipients)
                {
                    x.To.Add(new System.Net.Mail.MailAddress(email));
                }
                // x.Bcc.Add(model.MemberEmail);
                x.ViewName = "ContactUs";
                x.IsBodyHtml = true;

            });
        }

         public virtual MvcMailMessage EmailOrText(EmailModel model, string MailType = "Email")
         {
             
             ViewBag.Title = string.Format("Message from {0}",model.MinistryName);
            if (MailType == "Email")
            {
               ViewBag.Message = model.EmailMessage;
            }
            else
            {
                ViewBag.Message = model.TextMessage;
            }
            ViewBag.SenderCode = model.SenderCode;

            return Populate(x =>
            {
                
                foreach (var file in model.files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        x.Attachments.Add(new Attachment(file.InputStream, Path.GetFileName(file.FileName)));
                    }

                }
                x.Subject = model.Subject;
                x.ViewName = "EmailOrText";
                x.From = new System.Net.Mail.MailAddress(model.FromEmail);
                x.To.Add(new System.Net.Mail.MailAddress(model.FromEmail));
                x.IsBodyHtml = true;
                foreach (string e in model.EmailString)
                {
                    if (e != null)
                    {
                        x.Bcc.Add(e);
                    }
                }
                
            });
        }



         public virtual MvcMailMessage VisitorEmailOrText(EmailModel model, string ChurchName, string ChurchURL, string MailType = "Email")
         {

             ViewBag.ChurchName = ChurchName;
             ViewBag.Church_URL = ChurchURL;

            if (MailType == "Email")
            {
               ViewBag.Message = model.EmailMessage;
            }
            else
            {
                ViewBag.Message = model.TextMessage;
            }

            ViewBag.SenderCode = model.SenderCode;
            return Populate(x =>
            {
                
                foreach (var file in model.files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        x.Attachments.Add(new Attachment(file.InputStream, Path.GetFileName(file.FileName)));
                    }

                }
                x.Subject = model.Subject;
                x.ViewName = "VisitorEmailOrText";
                ViewBag.VisitorID = model.MemberID;
                ViewBag.Name = model.MemberName;
                @ViewBag.SeniorPastor = model.FromName;
                @ViewBag.ChurchName = string.Format("Message from: {0}",model.ChurchName);
               // ViewBag.From = model.FromName;
                x.From = new System.Net.Mail.MailAddress(model.FromEmail);
                x.To.Add(new System.Net.Mail.MailAddress(model.FromEmail));
                x.Bcc.Add(model.MemberEmail);
                x.IsBodyHtml = true;
                
            });
        }
    }
}
