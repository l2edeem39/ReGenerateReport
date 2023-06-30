using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class AlfrescoUpdateRequest
    {
        public byte[] file { get; set; }
        public string fileID { get; set; }
        public string name { get; set; }
    }
}
