using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class AlfrescoSearchResponse
    {
        public List<AlfrescoItem> items { get; set; }
        public int totalResults { get; set; }
    }

    public class AlfrescoItem
    {
        public string nodeRef { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string modifiedOn { get; set; }
        public string modifiedByUser { get; set; }
        public string modifiedBy { get; set; }
        public string createdOn { get; set; }
        public string createdByUser { get; set; }
        public string createdBy { get; set; }
        public string author { get; set; }
        public string size { get; set; }
        public string browseUrl { get; set; }
        public string parentFolder { get; set; }
        public string nodeType { get; set; }
        public string parentUrl { get; set; }
        public string[] aspects { get; set; }
        public AlfrescoProperties properties { get; set; }
    }

    public class AlfrescoProperties
    {
        public string vir_insuredname { get; set; }
        public string vir_birthdate { get; set; }
        public string vir_doctype { get; set; }
        public string vir_parentdocno { get; set; }
        public string vir_mobile { get; set; }
        public string vir_docsubtype { get; set; }
        public string vir_insuredlastname { get; set; }
        public string vir_idcard { get; set; }
        public string vir_compcode { get; set; }
        public string pocrest_hash { get; set; }
        public string vir_email { get; set; }
        public string vir_docnumber { get; set; }
        public string vir_periodend { get; set; }
        public string vir_periodstart { get; set; }
    }
}
