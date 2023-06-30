using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ESignResponse
    {
        public Result result { get; set; }
        public string statusDescription { get; set; }
        public string status { get; set; }
        public int statusCode { get; set; }
    }

    public class Result
    {
        public string task { get; set; }
        public string documentType { get; set; }
        public string document { get; set; }
    }
}
