using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class LogModel
    {
        public Guid Id { get; set; }
        public string AppYear { get; set; }
        public string AppBranch { get; set; }
        public string AppNo { get; set; }
        public string PolYear { get; set; }
        public string PolBranch { get; set; }
        public string PolNo { get; set; }
        public string ApplicationNo { get; set; }
        public string PolicyNo { get; set; }
        public string PolicyType { get; set; }
        public string PolicyLanguage { get; set; }
        public string JSReportTemplateId { get; set; }
        public string AlfrescoPath { get; set; }
        public string AlfrescoParameter { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime PolicyDate { get; set; }
        public string ProcessDocNumber { get; set; }
        public string ThreadID { get; set; }
    }
}
