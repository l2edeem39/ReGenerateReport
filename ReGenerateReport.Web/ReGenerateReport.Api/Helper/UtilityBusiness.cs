using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public class UtilityBusiness
    {
        public static string GetPolicyPath(string pol_br, string sale_code, string type)
        {
            string ls_path_type = string.Empty;

            if (type == "VMI") ls_path_type = "~/report/PolicyFormVMI/";
            else if (type == "CMI") ls_path_type = "~/report/PolicyFormCMI/";
            else if (type == "ECMI") ls_path_type = "~/report/EPolicyFormCMI/";
            else if (type == "EVMI") ls_path_type = "~/report/EPolicyFormVMI/";
            else if (type == "VMIC") ls_path_type = "~/report/PolicyFormVMICopy/";
            else if (type == "CMIC") ls_path_type = "~/report/PolicyFormCMICopy/";

            //if (!Directory.Exists(HttpContext.Current.Server.MapPath(ls_path_type)))
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ls_path_type));

            //string ls_path = HttpContext.Current.Server.MapPath(ls_path_type + DateHelper.GetCurYear() + "/" + pol_br + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/");
            string ls_path = ls_path_type + DateHelper.GetCurYear() + "/" + pol_br + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/";
            //if (!Directory.Exists(ls_path))
            //{
            //    Directory.CreateDirectory(ls_path);
            //}

            return ls_path;
        }
        public static string GetTaxinvoicePath(string pol_br, string sale_code, string type)
        {
            string ls_path_type = string.Empty;

            if (type == "VMI") ls_path_type = "~/report/TaxinvoiceFormVMI/";
            else if (type == "CMI") ls_path_type = "~/report/TaxinvoiceFormCMI/";
            else if (type == "ECMI") ls_path_type = "~/report/ETaxinvoiceFormCMI/";
            else if (type == "EVMI") ls_path_type = "~/report/ETaxinvoiceFormVMI/";
            else if (type == "VMIC") ls_path_type = "~/report/TaxinvoiceFormVMICopy/";
            else if (type == "CMIC") ls_path_type = "~/report/TaxinvoiceFormCMICopy/";

            //if (!Directory.Exists(HttpContext.Current.Server.MapPath(ls_path_type)))
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ls_path_type));

            //string ls_path = HttpContext.Current.Server.MapPath(ls_path_type + DateHelper.GetCurYear() + "/" + pol_br + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/");
            string ls_path = ls_path_type + DateHelper.GetCurYear() + "/" + pol_br + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/";
            if (!Directory.Exists(ls_path))
            {
                Directory.CreateDirectory(ls_path);
            }

            return ls_path;
        }
        public static string GetReportPath(string name)
        {
            //string ls_path = HttpContext.Current.Server.MapPath("~/crystalreport/" + name);
            string ls_path = "~/crystalreport/" + name;

            return ls_path;
        }

        public static string GetReportRdlcPath(string name)
        {
            //string ls_path = HttpContext.Current.Server.MapPath("~/ReportRdlc/" + name);
            string ls_path = "~/ReportRdlc/" + name;

            return ls_path;
        }

        public static string MD5Hash(string Data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }
    }
}
