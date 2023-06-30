using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReGenerateReport.Api.LogApiAnalytic;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    class TokenConfig
    {
        public static string ExprieIn = string.Empty;
        public static string Token = string.Empty;
    };

    public static class AlfrescoHelper
    {
        public static IConfiguration _Configuration;
        public static async void CheckTokenUploadExprie()
        {
            DateTime tokenExprie;
            TokenResponse resToken;

            //Get Configuration Token Exprie In
            string strTokenExprie = TokenConfig.ExprieIn;
            if (string.IsNullOrWhiteSpace(strTokenExprie) == true)
            {
                tokenExprie = DateTime.Now;
                //Call api token
                resToken = TokenHelper.GetToken().Result;
                if (string.IsNullOrEmpty(resToken.error) == true)
                {
                    //Set new token exprie
                    tokenExprie = tokenExprie.AddSeconds(resToken.expires_in);
                    //Set token exprie
                    TokenConfig.ExprieIn = tokenExprie.ToString("yyyy-MM-dd HH:mm:ss");
                    TokenConfig.Token = resToken.access_token;
                }
            }
            else
            {
                tokenExprie = DateTime.Parse(strTokenExprie);
                if ((tokenExprie - DateTime.Now).TotalSeconds < 300)
                {
                    //Call api token
                    resToken = TokenHelper.GetToken().Result;
                    if (string.IsNullOrEmpty(resToken.error) == true)
                    {
                        //Set new token exprie
                        tokenExprie = DateTime.Now.AddSeconds(resToken.expires_in);
                        //Set token exprie
                        TokenConfig.ExprieIn = tokenExprie.ToString("yyyy-MM-dd HH:mm:ss");
                        TokenConfig.Token = resToken.access_token;
                    }
                }
            }

            await Task.FromResult(0);
        }
        public static async Task<AlfrescoSearchResponse> Search(string policyNumber, string documentType, Guid logId, DateTime dateLog)
        {
            AlfrescoSearchResponse Response = new AlfrescoSearchResponse();
            try
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start search document alfresco");

                //Check Token
                CheckTokenUploadExprie();

                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        //Get Configuration Alfresco
                        string AlfrescoSearchEndpoint = _Configuration["Alfresco:AlfrescoSearchEndpoint"];    //ConfigurationManager.AppSettings["AlfrescoSearchEndpoint"].ToString();
                        string token = TokenConfig.Token;

                        //Set param
                        string query = $"(=vir\\:doctype:\"{documentType}\") AND (=vir\\:docnumber:\"{policyNumber}\")";
                        //string query = $"(TYPE:\"vir:polproperty\")";
                        string column = $"vir_docnumber," +
                                        $"vir_doctype," +
                                        $"vir_docsubtype," +
                                        $"vir_compcode," +
                                        $"vir_compname," +
                                        $"vir_branchid," +
                                        $"vir_payname," +
                                        $"vir_paylastname," +
                                        $"vir_insuredname," +
                                        $"vir_insuredlastname," +
                                        $"vir_idcard," +
                                        $"vir_taxno," +
                                        $"vir_birthdate," +
                                        $"vir_periodstart," +
                                        $"vir_periodend," +
                                        $"vir_transactiondate," +
                                        $"vir_accountiondate," +
                                        $"vir_mobile," +
                                        $"vir_email," +
                                        $"vir_parentdocno," +
                                        $"pocrest_hash";

                        //Set Authorization
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //Call Alfresco API
                        using (HttpResponseMessage HttpResponse = client.GetAsync($"{AlfrescoSearchEndpoint}&query={query}&columns={column}").Result)
                        {
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                var ContentResult = await HttpResponse.Content.ReadAsStringAsync();
                                Response = JsonConvert.DeserializeObject<AlfrescoSearchResponse>(ContentResult);
                                if (Response != null && Response.items != null && Response.items.Count > 0)
                                {
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Found document on alfresco.");
                                }
                                else
                                {
                                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Not found document on alfresco.");
                                }
                            }
                            else
                            {
                                Response = new AlfrescoSearchResponse();
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Search file alfersco Fail.");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, ex.Message);
                //throw;
            }
            return Response;
        }
        public static async Task<AlfrescoUploadResponse> UploadNonMotor(AlfrescoUploadRequest RequestData, Guid logId, DateTime dateLog)
        {
            AlfrescoUploadResponse Result = new AlfrescoUploadResponse();

            try
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start upload document to alfresco.");

                //Check Token Exprie
                CheckTokenUploadExprie();

                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        //Get Configuration Alfresco
                        string AlfrescoEndpoint = _Configuration["Alfresco:AlfrescoEndpoint"];   //ConfigurationManager.AppSettings["AlfrescoEndpoint"].ToString();
                        string token = TokenConfig.Token;
                        string status = "Signed";

                        //Set param
                        formData.Add(new ByteArrayContent(RequestData.file), "\"file\"", "\"" + RequestData.name + "\"");
                        formData.Add(new StringContent(RequestData.name ?? ""), "name");
                        formData.Add(new StringContent(RequestData.title ?? ""), "title");
                        formData.Add(new StringContent(RequestData.mimetype ?? ""), "mimetype");
                        formData.Add(new StringContent(RequestData.type ?? ""), "type");
                        formData.Add(new StringContent(RequestData.prop_vir_docnumber ?? ""), "prop_vir_docnumber");
                        formData.Add(new StringContent(RequestData.prop_vir_doctype ?? ""), "prop_vir_doctype");
                        formData.Add(new StringContent(RequestData.prop_vir_docsubtype ?? ""), "prop_vir_docsubtype");
                        formData.Add(new StringContent(RequestData.prop_vir_compcode ?? ""), "prop_vir_compcode");
                        formData.Add(new StringContent(RequestData.prop_vir_insuredname ?? ""), "prop_vir_insuredname");
                        formData.Add(new StringContent(RequestData.prop_vir_insuredlastname ?? ""), "prop_vir_insuredlastname");
                        formData.Add(new StringContent(RequestData.prop_vir_idcard ?? ""), "prop_vir_idcard");
                        formData.Add(new StringContent(RequestData.prop_vir_birthdate ?? ""), "prop_vir_birthdate");
                        formData.Add(new StringContent(RequestData.prop_vir_periodstart ?? ""), "prop_vir_periodstart");
                        formData.Add(new StringContent(RequestData.prop_vir_periodend ?? ""), "prop_vir_periodend");
                        formData.Add(new StringContent(RequestData.prop_vir_mobile ?? ""), "prop_vir_mobile");
                        formData.Add(new StringContent(RequestData.prop_vir_email ?? ""), "prop_vir_email");
                        formData.Add(new StringContent(RequestData.prop_vir_parentdocno ?? ""), "prop_vir_parentdocno");
                        formData.Add(new StringContent(RequestData.destination ?? ""), "destination");
                        formData.Add(new StringContent(status ?? ""), "prop_vir_status");

                        //Set Authorization
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //Call Alfresco API
                        using (HttpResponseMessage HttpResponse = client.PostAsync(AlfrescoEndpoint, formData).Result)
                        {
                            var ContentResult = await HttpResponse.Content.ReadAsStringAsync();
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                Result = JsonConvert.DeserializeObject<AlfrescoUploadResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Upload document successfully.");
                            }
                            else
                            {
                                Result = JsonConvert.DeserializeObject<AlfrescoUploadResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Upload document fail : " + Result.error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Upload document fail : " + ex.Message + " " + ex.InnerException.Message);
            }
            return Result;
        }
        public static async Task<AlfrescoUploadResponse> UpdateNonMotor(AlfrescoUpdateRequest RequestData, Guid logId, DateTime dateLog)
        {
            AlfrescoUploadResponse Result = new AlfrescoUploadResponse();
            try
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start update document to alfresco.");

                //Check Token Exprie
                CheckTokenUploadExprie();

                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        //Get Configuration Alfresco
                        string AlfrescoUpdateEndpoint = _Configuration["Alfresco:AlfrescoUpdateEndpoint"];    //ConfigurationManager.AppSettings["AlfrescoUpdateEndpoint"].ToString();
                        string token = TokenConfig.Token;
                        string status = "Signed";

                        //Set param
                        formData.Add(new StringContent(RequestData.fileID ?? ""), "fileID");
                        formData.Add(new ByteArrayContent(RequestData.file), "\"file\"", "\"" + RequestData.name + "\"");


                        //Set Authorization
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //Call Alfresco API
                        using (HttpResponseMessage HttpResponse = client.PutAsync(AlfrescoUpdateEndpoint, formData).Result)
                        {
                            var ContentResult = await HttpResponse.Content.ReadAsStringAsync();
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                Result = JsonConvert.DeserializeObject<AlfrescoUploadResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Update document successfully.");
                            }
                            else
                            {
                                Result = JsonConvert.DeserializeObject<AlfrescoUploadResponse>(ContentResult);
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Update document fail : " + Result.error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, ex.Message);
            }
            return Result;

        }
        public static async Task<string> DeleteNonMotor(string fileID, Guid logId, DateTime dateLog)
        {
            try
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start delete document alfresco");
                string ContentResult = string.Empty;
                //Check Token
                CheckTokenUploadExprie();

                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        //Get Configuration Alfresco
                        string AlfrescoDeleteEndpoint = _Configuration["Alfresco:AlfrescoDeleteEndpoint"];   //ConfigurationManager.AppSettings["AlfrescoDeleteEndpoint"].ToString();
                        string token = TokenConfig.Token;

                        formData.Add(new StringContent(fileID), "fileID");

                        //Set Authorization
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //Call Alfresco API
                        using (HttpResponseMessage HttpResponse = client.DeleteAsync($"{AlfrescoDeleteEndpoint}?fileID={fileID}").Result)
                        {
                            ContentResult = await HttpResponse.Content.ReadAsStringAsync();
                            if (HttpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Delete document successfully.");
                            }
                            else
                            {
                                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Delete document Fail : {ContentResult}");
                            }
                        }
                    }
                }

                return ContentResult;
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, ex.Message);
                throw;
            }
        }
    }
}
