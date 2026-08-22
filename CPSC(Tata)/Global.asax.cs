using System;
using System.IO;
using System.Text;
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
            // VAPT "Server Version Disclosure": drops the X-AspNetMvc-Version response header.
            // X-AspNet-Version is separately disabled via Web.config's enableVersionHeader="false".
            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // VAPT "Secure Cookies not set properly": Web.config's <httpCookies httpOnlyCookies="true"
        // requireSSL="true" /> covers HttpOnly + Secure for every cookie this app sets. SameSite
        // can't be expressed via that config element on .NET Framework 4.8, so it's appended to
        // each cookie's Path here instead. Lax (not Strict) so a link from an external page can
        // still land the user on an authenticated page rather than silently dropping the session.
        protected void Application_PreSendRequestHeaders()
        {
            HttpResponse response = Response;
            if (response == null) return;

            foreach (string name in response.Cookies.AllKeys)
            {
                HttpCookie cookie = response.Cookies[name];
                if (cookie != null && cookie.Path.IndexOf("SameSite", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    cookie.Path += "; SameSite=Lax";
                }
            }
        }

        // Global catch-all: fires for the original unhandled exception before customErrors' redirect,
        // regardless of whether the failure is in routing, an HTTP module, MVC action execution, or
        // view rendering (LoggingHandleErrorAttribute in FilterConfig only covers MVC action
        // exceptions - this covers everything else too).
        protected void Application_Error(object sender, EventArgs e)
        {
            string message;
            try
            {
                Exception ex = Server.GetLastError();
                var sb = new StringBuilder();
                sb.AppendLine("\r\n===== " + DateTime.Now + " =====");
                try { sb.AppendLine("URL: " + Request.Url); }
                catch (Exception urlEx) { sb.AppendLine("URL: <unavailable: " + urlEx.Message + ">"); }
                Exception current = ex;
                while (current != null)
                {
                    sb.AppendLine(current.GetType().FullName + ": " + current.Message);
                    sb.AppendLine(current.StackTrace);
                    current = current.InnerException;
                    if (current != null) sb.AppendLine("--- Inner Exception ---");
                }
                message = sb.ToString();
            }
            catch (Exception outerEx)
            {
                message = "\r\n===== " + DateTime.Now + " ===== (failed to read the real exception: " + outerEx + ")";
            }

            try { System.Diagnostics.EventLog.WriteEntry(".NET Runtime", "[AppErrors] " + message, System.Diagnostics.EventLogEntryType.Error); }
            catch { }

            try { File.AppendAllText(Server.MapPath("~/Logs/AppErrors.txt"), message); }
            catch { }
        }
    }
}
