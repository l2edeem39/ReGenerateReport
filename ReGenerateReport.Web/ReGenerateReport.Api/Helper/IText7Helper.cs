using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReGenerateReport.Api.Constant;
using ReGenerateReport.Api.LogApiAnalytic;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public static class IText7Helper
    {
        public static IConfiguration _Configuration;

        public static async Task<IText7ReportResponse> GenerateDocument(string policyNumber, string templateId, Guid logId, DateTime dateLog)
        {
            IText7ReportResponse result = new IText7ReportResponse();
            var Endpoint = _Configuration["Alfresco:ITEXT7_ReportCenter"];


            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Pdf));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Endpoint + "/GeneratePDF");
            string body = JsonConvert.SerializeObject(new ItextRequest()
            {
                GenerateType = "2",
                Key = "@PolicyNo='" + policyNumber + "'",
                TemplateId = templateId
            });

            request.Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();



            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ContentFile = await response.Content.ReadAsByteArrayAsync();
                result.StatusDownload = HttpStatusCode.OK.ToString();
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, " Itext7 Successfully.");
            }
            else
            {
                result.Description = response.ReasonPhrase;
                result.StatusDownload = response.StatusCode.ToString();
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, " Itext7 fail : " + result.Description);
            }
            return result;

        }
    }
}
