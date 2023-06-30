using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ESingResponseException
    {
        public string result { get; set; }
        public string statusDescription { get; set; }
        public string status { get; set; }
        public int statusCode { get; set; }
    }

    public class ESingResponseError
    {
        public string error { get; set; }
        public string error_description { get; set; }

    }
}
