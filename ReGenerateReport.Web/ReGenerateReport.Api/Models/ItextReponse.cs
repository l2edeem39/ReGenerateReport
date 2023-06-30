using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{ 
    public class ItextRequest
    {
        [JsonProperty("generateType")]
        public string GenerateType;

        [JsonProperty("key")]
        public string Key;

        [JsonProperty("templateId")]
        public string TemplateId;
    }
}
