using Newtonsoft.Json;
using ReGenerateReport.Api.LogApiAnalytic;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ReGenerateReport.Api.Helper
{
    public class SignHelper
    {
        public static IConfiguration _Configuration;

        public async static Task<ESignResponse> ESignDocument(ESignRequest requestData, Guid logId, DateTime dateLog)
        {
            var Response = new ESignResponse();
            try
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Sign document : ESignDocument");

                //Get Configuration
                string SingEndpoint = _Configuration["Alfresco:ESignEndpoint"];   //ConfigurationManager.AppSettings["ESignEndpoint"].ToString();
                string Token = _Configuration["Alfresco:ESignToken"];   //ConfigurationManager.AppSettings["ESignToken"].ToString();

                //Convert Class to JSON
                string RequestESign = JsonConvert.SerializeObject(requestData);

                //Set Content Type and Endcoding
                StringContent content = new StringContent(RequestESign, Encoding.UTF8, "application/json");

                //Call API
                try
                {
                    using (var client = new HttpClient())
                    {
                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Sign Normal");

                        //Add Authorization To Header
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Token);

                        using (HttpResponseMessage Result = client.PostAsync(SingEndpoint, content).Result)
                        {
                            //Get Result
                            if (Result.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var ContentResult = await Result.Content.ReadAsStringAsync();
                                Response = JsonConvert.DeserializeObject<ESignResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument successful");
                            }
                            else
                            {
                                try
                                {

                                    var ContentResult = await Result.Content.ReadAsStringAsync();
                                    var errorResult = JsonConvert.DeserializeObject<ESingResponseError>(ContentResult);
                                    if (!string.IsNullOrEmpty(errorResult.error))
                                    {
                                        Response.status = errorResult.error;
                                        Response.statusCode = 500;
                                        Response.statusDescription = ContentResult;
                                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);

                                    }
                                    else
                                    {

                                        var Exception = JsonConvert.DeserializeObject<ESingResponseException>(ContentResult);
                                        Response.statusCode = Exception.statusCode;
                                        Response.status = Exception.status;
                                        Response.statusDescription = ContentResult;
                                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);
                                    }
                                }
                                catch
                                {
                                    var ContentResult = await Result.Content.ReadAsStringAsync();
                                    Response.status = "Fail";
                                    Response.statusCode = 500;
                                    Response.statusDescription = ContentResult;
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);
                                }
                            }
                        }
                    }
                }
                catch (Exception exBeforeRetry)
                {
                    //HttpException httpException = new HttpException(null, exBeforeRetry);
                    //int httpExceptionCode = httpException.GetHttpCode();
                    string exMsg = exBeforeRetry.Message;
                    string innerExMsg = null;
                    if (exBeforeRetry.InnerException != null)
                    {
                        innerExMsg += exBeforeRetry.InnerException.Message;
                        if (exBeforeRetry.InnerException.InnerException != null)
                        {
                            innerExMsg += exBeforeRetry.InnerException.InnerException.ToString();
                        }
                    }
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + exMsg + innerExMsg);   //+ httpExceptionCode + " : " + exMsg + innerExMsg);

                    using (var client = new HttpClient())
                    {
                        Thread.Sleep(int.Parse(_Configuration["Alfresco:DelayRetry"]));  //ConfigurationManager.AppSettings["DelayRetry"]
                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Sign Retry");

                        //RE TRY
                        //Add Authorization To Header
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Token);

                        //Set Content Type and Endcoding
                        content = new StringContent(RequestESign, Encoding.UTF8, "application/json");

                        using (HttpResponseMessage Result = client.PostAsync(SingEndpoint, content).Result)
                        {
                            if (Result.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var ContentResult = await Result.Content.ReadAsStringAsync();
                                Response = JsonConvert.DeserializeObject<ESignResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument successful");
                            }
                            else
                            {
                                try
                                {

                                    var ContentResult = await Result.Content.ReadAsStringAsync();
                                    var errorResult = JsonConvert.DeserializeObject<ESingResponseError>(ContentResult);
                                    if (!string.IsNullOrEmpty(errorResult.error))
                                    {
                                        Response.status = errorResult.error;
                                        Response.statusCode = 500;
                                        Response.statusDescription = ContentResult;
                                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);

                                    }
                                    else
                                    {

                                        var Exception = JsonConvert.DeserializeObject<ESingResponseException>(ContentResult);
                                        Response.statusCode = Exception.statusCode;
                                        Response.status = Exception.status;
                                        Response.statusDescription = ContentResult;
                                        HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);
                                    }
                                }
                                catch
                                {
                                    var ContentResult = await Result.Content.ReadAsStringAsync();
                                    Response.status = "Fail";
                                    Response.statusCode = 500;
                                    Response.statusDescription = ContentResult;
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + Response.statusCode + " : " + Response.statusDescription);
                                }
                            }
                        }
                    }
                }


                return Response;
            }
            catch (Exception ex)
            {
                //HttpException httpException = new HttpException(null, ex);
                //int httpExceptionCode = httpException.GetHttpCode();
                string exMsg = ex.Message;
                string innerExMsg = null;
                if (ex.InnerException != null)
                {
                    innerExMsg += ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        innerExMsg += ex.InnerException.InnerException.ToString();
                    }
                }
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"ESignDocument fail : StatusCode => " + exMsg + innerExMsg);  //+ httpExceptionCode + " : " + exMsg + innerExMsg);
                throw new Exception("ESignDocument fail : StatusCode => " + exMsg + innerExMsg);  //+ httpExceptionCode + " : " + exMsg + innerExMsg);
            }
        }

        public static ESignResponse ESignDocument(ESignRequest requestData)
        {
            try
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
                ESignResponse Response = new ESignResponse();
                HttpClient Client = new HttpClient();

                //Get Configuration
                string SingEndpoint = _Configuration["Alfresco:ESignEndpoint"];   //ConfigurationManager.AppSettings["ESignEndpoint"].ToString();
                string Token = _Configuration["Alfresco:ESignToken"];   //ConfigurationManager.AppSettings["Token"].ToString();

                //Add Authorization To Header
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Token);

                //Convert Class to JSON
                string RequestESign = JsonConvert.SerializeObject(requestData);

                //Set Content Type and Endcoding
                StringContent content = new StringContent(RequestESign, Encoding.UTF8, "application/json");

                //Call API
                var Result = Client.PostAsync(SingEndpoint, content).Result;

                //Get Result
                if (Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Response = JsonConvert.DeserializeObject<ESignResponse>(Result.Content.ReadAsStringAsync().Result);
                }
                else
                {

                    var Exception = JsonConvert.DeserializeObject<ESingResponseException>(Result.Content.ReadAsStringAsync().Result);
                    Response.statusCode = Exception.statusCode;
                    Response.status = Exception.status;
                    Response.statusDescription = Exception.statusDescription;
                }

                return Response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
