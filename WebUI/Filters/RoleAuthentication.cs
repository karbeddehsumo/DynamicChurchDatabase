using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security;
using Domain;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;

using System.Security.Principal;


namespace WebUI.Filters
{
    public class RoleAuthentication: AuthorizeAttribute
    {
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAuthenticated)  //return if user is not logged in
                return false;

            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (MembershipRepositroy.IsUser(memberID))
            {
                user user = MembershipRepositroy.GetUserByID(memberID);

                foreach (string defineRole in this.Roles.Split(','))
                {
                    if (defineRole.Equals(user.role.Name))
                        return true;
                }
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/");
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}