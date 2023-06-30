using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public static class TokenHelper
    {
        public static IConfiguration _Configuration;

        public async static Task<TokenResponse> GetToken()
        {
            var Response = new TokenResponse();
            try
            {
                HttpClient Client = new HttpClient();

                //Get Configuration
                string tokenEndpoint = _Configuration["Alfresco:Token_Endpoint"];  //ConfigurationManager.AppSettings["Token_Endpoint"].ToString();
                string client_id = _Configuration["Alfresco:Token_ClientId"];  //ConfigurationManager.AppSettings["Token_ClientId"].ToString();
                string client_secret = _Configuration["Alfresco:Token_ClientSecret"];  //ConfigurationManager.AppSettings["Token_ClientSecret"].ToString();
                string grant_type = _Configuration["Alfresco:Token_GrantType"];  //ConfigurationManager.AppSettings["Token_GrantType"].ToString();
                string username = _Configuration["Alfresco:Token_UserName"];  //ConfigurationManager.AppSettings["Token_UserName"].ToString();
                string password = _Configuration["Alfresco:Token_Password"];  //ConfigurationManager.AppSettings["Token_Password"].ToString();

                //Set Content Type and Endcoding
                string param = $"client_id={client_id}&" +
                                $"client_secret={client_secret}&" +
                                $"grant_type={grant_type}&" +
                                $"username={username}&" +
                                $"password={password}";

                StringContent content = new StringContent(param, Encoding.UTF8, "application/x-www-form-urlencoded");

                //Call API
                var Result = Client.PostAsync(tokenEndpoint, content).Result;

                //Get Result
                if (Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var ContentResult = await Result.Content.ReadAsStringAsync();
                    Response = JsonConvert.DeserializeObject<TokenResponse>(ContentResult);
                }
                else
                {
                    var ContentResult = await Result.Content.ReadAsStringAsync();
                    var Exception = JsonConvert.DeserializeObject<TokenResponseException>(ContentResult);
                    Response.error = Exception.error;
                    Response.error_description = Exception.error_description;
                }

                return Response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
