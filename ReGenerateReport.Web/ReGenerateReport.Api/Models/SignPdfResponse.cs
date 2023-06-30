using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReGenerateReport.Api.Models
{
    [XmlRoot("signPdfResponse")]
    public class SignPdfResponse
    {
        public string reqRefNo { get; set; }
        public string respRefNo { get; set; }
        public string respCode { get; set; }
        public string respStatus { get; set; }
        public string respDesc { get; set; }
        public string signedPdf { get; set; }
    }
}
