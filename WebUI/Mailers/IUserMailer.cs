using Mvc.Mailer;
using System.Net;
using WebUI.Models;

namespace WebUI.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage Welcome(string firstName, string familyName, string email, string password, string ChurchName, string ChurchURL);
        MvcMailMessage WelcomeText(string firstName, string familyName, string email, string password, string ChurchName, string ChurchURL);
        MvcMailMessage PasswordReset(string FullName, string email, string password, string ChurchName, string ChurchURL);
        //MvcMailMessage ContactUs(string FullName, string email, string message, int personID, string webmasterEmail, string subject, string sender);
        MvcMailMessage EmailOrText(EmailModel model, string MailType = "Email");
        MvcMailMessage VisitorEmailOrText(EmailModel model, string ChurchName, string ChurchURL, string MailType = "Email");
        MvcMailMessage RentalEmail(RentProperty model, string[] Recipients);
        MvcMailMessage ContactUs(ContactUs model, string[] Recipients);


    }
}