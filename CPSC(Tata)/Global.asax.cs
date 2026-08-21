using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InputOutput
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_BeginRequest()
        //{
        //    string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        //    HttpContext.Current.Items["CSPNonce"] = nonce;
        //    HttpContext.Current.Response.Headers.Remove("Content-Security-Policy");
        //    HttpContext.Current.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; " + $"script-src 'self' 'unsafe-inline' 'unsafe-eval' https://code.jquery.com " +
        //        $"https://cdn.jsdelivr.net https://infoviz.cv.tatamotors https://tasscpsc.tatamotors.com; " +
        //        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        //        "img-src 'self' data:; " +
        //        "font-src 'self' https://fonts.gstatic.com; " +
        //        "connect-src 'self' https://infoviz.cv.tatamotors" +
        //        "https://tasscpsc.tatamotors.com; " +
        //        "frame-src https://infoviz.cv.tatamotors; " +
        //        "object-src 'none';"
        //        );
        //}
    }
}
