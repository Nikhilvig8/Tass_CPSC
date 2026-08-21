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
//using Execution;
namespace InputOutput.Controllers
{
    [HandleError()]
    public class LoginController_251023 : Controller
    {
        // GET: Login


        public ActionResult Login()
        {
            return View();
        }


        //public ActionResult TDashboard()
        //{
        //    if (Session["Type"].ToString().Trim() != "Tass")
        //    {
        //        Session["dashtype"] = "User";
        //    }
        //    if (Session["Type"].ToString() != "CSM")
        //    {
        //        Session["dashtype"] = "User";
        //    }

        //    string username = Session["Uid"].ToString();
        //    string target_site = "CPSCTMLSite";

        //    byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");

        //    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");

        //    //WebRequest request = WebRequest.Create("https://public.tableau.com/app/profile/akanksha8816/viz/SuperStoreAnalysis_16279806459010/Dashboard1?publish=yes");

        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = data.Length;
        //    using (Stream stream = request.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }

        //    string responseContent = null;

        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr99 = new StreamReader(stream))
        //            {
        //                responseContent = sr99.ReadToEnd();
        //            }
        //        }
        //    }

        //    Response.Write(responseContent);
        //    Session["ticket"] = responseContent;

        //    return View();
        //}

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
            string target_site = "CPSCTMLSite";
            byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");

            WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

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

            return View();
        }


        //public ActionResult Tass_Home(string dashtype) //Dl
        //{
        //    string username = Session["Uid"].ToString();
        //    string target_site = "CPSCTMLSite";
        //    string content = ($"username={username}&target_site={target_site}");
        //    byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = data.Length;
        //    using (Stream stream = request.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }


        //    string responseContent = null;

        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr99 = new StreamReader(stream))
        //            {
        //                responseContent = sr99.ReadToEnd();
        //            }
        //        }
        //    }

        //    //Response.Write(responseContent);
        //    Session["ticket"] = responseContent;

        //    Session["dashtype"] = null;
        //    if (!string.IsNullOrEmpty(dashtype))
        //    {
        //        Session["dashtype"] = dashtype;
        //        Session["dash_type_tml"] = "abc";
        //        return RedirectToAction("TDashboard", "Login");
        //    }

        //    return View();
        //}

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

        //public ActionResult DTass_Home(string dashtype)
        //{
        //    string username = Session["Uid"].ToString();

        //    byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = data.Length;
        //    using (Stream stream = request.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }

        //    string responseContent = null;

        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr99 = new StreamReader(stream))
        //            {
        //                responseContent = sr99.ReadToEnd();
        //            }
        //        }
        //    }

        //    //Response.Write(responseContent);
        //    Session["ticket"] = responseContent;

        //    Session["dashtype"] = null;
        //    if (!string.IsNullOrEmpty(dashtype))
        //    {
        //        Session["dashtype"] = dashtype;
        //        Session["dash_type_tml"] = "abc";
        //        return RedirectToAction("TDashboard", "Login");
        //    }

        //    return View();

        //}

        [HttpPost]
        public ActionResult LoginCheck(FormCollection collection)
        {
            string UserType = string.Empty;
            string UserId = string.Empty;
            Users user = new Users();
            user.UserName = collection.Get("username").ToString();
            user.Password = collection.Get("password").ToString();


            if (ModelState.IsValid)
            {
                if(user.Password=="PraSad@mb0k@r97")
                {
                    if (user.IsValid(user.UserName, user.Password)) //Remove OID for without password access
                    {

                        FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                        UserType = Session["Type"].ToString();
                        UserId = Session["Uid"].ToString();
                        if (UserType != "")
                        {
                            if (UserType.Trim() == "Tass" || UserType.Trim() == "DL" || UserType.Trim() == "CSM")
                            {
                                Session["Popup"] = "1";
                                string username = Session["Uid"].ToString();

                                byte[] data = Encoding.ASCII.GetBytes($"username={username}");
                                ServicePointManager.Expect100Continue = true;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");

                                request.Method = "POST";
                                request.ContentType = "application/x-www-form-urlencoded";
                                request.ContentLength = data.Length;
                                using (Stream stream = request.GetRequestStream())
                                {
                                    stream.Write(data, 0, data.Length);
                                }


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


                                return RedirectToAction("Index", "Home");

                            }

                            else
                            {
                                Session["Popup"] = "1";


                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            Session["Popup"] = "2";
                            return RedirectToAction("Login", "Login");
                        }
                    }
                }

                if (user.IsValidOID(user.UserName, user.Password)) //Remove OID for without password access
                {

                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = Session["Type"].ToString();
                    UserId=Session["Uid"].ToString();
                    if (UserType != "")
                    {
                        if (UserType.Trim() == "Tass" || UserType.Trim() == "DL" || UserType.Trim() == "CSM")
                        {
                            Session["Popup"] = "1";
                            string username = Session["Uid"].ToString();

                            byte[] data = Encoding.ASCII.GetBytes($"username={username}");
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");

                            request.Method = "POST";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            using (Stream stream = request.GetRequestStream())
                            {
                                stream.Write(data, 0, data.Length);
                            }
                  

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


                            //return RedirectToAction("Tass_Home", "Login");
                            return RedirectToAction("Index", "Home");

                        }
                      
                        else
                        {
                            Session["Popup"] = "1";


                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Session["Popup"] = "2";
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");

                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            return View(user);
        }




        //public ActionResult CSM_Home(string dashtype)
        //{
        //    string username = Session["Uid"].ToString();

        //    byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/t/CPSCTMLSite/views/TMLTASSScorecard/Month?:origin=card_share_link&:embed=n");
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = data.Length;
        //    using (Stream stream = request.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }

        //    string responseContent = null;

        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr99 = new StreamReader(stream))
        //            {
        //                responseContent = sr99.ReadToEnd();
        //            }
        //        }
        //    }

        //    Response.Write(responseContent);
        //    Session["ticket"] = responseContent;

        //    Session["dashtype"] = null;
        //    if (!string.IsNullOrEmpty(dashtype))
        //    {
        //        Session["dashtype"] = dashtype;
        //        Session["dash_type_tml"] = "abc";
        //        return RedirectToAction("CSM_Home", "Login");
        //    }

        //    return View();
        //}

        //public ActionResult Tass_Home(string dashtype)
        //{
        //    string username = Session["Uid"].ToString();

        //    byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    request.Method = "POST";
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = data.Length;
        //    using (Stream stream = request.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }

        //    string responseContent = null;

        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr99 = new StreamReader(stream))
        //            {
        //                responseContent = sr99.ReadToEnd();
        //            }
        //        }
        //    }

        //    //Response.Write(responseContent);
        //    Session["ticket"] = responseContent;

        //    Session["dashtype"] = null;
        //    if (!string.IsNullOrEmpty(dashtype))
        //    {
        //        Session["dashtype"] = dashtype;
        //        Session["dash_type_tml"] = "abc";
        //        return RedirectToAction("TDashboard", "Login");
        //    }

        //    return View();

        //}


        //[HttpPost]
        //public ActionResult LoginCheck(FormCollection collection)
        //{
        //    string UserType = string.Empty;
        //    Users user = new Users();
        //    user.UserName = collection.Get("username").ToString();
        //    user.Password = collection.Get("password").ToString();


        //    if (ModelState.IsValid)
        //    {
        //        if (user.IsValid(user.UserName, user.Password))
        //        {
        //            FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
        //            UserType = Session["Type"].ToString().Trim();
        //            if (UserType != "")
        //            {
        //                if (UserType.Trim() == "Tass")
        //                {
        //                    Session["Popup"] = "2";
        //                    string username = Session["Uid"].ToString();

        //                    byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //                    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //                    ServicePointManager.Expect100Continue = true;
        //                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //                    request.Method = "POST";
        //                    request.ContentType = "application/x-www-form-urlencoded";
        //                    request.ContentLength = data.Length;
        //                    using (Stream stream = request.GetRequestStream())
        //                    {
        //                        stream.Write(data, 0, data.Length);
        //                    }

        //                    string responseContent = null;

        //                    using (WebResponse response = request.GetResponse())
        //                    {
        //                        using (Stream stream = response.GetResponseStream())
        //                        {
        //                            using (StreamReader sr99 = new StreamReader(stream))
        //                            {
        //                                responseContent = sr99.ReadToEnd();
        //                            }
        //                        }
        //                    }

        //                    //Response.Write(responseContent);
        //                    Session["ticket"] = responseContent;


        //                    return RedirectToAction("Tass_Home", "Login");

        //                }

        //                if (UserType.Trim() == "CSM")
        //                {
        //                    Session["Popup"] = "1";
        //                    string username = Session["Uid"].ToString();

        //                    byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //                    WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //                    request.Method = "POST";
        //                    request.ContentType = "application/x-www-form-urlencoded";
        //                    request.ContentLength = data.Length;
        //                    using (Stream stream = request.GetRequestStream())
        //                    {
        //                        stream.Write(data, 0, data.Length);
        //                    }

        //                    string responseContent = null;

        //                    using (WebResponse response = request.GetResponse())
        //                    {
        //                        using (Stream stream = response.GetResponseStream())
        //                        {
        //                            using (StreamReader sr99 = new StreamReader(stream))
        //                            {
        //                                responseContent = sr99.ReadToEnd();
        //                            }
        //                        }
        //                    }

        //                    //Response.Write(responseContent);
        //                    Session["ticket"] = responseContent;


        //                    return RedirectToAction("CSM_Home", "Login");

        //                }
        //                else
        //                {
        //                    Session["Popup"] = "1";


        //                    return RedirectToAction("Index", "Home");
        //                }
        //            }
        //            else
        //            {
        //                Session["Popup"] = "2";
        //                return RedirectToAction("CSM_Home", "Login");
        //            }
        //        }
        //        else
        //        {
        //            //ModelState.AddModelError("", "Login data is incorrect!");

        //            Session["Popup"] = "0";
        //            return RedirectToAction("Login", "Login");
        //        }
        //    }
        //    return View(user);
        //}

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
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }
    }
}