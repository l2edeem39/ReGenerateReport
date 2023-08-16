using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public interface IPolicyReportService
    {
        public string VmiPolicyT3(string yr, string brcode, string polno, string sale_code);
        public string VmiPolicyT5(string yr, string brcode, string polno, string sale_code);
        public string VmiPolicyT5_Epol(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline);
        public string VmiPolicyT3_Epol(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline);
        public SignatureModel getSignature(string br_code);
    }
}
