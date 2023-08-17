using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public interface IWsService
    {
        GetPolicy getPolicyType(string policy);
        public Task<WsGetLinkResponse> PolicyVMIType3(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel);
        public Task<WsGetLinkResponse> PolicyVMIType4(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel);
        public Task<WsGetLinkResponse> PolicyVMIType4Copy(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel);
        public Task<WsGetLinkResponse> PolicyVMIType5noPrint(string polyr, string polbr, string polno, string sale_code, string flagOnline, string productClass);
        public Task<WsGetLinkResponse> PolicyVMIType5(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel);
        public Task<WsGetLinkResponse> PolicyCMI(string actionName, string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel);


    }
}
