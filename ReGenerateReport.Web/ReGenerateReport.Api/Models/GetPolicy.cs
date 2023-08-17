using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class GetPolicy
    {
        public string polType { get; set; }
        public string polYr { get; set; }
        public string polBrCode { get; set; }
        public string polNo { get; set; }
        public string saleCode { get; set; }
        public string productClass { get; set; }
        public string channelCode { get; set; }
        public string flagOnline { get; set; }
    }

    public class ReqPolicy
    {
        public string PolicyNo { get; set; }
    }
}
