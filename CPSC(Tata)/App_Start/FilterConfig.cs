using System.Web;
using System.Web.Mvc;

namespace InputOutput
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Was HandleErrorAttribute() - a bare HandleErrorAttribute marks the exception handled
            // before Global.asax's Application_Error runs, so no action-level exception was ever
            // actually logged anywhere. LoggingHandleErrorAttribute logs first, then defers to the
            // same base behavior (same generic Error page either way).
            filters.Add(new LoggingHandleErrorAttribute());
            // VAPT "Back Refresh Attack": no MVC-rendered page is ever served from browser cache.
            filters.Add(new NoCacheAttribute());
            // VAPT "Concurrent login allowed": at most one active session per username.
            filters.Add(new SingleSessionAttribute());
        }
    }
}
