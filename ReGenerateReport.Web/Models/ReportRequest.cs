using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Web.Models
{
    public class ReportRequest
    {
        public string strMethodName { get; set; }
        public string strUrl { get; set; }
        public string strJson { get; set; }
    }
}
