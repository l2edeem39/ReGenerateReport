using Microsoft.Extensions.Configuration;
using ReGenerateReport.Api.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public class TaxinvoiceService : ITaxinvoiceService
    {
        private readonly IConfiguration _Configuration;
        private readonly IPolicyReportService _policyReportService;

        public TaxinvoiceService(IConfiguration configuration, IPolicyReportService policyReportService)
        {
            _Configuration = configuration;
            _policyReportService = policyReportService;
        }
        public string TaxInvoiceVmiRdlc(string yr, string brcode, string polno, string sale_code, string isOnline) //printTaxinvoiceVmi and printETaxinvoiceVmi
        {
            string filename = string.Empty, ls_path = string.Empty;
            string path, mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
              //  DataSet ds = GetVmiTaxInvoice(yr, brcode, polno);
              //  if (ds.Tables[0].Rows.Count == 0) return filename;
              //  ds.Tables[0].Rows[0]["isOnline"] = isOnline;

              //  var signature = _policyReportService.getSignature(brcode);
              //  ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
              //  ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

              //  string deviceInfo =
              //@"<DeviceInfo>
              //    <OutputFormat>PDF</OutputFormat>
              //    <EndPage>1</EndPage>
              //</DeviceInfo>";

                if (isOnline == "true")
                {
                    ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "EVMI");
                }
                else
                {
                    ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMI");
                }

                var encrypt_name = UtilityBusiness.MD5Hash("VMITAX" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiTaxInvoice", ds.Tables["VmiTaxInvoiceTable"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiTaxInvoice.rdlc");
                //rv.LocalReport.DataSources.Add(rds);
                //rv.LocalReport.Refresh();
                //byte[] bytes = rv.LocalReport.Render("PDF", deviceInfo, out mineType, out encoding, out filenameExtension, out streamids, out warnings);
                //rv.Dispose();
                //using (FileStream stream = new FileStream(path, FileMode.Create))
                //{
                //    stream.Write(bytes, 0, bytes.Length);
                //}
                return filename;
            }
            catch (Exception)
            {
                return filename;
            }
        }
        public string TaxInvoiceVmiCopyRdlc(string yr, string brcode, string polno, string sale_code, string isOnline)
        {
            string filename = string.Empty, ls_path = string.Empty;
            string path, mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
              //  DataSet ds = GetVmiTaxInvoice(yr, brcode, polno);
              //  if (ds.Tables[0].Rows.Count == 0) return filename;
              //  ds.Tables[0].Rows[0]["isOnline"] = isOnline;

              //  var signature = _policyReportService.getSignature(brcode);
              //  ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
              //  ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

              //  string deviceInfo =
              //@"<DeviceInfo>
              //    <OutputFormat>PDF</OutputFormat>
              //    <EndPage>1</EndPage>
              //</DeviceInfo>";

                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMIC");

                var encrypt_name = UtilityBusiness.MD5Hash("VMITAXC" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiTaxInvoice", ds.Tables["VmiTaxInvoiceTable"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiTaxInvoiceCopy.rdlc");
                //rv.LocalReport.DataSources.Add(rds);
                //rv.LocalReport.Refresh();
                //byte[] bytes = rv.LocalReport.Render("PDF", deviceInfo, out mineType, out encoding, out filenameExtension, out streamids, out warnings);
                //rv.Dispose();
                //using (FileStream stream = new FileStream(path, FileMode.Create))
                //{
                //    stream.Write(bytes, 0, bytes.Length);
                //}
                return filename;
            }
            catch (Exception)
            {
                return filename;
            }
        }
        public string TaxInvoiceVmiStickerRdlc(string yr, string brcode, string polno, string sale_code, string isOnline, bool InsuranceBanner = true)
        {
            string filename = string.Empty, ls_path = string.Empty;
            string path, mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
              //  DataSet ds = GetVmiTaxInvoice(yr, brcode, polno);
              //  if (ds.Tables[0].Rows.Count == 0) return filename;
              //  ds.Tables[0].Rows[0]["isOnline"] = isOnline;

              //  var signature = _policyReportService.getSignature(brcode);
              //  ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
              //  ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

              //  string deviceInfo =
              //@"<DeviceInfo>
              //    <OutputFormat>PDF</OutputFormat>
              //    <EndPage>1</EndPage>
              //</DeviceInfo>";

                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMI");

                var encrypt_name = UtilityBusiness.MD5Hash("VMITAX" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiTaxInvoice", ds.Tables["VmiTaxInvoiceTable"]);
                //if (InsuranceBanner)
                //{
                //    rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiTaxinvoiceSticker.rdlc");
                //}
                //else
                //{
                //    rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiTaxinvoiceNoSticker.rdlc");
                //}
                //rv.LocalReport.DataSources.Add(rds);
                //rv.LocalReport.Refresh();
                //byte[] bytes = rv.LocalReport.Render("PDF", deviceInfo, out mineType, out encoding, out filenameExtension, out streamids, out warnings);
                //rv.Dispose();
                //using (FileStream stream = new FileStream(path, FileMode.Create))
                //{
                //    stream.Write(bytes, 0, bytes.Length);
                //}
                return filename;
            }
            catch (Exception)
            {
                return filename;
            }
        }

        public DataSet GetVmiTaxInvoice(string yr, string brcode, string polno)
        {
            DataSet ds = new DataSet(); ;
            string sql = "dbo.VmiTaxInvoicePrint";

            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand();
                com.Connection = conn;
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(new SqlParameter("@yr", yr));
                com.Parameters.Add(new SqlParameter("@br", brcode));
                com.Parameters.Add(new SqlParameter("@pol_no", polno));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(ds, "VmiTaxInvoiceTable");
                }
            }
            return ds;
        }
    }
}
