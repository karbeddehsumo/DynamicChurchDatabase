using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using System.Web.Security;
using Domain;
using Ninject.Web.Common;
using Ninject.Web.Mvc;
using WebOptimizationWithMvc3.App_Start; //NEW MAGIC GOODNESS
using System.Web.Optimization;           //NEW MAGIC GOODNESS

namespace WebUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }
        /*
                protected override IKernel CreateKernel()
                {
                    var kernel = new StandardKernel();
                    RegisterServices(kernel);
                    return kernel;
                }

                /// 
                /// Load your modules or register your services here!
                /// 
                /// The kernel.
                private void RegisterServices(IKernel kernel)
                {
                    // e.g. kernel.Load(Assembly.GetExecutingAssembly());
                }

                protected override void OnApplicationStarted()
                {
                    base.OnApplicationStarted();

                    AreaRegistration.RegisterAllAreas();
                    RegisterGlobalFilters(GlobalFilters.Filters);
                    RegisterRoutes(RouteTable.Routes);
                }
                */
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Use LocalDB for Entity Framework by default
            Database.DefaultConnectionFactory = new SqlConnectionFactory(@"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");
            //   ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //NEW MAGIC GOODNESS START
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void FormsAuthentication_OnAuthenticate(Object sender, FormsAuthenticationEventArgs e)
        {

            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        //let us take out the username now                
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        string roles = string.Empty;


                        using (churchdatabase_userEntities entities = new churchdatabase_userEntities())
                        {

                            user user = entities.users.SingleOrDefault(u => u.UserName == username);
                            user.role = entities.roles.SingleOrDefault(u => u.roleID == user.RoleID);
                            roles = user.role.Name;
                        }
                        //let us extract the roles from our own custom cookie


                        //Let us set the Pricipal with our user specific details
                        e.User = new System.Security.Principal.GenericPrincipal(
                          new System.Security.Principal.GenericIdentity(username, "Forms"), roles.Split(';'));
                    }
                    catch (Exception ex)
                    {
                        //somehting went wrong
                    }
                }
            }
        }
        /*
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        //let us take out the username now                
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        string roles = string.Empty;

                        using (churchdatabase_userEntities entities = new churchdatabase_userEntities())
                        {
                            user user = entities.users.SingleOrDefault(u => u.UserName == username);

                            roles = user.role.Name;
                        //let us extract the roles from our own custom cookie


                        //Let us set the Pricipal with our user specific details
                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                          new System.Security.Principal.GenericIdentity(username, "Forms"), roles.Split(';'));
                    }
                    catch (Exception)
                    {
                        //somehting went wrong
                    }
                }
            }
        } 
    }
        */
    }
}