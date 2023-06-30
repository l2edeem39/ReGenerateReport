using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class TokenResponseException
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
