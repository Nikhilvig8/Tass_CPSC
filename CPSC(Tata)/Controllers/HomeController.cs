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
using DataUtilityLayer;
using System.Configuration;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Execution;


namespace InputOutput.Controllers
{
    [HandleError()]
    public class HomeController : Controller
    {
        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;

        //[Execution_Logs]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AdminIndex()
        {
            return View();
        }

        public ActionResult Data_Complition_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_Actual()
        {
            return View();
        }

        public ActionResult Data_Complition_Sales_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_Sales_Actual()
        {
            return View();
        }

        public ActionResult Data_Complition_AfterSales_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_AfterSales_Actual()
        {
            return View();
        }

        public ActionResult CurrentStatus()
        {
            return View();
        }

        public ActionResult CurrentStatus_target()
        {
            return View();
        }

        public ActionResult CurrentStatus_actual()
        {
            return View();
        }

        public ActionResult PandingInputTSM_CSMWise_Target()
        {
            return View();
        }

        public ActionResult PandingInputTSM_CSMWise_Actual()
        {
            return View();
        }

        public ActionResult DelayedData_Target()
        {
            return View();
        }

        public ActionResult DelayedData_Actual()
        {
            return View();
        }
        public ActionResult Unlock_Target()
        {
            return View();
        }
        public ActionResult Unlock_Actual()
        {
            return View();
        }
        public ActionResult ProfitabilityReport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProfitabilityReport(FormCollection form)
        {
            string userId = Session["Uid"].ToString();
            string tassCode = form["Dealer"];
            int year = Convert.ToInt32(form["Year"]);
            int month = Convert.ToInt32(form["Month"]);

            decimal question1 = Convert.ToDecimal(form["ManpowerExpenses"]);
            decimal question2 = Convert.ToDecimal(form["OverheadExpenses"]);
            decimal question3 = Convert.ToDecimal(form["RentExpenses"]);
            decimal question4 = Convert.ToDecimal(form["Depreciation"]);
            decimal question5 = Convert.ToDecimal(form["InterestonTermLoans"]);
            decimal question6 = Convert.ToDecimal(form["InterestonWorkingCapital"]);
            decimal question7 = Convert.ToDecimal(form["OtherWorkshopExpenses"]);
            decimal question8 = Convert.ToDecimal(form["SaleOfUsedOil"]);
            decimal question9 = Convert.ToDecimal(form["SaleOfScrap"]);

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                string checkSql = @"SELECT COUNT(*) FROM ProfitabilityReport 
                    WHERE UserId = @UserId AND TASSCode = @TASSCode 
                    AND [Year] = @Year AND [Month] = @Month";

                SqlCommand checkCmd = new SqlCommand(checkSql, con);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                checkCmd.Parameters.AddWithValue("@Year", year);
                checkCmd.Parameters.AddWithValue("@Month", month);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    string updateSql = @"
        UPDATE ProfitabilityReport
        SET Question1 = @Q1, Question2 = @Q2, Question3 = @Q3,
            Question4 = @Q4, Question5 = @Q5, Question6 = @Q6, Question7 = @Q7,
            Question8 = @Q8,Question9 = @Q9,
            UpdatedOn = GETDATE()
        WHERE UserId = @UserId AND TASSCode = @TASSCode
        AND [Year] = @Year AND [Month] = @Month";

                    SqlCommand updateCmd = new SqlCommand(updateSql, con);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    updateCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                    updateCmd.Parameters.AddWithValue("@Year", year);
                    updateCmd.Parameters.AddWithValue("@Month", month);
                    updateCmd.Parameters.AddWithValue("@Q1", question1);
                    updateCmd.Parameters.AddWithValue("@Q2", question2);
                    updateCmd.Parameters.AddWithValue("@Q3", question3);
                    updateCmd.Parameters.AddWithValue("@Q4", question4);
                    updateCmd.Parameters.AddWithValue("@Q5", question5);
                    updateCmd.Parameters.AddWithValue("@Q6", question6);
                    updateCmd.Parameters.AddWithValue("@Q7", question7);
                    updateCmd.Parameters.AddWithValue("@Q8", question8);
                    updateCmd.Parameters.AddWithValue("@Q9", question9);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    string insertSql = @"
        INSERT INTO ProfitabilityReport 
            (UserId, TASSCode, [Year], [Month],
             Question1, Question2, Question3, Question4, Question5, Question6, Question7, Question8, Question9)
        VALUES 
            (@UserId, @TASSCode, @Year, @Month,
             @Q1, @Q2, @Q3, @Q4, @Q5, @Q6, @Q7, @Q8, @Q9)";

                    SqlCommand insertCmd = new SqlCommand(insertSql, con);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                    insertCmd.Parameters.AddWithValue("@Year", year);
                    insertCmd.Parameters.AddWithValue("@Month", month);
                    insertCmd.Parameters.AddWithValue("@Q1", question1);
                    insertCmd.Parameters.AddWithValue("@Q2", question2);
                    insertCmd.Parameters.AddWithValue("@Q3", question3);
                    insertCmd.Parameters.AddWithValue("@Q4", question4);
                    insertCmd.Parameters.AddWithValue("@Q5", question5);
                    insertCmd.Parameters.AddWithValue("@Q6", question6);
                    insertCmd.Parameters.AddWithValue("@Q7", question7);
                    insertCmd.Parameters.AddWithValue("@Q8", question8);
                    insertCmd.Parameters.AddWithValue("@Q9", question9);
                    insertCmd.ExecuteNonQuery();
                }

                con.Close();
            }

            TempData["Success"] = "Record saved successfully.";
            return RedirectToAction("ProfitabilityReport");
        }

        public ActionResult WorkshopDetails()
        {
            return View();
        }

        [HttpPost]
        public ActionResult WorkshopDetails(FormCollection form)
        {
            string userId = Session["Uid"].ToString();
            string tassCode = form["Dealer"];
            int year = Convert.ToInt32(form["Year"]);
            int month = Convert.ToInt32(form["Month"]);

            decimal question1 = Convert.ToDecimal(form["ManpowerExpenses"]);
            decimal question2 = Convert.ToDecimal(form["OverheadExpenses"]);
            decimal question3 = Convert.ToDecimal(form["RentExpenses"]);
            decimal question4 = Convert.ToDecimal(form["Depreciation"]);
            decimal question5 = Convert.ToDecimal(form["InterestonTermLoans"]);
            decimal question6 = Convert.ToDecimal(form["InterestonWorkingCapital"]);
            decimal question7 = Convert.ToDecimal(form["OtherWorkshopExpenses"]);
            decimal question8 = Convert.ToDecimal(form["SaleOfUsedOil"]);
            decimal question9 = Convert.ToDecimal(form["CreditExpoToCustomer"]);
            decimal question10 = Convert.ToDecimal(form["CreditFromDistributor"]);
            decimal question11 = Convert.ToDecimal(form["ReceivablesFromTML"]);
            decimal question12 = Convert.ToDecimal(form["ReceivablesFromInsurance"]);
            decimal question13 = Convert.ToDecimal(form["SparePartsStock"]);

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                string checkSql = @"SELECT COUNT(*) FROM WorkshopDetails 
                    WHERE UserId = @UserId AND TASSCode = @TASSCode 
                    AND [Year] = @Year AND [Month] = @Month";

                SqlCommand checkCmd = new SqlCommand(checkSql, con);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                checkCmd.Parameters.AddWithValue("@Year", year);
                checkCmd.Parameters.AddWithValue("@Month", month);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    string updateSql = @"
        UPDATE WorkshopDetails
        SET Question1 = @Q1, Question2 = @Q2, Question3 = @Q3,
            Question4 = @Q4, Question5 = @Q5, Question6 = @Q6, Question7 = @Q7,
            Question8 = @Q8,Question9 = @Q9, Question10 = @Q10, Question11 = @Q11, Question12 = @Q12,
            Question13 = @Q13,
            UpdatedOn = GETDATE()
        WHERE UserId = @UserId AND TASSCode = @TASSCode
        AND [Year] = @Year AND [Month] = @Month";

                    SqlCommand updateCmd = new SqlCommand(updateSql, con);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    updateCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                    updateCmd.Parameters.AddWithValue("@Year", year);
                    updateCmd.Parameters.AddWithValue("@Month", month);
                    updateCmd.Parameters.AddWithValue("@Q1", question1);
                    updateCmd.Parameters.AddWithValue("@Q2", question2);
                    updateCmd.Parameters.AddWithValue("@Q3", question3);
                    updateCmd.Parameters.AddWithValue("@Q4", question4);
                    updateCmd.Parameters.AddWithValue("@Q5", question5);
                    updateCmd.Parameters.AddWithValue("@Q6", question6);
                    updateCmd.Parameters.AddWithValue("@Q7", question7);
                    updateCmd.Parameters.AddWithValue("@Q8", question8);
                    updateCmd.Parameters.AddWithValue("@Q9", question9);
                    updateCmd.Parameters.AddWithValue("@Q10", question10);
                    updateCmd.Parameters.AddWithValue("@Q11", question11);
                    updateCmd.Parameters.AddWithValue("@Q12", question12);
                    updateCmd.Parameters.AddWithValue("@Q13", question13);

                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    string insertSql = @"
        INSERT INTO WorkshopDetails 
            (UserId, TASSCode, [Year], [Month],
             Question1, Question2, Question3, Question4, Question5, Question6, Question7, Question8, Question9, Question10, Question11, Question12, Question13)
        VALUES 
            (@UserId, @TASSCode, @Year, @Month,
             @Q1, @Q2, @Q3, @Q4, @Q5, @Q6, @Q7, @Q8, @Q9, @Q10, @Q11, @Q12, @Q13)";

                    SqlCommand insertCmd = new SqlCommand(insertSql, con);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@TASSCode", tassCode);
                    insertCmd.Parameters.AddWithValue("@Year", year);
                    insertCmd.Parameters.AddWithValue("@Month", month);
                    insertCmd.Parameters.AddWithValue("@Q1", question1);
                    insertCmd.Parameters.AddWithValue("@Q2", question2);
                    insertCmd.Parameters.AddWithValue("@Q3", question3);
                    insertCmd.Parameters.AddWithValue("@Q4", question4);
                    insertCmd.Parameters.AddWithValue("@Q5", question5);
                    insertCmd.Parameters.AddWithValue("@Q6", question6);
                    insertCmd.Parameters.AddWithValue("@Q7", question7);
                    insertCmd.Parameters.AddWithValue("@Q8", question8);
                    insertCmd.Parameters.AddWithValue("@Q9", question9);
                    insertCmd.Parameters.AddWithValue("@Q10", question10);
                    insertCmd.Parameters.AddWithValue("@Q11", question11);
                    insertCmd.Parameters.AddWithValue("@Q12", question12);
                    insertCmd.Parameters.AddWithValue("@Q13", question13);

                    insertCmd.ExecuteNonQuery();
                }

                con.Close();
            }

            TempData["Success"] = "Record saved successfully.";
            return RedirectToAction("WorkshopDetails");
        }

        [HttpGet]
        [DeleteFileAttribute] //Action Filter, it will auto delete the file after download, 

        public ActionResult DataDumpDownload(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/Reports"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        public ActionResult Update_Unlock_Target_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Unlock_Target_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@fromwhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@month", SqlDbType.NVarChar))
                  .Value = month.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@notifi_id", SqlDbType.NVarChar))
                  .Value = Noti_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = flag.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();
            cmdObj.Parameters
              .Add(new SqlParameter("@Un_id", SqlDbType.NVarChar))
              .Value = Unid.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult Update_Unlock_Actual_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Unlock_Actual_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@fromwhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@month", SqlDbType.NVarChar))
                  .Value = month.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@notifi_id", SqlDbType.NVarChar))
                  .Value = Noti_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = flag.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();
            cmdObj.Parameters
              .Add(new SqlParameter("@Un_id", SqlDbType.NVarChar))
              .Value = Unid.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult Insert_Unlock_Target_Notifi(string SPM_SSM, string Dealer_code, string Division_id, string LOB, string remark, string month)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Unlock_Target_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@SPM_SSM", SqlDbType.NVarChar))
                .Value = SPM_SSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = LOB.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = Division_id.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = "UnlockTarget";
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["Type"].ToString().Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark;

            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }

        }

        public ActionResult Insert_Unlock_Actual_Notifi(string TSM_CSM, string Dealer_code, string Division_id, string LOB, string remark, string month)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Unlock_Actual_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@TSM_CSM", SqlDbType.NVarChar))
                .Value = TSM_CSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = LOB.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = Division_id.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = "UnlockActual";
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["Type"].ToString().Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark;

            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }

        }

        public ActionResult Unlock_Target_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, FormCollection coll)
        {

            return View();
        }

        public ActionResult Unlock_Actual_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, FormCollection coll)
        {

            return View();
        }

        public ActionResult Dashboard(string dash_type_Tml)
        {
            //var client = new RestClient("https://infoviz.cv.tatamotors/trusted");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("postman-token", "d7f81c00-9f2a-b346-14ce-e9bcd57fdba8");
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("application/x-www-form-urlencoded", "username=PSHANBHAG&target_site=CPSCTMLSite", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);


            string username = Session["Uid"].ToString();
            string target_site = "CPSCTMLSite";
            string content = ($"username={username}&target_site={target_site}");
            byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");

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
            Session["dash_type_tml"] = dash_type_Tml;

            return RedirectToAction("TDashboard", "Login");

        }

        //public ActionResult Dashboard(string dash_type_Tml)
        //{
        //    //string username = Session["Uid"].ToString();

        //    //byte[] data = Encoding.ASCII.GetBytes($"username={username}");

        //    //WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //    //request.Method = "POST";
        //    //request.ContentType = "application/x-www-form-urlencoded";
        //    //request.ContentLength = data.Length;
        //    var user = Session["Uid"].ToString(); //UserInformation.GetAuthenticatedUsername();
        //    var request = (HttpWebRequest)WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
        //    var encoding = new UTF8Encoding();
        //    var postData = user;
        //    postData += "&target_site=<CPSCTMLSite>";
        //    byte[] data = encoding.GetBytes(postData);
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
        //    Session["dash_type_tml"] = dash_type_Tml;
        //    return RedirectToAction("TDashboard", "Login");

        //}
        public ActionResult AMWiseKPI()
        {

            return RedirectToAction("Index", "KPI");
        }

    public ActionResult DTToExcel(string Proc, string flag, string filename)
    {
            if (flag == "DealerList")
            {
                DataTable dt = new DataTable("Report");
                string CS1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {

                    //var cmd = new SqlCommand(_sql, cn);
                    var daCampus = new SqlDataAdapter(_sql2, cn);
                    daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daCampus.SelectCommand.Parameters.AddWithValue("@Uid", Session["Uid"].ToString());
                    //daCampus.SelectCommand.Parameters.AddWithValue("@ForActual_Target", flag);
                    daCampus.Fill(dt);

                    //Remove Unwanted columns
                    List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID" };
                    for (int i = dt.Columns.Count - 1; i >= 0; i--)
                    {
                        DataColumn dc = dt.Columns[i];
                        if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                        {
                            dt.Columns.Remove(dc);
                        }
                    }
                }

                //DataSet data = JsonConvert.DeserializeObject<DataSet>(dt);
                //your datatable
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);

                    string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
                    filename = path + "\\" + filename;
                    wb.SaveAs(filename);

                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else if (flag == "DataDump")
            {

                DataTable dt = new DataTable("Report");
                string CS1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {

                    //var cmd = new SqlCommand(_sql, cn);
                    var daCampus = new SqlDataAdapter(_sql2, cn);
                    daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daCampus.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
                    //daCampus.SelectCommand.Parameters.AddWithValue("@ForActual_Target", flag);
                    daCampus.Fill(dt);

                    //Remove Unwanted columns
                    List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID" };
                    for (int i = dt.Columns.Count - 1; i >= 0; i--)
                    {
                        DataColumn dc = dt.Columns[i];
                        if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                        {
                            dt.Columns.Remove(dc);
                        }
                    }
                }

                //DataSet data = JsonConvert.DeserializeObject<DataSet>(dt);
                //your datatable
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);

                    string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
                    filename = path + "\\" + filename;
                    wb.SaveAs(filename);

                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {

                DataTable dt = new DataTable("Report");
                string CS1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {

                    //var cmd = new SqlCommand(_sql, cn);
                    var daCampus = new SqlDataAdapter(_sql2, cn);
                    daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daCampus.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
                    daCampus.SelectCommand.Parameters.AddWithValue("@ForActual_Target", flag);
                    daCampus.Fill(dt);

                    //Remove Unwanted columns
                    List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID" };
                    for (int i = dt.Columns.Count - 1; i >= 0; i--)
                    {
                        DataColumn dc = dt.Columns[i];
                        if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                        {
                            dt.Columns.Remove(dc);
                        }
                    }
                }

                //DataSet data = JsonConvert.DeserializeObject<DataSet>(dt);
                //your datatable
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);

                    string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
                    filename = path + "\\" + filename;
                    wb.SaveAs(filename);

                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
    }

    [HttpGet]
    [DeleteFileAttribute] //Action Filter, it will auto delete the file after download, 
                          
    public ActionResult Download(string file)
    {
        //get the temp folder and file path in server
        string fullPath = Path.Combine(Server.MapPath("~/Reports"), file);

        //return the file for download, this is an Excel 
        //so I set the file content type to "application/vnd.ms-excel"
        return File(fullPath, "application/vnd.ms-excel", file);
    }
}
public class DeleteFileAttribute : ActionFilterAttribute
{
    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
        filterContext.HttpContext.Response.Flush();

        //convert the current filter context to file and get the file path
        string filePath = (filterContext.Result as FilePathResult).FileName;

        //delete the file after download
        System.IO.File.Delete(filePath);
    }
}
}