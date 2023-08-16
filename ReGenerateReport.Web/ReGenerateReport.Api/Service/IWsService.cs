using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public interface IWsService
    {
        public Task<WsGetLinkResponse> getLinkPolicy(string policy, string sale_code);
        GetPolicy getPolicyType(string policy);
        public Task<WsGetLinkResponse> CheckDataDB(WsGetLinkResponse req);
        public Task<WsGetLinkResponse> CheckFilePDF(WsGetLinkResponse req, string sale_code, string isCopy);
        public Task<WsGetLinkResponse> CheckFileSign(WsGetLinkResponse req, string sale_code);
    }
}
