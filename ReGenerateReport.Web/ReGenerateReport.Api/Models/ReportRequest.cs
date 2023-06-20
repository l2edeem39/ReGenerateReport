using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ReportRequest
    {
        public ReportRequest()
        {
            json = new Json();
        }
        public Json json { get; set; }

        public class Json
        {
            public string strJson { get; set; }
        }
    }
}
