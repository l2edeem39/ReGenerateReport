using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Globalization;
using ReGenerateReport.Api.Helper;
using ReGenerateReport.Api.Constant;
using Microsoft.EntityFrameworkCore;
using ReGenerateReport.Api.LogApiAnalytic;

namespace ReGenerateReport.Api.Controllers
{
    //[Authorize]
    [ApiController]
    public class ReGenerateReportController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Service.IUserService _userRepository;

        public ReGenerateReportController(IConfiguration config, IHttpContextAccessor httpContextAccessor, Service.IUserService IUserService)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = IUserService;
        }

        [Route("api/ReGenerateReport")]
        [HttpPost]
        public async Task<IActionResult> ReGenerateReport([FromBody] ReportRequestParameter Parameter)
        {
            ReportResponse Result = new ReportResponse();

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Result.Status = "Unauthorized";
                Result.StatusCode = "401";
                Result.ContentType = "error";
                return StatusCode(401, Result);
            }
            string authHeader = Request.Headers["Authorization"];
            byte[] authBytes = Convert.FromBase64String(authHeader.Replace("Authorization", "").Substring("Basic ".Length));
            string authString = Encoding.UTF8.GetString(authBytes);
            string[] authArray = authString.Split(':');
            string username = authArray[0];
            string password = authArray[1];

            if (!_userRepository.ValidateCredentials(username, password))
            {
                Result.Status = "Unauthorized";
                Result.StatusCode = "401";
                Result.ContentType = "error";
                return StatusCode(401, Result);
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var client = new HttpClient())
                    {
                        string Endpoint = Parameter.strUrl;
                        ReportRequest RequestData = new ReportRequest();
                        RequestData.json.strJson = Parameter.strJson.ToString();
                        string JsonRequest = RequestData.json.strJson;// JsonConvert.SerializeObject(RequestData);

                        if (Parameter.strMethodName.ToUpper() == "POST")
                        {
                            StringContent content = new StringContent(JsonRequest, Encoding.UTF8, "application/json");                     //Call API
                            using (HttpResponseMessage HttpResponse = client.PostAsync(Endpoint, content).Result)
                            {
                                if (HttpResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    if (HttpResponse.Content.Headers.ContentType.MediaType.ToString() == "application/pdf")
                                    {
                                        //return File(await HttpResponse.Content.ReadAsByteArrayAsync(), "application/pdf", "");
                                        Result.Status = HttpResponse.ReasonPhrase;
                                        Result.StatusCode = ((int)HttpResponse.StatusCode).ToString();
                                        Result.ContentFile = await HttpResponse.Content.ReadAsByteArrayAsync();
                                        Result.ContentType = HttpResponse.Content.Headers.ContentType.MediaType.ToString();
                                    }
                                    else
                                    {
                                        Result.Status = HttpResponse.ReasonPhrase;
                                        Result.StatusCode = ((int)HttpResponse.StatusCode).ToString();
                                        Result.Json = Json(await HttpResponse.Content.ReadAsStringAsync());
                                        Result.ContentType = HttpResponse.Content.Headers.ContentType.MediaType.ToString();
                                        //return Json(await HttpResponse.Content.ReadAsStringAsync());
                                    }
                                }
                                else
                                {
                                    Result.Status = HttpResponse.ReasonPhrase;
                                    Result.StatusCode = ((int)HttpResponse.StatusCode).ToString();
                                }
                                return StatusCode((int)HttpResponse.StatusCode, Result);
                            }
                        }
                        else if (true)
                        {
                            using (HttpResponseMessage HttpResponse = client.GetAsync(Endpoint).Result)
                            {
                                if (HttpResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    if (HttpResponse.Content.Headers.ContentType.MediaType.ToString() == "application/pdf")
                                    {
                                        return File(await HttpResponse.Content.ReadAsByteArrayAsync(), "application/pdf", "");
                                    }
                                    else
                                    {
                                        return Json(await HttpResponse.Content.ReadAsStringAsync());
                                    }
                                }
                                else
                                {
                                    Result.Status = HttpResponse.ReasonPhrase;
                                    Result.StatusCode = ((int)HttpResponse.StatusCode).ToString();
                                    return StatusCode((int)HttpResponse.StatusCode, Result);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //return File(Result.ContentFile.ToArray(), "application/pdf", "");
                Result.Status = ex.Message;
                Result.StatusCode = "500";
                Result.ContentType = "error";
                return StatusCode(500, Result);
            }
        }


        [Route("api/UploadToAlfresco")]
        [HttpPost]
        public async Task<IActionResult> UploadToAlfresco([FromBody] UploadAlfrescoRequestParameter Parameter)
        {
            UploadAlfrescoResponse Result = new UploadAlfrescoResponse();

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Result.message = "Unauthorized";
                Result.statusCode = "401";
                return StatusCode(401, Result);
            }
            string authHeader = Request.Headers["Authorization"];
            byte[] authBytes = Convert.FromBase64String(authHeader.Replace("Authorization", "").Substring("Basic ".Length));
            string authString = Encoding.UTF8.GetString(authBytes);
            string[] authArray = authString.Split(':');
            string username = authArray[0];
            string password = authArray[1];

            if (!_userRepository.ValidateCredentials(username, password))
            {
                Result.message = "Unauthorized";
                Result.statusCode = "401";
                return StatusCode(401, Result);
            }
            try
            {
                //Parameter.policyNo = "23181/POL/000013-580";
                //Parameter.templateId = "A010";

                string _policyNo = Parameter.policyNo.Trim();
                string pol_yr = "", pol_br = "", pol_no = "", pol_pre = "", type = "";
                //string return_message = "";
                //int return_statusCode = 200;
                if (_policyNo.Length == 20)
                {
                    pol_yr = _policyNo.Substring(0, 2);
                    pol_br = _policyNo.Substring(2, 3);
                    pol_no = _policyNo.Substring(10, 6);
                    pol_pre = _policyNo.Substring(17, 3);
                    type = "NonMotor";
                }
                else
                {
                    _policyNo = _policyNo.Replace("-", "");
                    if (_policyNo.Length == 12)
                    {
                        type = "CMI";
                        pol_pre = "";
                        pol_yr = _policyNo.Substring(0, 2);
                        pol_br = _policyNo.Substring(2, 3);
                        pol_no = _policyNo.Substring(5, 7);
                    }
                    else
                    {
                        type = "VMI";
                        pol_pre = "";
                        pol_yr = _policyNo.Substring(0, 2);
                        pol_br = _policyNo.Substring(2, 3);
                        pol_no = _policyNo.Substring(9, 6);

                    }




                    //if (_data2[0].Length == 6)
                    //{
                    //    type = "VMI";
                    //    pol_pre = "";
                    //    if (_policyNo.Length == 11)
                    //    {
                    //        pol_yr = _policyNo.Substring(0, 2);
                    //        pol_br = _policyNo.Substring(2, 3);
                    //        pol_no = _policyNo.Substring(5, 6);
                    //    }
                    //    else
                    //    {
                    //        //lenght 15
                    //        pol_yr = _policyNo.Substring(0, 2);
                    //        pol_br = _policyNo.Substring(2, 3);
                    //        pol_no = _policyNo.Substring(9, 6);
                    //    }

                    //}
                    //else if (_data2[0].Length == 7)
                    //{
                    //    type = "CMI";
                    //    pol_pre = "";
                    //    if (_policyNo.Length == 12)
                    //    {
                    //        pol_yr = _policyNo.Substring(0, 2);
                    //        pol_br = _policyNo.Substring(2, 3);
                    //        pol_no = _policyNo.Substring(5, 7);
                    //    }
                    //    else
                    //    {
                    //        //lenght 15
                    //        pol_yr = _policyNo.Substring(0, 2);
                    //        pol_br = _policyNo.Substring(2, 3);
                    //        pol_no = _policyNo.Substring(9, 7);
                    //    }

                    //}
                }

                string ResultData = "";
                if (type == "NonMotor")
                {
                    ResultData = UploadPolicy_NonMotor(pol_yr, pol_br, pol_no, pol_pre, Parameter.templateId);
                }
                else if (type == "VMI")
                {
                    ResultData = UploadPolicy_VMI(pol_yr, pol_br, pol_no, Parameter.templateId);
                }
                else if (type == "CMI")
                {
                    ResultData = UploadPolicy_CMI(pol_yr, pol_br, pol_no, Parameter.templateId);
                }
                else
                {
                    ResultData = "404##Data Not Found";

                }
                string[] _ResultData = ResultData.Split("##");
                Result.message = _ResultData[1].ToString();
                Result.statusCode = _ResultData[0].ToString();
                return StatusCode(Int32.Parse(_ResultData[0]), Result);
            }
            catch (Exception ex)
            {
                Result.message = ex.Message;
                Result.statusCode = "500";
                return StatusCode(500, Result);
            }

        }

        private string UploadPolicy_NonMotor(string pol_yr, string pol_br, string pol_no, string pol_pre, string templateId)
        {
            var sequence = 0;
            int _statusCode = 404;
            string _message = "Not Found";
            bool flagconnfire = false;
            SqlConnection connfire = null;
            SqlTransaction tranfire = null;

            if (_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim() != "")  //ConfigurationManager.ConnectionStrings["nonmotorfirewebdbConstr"].ToString().Trim()
            {
                flagconnfire = true;
            }


            List<SpGetDataPolicyNonMotor> models = new List<SpGetDataPolicyNonMotor>();
            List<SpGetDataPolicyNonMotor> modelsfire = new List<SpGetDataPolicyNonMotor>();
            List<LogModel> dataLog = new List<LogModel>();

            List<SpGetDataPolicyNonMotor> modelsAll = new List<SpGetDataPolicyNonMotor>();

            Guid logId = Guid.NewGuid();
            DateTime dateLog = DateTime.Now;

            //connect database policy
            //Console.WriteLine("Start job schedule...");
            #region call log start job schedule 
            HelperLogFile.WriteLogDB(sequence++, "Start AmitySubmitDocumentPolicyNonMotor Job Schedule");
            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, "Start AmitySubmitDocumentPolicyNonMotor Job Schedule");
            #endregion

            //Console.WriteLine("Connecting database...");
            var conn = new SqlConnection(_config["ConnectionString:nonmotordbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotordbConstr"].ToString());
            //conn.Open();
            //SqlTransaction tran = conn.BeginTransaction();

            if (flagconnfire)
            {
                connfire = new SqlConnection(_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotorfirewebdbConstr"].ToString());
                //connfire.Open();
                tranfire = connfire.BeginTransaction();
            }

            //connect database LOG
            var logConn = new SqlConnection(_config["ConnectionString:logdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["logdbConstr"].ToString());
            logConn.Open();


            //Console.WriteLine("Database connected...");


            try
            {
                HelperLogFile.WriteLogDB(sequence++, $"Get list policy start : {DateTime.Now.ToString()}");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy start : {DateTime.Now.ToString()}");
                //Must call store get data
                //Console.WriteLine("Get data from database...");
                HelperLogFile.WriteLogDB(sequence++, $"Range date Start : ");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Range date Start : "); //{CommonConfigs.StartDate}, End : {CommonConfigs.EndDate}");

                using (SqlConnection connection = conn)
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpGetDataPolicyByPolicyNonMotor", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pol_yr", pol_yr);
                        command.Parameters.AddWithValue("@pol_br", pol_br);
                        command.Parameters.AddWithValue("@pol_no", pol_no);
                        command.Parameters.AddWithValue("@pol_pre", pol_pre);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);
                            var dt = dataSet.Tables[0];

                            foreach (DataRow dr in dt.Rows)
                            {
                                SpGetDataPolicyNonMotor data = new SpGetDataPolicyNonMotor();
                                data.isProtected = dr["isProtected"] != null ? (int)dr["isProtected"] : 0;
                                data.PolYear = (string)dr["PolYear"].ToString();
                                data.PolBranch = (string)dr["PolBranch"].ToString();
                                data.PolNo = (string)dr["PolNo"].ToString();
                                data.PolPre = (string)dr["PolPre"].ToString();
                                data.AppYear = (string)dr["AppYear"].ToString();
                                data.AppBranch = (string)dr["AppBranch"].ToString();
                                data.AppNo = (string)dr["AppNo"].ToString();
                                data.ApplicationNo = (string)dr["ApplicationNo"].ToString();
                                data.PolicyNo = (string)dr["PolicyNo"].ToString();
                                data.PolicyType = string.IsNullOrEmpty((string)dr["PolicyType"].ToString()) ? null : Convert.ToInt32((int)dr["PolicyType"]);
                                data.PolicyLanguage = (string)dr["PolicyLanguage"].ToString();
                                data.fname = (string)dr["fname"].ToString();
                                data.lname = (string)dr["lname"].ToString();
                                data.ident_card = (string)dr["ident_card"].ToString();
                                data.period_from = dr["period_from"] != null ? DateTime.Parse((string)dr["period_from"]) : new DateTime();
                                data.period_to = dr["period_to"] != null ? DateTime.Parse((string)dr["period_to"]) : new DateTime();
                                data.phone = (string)dr["phone"].ToString();
                                data.e_email = (string)dr["e_email"].ToString();
                                data.saleCode = (string)dr["saleCode"].ToString();
                                data.tr_date = dr["tr_date"] != null ? DateTime.Parse((string)dr["tr_date"]) : new DateTime();
                                models.Add(data);
                            }
                        }
                    }
                    connection.Close();
                }

                if (models.Count <= 0)
                {
                    using (SqlConnection connection = connfire)
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("SpGetDataPolicyByPolicyNonMotor", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@pol_yr", pol_yr);
                            command.Parameters.AddWithValue("@pol_br", pol_br);
                            command.Parameters.AddWithValue("@pol_no", pol_no);
                            command.Parameters.AddWithValue("@pol_pre", pol_pre);

                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataSet dataSet = new DataSet();
                                adapter.Fill(dataSet);
                                var dt = dataSet.Tables[0];

                                foreach (DataRow dr in dt.Rows)
                                {
                                    SpGetDataPolicyNonMotor data = new SpGetDataPolicyNonMotor();
                                    data.isProtected = dr["isProtected"] != null ? (int)dr["isProtected"] : 0;
                                    data.PolYear = (string)dr["PolYear"].ToString();
                                    data.PolBranch = (string)dr["PolBranch"].ToString();
                                    data.PolNo = (string)dr["PolNo"].ToString();
                                    data.PolPre = (string)dr["PolPre"].ToString();
                                    data.AppYear = (string)dr["AppYear"].ToString();
                                    data.AppBranch = (string)dr["AppBranch"].ToString();
                                    data.AppNo = (string)dr["AppNo"].ToString();
                                    data.ApplicationNo = (string)dr["ApplicationNo"].ToString();
                                    data.PolicyNo = (string)dr["PolicyNo"].ToString();
                                    data.PolicyType = string.IsNullOrEmpty((string)dr["PolicyType"].ToString()) ? null : Convert.ToInt32((int)dr["PolicyType"]);
                                    data.PolicyLanguage = (string)dr["PolicyLanguage"].ToString();
                                    data.fname = (string)dr["fname"].ToString();
                                    data.lname = (string)dr["lname"].ToString();
                                    data.ident_card = (string)dr["ident_card"].ToString();
                                    data.period_from = dr["period_from"] != null ? DateTime.ParseExact((string)dr["period_from"], "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.ParseExact(new DateTime().ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    data.period_to = dr["period_to"] != null ? DateTime.ParseExact((string)dr["period_to"], "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.ParseExact(new DateTime().ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    data.phone = (string)dr["phone"].ToString();
                                    data.e_email = (string)dr["e_email"].ToString();
                                    data.saleCode = (string)dr["saleCode"].ToString();
                                    data.tr_date = dr["tr_date"] != null ? DateTime.ParseExact((string)dr["tr_date"], "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.ParseExact(new DateTime().ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    models.Add(data);
                                }
                            }
                        }
                        connection.Close();
                    }
                }

                HelperLogFile.WriteLogDB(sequence++, $"Get list policy end : {DateTime.Now.ToString()}");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get data from stored Successful. Total {models.Count.ToString()} item...");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy end : {DateTime.Now.ToString()}");

                //Console.WriteLine("Begin generate document, esign document and upload to alfresco...");
                //Console.WriteLine($"Total {models.Count} item...");
                CultureInfo culture = new CultureInfo("en-US");
                int index = 0;
                int indexLog = 0;

                Parallel.ForEach(
                    models,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    model =>
                    {
                        Guid ID = Guid.NewGuid();
                        index++;
                        string templateSetYear = _config["TemplateSetYearNonMotor:SetYear_" + model.PolPre].ToString().Trim();   //ConfigurationManager.AppSettings["SetYear_" + model.PolPre].ToString();
                        int templateAddYear = _config["TemplateSetYearNonMotor:AddYear_" + model.PolPre.ToString()].ToString().Trim() == "" ? 0 : int.Parse(_config["TemplateSetYearNonMotor:AddYear_" + model.PolPre.ToString()].ToString().Trim());   //ConfigurationManager.AppSettings["AddYear_" + model.PolPre].ToString() == "" ? 0 : int.Parse(ConfigurationManager.AppSettings["AddYear_" + model.PolPre].ToString());
                        string Path = _config["Alfresco:AlfrescoPath"].ToString();
                        string Type = _config["Alfresco:AlfrescoType"].ToString();
                        string MimeType = _config["Alfresco:AlfrescoMimeType"].ToString();
                        try
                        {
                            #region call log api start
                            indexLog++;
                            HelperLogFile.WriteLogDB(sequence++, $"Start request document policyNo : {model.PolicyNo} | Index [{indexLog}/{models.Count.ToString()}]");
                            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start request document policyNo : {model.PolicyNo} | Index [{indexLog}/{models.Count.ToString()}]", model.PolicyNo);
                            #endregion
                            AlfrescoSearchResponse listFileLibrary = AlfrescoHelper.Search(model.PolicyNo, "NonMotor", logId, dateLog).Result;
                            if (listFileLibrary != null && listFileLibrary.items != null && listFileLibrary.items.Count > 0)
                            {
                                string isReplaceFile = _config["Alfresco:IsReplaceFile"].ToString();  //ConfigurationManager.AppSettings["IsReplaceFile"].ToString();
                                if (isReplaceFile == "true")
                                {
                                    string fileID = listFileLibrary.items[0].nodeRef.Replace("workspace://SpacesStore/", "");
                                    string delResult = AlfrescoHelper.Delete(fileID, logId, dateLog).Result;
                                }
                                else
                                {
                                    //Console.WriteLine($"(Found this file ) Completed {index} of {models.Count} items...");
                                    return;
                                }
                            }

                            //string templateId = _config["TemplateITextNonMotor:" + model.PolPre + ""].ToString();    //ConfigurationManager.AppSettings[model.PolPre].ToString();
                            string policyFormat = model.PolicyNo;

                            IText7ReportResponse documentResponse = IText7Helper.GenerateDocument(policyFormat, templateId, logId, dateLog).Result;
                            if (documentResponse.ContentFile != null)
                            {
                                //Esignnning
                                var response = DigitalSignHelper.InvokeESign(documentResponse.ContentFile, model, logId, dateLog).Result;
                                AlfrescoUploadResponse alfrescoResponse = new AlfrescoUploadResponse();

                                string yearPolicy = (int.Parse(templateSetYear + model.PolicyNo.Substring(0, 2)) - templateAddYear).ToString();
                                string destination = string.Format(Path, yearPolicy, model.PolPre);
                                if (response.respStatus == "success")
                                {
                                    //data upload document
                                    HelperLogFile.WriteLogDB(sequence++, $"Prepare data upload to alfresco.");
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Prepare data upload to alfresco.");

                                    byte[] contentFile = Convert.FromBase64String(response.signedPdf);

                                    //convert datetime to string
                                    string period_from = model.period_from.ToString("d", culture);
                                    string period_to = model.period_to.ToString("d", culture);
                                    AlfrescoUploadRequest alfresco = new AlfrescoUploadRequest
                                    {
                                        file = null, //not keep file to database 
                                        name = model.PolicyNo.Replace("/", "-") + ".pdf",
                                        title = "AmitySubmitDocumentPolicyNonMotor",
                                        mimetype = MimeType,
                                        type = Type,
                                        prop_vir_docnumber = model.PolicyNo,
                                        prop_vir_doctype = "NonMotor",
                                        prop_vir_docsubtype = null,
                                        prop_vir_compcode = "VIB",
                                        prop_vir_insuredname = model.fname,
                                        prop_vir_insuredlastname = model.lname,
                                        prop_vir_idcard = model.ident_card,
                                        prop_vir_birthdate = "",
                                        prop_vir_periodstart = period_from,
                                        prop_vir_periodend = period_to,
                                        prop_vir_mobile = model.phone,
                                        prop_vir_email = model.e_email,
                                        prop_vir_parentdocno = "",
                                        prop_vir_status = "Signed",
                                        destination = destination,
                                        policyNumber = model.PolicyNo,
                                    };

                                    alfresco.file = contentFile;
                                    alfrescoResponse = AlfrescoHelper.Upload(alfresco, logId, dateLog).Result;
                                    alfresco.file = null;
                                    if (alfrescoResponse.success)
                                    {
                                        InsertLogNonMotor(new LogPolicyDTONonMotor()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            PolPre = model.PolPre,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "NonMotor",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Sucessful",
                                            CreateDate = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US")),  //DateTime.Now.ToString(),
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = Convert.ToDateTime(model.tr_date.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US"))),
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        _statusCode = 200;
                                        _message = "Sucessful";
                                        //Console.WriteLine($"Completed {index} of {models.Count} items...");
                                    }
                                    else
                                    {
                                        _statusCode = 500;
                                        _message = "Alfresco Upload Fail  : " + alfrescoResponse.error;
                                        //Console.WriteLine($"ERORR  {index} of {models.Count} items...alfresco");
                                        InsertLogNonMotor(new LogPolicyDTONonMotor()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            PolPre = model.PolPre,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "NonMotor",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Fail",
                                            Message = "alfresco Upload Fail  : " + alfrescoResponse.error,
                                            CreateDate = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US")),  //DateTime.Now.ToString(),
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = Convert.ToDateTime(model.tr_date.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US"))),  //model.tr_date,
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        return;
                                    }
                                }
                                else
                                {
                                    _statusCode = 500;
                                    _message = "DigitalSign Fail : " + response.respDesc;
                                    InsertLogNonMotor(new LogPolicyDTONonMotor()
                                    {
                                        Id = ID,
                                        AppYear = model.AppYear,
                                        AppBranch = model.AppBranch,
                                        AppNo = model.AppNo,
                                        PolYear = model.PolYear,
                                        PolBranch = model.PolBranch,
                                        PolNo = model.PolNo,
                                        PolPre = model.PolPre,
                                        ApplicationNo = model.ApplicationNo,
                                        PolicyNo = model.PolicyNo,
                                        PolicyType = "NonMotor",
                                        PolicyLanguage = model.PolicyLanguage,
                                        JSReportTemplateId = templateId,
                                        AlfrescoPath = destination,
                                        Status = "Fail",
                                        Message = "DigitalSign Fail : " + response.respDesc,
                                        CreateDate = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US")), //DateTime.Now.ToString(),
                                        ThreadID = Task.CurrentId.ToString(),
                                        PolicyDate = Convert.ToDateTime(model.tr_date.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US"))),  //model.tr_date,
                                        ProcessDocNumber = $"{index} of {models.Count}"
                                    }, logConn);
                                }


                                #region call log api end
                                string resjson = JsonConvert.SerializeObject(alfrescoResponse);
                                HelperLogFile.WriteLogDB(sequence++, $"End request amity submit document policyNo : {model.PolicyNo}");
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, resjson);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"End request amity submit document policyNo : {model.PolicyNo}");
                                #endregion
                            }
                            else
                            {
                                _statusCode = 500;
                                _message = "IText7 Fail : " + documentResponse.Description;
                                //Console.WriteLine($"ERORR {index} of {models.Count} items... itext7 {documentResponse.Description} ");
                                InsertLogNonMotor(new LogPolicyDTONonMotor()
                                {
                                    Id = ID,
                                    AppYear = model.AppYear,
                                    AppBranch = model.AppBranch,
                                    AppNo = model.AppNo,
                                    PolYear = model.PolYear,
                                    PolBranch = model.PolBranch,
                                    PolNo = model.PolNo,
                                    PolPre = model.PolPre,
                                    ApplicationNo = model.ApplicationNo,
                                    PolicyNo = model.PolicyNo,
                                    PolicyType = "NonMotor",
                                    PolicyLanguage = model.PolicyLanguage,
                                    JSReportTemplateId = templateId,
                                    AlfrescoPath = "",
                                    Status = "Fail",
                                    Message = "IText7 Fail : " + documentResponse.Description,
                                    CreateDate = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US")), //DateTime.Now.ToString(),
                                    ThreadID = Task.CurrentId.ToString(),
                                    PolicyDate = Convert.ToDateTime(model.tr_date.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US"))), //model.tr_date,
                                    ProcessDocNumber = $"{index} of {models.Count}"
                                }, logConn);

                            }

                        }
                        catch (Exception ex)
                        {
                            _statusCode = 500;
                            _message = "Exception : " + ex.Message;
                            string yearPolicy = (int.Parse(templateSetYear + model.PolicyNo.Substring(0, 2)) - templateAddYear).ToString();
                            string destination = string.Format(Path, yearPolicy, model.PolPre);
                            //Console.WriteLine($"ERORR {index} of {models.Count} items... {ex.Message} "); //{ex.InnerException.Message}
                            //insert log
                            InsertLogNonMotor(new LogPolicyDTONonMotor()
                            {
                                Id = ID,
                                AppYear = model.AppYear,
                                AppBranch = model.AppBranch,
                                AppNo = model.AppNo,
                                PolYear = model.PolYear,
                                PolBranch = model.PolBranch,
                                PolNo = model.PolNo,
                                PolPre = model.PolPre,
                                ApplicationNo = model.ApplicationNo,
                                PolicyNo = model.PolicyNo,
                                PolicyType = "NonMotor",
                                PolicyLanguage = model.PolicyLanguage,
                                JSReportTemplateId = templateId,
                                AlfrescoPath = "",
                                Status = "Fail",
                                ExceptionMessage = ex.Message, //+ " " + ex.InnerException.Message,
                                CreateDate = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US")), //DateTime.Now.ToString(),
                                ThreadID = Task.CurrentId.ToString(),
                                PolicyDate = Convert.ToDateTime(model.tr_date.ToString("yyy-MM-dd HH:mm:ss", new CultureInfo("en-US"))), //model.tr_date,
                                ProcessDocNumber = $"{index} of {models.Count}"
                            }, logConn);
                        }
                    }
                );

                //Console.WriteLine("AmitySubmitDocumentPolicyNonMotor job schedule successfully...");
            }
            catch (Exception ex)
            {
                HelperLogFile.WriteLogDB(99, ex.Message);
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, ex);
                conn.Dispose();
                if (flagconnfire)
                {
                    connfire.Dispose();
                }
                logConn.Close();
                //Environment.Exit(1);
            }
            finally
            {
                conn.Dispose();
                if (flagconnfire)
                {
                    connfire.Dispose();
                }
                logConn.Close();
                //Environment.Exit(2);
            }

            return _statusCode.ToString() + "##" + _message;
        }

        private string UploadPolicy_VMI(string pol_yr, string pol_br, string pol_no, string templateId)
        {
            CultureInfo culture = new CultureInfo("en-US");

            int _statusCode = 404;
            string _message = "Not Found";
            List<SpGetDataPolicyVmi> models = new List<SpGetDataPolicyVmi>();
            List<LogPolicyDTOVmi> logs = new List<LogPolicyDTOVmi>();
            Guid logId = Guid.NewGuid();
            DateTime dateLog = DateTime.Now;

            //connect database policy
            //Console.WriteLine("Start job schedule...");
            #region call log start job schedule 
            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, "Start AmitySubmitDocumentPolicyVMI Job Schedule");
            #endregion

            //Console.WriteLine("Connecting database...");
            var conn = new SqlConnection(_config["ConnectionString:vmimotordbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotordbConstr"].ToString());
            //conn.Open();
            //SqlTransaction tran = conn.BeginTransaction();

            //connect database LOG
            var logConn = new SqlConnection(_config["ConnectionString:logdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["logdbConstr"].ToString());
            logConn.Open();


            //Console.WriteLine("Database connected...");


            try
            {

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy start : {DateTime.Now.ToString()}");
                //Must call store get data
                //Console.WriteLine("Get data from database...");

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Range date Start : "); //{CommonConfigs.StartDate}, End : {CommonConfigs.EndDate}");

                using (SqlConnection connection = conn)
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpGetDataPolicyByPolicyVMI", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pol_yr", pol_yr);
                        command.Parameters.AddWithValue("@pol_br", pol_br);
                        command.Parameters.AddWithValue("@pol_no", pol_no);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);
                            var dt = dataSet.Tables[0];

                            foreach (DataRow dr in dt.Rows)
                            {

                                var dd = Convert.ToDateTime(dr["period_to"], culture).ToString("d", new CultureInfo("en-US"));

                                SpGetDataPolicyVmi data = new SpGetDataPolicyVmi();
                                data.isProtected = dr["isProtected"] != null ? (int)dr["isProtected"] : 0;
                                data.PolYear = (string)dr["PolYear"].ToString();
                                data.PolBranch = (string)dr["PolBranch"].ToString();
                                data.PolNo = (string)dr["PolNo"].ToString();
                                data.AppYear = (string)dr["AppYear"].ToString();
                                data.AppBranch = (string)dr["AppBranch"].ToString();
                                data.AppNo = (string)dr["AppNo"].ToString();
                                data.ApplicationNo = (string)dr["ApplicationNo"].ToString();
                                data.PolicyNo = (string)dr["PolicyNo"].ToString();
                                data.PolicyType = string.IsNullOrEmpty((string)dr["PolicyType"].ToString()) ? null : (string)dr["PolicyType"];
                                data.PolicyLanguage = (string)dr["PolicyLanguage"].ToString();
                                data.fname = (string)dr["fname"].ToString();
                                data.lname = (string)dr["lname"].ToString();
                                data.ident_card = (string)dr["ident_card"].ToString();
                                data.period_from = dr["period_from"] != null ? DateTime.Parse((string)dr["period_from"]) : new DateTime();
                                data.period_to = dr["period_to"] != null ? DateTime.Parse((string)dr["period_to"]) : new DateTime();
                                data.phone = (string)dr["phone"].ToString();
                                data.e_email = (string)dr["e_email"].ToString();
                                data.agree_date = dr["agree_date"] != null ? DateTime.Parse((string)dr["agree_date"]) : new DateTime();
                                models.Add(data);
                            }
                        }
                    }
                    connection.Close();
                }

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get data from stored Successful. Total {models.Count.ToString()} item...");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy end : {DateTime.Now.ToString()}");

                //Console.WriteLine("Begin generate document, esign document and upload to alfresco...");
                //Console.WriteLine($"Total {models.Count} item...");
                
                int index = 0;
                int indexLog = 0;

                Parallel.ForEach(
                    models,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    model =>
                    {
                        Guid ID = Guid.NewGuid();
                        index++;

                        string Path = _config["Alfresco:AlfrescoPathVmi"].ToString();
                        string Type = _config["Alfresco:AlfrescoTypeVmi"].ToString();
                        string MimeType = _config["Alfresco:AlfrescoMimeTypeVmi"].ToString();
                        try
                        {
                            #region call log api start
                            indexLog++;
                            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start request document policyNo : {model.PolicyNo} | Index [{indexLog}/{models.Count.ToString()}]", model.PolicyNo);
                            #endregion
                            AlfrescoSearchResponse listFileLibrary = AlfrescoHelper.Search(model.PolicyNo, "VMI", logId, dateLog).Result;
                            if (listFileLibrary != null && listFileLibrary.items != null && listFileLibrary.items.Count > 0)
                            {
                                string isReplaceFile = _config["Alfresco:IsReplaceFile"].ToString().Trim();  //ConfigurationManager.AppSettings["IsReplaceFile"].ToString();
                                if (isReplaceFile == "true")
                                {
                                    string fileID = listFileLibrary.items[0].nodeRef.Replace("workspace://SpacesStore/", "");
                                    string delResult = AlfrescoHelper.Delete(fileID, logId, dateLog).Result;
                                }
                                else
                                {
                                    //Console.WriteLine($"(Found this file ) Completed {index} of {models.Count} items...");
                                    return;
                                }
                            }

                            //string templateId = _config["TemplateITextNonMotor:" + model.PolPre + ""].ToString();    //ConfigurationManager.AppSettings[model.PolPre].ToString();
                            string policyFormat = $"@PolicyNo='{model.PolicyNo}'";
                            string yearPolicy = (int.Parse("25" + model.PolicyNo.Substring(0, 2)) - 543).ToString();
                            string destination = $"{Path}/{yearPolicy}/Motor/VMI";

                            IText7ReportResponse documentResponse = IText7Helper.GenerateDocument(policyFormat, templateId, logId, dateLog).Result;  //templateId = CommonHelper.GetItext7TemplateConfigs(model.PolicyType, model.PolicyLanguage)
                            if (documentResponse.ContentFile != null)
                            {
                                //Esignnning
                                var response = DigitalSignHelper.InvokeESignVmi(documentResponse.ContentFile, model, logId, dateLog).Result;
                                AlfrescoUploadResponse alfrescoResponse = new AlfrescoUploadResponse();

                                if (response.respStatus == "success")
                                {
                                    //data upload document
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Prepare data upload to alfresco.");

                                    byte[] contentFile = Convert.FromBase64String(response.signedPdf);

                                    //convert datetime to string
                                    string period_from = model.period_from.ToString("d", culture);
                                    string period_to = model.period_to.ToString("d", culture);
                                    AlfrescoUploadRequest alfresco = new AlfrescoUploadRequest
                                    {
                                        file = contentFile, //not keep file to database 
                                        name = model.PolicyNo.Replace('/', '-') + ".pdf",
                                        title = "AmitySubmitDocument",
                                        mimetype = MimeType,
                                        type = Type,
                                        prop_vir_docnumber = model.PolicyNo,
                                        prop_vir_doctype = "VMI",
                                        prop_vir_docsubtype = model.PolicyType,
                                        prop_vir_compcode = "VIB",
                                        prop_vir_insuredname = model.fname,
                                        prop_vir_insuredlastname = model.lname,
                                        prop_vir_idcard = model.ident_card,
                                        prop_vir_birthdate = "",
                                        prop_vir_periodstart = model.period_from.ToString(),
                                        prop_vir_periodend = model.period_to.ToString(),
                                        prop_vir_mobile = model.phone,
                                        prop_vir_email = model.e_email,
                                        prop_vir_parentdocno = "",
                                        destination = destination,
                                        prop_vir_status = "Signed",
                                        policyNumber = model.PolicyNo
                                    };

                                    alfrescoResponse = AlfrescoHelper.Upload(alfresco, logId, dateLog).Result;
                                    alfresco.file = null;
                                    if (alfrescoResponse.success)
                                    {
                                        InsertLogVmi(new LogPolicyDTOVmi()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "VMI",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Sucessful",
                                            CreateDate = DateTime.Now,
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = model.agree_date,
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        _statusCode = 200;
                                        _message = "Sucessful";
                                        //Console.WriteLine($"Completed {index} of {models.Count} items...");
                                    }
                                    else
                                    {
                                        _statusCode = 500;
                                        _message = "Alfresco Upload Fail  : " + alfrescoResponse.error;
                                        //Console.WriteLine($"ERORR  {index} of {models.Count} items...alfresco");
                                        InsertLogVmi(new LogPolicyDTOVmi()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "VMI",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Fail",
                                            Message = "alfresco Upload Fail  : " + alfrescoResponse.error,
                                            CreateDate = DateTime.Now,
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = model.agree_date,
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        return;
                                    }
                                }
                                else
                                {
                                    _statusCode = 500;
                                    _message = "DigitalSign Fail : " + response.respDesc;
                                    InsertLogVmi(new LogPolicyDTOVmi()
                                    {
                                        Id = ID,
                                        AppYear = model.AppYear,
                                        AppBranch = model.AppBranch,
                                        AppNo = model.AppNo,
                                        PolYear = model.PolYear,
                                        PolBranch = model.PolBranch,
                                        PolNo = model.PolNo,
                                        ApplicationNo = model.ApplicationNo,
                                        PolicyNo = model.PolicyNo,
                                        PolicyType = "VMI",
                                        PolicyLanguage = model.PolicyLanguage,
                                        JSReportTemplateId = templateId,
                                        AlfrescoPath = destination,
                                        Status = "Fail",
                                        Message = "DigitalSign Fail : " + response.respDesc,
                                        CreateDate = DateTime.Now,
                                        ThreadID = Task.CurrentId.ToString(),
                                        PolicyDate = model.agree_date,
                                        ProcessDocNumber = $"{index} of {models.Count}"
                                    }, logConn);
                                }


                                #region call log api end
                                string resjson = JsonConvert.SerializeObject(alfrescoResponse);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, resjson);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"End request amity submit document policyNo : {model.PolicyNo}");
                                #endregion
                            }
                            else
                            {
                                _statusCode = 500;
                                _message = "IText7 Fail : " + documentResponse.Description;
                                //Console.WriteLine($"ERORR {index} of {models.Count} items... itext7 {documentResponse.Description} ");
                                InsertLogVmi(new LogPolicyDTOVmi()
                                {
                                    Id = ID,
                                    AppYear = model.AppYear,
                                    AppBranch = model.AppBranch,
                                    AppNo = model.AppNo,
                                    PolYear = model.PolYear,
                                    PolBranch = model.PolBranch,
                                    PolNo = model.PolNo,
                                    ApplicationNo = model.ApplicationNo,
                                    PolicyNo = model.PolicyNo,
                                    PolicyType = "VMI",
                                    PolicyLanguage = model.PolicyLanguage,
                                    JSReportTemplateId = templateId,
                                    AlfrescoPath = "",
                                    Status = "Fail",
                                    Message = "IText7 Fail : " + documentResponse.Description,
                                    CreateDate = DateTime.Now,
                                    ThreadID = Task.CurrentId.ToString(),
                                    PolicyDate = model.agree_date,
                                    ProcessDocNumber = $"{index} of {models.Count}"
                                }, logConn);

                            }

                        }
                        catch (Exception ex)
                        {
                            _statusCode = 500;
                            _message = "Exception : " + ex.Message;
                            string yearPolicy = (int.Parse("25" + model.PolicyNo.Substring(0, 2)) - 543).ToString();
                            string destination = string.Format(Path, yearPolicy);
                            //Console.WriteLine($"ERORR {index} of {models.Count} items... {ex.Message}");//{ex.InnerException.Message}
                            //insert log
                            InsertLogVmi(new LogPolicyDTOVmi()
                            {
                                Id = ID,
                                AppYear = model.AppYear,
                                AppBranch = model.AppBranch,
                                AppNo = model.AppNo,
                                PolYear = model.PolYear,
                                PolBranch = model.PolBranch,
                                PolNo = model.PolNo,
                                ApplicationNo = model.ApplicationNo,
                                PolicyNo = model.PolicyNo,
                                PolicyType = "VMI",
                                PolicyLanguage = model.PolicyLanguage,
                                JSReportTemplateId = templateId,
                                AlfrescoPath = "",
                                Status = "Fail",
                                ExceptionMessage = ex.Message, //+ " " + ex.InnerException.Message,
                                CreateDate = DateTime.Now,
                                ThreadID = Task.CurrentId.ToString(),
                                PolicyDate = model.agree_date,
                                ProcessDocNumber = $"{index} of {models.Count}"
                            }, logConn);
                        }
                    }
                );

                //Console.WriteLine("AmitySubmitDocumentPolicyNonMotor job schedule successfully...");
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, ex);
                conn.Dispose();
                logConn.Close();
                //Environment.Exit(1);
            }
            finally
            {
                conn.Dispose();
                logConn.Close();
                //Environment.Exit(2);
            }

            return _statusCode.ToString() + "##" + _message;
        }

        private string UploadPolicy_CMI(string pol_yr, string pol_br, string pol_no, string templateId)
        {
            int _statusCode = 404;
            string _message = "Not Found";
            List<SpGetDataPolicyCmi> models = new List<SpGetDataPolicyCmi>();
            List<LogModel> dataLog = new List<LogModel>();

            Guid logId = Guid.NewGuid();
            DateTime dateLog = DateTime.Now;

            //connect database policy
            //Console.WriteLine("Start job schedule...");
            #region call log start job schedule 
            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, "Start AmitySubmitDocumentPolicyCmi Job Schedule");
            #endregion

            //Console.WriteLine("Connecting database...");
            var conn = new SqlConnection(_config["ConnectionString:cmimotordbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotordbConstr"].ToString());
            //conn.Open();
            //SqlTransaction tran = conn.BeginTransaction();

            //connect database LOG
            var logConn = new SqlConnection(_config["ConnectionString:logdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["logdbConstr"].ToString());
            logConn.Open();

            //Console.WriteLine("Database connected...");

            try
            {

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy start : {DateTime.Now.ToString()}");
                //Must call store get data
                //Console.WriteLine("Get data from database...");

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Range date Start : "); //{CommonConfigs.StartDate}, End : {CommonConfigs.EndDate}");

                using (SqlConnection connection = conn)
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpGetDataPolicyByPolicyCMI", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pol_yr", pol_yr);
                        command.Parameters.AddWithValue("@pol_br", pol_br);
                        command.Parameters.AddWithValue("@pol_no", pol_no);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);
                            var dt = dataSet.Tables[0];

                            foreach (DataRow dr in dt.Rows)
                            {
                                SpGetDataPolicyCmi data = new SpGetDataPolicyCmi();
                                data.isProtected = dr["isProtected"] != null ? (int)dr["isProtected"] : 0;
                                data.PolYear = (string)dr["PolYear"].ToString();
                                data.PolBranch = (string)dr["PolBranch"].ToString();
                                data.PolNo = (string)dr["PolNo"].ToString();
                                data.AppYear = (string)dr["AppYear"].ToString();
                                data.AppBranch = (string)dr["AppBranch"].ToString();
                                data.AppNo = (string)dr["AppNo"].ToString();
                                data.ApplicationNo = (string)dr["ApplicationNo"].ToString();
                                data.PolicyNo = (string)dr["PolicyNo"].ToString();
                                data.PolicyType = string.IsNullOrEmpty((string)dr["PolicyType"].ToString()) ? null : (int)dr["PolicyType"];
                                data.PolicyLanguage = (string)dr["PolicyLanguage"].ToString();
                                data.fname = (string)dr["fname"].ToString();
                                data.lname = (string)dr["lname"].ToString();
                                data.ident_card = (string)dr["ident_card"].ToString();
                                data.period_from = dr["period_from"] != null ? DateTime.Parse((string)dr["period_from"]) : new DateTime();
                                data.period_to = dr["period_to"] != null ? DateTime.Parse((string)dr["period_to"]) : new DateTime();
                                data.phone = (string)dr["phone"].ToString();
                                data.e_email = (string)dr["e_email"].ToString();
                                data.saleCode = (string)dr["saleCode"].ToString();
                                data.tr_date = dr["tr_date"] != null ? DateTime.Parse((string)dr["tr_date"]) : new DateTime();
                                models.Add(data);
                            }
                        }
                    }
                    connection.Close();
                }

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get data from stored Successful. Total {models.Count.ToString()} item...");
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy end : {DateTime.Now.ToString()}");

                //Console.WriteLine("Begin generate document, esign document and upload to alfresco...");
                //Console.WriteLine($"Total {models.Count} item...");
                CultureInfo culture = new CultureInfo("en-US");
                int index = 0;
                int indexLog = 0;

                Parallel.ForEach(
                    models,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    model =>
                    {
                        Guid ID = Guid.NewGuid();
                        index++;
                        string Path = _config["Alfresco:AlfrescoPathCmi"].ToString();
                        string Type = _config["Alfresco:AlfrescoTypeCmi"].ToString();
                        string MimeType = _config["Alfresco:AlfrescoMimeTypeCmi"].ToString();
                        try
                        {
                            #region call log api start
                            indexLog++;
                            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start request document policyNo : {model.PolicyNo} | Index [{indexLog}/{models.Count.ToString()}]", model.PolicyNo);
                            #endregion
                            AlfrescoSearchResponse listFileLibrary = AlfrescoHelper.Search(model.PolicyNo, "CMI", logId, dateLog).Result;
                            if (listFileLibrary != null && listFileLibrary.items != null && listFileLibrary.items.Count > 0)
                            {
                                string isReplaceFile = _config["Alfresco:IsReplaceFile"].ToString().Trim();  //ConfigurationManager.AppSettings["IsReplaceFile"].ToString();
                                if (isReplaceFile == "true")
                                {
                                    string fileID = listFileLibrary.items[0].nodeRef.Replace("workspace://SpacesStore/", "");
                                    string delResult = AlfrescoHelper.Delete(fileID, logId, dateLog).Result;
                                }
                                else
                                {
                                    //Console.WriteLine($"(Found this file ) Completed {index} of {models.Count} items...");
                                    return;
                                }
                            }

                            string[] policyArray = model.PolicyNo.Split('-');
                            string policyFormat = $"@PolicyNo='{policyArray[0]}/กธ/{policyArray[1]}'";

                            IText7ReportResponse documentResponse = IText7Helper.GenerateDocument(policyFormat, templateId, logId, dateLog).Result;
                            if (documentResponse.ContentFile != null)
                            {
                                //Esignnning
                                var response = DigitalSignHelper.InvokeESignCmi(documentResponse.ContentFile, model, logId, dateLog).Result;
                                AlfrescoUploadResponse alfrescoResponse = new AlfrescoUploadResponse();

                                string yearPolicy = (int.Parse("25" + model.PolicyNo.Substring(0, 2)) - 543).ToString();
                                string destination = string.Format(Path, yearPolicy);
                                if (response.respStatus == "success")
                                {
                                    //data upload document
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Prepare data upload to alfresco.");

                                    byte[] contentFile = Convert.FromBase64String(response.signedPdf);

                                    //convert datetime to string
                                    string period_from = model.period_from.ToString("d", culture);
                                    string period_to = model.period_to.ToString("d", culture);
                                    AlfrescoUploadRequest alfresco = new AlfrescoUploadRequest
                                    {
                                        file = null, //not keep file to database 
                                        name = model.PolicyNo.Replace("/", "-") + ".pdf",
                                        title = "AmitySubmitDocumentPolicyCMI",
                                        mimetype = MimeType,
                                        type = Type,
                                        prop_vir_docnumber = model.PolicyNo,
                                        prop_vir_doctype = "CMI",
                                        prop_vir_docsubtype = null,
                                        prop_vir_compcode = "VIB",
                                        prop_vir_insuredname = model.fname,
                                        prop_vir_insuredlastname = model.lname,
                                        prop_vir_idcard = model.ident_card,
                                        prop_vir_birthdate = "",
                                        prop_vir_periodstart = period_from,
                                        prop_vir_periodend = period_to,
                                        prop_vir_mobile = model.phone,
                                        prop_vir_email = model.e_email,
                                        prop_vir_parentdocno = "",
                                        prop_vir_status = "Signed",
                                        destination = destination,
                                        policyNumber = model.PolicyNo,
                                    };

                                    alfresco.file = contentFile;
                                    alfrescoResponse = AlfrescoHelper.Upload(alfresco, logId, dateLog).Result;
                                    alfresco.file = null;
                                    if (alfrescoResponse.success)
                                    {
                                        InsertLogCmi(new LogPolicyDTOCmi()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "CMI",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Sucessful",
                                            CreateDate = DateTime.Now,
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = model.tr_date,
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        _statusCode = 200;
                                        _message = "Sucessful";
                                        //Console.WriteLine($"Completed {index} of {models.Count} items...");
                                    }
                                    else
                                    {
                                        _statusCode = 500;
                                        _message = "Alfresco Upload Fail  : " + alfrescoResponse.error;
                                        //Console.WriteLine($"ERORR  {index} of {models.Count} items...alfresco");
                                        InsertLogCmi(new LogPolicyDTOCmi()
                                        {
                                            Id = ID,
                                            AppYear = model.AppYear,
                                            AppBranch = model.AppBranch,
                                            AppNo = model.AppNo,
                                            PolYear = model.PolYear,
                                            PolBranch = model.PolBranch,
                                            PolNo = model.PolNo,
                                            ApplicationNo = model.ApplicationNo,
                                            PolicyNo = model.PolicyNo,
                                            PolicyType = "CMI",
                                            PolicyLanguage = model.PolicyLanguage,
                                            JSReportTemplateId = templateId,
                                            AlfrescoPath = destination,
                                            AlfrescoParameter = JsonConvert.SerializeObject(alfresco),
                                            Status = "Fail",
                                            Message = "alfresco Upload Fail  : " + alfrescoResponse.error,
                                            CreateDate = DateTime.Now,
                                            ThreadID = Task.CurrentId.ToString(),
                                            PolicyDate = model.tr_date,
                                            ProcessDocNumber = $"{index} of {models.Count}",
                                        }, logConn);
                                        return;
                                    }
                                }
                                else
                                {
                                    _statusCode = 500;
                                    _message = "DigitalSign Fail : " + response.respDesc;
                                    InsertLogCmi(new LogPolicyDTOCmi()
                                    {
                                        Id = ID,
                                        AppYear = model.AppYear,
                                        AppBranch = model.AppBranch,
                                        AppNo = model.AppNo,
                                        PolYear = model.PolYear,
                                        PolBranch = model.PolBranch,
                                        PolNo = model.PolNo,
                                        ApplicationNo = model.ApplicationNo,
                                        PolicyNo = model.PolicyNo,
                                        PolicyType = "CMI",
                                        PolicyLanguage = model.PolicyLanguage,
                                        JSReportTemplateId = templateId,
                                        AlfrescoPath = destination,
                                        Status = "Fail",
                                        Message = "DigitalSign Fail : " + response.respDesc,
                                        CreateDate = DateTime.Now,
                                        ThreadID = Task.CurrentId.ToString(),
                                        PolicyDate = model.tr_date,
                                        ProcessDocNumber = $"{index} of {models.Count}"
                                    }, logConn);
                                }


                                #region call log api end
                                string resjson = JsonConvert.SerializeObject(alfrescoResponse);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, resjson);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"End request amity submit document policyNo : {model.PolicyNo}");
                                #endregion
                            }
                            else
                            {
                                _statusCode = 500;
                                _message = "IText7 Fail : " + documentResponse.Description;
                                //Console.WriteLine($"ERORR {index} of {models.Count} items... itext7 {documentResponse.Description} ");
                                InsertLogCmi(new LogPolicyDTOCmi()
                                {
                                    Id = ID,
                                    AppYear = model.AppYear,
                                    AppBranch = model.AppBranch,
                                    AppNo = model.AppNo,
                                    PolYear = model.PolYear,
                                    PolBranch = model.PolBranch,
                                    PolNo = model.PolNo,
                                    ApplicationNo = model.ApplicationNo,
                                    PolicyNo = model.PolicyNo,
                                    PolicyType = "CMI",
                                    PolicyLanguage = model.PolicyLanguage,
                                    JSReportTemplateId = templateId,
                                    AlfrescoPath = "",
                                    Status = "Fail",
                                    Message = "IText7 Fail : " + documentResponse.Description,
                                    CreateDate = DateTime.Now,
                                    ThreadID = Task.CurrentId.ToString(),
                                    PolicyDate = model.tr_date,
                                    ProcessDocNumber = $"{index} of {models.Count}"
                                }, logConn);

                            }

                        }
                        catch (Exception ex)
                        {
                            _statusCode = 500;
                            _message = "Exception : " + ex.Message;
                            string yearPolicy = (int.Parse("25" + model.PolicyNo.Substring(0, 2)) - 543).ToString();
                            string destination = string.Format(Path, yearPolicy);
                            //Console.WriteLine($"ERORR {index} of {models.Count} items... {ex.Message} "); //{ex.InnerException.Message}
                            //insert log
                            InsertLogCmi(new LogPolicyDTOCmi()
                            {
                                Id = ID,
                                AppYear = model.AppYear,
                                AppBranch = model.AppBranch,
                                AppNo = model.AppNo,
                                PolYear = model.PolYear,
                                PolBranch = model.PolBranch,
                                PolNo = model.PolNo,
                                ApplicationNo = model.ApplicationNo,
                                PolicyNo = model.PolicyNo,
                                PolicyType = "CMI",
                                PolicyLanguage = model.PolicyLanguage,
                                JSReportTemplateId = templateId,
                                AlfrescoPath = "",
                                Status = "Fail",
                                ExceptionMessage = ex.Message, //+ " " + ex.InnerException.Message,
                                CreateDate = DateTime.Now,
                                ThreadID = Task.CurrentId.ToString(),
                                PolicyDate = model.tr_date,
                                ProcessDocNumber = $"{index} of {models.Count}"
                            }, logConn);
                        }
                    }
                );

                //Console.WriteLine("AmitySubmitDocumentPolicyNonMotor job schedule successfully...");
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, ex);
                conn.Dispose();
                logConn.Close();
                //Environment.Exit(1);
            }
            finally
            {
                conn.Dispose();
                logConn.Close();
                //Environment.Exit(2);
            }

            return _statusCode.ToString() + "##" + _message;
        }

        private void InsertLogNonMotor(LogPolicyDTONonMotor item, SqlConnection logConn)
        {
            try
            {
                using (var command = new SqlCommand("SpInsertLogSubmitDocumentDev", logConn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@AppYear", item.AppYear);
                    command.Parameters.AddWithValue("@AppBranch", item.AppBranch);
                    command.Parameters.AddWithValue("@AppNo", item.AppNo);
                    command.Parameters.AddWithValue("@PolYear", item.PolYear);
                    command.Parameters.AddWithValue("@PolBranch", item.PolBranch);
                    command.Parameters.AddWithValue("@PolNo", item.PolNo);
                    command.Parameters.AddWithValue("@PolPre", item.PolPre);
                    command.Parameters.AddWithValue("@ApplicationNo", item.ApplicationNo);
                    command.Parameters.AddWithValue("@PolicyNo", item.PolicyNo);
                    command.Parameters.AddWithValue("@Message", "BankTest - " + item.Message);
                    command.Parameters.AddWithValue("@PolicyType", item.PolicyType);
                    command.Parameters.AddWithValue("@PolicyLanguage", item.PolicyLanguage);
                    command.Parameters.AddWithValue("@JSReportTemplateId", item.JSReportTemplateId);
                    command.Parameters.AddWithValue("@AlfrescoPath", item.AlfrescoPath);
                    command.Parameters.AddWithValue("@AlfrescoParameter", item.AlfrescoParameter);
                    command.Parameters.AddWithValue("@Status", item.Status);
                    command.Parameters.AddWithValue("@CreateDate", item.CreateDate);
                    command.Parameters.AddWithValue("@PolicyDate", item.PolicyDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ProcessDocNumber", item.ProcessDocNumber);
                    command.Parameters.AddWithValue("@ThreadID", item.ThreadID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var msg = ex;
            }
        }

        private void InsertLogVmi(LogPolicyDTOVmi item, SqlConnection logConn)
        {
            try
            {
                using (var command = new SqlCommand("SpInsertLogSubmitDocumentDev", logConn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@AppYear", item.AppYear);
                    command.Parameters.AddWithValue("@AppBranch", item.AppBranch);
                    command.Parameters.AddWithValue("@AppNo", item.AppNo);
                    command.Parameters.AddWithValue("@PolYear", item.PolYear);
                    command.Parameters.AddWithValue("@PolBranch", item.PolBranch);
                    command.Parameters.AddWithValue("@PolNo", item.PolNo);
                    command.Parameters.AddWithValue("@PolPre", null);
                    command.Parameters.AddWithValue("@ApplicationNo", item.ApplicationNo);
                    command.Parameters.AddWithValue("@PolicyNo", item.PolicyNo);
                    command.Parameters.AddWithValue("@Message", "BankTest - " + item.Message);
                    command.Parameters.AddWithValue("@PolicyType", item.PolicyType);
                    command.Parameters.AddWithValue("@PolicyLanguage", item.PolicyLanguage);
                    command.Parameters.AddWithValue("@JSReportTemplateId", item.JSReportTemplateId);
                    command.Parameters.AddWithValue("@AlfrescoPath", item.AlfrescoPath);
                    command.Parameters.AddWithValue("@AlfrescoParameter", item.AlfrescoParameter);
                    command.Parameters.AddWithValue("@Status", item.Status);
                    command.Parameters.AddWithValue("@CreateDate", item.CreateDate);
                    command.Parameters.AddWithValue("@PolicyDate", item.PolicyDate);
                    command.Parameters.AddWithValue("@ProcessDocNumber", item.ProcessDocNumber);
                    command.Parameters.AddWithValue("@ThreadID", item.ThreadID.Length > 2 ? item.ThreadID.Substring(0, 2) : item.ThreadID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var msg = ex;
            }
        }

        private void InsertLogCmi(LogPolicyDTOCmi item, SqlConnection logConn)
        {
            try
            {
                using (var command = new SqlCommand("SpInsertLogSubmitDocumentDev", logConn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@AppYear", item.AppYear);
                    command.Parameters.AddWithValue("@AppBranch", item.AppBranch);
                    command.Parameters.AddWithValue("@AppNo", item.AppNo);
                    command.Parameters.AddWithValue("@PolYear", item.PolYear);
                    command.Parameters.AddWithValue("@PolBranch", item.PolBranch);
                    command.Parameters.AddWithValue("@PolNo", item.PolNo);
                    command.Parameters.AddWithValue("@PolPre", null);
                    command.Parameters.AddWithValue("@ApplicationNo", item.ApplicationNo);
                    command.Parameters.AddWithValue("@PolicyNo", item.PolicyNo);
                    command.Parameters.AddWithValue("@Message", "BankTest - " + item.Message);
                    command.Parameters.AddWithValue("@PolicyType", item.PolicyType);
                    command.Parameters.AddWithValue("@PolicyLanguage", item.PolicyLanguage);
                    command.Parameters.AddWithValue("@JSReportTemplateId", item.JSReportTemplateId);
                    command.Parameters.AddWithValue("@AlfrescoPath", item.AlfrescoPath);
                    command.Parameters.AddWithValue("@AlfrescoParameter", item.AlfrescoParameter);
                    command.Parameters.AddWithValue("@Status", item.Status);
                    command.Parameters.AddWithValue("@CreateDate", item.CreateDate);
                    command.Parameters.AddWithValue("@PolicyDate", item.PolicyDate);
                    command.Parameters.AddWithValue("@ProcessDocNumber", item.ProcessDocNumber);
                    command.Parameters.AddWithValue("@ThreadID", item.ThreadID.Length > 2 ? item.ThreadID.Substring(0, 2) : item.ThreadID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var msg = ex;
            }
        }
    }
}
