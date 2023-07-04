using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class UploadAlfrescoRequestParameter
    {
        //public string pol_yr { get; set; }
        //public string pol_br { get; set; }
        //public string pol_no { get; set; }
        //public string pol_pre { get; set; }
        public string policyNo { get; set; }
        public string templateId { get; set; }
    }
}
