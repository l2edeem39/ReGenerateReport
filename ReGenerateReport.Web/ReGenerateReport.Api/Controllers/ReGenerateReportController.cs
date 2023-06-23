using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
    }
}
