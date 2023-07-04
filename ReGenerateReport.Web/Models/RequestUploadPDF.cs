using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Web.Models
{
    public class RequestUploadPDF
    {
        public string policyNo { get; set; }
        public string templateId { get; set; }
    }
}
