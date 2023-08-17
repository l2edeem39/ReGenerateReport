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
        public string VmiPolicyT4Copy(out string barcode, string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline);
        public string VmiPolicyT4(out string barcode, string yr, string brcode, string polno, string sale_code);
        public string PolicyVMIType4(out string barcode, string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline);
        public string CmiCopyrdlc(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline);
        public string Cmirdlc(string yr, string brcode, string polno, string sale_code);
        public SignatureModel getSignature(string br_code);
    }
}
