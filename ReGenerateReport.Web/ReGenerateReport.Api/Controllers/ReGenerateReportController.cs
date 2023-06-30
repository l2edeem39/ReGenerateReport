﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> UploadToAlfresco()
        {
            
            var data = UploadPolicy_NonMotor("23", "181", "000031", "601", "A010");
            return null;
        }

        private bool UploadPolicy_NonMotor(string pol_yr, string pol_br, string pol_no, string pol_pre, string templateId)
        {
            bool flagconnfire = false;
            SqlConnection connfire = null;
            SqlTransaction tranfire = null;

            if (_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim()  != "")  //ConfigurationManager.ConnectionStrings["nonmotorfirewebdbConstr"].ToString().Trim()
            {
                flagconnfire = true;
            }

            bool contError = false;
            List<SpGetDataPolicyNonMotor> models = new List<SpGetDataPolicyNonMotor>();
            List<SpGetDataPolicyNonMotor> modelsfire = new List<SpGetDataPolicyNonMotor>();
            List<LogModel> dataLog = new List<LogModel>();

            List<SpGetDataPolicyNonMotor> modelsAll = new List<SpGetDataPolicyNonMotor>();

            Guid logId = Guid.NewGuid();
            DateTime dateLog = DateTime.Now;

            //connect database policy
            //Console.WriteLine("Start job schedule...");
            #region call log start job schedule 
            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, "Start AmitySubmitDocumentPolicyNonMotor Job Schedule");
            #endregion

            //Console.WriteLine("Connecting database...");
            var conn = new SqlConnection(_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotordbConstr"].ToString());
            conn.Open();
            SqlTransaction tran = conn.BeginTransaction();

            if (flagconnfire)
            {
                connfire = new SqlConnection(_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["nonmotorfirewebdbConstr"].ToString());
                connfire.Open();
                tranfire = connfire.BeginTransaction();
            }

            //connect database LOG
            var logConn = new SqlConnection(_config["ConnectionString:nonmotorfirewebdbConstr"].ToString().Trim()); //ConfigurationManager.ConnectionStrings["logdbConstr"].ToString());
            logConn.Open();


            //Console.WriteLine("Database connected...");


            try
            {

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Get list policy start : {DateTime.Now.ToString()}");
                //Must call store get data
                //Console.WriteLine("Get data from database...");

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Range date Start : "); //{CommonConfigs.StartDate}, End : {CommonConfigs.EndDate}");
                using (var command = new SqlCommand("SpGetDataPolicyByPolicyNonMotor", conn))
                {
                    command.Parameters.AddWithValue("@PolYear", pol_yr);
                    command.Parameters.AddWithValue("@PolBranch", pol_br);
                    command.Parameters.AddWithValue("@PolNo", pol_no);
                    command.Parameters.AddWithValue("@PolPre", pol_pre);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        SpGetDataPolicyNonMotor data = new SpGetDataPolicyNonMotor();
                        data.isProtected = (int)rdr.GetValue("isProtected");
                        data.PolYear = (string)rdr.GetString("PolYear");
                        data.PolBranch = (string)rdr.GetString("PolBranch");
                        data.PolNo = (string)rdr.GetString("PolNo");
                        data.PolPre = (string)rdr.GetString("PolPre");
                        data.AppYear = (string)rdr.GetString("AppYear");
                        data.AppBranch = (string)rdr.GetString("AppBranch");
                        data.AppNo = (string)rdr.GetString("AppNo");
                        data.ApplicationNo = (string)rdr.GetString("ApplicationNo");
                        data.PolicyNo = (string)rdr.GetString("PolicyNo");
                        data.PolicyType = String.IsNullOrEmpty(rdr.GetString("PolicyType")) ? null : Convert.ToInt32(rdr.GetString("PolicyType"));
                        data.PolicyLanguage = (string)rdr.GetString("PolicyLanguage");
                        data.fname = (string)rdr.GetString("fname");
                        data.lname = (string)rdr.GetString("lname");
                        data.ident_card = (string)rdr.GetString("ident_card");
                        data.period_from = String.IsNullOrEmpty(rdr.GetString("period_from")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("period_from"));
                        data.period_to = String.IsNullOrEmpty(rdr.GetString("period_to")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("period_to"));
                        data.phone = (string)rdr.GetString("phone");
                        data.e_email = (string)rdr.GetString("e_email");
                        data.saleCode = (string)rdr.GetString("saleCode");
                        data.tr_date = String.IsNullOrEmpty(rdr.GetString("tr_date")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("tr_date"));
                        models.Add(data);
                    }
                }

                if (models.Count <= 0)
                {
                    using (var command = new SqlCommand("SpGetDataPolicyByPolicyNonMotor", connfire))
                    {
                        command.Parameters.AddWithValue("@PolYear", pol_yr);
                        command.Parameters.AddWithValue("@PolBranch", pol_br);
                        command.Parameters.AddWithValue("@PolNo", pol_no);
                        command.Parameters.AddWithValue("@PolPre", pol_pre);
                        command.CommandType = CommandType.StoredProcedure;
                        SqlDataReader rdr = command.ExecuteReader();
                        while (rdr.Read())
                        {
                            SpGetDataPolicyNonMotor data = new SpGetDataPolicyNonMotor();
                            data.isProtected = (int)rdr.GetValue("isProtected");
                            data.PolYear = (string)rdr.GetString("PolYear");
                            data.PolBranch = (string)rdr.GetString("PolBranch");
                            data.PolNo = (string)rdr.GetString("PolNo");
                            data.PolPre = (string)rdr.GetString("PolPre");
                            data.AppYear = (string)rdr.GetString("AppYear");
                            data.AppBranch = (string)rdr.GetString("AppBranch");
                            data.AppNo = (string)rdr.GetString("AppNo");
                            data.ApplicationNo = (string)rdr.GetString("ApplicationNo");
                            data.PolicyNo = (string)rdr.GetString("PolicyNo");
                            data.PolicyType = String.IsNullOrEmpty(rdr.GetString("PolicyType")) ? null : Convert.ToInt32(rdr.GetString("PolicyType"));
                            data.PolicyLanguage = (string)rdr.GetString("PolicyLanguage");
                            data.fname = (string)rdr.GetString("fname");
                            data.lname = (string)rdr.GetString("lname");
                            data.ident_card = (string)rdr.GetString("ident_card");
                            data.period_from = String.IsNullOrEmpty(rdr.GetString("period_from")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("period_from"));
                            data.period_to = String.IsNullOrEmpty(rdr.GetString("period_to")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("period_to"));
                            data.phone = (string)rdr.GetString("phone");
                            data.e_email = (string)rdr.GetString("e_email");
                            data.saleCode = (string)rdr.GetString("saleCode");
                            data.tr_date = String.IsNullOrEmpty(rdr.GetString("tr_date")) ? Convert.ToDateTime("") : Convert.ToDateTime(rdr.GetString("tr_date"));
                            models.Add(data);
                        }
                    }
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
                        string templateSetYear = _config["TemplateSetYearNonMotor:SetYear_" + model.PolPre].ToString().Trim();   //ConfigurationManager.AppSettings["SetYear_" + model.PolPre].ToString();
                        int templateAddYear = _config["TemplateSetYearNonMotor:SetYear_" + model.PolPre].ToString().Trim() == "" ? 0 : int.Parse(_config["ConnectionString:IsReplaceFile"].ToString().Trim());   //ConfigurationManager.AppSettings["AddYear_" + model.PolPre].ToString() == "" ? 0 : int.Parse(ConfigurationManager.AppSettings["AddYear_" + model.PolPre].ToString());
                        try
                        {
                            #region call log api start
                            indexLog++;
                            HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start request document policyNo : {model.PolicyNo} | Index [{indexLog}/{models.Count.ToString()}]", model.PolicyNo);
                            #endregion
                                AlfrescoSearchResponse listFileLibrary = AlfrescoHelper.Search(model.PolicyNo, "NonMotor", logId, dateLog).Result;
                                if (listFileLibrary != null && listFileLibrary.items != null && listFileLibrary.items.Count > 0)
                                {
                                    string isReplaceFile = _config["ConnectionString:IsReplaceFile"].ToString().Trim();  //ConfigurationManager.AppSettings["IsReplaceFile"].ToString();
                                    if (isReplaceFile == "true")
                                    {
                                        string fileID = listFileLibrary.items[0].nodeRef.Replace("workspace://SpacesStore/", "");
                                        string delResult = AlfrescoHelper.DeleteNonMotor(fileID, logId, dateLog).Result;
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
                                string destination = string.Format(AlfrescoConfigNonMotor.Path, yearPolicy, model.PolPre);
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
                                        title = "AmitySubmitDocumentPolicyNonMotor",
                                        mimetype = AlfrescoConfigNonMotor.MimeType,
                                        type = AlfrescoConfigNonMotor.Type,
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
                                    alfrescoResponse = AlfrescoHelper.UploadNonMotor(alfresco, logId, dateLog).Result;
                                    if (alfrescoResponse.success)
                                    {
                                        alfresco.file = null;
                                        InsertLog(new LogPolicyDTO()
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
                                        Console.WriteLine($"Completed {index} of {models.Count} items...");
                                        contError = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"ERORR  {index} of {models.Count} items...alfresco");
                                        InsertLog(new LogPolicyDTO()
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
                                    InsertLog(new LogPolicyDTO()
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
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, resjson);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"End request amity submit document policyNo : {model.PolicyNo}");
                                #endregion
                            }
                            else
                            {
                                //Console.WriteLine($"ERORR {index} of {models.Count} items... itext7 {documentResponse.Description} ");
                                InsertLog(new LogPolicyDTO()
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
                            string templateId = "";
                            string yearPolicy = (int.Parse(templateSetYear + model.PolicyNo.Substring(0, 2)) - templateAddYear).ToString();
                            string destination = string.Format(AlfrescoConfigNonMotor.Path, yearPolicy, model.PolPre);
                            //Console.WriteLine($"ERORR {index} of {models.Count} items... {ex.Message} "); //{ex.InnerException.Message}
                            //insert log
                            InsertLog(new LogPolicyDTO()
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

            return contError;
        }

        private void InsertLog(LogPolicyDTO item, SqlConnection logConn)
        {
            try
            {
                using (var command = new SqlCommand("SpInsertLogSubmitDocument", logConn))
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
                    command.Parameters.AddWithValue("@Message", item.Message);
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
            catch (Exception)
            {

            }
        }

        class AlfrescoConfigNonMotor
        {
            public static string Path = ConfigurationManager.AppSettings["Alfresco:AlfrescoPath"].ToString();
            public static string Type = ConfigurationManager.AppSettings["AlfrescoType"].ToString();
            public static string MimeType = ConfigurationManager.AppSettings["AlfrescoMimeType"].ToString();
        };
    }
}
