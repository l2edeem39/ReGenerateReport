using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Models
{
    public class SignatureModel
    {
        public string signature_fullname => pname + fname + " " + lname;
        public string pname { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string br_code { get; set; }
        public string position { get; set; }
        public string empcode { get; set; }
        public byte[] signature_picture { get; set; }
    }
}
