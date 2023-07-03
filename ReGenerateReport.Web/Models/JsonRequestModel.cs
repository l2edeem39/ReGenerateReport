using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Web.Models
{
    public class JsonRequestModel
    {
        public string key { get; set; }
        public string templateId { get; set; }
        public string generateType { get; set; }
    }
}
