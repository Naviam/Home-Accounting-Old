using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Combres;
using System.Globalization;
using System.Threading;
using Naviam.WebUI;
using Naviam.WebUI.Helpers;

namespace Naviam.WebUI
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            //combres
            routes.AddCombresRoute("Combres");

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "LogOn", id = UrlParameter.Optional } // Parameter defaults
            );

        }

// ReSharper disable InconsistentNaming
        protected void Application_Start()
// ReSharper restore InconsistentNaming
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.Add(typeof(decimal?), new CustomDecimalModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new CustomDateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(string), new CustomStringModelBinder());

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());

            ValueProviderFactories.Factories.Add(new MyFormValueProviderFactory());
        }

// ReSharper disable InconsistentNaming
        protected void Application_AcquireRequestState(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (HttpContext.Current.Session != null)
            {
                var ci = (CultureInfo)Session["Culture"];
                if (ci == null)
                {
                    //default
                    string langName = "en";
                    //from user browser
                    if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    {
                        langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    }
                    ci = new CultureInfo(langName);
                    Session["Culture"] = ci;
                }
                //Устанавливаем культуру для каждого запроса
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }
    }
}