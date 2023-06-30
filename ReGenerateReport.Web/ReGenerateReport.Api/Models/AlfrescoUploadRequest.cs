using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class AlfrescoUploadRequest
    {
        public byte[] file { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string mimetype { get; set; }
        public string type { get; set; }
        public string prop_vir_docnumber { get; set; }
        public string prop_vir_doctype { get; set; }
        public string prop_vir_docsubtype { get; set; }
        public string prop_vir_compcode { get; set; }
        public string prop_vir_insuredname { get; set; }
        public string prop_vir_insuredlastname { get; set; }
        public string prop_vir_idcard { get; set; }
        public string prop_vir_birthdate { get; set; }
        public string prop_vir_periodstart { get; set; }
        public string prop_vir_periodend { get; set; }
        public string prop_vir_mobile { get; set; }
        public string prop_vir_email { get; set; }
        public string prop_vir_parentdocno { get; set; }
        public string prop_vir_status { get; set; }
        public string destination { get; set; }
        public string policyNumber { get; set; }
    }
}
