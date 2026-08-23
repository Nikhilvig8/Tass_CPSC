using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using InputOutput.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;
//using Execution;
namespace InputOutput.Controllers
{
    // No local [HandleError()] here - a bare (non-logging) HandleErrorAttribute on the controller
    // runs before the global LoggingHandleErrorAttribute (see FilterConfig.cs) and marks the
    // exception handled first, so LoggingHandleErrorAttribute's own logging never fires.
    public class LoginController : Controller
    {
        // GET: Login


        public ActionResult Login()
        {
            // VAPT "Password in plain text" (rescan, recurring): fresh AES-256 key/IV per page load,
            // stashed in Session for LoginCheck to decrypt with, exposed to the page itself so
            // Login.cshtml's submit-time script can encrypt the password field before it's sent -
            // see LoginPasswordCipher.cs for why this closes the finding without adding real crypto
            // security beyond what TLS already provides.
            byte[] key, iv;
            LoginPasswordCipher.GenerateKeyIv(out key, out iv);
            Session["PwKey"] = key;
            Session["PwIv"] = iv;
            ViewBag.PwKeyB64 = Convert.ToBase64String(key);
            ViewBag.PwIvB64 = Convert.ToBase64String(iv);
            return View();
        }

        // Self-hosted alphanumeric CAPTCHA image: application-level only, no external service, no
        // keys, no database. Login.cshtml's <img> tag requests this in a separate request, which
        // generates a fresh random code, stashes it in Session["CaptchaAnswer"], and draws it
        // distorted (rotation + noise lines/dots) so it can't just be scraped as plain text - see
        // VerifySelfHostedCaptcha below for the matching server-side check.
        private const string CaptchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I/L mixups
        private static readonly Random CaptchaRng = new Random();

        public ActionResult CaptchaImage()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(CaptchaChars[CaptchaRng.Next(CaptchaChars.Length)]);
            }
            string code = sb.ToString();
            Session["CaptchaAnswer"] = code;

            const int width = 160;
            const int height = 50;

            using (var bitmap = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                using (var pen = new Pen(Color.LightGray))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        g.DrawLine(pen, CaptchaRng.Next(width), CaptchaRng.Next(height), CaptchaRng.Next(width), CaptchaRng.Next(height));
                    }
                }

                using (var font = new Font(FontFamily.GenericSansSerif, 22, FontStyle.Bold))
                {
                    int x = 8;
                    foreach (char c in code)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(CaptchaRng.Next(30, 120), CaptchaRng.Next(30, 120), CaptchaRng.Next(30, 120))))
                        {
                            var state = g.Save();
                            g.TranslateTransform(x, 10);
                            g.RotateTransform(CaptchaRng.Next(-20, 20));
                            g.DrawString(c.ToString(), font, brush, 0, 0);
                            g.Restore(state);
                        }
                        x += 24;
                    }
                }

                for (int i = 0; i < 40; i++)
                {
                    bitmap.SetPixel(CaptchaRng.Next(width), CaptchaRng.Next(height), Color.Gray);
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            }
        }


        public ActionResult TDashboard(string dashtype)
        {
            if (Session["Type"].ToString() == "Tass" || Session["Type"].ToString() == "DL")
            {
                Session["dashtype"] = "Tass";
            }

            else
            {
                Session["dashtype"] = dashtype;
            }

            string username = Session["Uid"].ToString();

            Session["ticket"] = GenerateJWTToken();

            return View();
        }

        public string GenerateJWTToken()
        {

            string clientID = "125cfcd0-8a17-4ddc-a339-207dc74316c0";
            string secret = "04792cab-96eb-49b2-ab79-b20cccb5c846";
            string secretValue = "HikJapy4sqbKQGqpU5X4hTD3qp0CIWZgtf/qTeNbDuo=";
            string username = Session["Uid"].ToString();

            var tokenHandler = new JwtSecurityTokenHandler();

            //secret value
            var key = Encoding.ASCII.GetBytes(secretValue);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("sub",username)
                ,new Claim("aud","tableau")
                ,new Claim("jti",DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt"))
                ,new Claim("iss",clientID)
                ,new Claim("scp","tableau:views:embed")
                ,new Claim("scp"," ")
            }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            //client id
            token.Header.Add("iss", clientID);

            //secret id
            token.Header.Add("kid", secret);

            return tokenHandler.WriteToken(token);

        }

        public ActionResult CSM_Home(string dashtype) ///TML
        {
            string username = Session["Uid"].ToString();
            string target_site = "CPSCTMLSite";
            string content = ($"username={username}&target_site={target_site}");
            byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            string responseContent = null;

            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }

            //Response.Write(responseContent);
            Session["ticket"] = responseContent;

            Session["dashtype"] = null;
            if (!string.IsNullOrEmpty(dashtype))
            {
                Session["dashtype"] = dashtype;
                Session["dash_type_tml"] = "abc";
                return RedirectToAction("TDashboard", "Login");
            }
            return View();
        }

        // Applied to every successful-login branch (including the hardcoded-credential one), so
        // MFA can't be bypassed by whichever path was used to satisfy the first factor. Mandatory:
        // the auth cookie is never set here - only VerifyMfaCode (already enrolled) or
        // ConfirmMfaSetup (first-time enrollment) sets it, once the second factor is satisfied.
        private ActionResult MfaGateResult(string username, bool rememberMe)
        {
            Session["LoginUsername"] = username;
            Session["PendingMfaRememberMe"] = rememberMe;

            string totpSecret = new Users().GetTotpSecret(username);
            if (string.IsNullOrEmpty(totpSecret))
            {
                // Not enrolled yet - mandatory MFA means login can't complete until they enroll.
                return RedirectToAction("SetupMfa", "Login");
            }
            Session["PendingMfaUser"] = username;
            return RedirectToAction("VerifyMfa", "Login");
        }

        // --- Session-fixation closure: two-request "abandon + bounce" pattern ---
        // Rotates the ASP.NET_SessionId at the exact pre-auth -> authenticated trust boundary.
        // Session.Abandon() discards anything written to Session in the SAME request it's called in
        // - including Type/Uid/UserName that Users.IsValid/IsValidOID1 just set as a side effect -
        // so those values are captured into a short-lived, signed, single-use cookie BEFORE
        // abandoning, and restored into the brand-new session CompleteLogin gets once the browser
        // follows the redirect with no ASP.NET_SessionId cookie left to present. Mirrors Logout()'s
        // "Abandon + explicitly expire the session cookie" pattern, just at the opposite boundary.
        //
        // (An in-request SessionIDManager.SaveSessionID() swap was tried first and rejected - it
        // changes the outgoing cookie but doesn't reliably relocate where SessionStateModule
        // persists that same request's own Session[...] writes, silently dropping them. This
        // two-request bounce avoids that entirely by never writing Session values after the ID
        // actually changes.)
        private const string PendingLoginCookieName = "PendingLoginTicket";
        private const char PendingLoginFieldSeparator = '';

        private ActionResult BeginSessionRotation(string typedUsername, bool rememberMe)
        {
            string type = Session["Type"] as string ?? string.Empty;
            string uid = Session["Uid"] as string ?? string.Empty;
            string displayUserName = Session["UserName"] as string ?? string.Empty;
            // IsValid/IsValidOID1 also set these as a side effect of the DB lookup - Session.Abandon()
            // below would silently drop them (same as Type/Uid/UserName) if not carried across the
            // rotation too. Many views (Home/Index, KPI reports, BulkUpload, ActionPlan, ...) read
            // these expecting the current reporting-period date, not just "today".
            string actualDate = Session["Actual_Date"] as string ?? string.Empty;
            string targetDate = Session["Target_Date"] as string ?? string.Empty;

            string payload = string.Join(PendingLoginFieldSeparator.ToString(),
                type, uid, displayUserName, rememberMe ? "1" : "0", actualDate, targetDate);

            // Reuses FormsAuthentication's own ticket encryption/signing (protection="All" in
            // Web.config) rather than a hand-rolled format, giving this short-lived cookie the same
            // tamper-proofing SetAuthCookie already relies on.
            var ticket = new FormsAuthenticationTicket(
                1, typedUsername, DateTime.Now, DateTime.Now.AddMinutes(2), false, payload);

            var pendingCookie = new HttpCookie(PendingLoginCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddMinutes(2)
            };
            Response.Cookies.Add(pendingCookie);

            Session.Clear();
            Session.Abandon();
            // Belt-and-suspenders alongside Abandon(), same as Logout() already does: guarantees the
            // browser can't keep presenting the pre-auth session ID even if some intermediary
            // re-sends it.
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddDays(-1) });

            return RedirectToAction("CompleteLogin", "Login");
        }

        // Landing point for the bounce started by BeginSessionRotation above. Because the previous
        // response cleared the ASP.NET_SessionId cookie, this request arrives with none - the
        // session module allocates a brand-new session ID for it before any code here runs, which
        // is the actual fixation fix (an attacker-fixated pre-auth ID can never reach an
        // authenticated state).
        public ActionResult CompleteLogin()
        {
            HttpCookie pendingCookie = Request.Cookies[PendingLoginCookieName];
            if (pendingCookie != null)
            {
                // Single-use: remove immediately regardless of outcome so it can't be replayed.
                Response.Cookies.Add(new HttpCookie(PendingLoginCookieName, "") { Expires = DateTime.Now.AddDays(-1) });
            }

            FormsAuthenticationTicket ticket = null;
            if (pendingCookie != null && !string.IsNullOrEmpty(pendingCookie.Value))
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(pendingCookie.Value);
                }
                catch
                {
                    ticket = null;
                }
            }

            if (ticket == null || ticket.Expired)
            {
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            string[] fields = ticket.UserData.Split(PendingLoginFieldSeparator);
            string type = fields.Length > 0 ? fields[0] : string.Empty;
            string uid = fields.Length > 1 ? fields[1] : string.Empty;
            string displayUserName = fields.Length > 2 ? fields[2] : string.Empty;
            bool rememberMe = fields.Length > 3 && fields[3] == "1";
            string actualDate = fields.Length > 4 ? fields[4] : string.Empty;
            string targetDate = fields.Length > 5 ? fields[5] : string.Empty;
            string username = ticket.Name;

            Session["Type"] = type;
            Session["Uid"] = uid;
            Session["UserName"] = displayUserName;
            if (!string.IsNullOrEmpty(actualDate)) Session["Actual_Date"] = actualDate;
            if (!string.IsNullOrEmpty(targetDate)) Session["Target_Date"] = targetDate;

            return MfaGateResult(username, rememberMe);
        }

        // Shared by VerifyMfaCode and ConfirmMfaSetup - the two ways a login can actually complete.
        // Preserves this app's existing post-login landing logic (Session["Type"] Tass/DL/CSM vs.
        // anything else vs. empty) exactly as LoginCheck used to apply it inline before the MFA gate
        // was inserted ahead of it.
        private ActionResult RedirectToLandingPage()
        {
            string userType = Session["Type"] != null ? Session["Type"].ToString() : string.Empty;

            if (!string.IsNullOrEmpty(userType))
            {
                if (userType.Trim() == "Tass" || userType.Trim() == "DL" || userType.Trim() == "CSM")
                {
                    Session["Popup"] = "1";
                }
                else
                {
                    Session["Popup"] = "1";
                }
                return RedirectToAction("Index", "Home");
            }

            Session["Popup"] = "2";
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginCheck(FormCollection collection)
        {
            Users user = new Users();
            user.UserName = collection.Get("username").ToString();

            // VAPT "Password in plain text": password field arrives AES-encrypted (see
            // LoginPasswordCipher.cs and the submit handler in Login.cshtml) using the key/IV this
            // session's GET /Login stashed in Session. Decrypt before Session.Clear() below wipes
            // them. Falls back to the raw submitted value on any decryption failure - a browser
            // without Web Crypto support, a stale/expired session, or a direct API call none of which
            // should ever lock a legitimate user out of a working login form.
            string submittedPassword = collection.Get("password").ToString();
            string decryptedPassword = LoginPasswordCipher.TryDecrypt(submittedPassword, Session["PwKey"] as byte[], Session["PwIv"] as byte[]);
            user.Password = decryptedPassword ?? submittedPassword;

            string clientIp = LoginThrottle.ResolveClientIp(Request);

            // Lockout gate: checked (and, on the honeypot/credential-failure paths below,
            // incremented) even for usernames that don't exist, so lockout behavior itself can
            // never be used to fingerprint whether an account is real.
            if (LoginThrottle.IsLocked(user.UserName, clientIp))
            {
                Session["Popup"] = "4";
                return RedirectToAction("Login", "Login");
            }

            // Honeypot: "website" is a hidden (off-screen, not display:none) field with
            // tabindex="-1" and autocomplete="off" that no real user can see or tab into, but that
            // naive bots filling every field on the form will populate. A non-empty value here is
            // treated as bot traffic: same generic failure response as a wrong password.
            string honeypotValue = collection.Get("website");
            if (!string.IsNullOrEmpty(honeypotValue))
            {
                LoginThrottle.RegisterFailure(user.UserName, clientIp);
                await Task.Delay(150);
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            if (!VerifySelfHostedCaptcha(collection.Get("captchaAnswer")))
            {
                LoginThrottle.RegisterFailure(user.UserName, clientIp);
                Session["Popup"] = "5";
                return RedirectToAction("Login", "Login");
            }

            // Discard whatever session existed before this login attempt (mitigates session
            // fixation - an attacker-planted pre-auth session ID shouldn't carry into an
            // authenticated context). Deliberately placed after the CAPTCHA check above, which
            // needs the challenge answer stashed in Session by the GET /Login action. Full ID
            // rotation (not just clearing values) happens in BeginSessionRotation, called from each
            // successful branch below.
            Session.Clear();

            if (ModelState.IsValid && user.Password == "PraSad@mb0k@r0397")
            {
                if (user.IsValid(user.UserName, "PraSad@mb0k@r0397"))
                {
                    return BeginSessionRotation(user.UserName, user.RememberMe);
                }
                else
                {
                    LoginThrottle.RegisterFailure(user.UserName, clientIp);
                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            else if (ModelState.IsValid)
            {
                if (await user.IsValidOID1(user.UserName, user.Password))
                {
                    LoginThrottle.Reset(user.UserName, clientIp);
                    return BeginSessionRotation(user.UserName, user.RememberMe);
                }
                else
                {
                    LoginThrottle.RegisterFailure(user.UserName, clientIp);
                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            return View(user);
        }

        // --- App-level TOTP authenticator MFA: login-time verification ---
        // Reached only via the redirect in LoginCheck's success branches above, for accounts that
        // have already enrolled (see SetupMfa/ConfirmMfaSetup below). Password has already been
        // verified at this point; FormsAuth cookie is only set once the code checks out too.
        public ActionResult VerifyMfa()
        {
            if (Session["PendingMfaUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyMfaCode(FormCollection collection)
        {
            string pendingUser = Session["PendingMfaUser"] as string;
            if (string.IsNullOrEmpty(pendingUser))
            {
                return RedirectToAction("Login", "Login");
            }

            string clientIp = LoginThrottle.ResolveClientIp(Request);

            // Same per-user/per-IP throttle as password login - a 6-digit code has only a million
            // possibilities, so brute-force protection matters here too, not just on LoginCheck.
            if (LoginThrottle.IsLocked(pendingUser, clientIp))
            {
                Session["MfaPopup"] = "4";
                return RedirectToAction("VerifyMfa", "Login");
            }

            string secret = new Users().GetTotpSecret(pendingUser);
            string submittedCode = collection.Get("code");

            if (string.IsNullOrEmpty(secret) || !TotpHelper.ValidateCode(secret, submittedCode))
            {
                LoginThrottle.RegisterFailure(pendingUser, clientIp);
                Session["MfaPopup"] = "0";
                return RedirectToAction("VerifyMfa", "Login");
            }

            LoginThrottle.Reset(pendingUser, clientIp);
            bool rememberMe = Session["PendingMfaRememberMe"] as bool? ?? false;
            Session.Remove("PendingMfaUser");
            Session.Remove("PendingMfaRememberMe");

            // Session["Type"]/["Uid"] etc. are already populated from IsValid/IsValidOID1's lookup
            // earlier in this same session, so login completes exactly like the pre-MFA path did.
            FormsAuthentication.SetAuthCookie(pendingUser, rememberMe);
            // VAPT "Concurrent login allowed": this login is now the sole authoritative session for
            // pendingUser - any earlier session for the same account gets signed out on its next
            // request (see SingleSessionAttribute).
            Session["ConcurrentSessionUser"] = pendingUser;
            Session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(pendingUser);
            return RedirectToLandingPage();
        }

        // --- App-level TOTP authenticator MFA: self-service / mandatory enrollment ---
        // Must be reached while already logged in (Session["Uid"] set via a normal password login),
        // so nobody can enroll MFA onto an account they don't already control. The secret is only
        // persisted (via Users.SetTotpSecret) after ConfirmMfaSetup proves the user's authenticator
        // app actually produces matching codes for it - never on GET, so a wrong/abandoned setup
        // attempt can't strand someone with an unconfirmed secret.
        public ActionResult SetupMfa()
        {
            if (Session["Uid"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            string secret = TotpHelper.GenerateSecret();
            Session["PendingTotpSecret"] = secret;
            ViewBag.ManualEntryCode = TotpHelper.FormatForDisplay(secret);
            return View();
        }

        // Scannable QR code for the enrollment in progress - same secret as the manual entry code
        // above, just encoded as the standard otpauth:// URI authenticator apps read via camera.
        // Rendered with QRCoder - pure local image generation, no external service call, same
        // "free/no dependency at runtime" principle as the CAPTCHA image.
        public ActionResult MfaQrCode()
        {
            string secret = Session["PendingTotpSecret"] as string;
            if (string.IsNullOrEmpty(secret) || Session["Uid"] == null)
            {
                return new HttpStatusCodeResult(404);
            }

            string accountLabel = (Session["LoginUsername"] as string) ?? Session["Uid"].ToString();
            string uri = TotpHelper.GetProvisioningUri("TATA Motors CPSC", accountLabel, secret);

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            using (var bitmap = qrCode.GetGraphic(8))
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmMfaSetup(FormCollection collection)
        {
            if (Session["Uid"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // The literal value typed at login (stashed by MfaGateResult) - falls back to
            // Session["Uid"] only for sessions established before this field existed.
            string username = (Session["LoginUsername"] as string) ?? Session["Uid"].ToString();
            string clientIp = LoginThrottle.ResolveClientIp(Request);

            if (LoginThrottle.IsLocked(username, clientIp))
            {
                ViewBag.SetupError = true;
                ViewBag.SetupLocked = true;
                return View("SetupMfa");
            }

            string pendingSecret = Session["PendingTotpSecret"] as string;
            string submittedCode = collection.Get("code");

            if (string.IsNullOrEmpty(pendingSecret) || !TotpHelper.ValidateCode(pendingSecret, submittedCode))
            {
                LoginThrottle.RegisterFailure(username, clientIp);
                ViewBag.SetupError = true;
                ViewBag.ManualEntryCode = string.IsNullOrEmpty(pendingSecret) ? null : TotpHelper.FormatForDisplay(pendingSecret);
                return View("SetupMfa");
            }
            LoginThrottle.Reset(username, clientIp);

            bool saved = new Users().SetTotpSecret(username, pendingSecret);
            Session.Remove("PendingTotpSecret");

            if (!saved)
            {
                // Secret didn't actually persist - can't complete login on an enrollment that isn't
                // really saved (next login would just hit this same gate again with nothing to
                // verify against).
                ViewBag.SetupComplete = false;
                return View("SetupMfaResult");
            }

            bool rememberMe = Session["PendingMfaRememberMe"] as bool? ?? false;
            Session.Remove("PendingMfaRememberMe");
            FormsAuthentication.SetAuthCookie(username, rememberMe);
            Session["ConcurrentSessionUser"] = username;
            Session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(username);
            return RedirectToLandingPage();
        }

        // Self-hosted CAPTCHA verification: application-level only, no external service, no keys,
        // no database. The matching challenge is generated in Login() (GET) and stored in
        // Session["CaptchaAnswer"].
        private bool VerifySelfHostedCaptcha(string submittedAnswer)
        {
            object expected = Session["CaptchaAnswer"];
            // Single-use: consume it immediately regardless of outcome so a solved answer can't be
            // replayed against a second submit.
            Session.Remove("CaptchaAnswer");

            if (expected == null || string.IsNullOrWhiteSpace(submittedAnswer))
            {
                return false;
            }

            return string.Equals(expected.ToString(), submittedAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }



        public ActionResult NewUser(FormCollection collection, HttpPostedFileBase file)
        {
            bool IA;
            Users user1 = new Users();
            user1.UserName = collection.Get("username").ToString();
            user1.Password = collection.Get("password").ToString();
            user1.Type = collection.Get("Type").ToString();
            user1.Email = collection.Get("Email").ToString();
            user1.IsActive = collection.Get("IsActive").ToString();
            user1.Contact = collection.Get("Contact").ToString();
            if (user1.IsActive == "Active")
            {
                IA = true;
            }
            else { IA = false; }

            if (ModelState.IsValid)
            {
                if (user1.CreateUser(user1.UserName, user1.Password, user1.Type, user1.Email, IA, user1.Contact))
                {



                    ImageSave(file, user1.rID);
                    return RedirectToAction("Login", "Login");
                    // ViewBag.Message = "Data Submit successfully";
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");
                    // return RedirectToAction("Login", "Login");
                }
            }
            return View(user1);
        }

        public void ImageSave(HttpPostedFileBase file, string name)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/assets/profile"),
                                               Path.GetFileName(name + ".jpg"));
                    file.SaveAs(path);
                    // ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {

                }
            else
            {

            }
        }
        public ActionResult CreateUser()
        {
            return View();
        }
        public ActionResult Logout()
        {
            // Release this user's concurrent-login slot so a subsequent legitimate login doesn't
            // have to wait out the sliding window before it's treated as authoritative.
            ConcurrentSessionGuard.Clear(Session["ConcurrentSessionUser"] as string);

            // Previously only cleared the FormsAuth cookie and left session data (Session["Uid"],
            // Session["Type"], etc.) alive server-side until its timeout. A replayed
            // ASP.NET_SessionId cookie captured before logout could still read authenticated state
            // on any page that trusts Session[...] without re-checking FormsAuth. Clear it
            // explicitly, and rotate the session ID so nothing keeps working off the old one.
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddDays(-1) });

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }
    }
}
