using System;
using System.Web;

namespace InputOutput
{
    // VAPT "Missing HTTP Security Headers" (rescan): removes 'unsafe-inline' from the
    // Content-Security-Policy script-src directive. A fresh random nonce is generated once per
    // request (Global.asax.Application_BeginRequest) and stashed in HttpContext.Items; the CSP
    // header (built in Global.asax.Application_PreSendRequestHeaders) allows only <script> tags
    // carrying that exact nonce, instead of every inline script indiscriminately.
    //
    // "InputOutput" is a globally-imported namespace for all Razor views (see Views/Web.config), so
    // any view can reference this directly as nonce="@CspNonce.Current" with no @using needed.
    public static class CspNonce
    {
        private const string ItemsKey = "CSPNonce";

        public static string Generate()
        {
            var bytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            string nonce = Convert.ToBase64String(bytes);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[ItemsKey] = nonce;
            }
            return nonce;
        }

        public static string Current
        {
            get
            {
                return HttpContext.Current != null
                    ? HttpContext.Current.Items[ItemsKey] as string
                    : null;
            }
        }
    }
}
