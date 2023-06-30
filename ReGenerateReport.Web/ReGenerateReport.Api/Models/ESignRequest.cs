using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class ESignRequest
    {
        public string task
        {
            get { return ConfigurationManager.AppSettings["ABBR"].ToString(); }

        }
        public string documentType { get; set; }
        public string referenceId { get; set; }
        public string document { get; set; }
        public string password { get; set; }
    }
}
