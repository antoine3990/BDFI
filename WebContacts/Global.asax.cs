using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebContacts.Models;

namespace WebContacts
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Keep the reference of the DAL of the Contacts in order to allow all the session have access to it
            HttpRuntime.Cache["Movies"] = new Movies(Server.MapPath(@"~/App_Data/Contacts.txt"));

            // Keep the reference of the DAL of the Friends in order to allow all the session have access to it
            HttpRuntime.Cache["Friends"] = new Friends(Server.MapPath(@"~/App_Data/Friends.txt"));
        }

        protected void Session_Start()
        {
            // Keep the reference of the ContactsView in order to allow all the controllers and views have access to it
            // This allows all the session to have their own sorted contact list
            Session["MoviesView"] = new MoviesView((Movies)HttpRuntime.Cache["Movies"]);
        }
    }
}
