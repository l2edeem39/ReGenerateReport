using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class SpGetDataPolicyCmi
    {
        public int isProtected { get; set; }
        public string PolYear { get; set; }
        public string PolBranch { get; set; }
        public string PolNo { get; set; }
        public string AppYear { get; set; }
        public string AppBranch { get; set; }
        public string AppNo { get; set; }
        public string ApplicationNo { get; set; }
        public string PolicyNo { get; set; }
        public Nullable<int> PolicyType { get; set; }
        public string PolicyLanguage { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string ident_card { get; set; }
        public System.DateTime period_from { get; set; }
        public System.DateTime period_to { get; set; }
        public string phone { get; set; }
        public string e_email { get; set; }
        public string saleCode { get; set; }
        public DateTime tr_date { get; set; }
    }

    public class LogPolicyDTOCmi
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
        public System.DateTime CreateDate { get; set; }
        public System.DateTime UpdateDate { get; set; }
        public System.DateTime PolicyDate { get; set; }
        public string ProcessDocNumber { get; set; }
        public string ThreadID { get; set; }
    }
}
