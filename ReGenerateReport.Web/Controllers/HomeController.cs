using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReGenerateReport.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace ReGenerateReport.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;
        }

        public IActionResult Index()
        {
            //Task<ReportResponse> Result = PdfPartial("POST", "https://hqcapidev.viriyah.co.th/ItextGenerateDoc/api/GeneratePDF", "{\"key\":\"@EndosNo='23181/END/000013-580'\",\"templateId\":\"NE02\",\"generateType\":\"2\"}");
            //var byteA = Result.Result.ContentFile;
            //var imageBytes = Convert.FromBase64String(byteA);
            //String hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(GetIp());
            //String hostName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

            String hostName = hostEntry.HostName;
            //var hostName = GetIp();

            //string imageBase64Data = Convert.ToBase64String(imageBytes);
            //string imageDataURL = string.Format("data:application/pdf;base64,{0}", imageBase64Data);
            //ViewBag.ImageData = imageDataURL;
            //ViewBag.Data = imageDataURL;
 
            ViewBag.HostName = hostName;
            return View();
        }

        public async Task<WebPageResponse> PdfPartial(string strMethodName, string strUrl, string strJson, string strUser)
        {
            WebPageResponse WebResult = new WebPageResponse();
            string imageBase64Data = null;
            string imageDataURL = null;
            WebResult.contentFile = imageBase64Data;
            WebResult.contentFilePreview = imageDataURL;
            WebResult.contentType = "error";
            WebResult.json = "";
            WebResult.status = "Internal Server";
            WebResult.statusCode = "500";
            try
            {
                
                Task<ReportResponse> Result = SendCallApi(strMethodName, strUrl, strJson, strUser, "");
                if (Result != null && Result.Result.statusCode != null && Result.Result.statusCode != "")
                {
                    if (Result.Result.contentType != null && Result.Result.contentType != "")
                    {
                        if (Result.Result.contentType == "application/pdf")
                        {
                            var byteA = Result.Result.contentFile;
                            var imageBytes = Convert.FromBase64String(byteA);
                            imageBase64Data = Convert.ToBase64String(imageBytes);
                            imageDataURL = string.Format("data:application/pdf;base64,{0}", imageBase64Data);
                        }
                    }
                    WebResult.contentFile = imageBase64Data;
                    WebResult.contentFilePreview = imageDataURL;
                    WebResult.contentType = Result.Result.contentType;
                    WebResult.status = Result.Result.status;
                    WebResult.statusCode = Result.Result.statusCode;
                    WebResult.json = Result.Result.json == null?"": Result.Result.json.value;
                }
            }
            catch (Exception ex)
            {
                WebResult.status = ex.Message;
            }

            return WebResult;
        }

        public async Task<ReportResponse> SendCallApi(string strMethodName, string strUrl, string strJson, string Username, string Password)
        {
            try
            {
                ReportResponse Result = new ReportResponse();
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var client = new HttpClient())
                    {
                        string Endpoint = _config.GetSection("DefaultUrl").Value.ToString();
                        ReportRequest RequestData = new ReportRequest();
                        RequestData.strJson = strJson.ToString();
                        RequestData.strMethodName = strMethodName.ToString();
                        RequestData.strUrl = strUrl.ToString();
                        string JsonRequest = JsonConvert.SerializeObject(RequestData);
                        StringContent content = new StringContent(JsonRequest, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode($"{Username}:{Password}")}");
                        using (HttpResponseMessage HttpResponse = client.PostAsync(Endpoint, content).Result)
                        {
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                var responseContent = await HttpResponse.Content.ReadAsStringAsync();
                                var reportResponse = JsonConvert.DeserializeObject<ReportResponse>(responseContent);

                                //object data = await HttpResponse.Content.Headers.ContentType.ToString();
                                //var imageBytes = Convert.FromBase64String(byteA);

                                //string imageBase64Data = Convert.ToBase64String(imageBytes);
                                //string imageDataURL = string.Format("data:application/pdf;base64,{0}", imageBase64Data);
                                //ViewBag.ImageData = imageDataURL;
                                //return byteA;
                                return reportResponse;
                            }
                            else
                            {
                                var responseContent = await HttpResponse.Content.ReadAsStringAsync();
                                var reportResponse = JsonConvert.DeserializeObject<ReportResponse>(responseContent);

                                //object data = await HttpResponse.Content.Headers.ContentType.ToString();
                                //var imageBytes = Convert.FromBase64String(byteA);

                                //string imageBase64Data = Convert.ToBase64String(imageBytes);
                                //string imageDataURL = string.Format("data:application/pdf;base64,{0}", imageBase64Data);
                                //ViewBag.ImageData = imageDataURL;
                                //return byteA;
                                return reportResponse;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Base64Encode(string textToEncode)
        {
            byte[] textAsBytes = Encoding.UTF8.GetBytes(textToEncode);
            return Convert.ToBase64String(textAsBytes);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult WSPolicy()
        {
            var hostEntry = Dns.GetHostEntry(GetIp());
            String hostName = hostEntry.HostName;
            ViewBag.HostName = hostName;
            return View();
        }
        public IActionResult Default()
        {
            var hostEntry = Dns.GetHostEntry(GetIp());
            String hostName = hostEntry.HostName;
            ViewBag.HostName = hostName;
            string ReGenerateItext = _config.GetSection("Menu_ReGenerateItext").Value.ToString();
            string WSPolicyRepair = _config.GetSection("Menu_WSPolicyRepair").Value.ToString();
            if (ReGenerateItext == "true")
            {
                return View("Index");
            }
            else if (WSPolicyRepair == "true")
            {
                return View("WSPolicy");
            }
            else
            {
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public string GetIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            //var remoteIpAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            return ip;
        }
        public async Task<ResponseUploadPDF> UploadPDF(string strJson, string Username)
        {
            try
            {
                //string jsonArray = "{\"key\":\"@PolicyNo='23181/POL/000013-580'\",\"templateId\":\"NE02\",\"generateType\":\"2\"}";
                var RequestData = new RequestUploadPDF();
                var jsonModel = JsonConvert.DeserializeObject<JsonRequestModel>(strJson);
                var policyNumber = jsonModel.key.Remove(0, 10).ToString().Replace("'", "");
                RequestData.policyNo = policyNumber;
                RequestData.templateId = jsonModel.templateId;
                var Password = string.Empty;

                using (var client = new HttpClient())
                {
                    string Endpoint = _config.GetSection("DefaultUrlUpload").Value.ToString();
                    string JsonRequest = JsonConvert.SerializeObject(RequestData);
                    StringContent content = new StringContent(JsonRequest, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode($"{Username}:{Password}")}");
                    using (HttpResponseMessage HttpResponse = client.PostAsync(Endpoint, content).Result)
                    {
                        var responseContent = await HttpResponse.Content.ReadAsStringAsync();
                        var reportResponse = JsonConvert.DeserializeObject<ResponseUploadPDF>(responseContent);
                        return reportResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                var res = new ResponseUploadPDF()
                {
                    statusCode = "500",
                    message = ex.Message
                };
                return res;
            }
        }

        public async Task<WsGetLinkResponse> GetLinkWSPolicy(string policyNo, string strUser)
        {
            WsGetLinkResponse WebResult = new WsGetLinkResponse();
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var client = new HttpClient())
                    {
                        string Endpoint = _config.GetSection("DefaultUrlWsPolicy").Value.ToString();
                        ReqPolicy RequestData = new ReqPolicy();
                        RequestData.PolicyNo = policyNo.ToString();
                        //RequestData.SaleCode = saleCode.ToString();
                        string JsonRequest = JsonConvert.SerializeObject(RequestData);
                        StringContent content = new StringContent(JsonRequest, Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode($"{strUser}:{""}")}");
                        using (HttpResponseMessage HttpResponse = client.PostAsync(Endpoint, content).Result)
                        {
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                var responseContent = await HttpResponse.Content.ReadAsStringAsync();
                                var reportResponse = JsonConvert.DeserializeObject<WsGetLinkResponse>(responseContent);
                                return reportResponse;
                            }
                            else
                            {
                                var responseContent = await HttpResponse.Content.ReadAsStringAsync();
                                var reportResponse = JsonConvert.DeserializeObject<WsGetLinkResponse>(responseContent);
                                return reportResponse;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WebResult.StatusCode = "500";
                WebResult.Status = ex.Message;
                return WebResult;
            }
        }

    }
}
