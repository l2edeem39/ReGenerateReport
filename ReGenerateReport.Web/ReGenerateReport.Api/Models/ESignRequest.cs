using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ESignRequest
    {
        public static IConfiguration _Configuration;
        public string task
        {
            get { return _Configuration["Alfresco:ABBR"].ToString(); }

        }
        public string documentType { get; set; }
        public string referenceId { get; set; }
        public string document { get; set; }
        public string password { get; set; }
    }
}
