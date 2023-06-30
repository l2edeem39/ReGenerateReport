using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class AlfrescoUploadResponse
    {
        public bool success { get; set; }
        public string error { get; set; }
        public Guid fileID { get; set; }
        public string hash { get; set; }
        public string valid { get; set; }
    }
}
