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

        // VAPT "Password in plain text" (rescan): the site had no HTTP->HTTPS redirect after the
        // IIS <rewrite> block was removed (this server has no URL Rewrite module installed - see
        // Web.config history), so a request could still reach the login form over plain HTTP and
        // submit credentials unencrypted. Done at the application level instead - no IIS module
        // required. Checks X-Forwarded-Proto first (the standard header a reverse proxy/CDN sets to
        // the scheme the client actually used; Request.IsSecureConnection would read false for
        // every request if TLS is terminated upstream, causing a redirect loop) and falls back to
        // Request.IsSecureConnection when that header isn't present, i.e. this app is hit directly.
        protected void Application_BeginRequest()
        {
            if (Request.IsSecureConnection) return;

            // Local dev/debugging (IIS Express on localhost, e.g. F5 in Visual Studio) has no HTTPS
            // listener on the same port - redirecting there would just be a dead end
            // (ERR_SSL_PROTOCOL_ERROR, confirmed via DevTools showing a 301 to
            // https://localhost:1311/... with nothing speaking TLS there).
            //
            // Both Request.IsLocal and Request.Url.IsLoopback were tried first and neither reliably
            // detected this as local in this environment (confirmed empirically - the redirect still
            // fired). Checking the port instead sidesteps hostname/loopback detection entirely: real
            // production traffic always arrives through Cloudflare on the standard HTTP port 80 (the
            // X-Forwarded-Proto check above already handles the case where TLS terminated upstream) -
            // there's no legitimate reason to redirect a request on any other port.
            if (Request.Url.Port != 80) return;

            string forwardedProto = Request.Headers["X-Forwarded-Proto"];
            if (!string.IsNullOrEmpty(forwardedProto))
            {
                if (string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase)) return;
            }

            // Request.Url.Host is the hostname ONLY - it never includes the port (that's the separate
            // Request.Url.Port property). Appending it bare works for production (standard port 443,
            // implicit) but would silently drop any non-standard port for anything else hitting this
            // path. Preserve it explicitly whenever it isn't the plain-HTTP default (80).
            string host = Request.Url.Host;
            if (Request.Url.Port != 80 && Request.Url.Port != -1)
            {
                host += ":" + Request.Url.Port;
            }
            string httpsUrl = "https://" + host + Request.RawUrl;
            Response.Status = "301 Moved Permanently";
            Response.RedirectLocation = httpsUrl;
            Response.End();
        }

        // VAPT "Missing HTTP Security Headers" (rescan): removes 'unsafe-inline' from script-src.
        // One fresh nonce per request - every inline <script> tag across the app carries this same
        // value (see CspNonce.cs), so only scripts this app actually rendered can run; an injected
        // <script> from an XSS payload won't have the current nonce and is blocked.
        protected void Application_AcquireRequestState()
        {
            CspNonce.Generate();
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

            // VAPT "Missing HTTP Security Headers" (rescan): built here instead of as a static
            // Web.config <customHeaders> entry because it needs this request's nonce (see
            // CspNonce.cs). Two browsers can't safely receive two different CSP headers on the same
            // response, so this is now the single source of truth for this header - nothing else
            // should add a Content-Security-Policy entry to Web.config.
            //
            // script-src: 'unsafe-inline' is gone, replaced by the nonce - only <script> tags this
            // app actually rendered (carrying nonce="@CspNonce.Current") can execute.
            //
            // 'unsafe-eval' is also gone. Audited every eval()/new Function() call site actually
            // reachable by this app (most of the ~30 files matching "eval(" turned out to be dead
            // theme-bundle files never loaded by any view, or false-positive matches like Angular's
            // $eval() scope method, not the real eval() global):
            //  - Scripts/pdf.js (55 views): one real `new Function()` call, in FontFaceObject's glyph
            //    compiler, already gated behind `this.options.isEvalSupported && IsEvalSupportedCached.value`
            //    - the library's own author-provided CSP-safe fallback path. Nothing to change here.
            //  - AngularJS + ui-select (35 views load the scripts): real `new Function()` calls exist
            //    in Angular's $parse service, but Angular is never bootstrapped anywhere in this app -
            //    no ng-app, no angular.bootstrap(), and zero ng-* directives or {{ }} interpolation in
            //    any view (confirmed via full-codebase search). The scripts load but nothing on any
            //    page ever invokes Angular, so this code path never executes regardless of CSP.
            //  - Every other file matching "eval(" (bootstrap-markdown, morris examples, wysihtml5,
            //    jquery-gantt, codemirror, jquery-1.10.2.js, etc.) is not loaded by any view at all.
            //
            // script-src-attr: kept at 'unsafe-inline' deliberately. Nonces only cover <script>
            // elements, never inline event-handler attributes (onclick=, onchange=, etc.) - CSP
            // Level 3 lets those be governed independently via this sub-directive. ~18 views still
            // use inline handlers; rewriting them to addEventListener is separate follow-up work,
            // not silently broken by this change.
            string nonce = CspNonce.Current;
            string csp = "default-src 'self'; "
                + "script-src 'self' 'nonce-" + nonce + "' https://code.jquery.com https://ajax.googleapis.com https://cdn.datatables.net https://infoviz.cv.tatamotors https://infoviz.tatamotors.com; "
                + "script-src-attr 'unsafe-inline'; "
                + "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.datatables.net; "
                + "font-src 'self' data: https://fonts.gstatic.com; "
                + "img-src 'self' data: https://*.gstatic.com; "
                + "connect-src 'self' https://infoviz.cv.tatamotors https://infoviz.tatamotors.com; "
                + "frame-src 'self' https://infoviz.cv.tatamotors https://infoviz.tatamotors.com; "
                + "frame-ancestors 'self'; object-src 'none';";
            response.Headers.Remove("Content-Security-Policy");
            response.Headers.Add("Content-Security-Policy", csp);
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
