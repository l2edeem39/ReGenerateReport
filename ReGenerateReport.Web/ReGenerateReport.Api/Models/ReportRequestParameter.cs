using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ReportRequestParameter
    {
        public string strMethodName { get; set; }
        public string strUrl { get; set; }
        public object strJson { get; set; }
    }
}
