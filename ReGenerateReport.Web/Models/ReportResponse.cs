using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Web.Models
{
    public class ReportResponse
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string contentFile { get; set; }
        public Json json { get; set; }
        public string contentType { get; set; }
    }

    

    public class Json
    {
        public string value { get; set; }
    }
}
