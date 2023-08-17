using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public interface ITaxinvoiceService
    {
        public string TaxInvoiceVmiCopyRdlc(string yr, string brcode, string polno, string sale_code, string isOnline);
        public string TaxInvoiceVmiRdlc(string yr, string brcode, string polno, string sale_code, string isOnline);
        public string TaxInvoiceVmiStickerRdlc(string yr, string brcode, string polno, string sale_code, string isOnline, bool InsuranceBanner = true);
        public string TaxinvoiceCmiRdlc(string brcode, string cmi_taxno, string sale_code, string isOnline);
    }
}
