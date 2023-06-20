using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ReportResponse
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }
        public byte[] ContentFile { get; set; }
        public string Json { get; set; }
    }
}
