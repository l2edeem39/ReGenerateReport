using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class IText7ReportResponse
    {
        public byte[] ContentFile { get; set; }
        public string Description { get; set; }
        public string StatusDownload { get; set; }
    }
}
