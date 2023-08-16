using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class WsGetLinkResponse
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }
        public string polType { get; set; }
        public string polYr { get; set; }
        public string polBr { get; set; }
        public string polNo { get; set; }
        public string FileNamePolicy { get; set; }
        public string FileNameTax { get; set; }
        public string linkPolicy { get; set; }
        public string linkTax { get; set; }
    }
}
