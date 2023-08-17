using Microsoft.Extensions.Configuration;
using Microsoft.Reporting.NETCore;
using ReGenerateReport.Api.Helper;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public class PolicyReportService : IPolicyReportService
    {
        private readonly IConfiguration _Configuration;

        public PolicyReportService(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public string VmiPolicyT3(string yr, string brcode, string polno, string sale_code)
        {
            string filename = string.Empty, ls_path = string.Empty, path = string.Empty;
            //string mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
            //    DataSet ds = getVmiPolicyT3(yr, brcode, polno);
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }

            //    var signature = getSignature(brcode);
            //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
            //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

            //    string deviceInfo =
            //  @"<DeviceInfo>
            //    <OutputFormat>PDF</OutputFormat>
            //    <EndPage>1</EndPage>
            //</DeviceInfo>";

                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMI");
                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                var encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT3", ds.Tables["VmiPolicyT3Table"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT3.rdlc");
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
            catch (Exception ex)
            {
                return filename;
            }
        }

        public string PolicyVMIType4(out string barcode, string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline)
        {
            string filename = string.Empty;
            barcode = string.Empty;
            try
            {
                DataSet ds = getVmiPolicyT4(sale_code, yr, brcode, polno);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return filename;
                }
                //    ds.Tables[0].Rows[0]["isCopy"] = isCopy;
                //    ds.Tables[0].Rows[0]["isOnline"] = isOnline;

                barcode = ds.Tables[0].Rows[0]["serial"].ToString();

                //    var signature = getSignature(brcode);
                //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
                //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

                //    string deviceInfo =
                //  @"<DeviceInfo>
                //    <OutputFormat>PDF</OutputFormat>
                //    <EndPage>1</EndPage>
                //</DeviceInfo>";

                //Save
                string ls_path, path, mineType, encoding, filenameExtension;
                if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMIC");
                else ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "EVMI");

                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                //string[] streamids;
                //Warning[] warnings;
                string encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT4Copy", ds.Tables["VmiPolicyT4Table"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT4Copy.rdlc");
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

        public string VmiPolicyT4(out string barcode, string yr, string brcode, string polno, string sale_code)
        {
            string filename = string.Empty;
            barcode = string.Empty;
            try
            {
                DataSet ds = getVmiPolicyT4(sale_code, yr, brcode, polno);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return filename;
                }
                barcode = ds.Tables[0].Rows[0]["serial"].ToString();

              //  var signature = getSignature(brcode);
              //  ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
              //  ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;
              //  string deviceInfo =
              //  @"<DeviceInfo>
              //    <OutputFormat>PDF</OutputFormat>
              //    <EndPage>1</EndPage>
              //</DeviceInfo>";

                //Save
                string ls_path, path, mineType, encoding, filenameExtension;
                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMI");
                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                string encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT4Copy", ds.Tables["VmiPolicyT4Table"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT4.rdlc");
                //rv.LocalReport.DataSources.Add(rds);
                //rv.LocalReport.Refresh();
                //byte[] bytes = rv.LocalReport.Render("PDF", deviceInfo, out mineType, out encoding, out filenameExtension, out string[] streamids, out Warning[] warnings);
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

        public string VmiPolicyT4Copy(out string barcode, string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline)
        {
            string filename = string.Empty;
            barcode = string.Empty;
            try
            {
                DataSet ds = getVmiPolicyT4(sale_code, yr, brcode, polno);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return filename;
                }
                //    ds.Tables[0].Rows[0]["isCopy"] = isCopy;
                //    ds.Tables[0].Rows[0]["isOnline"] = isOnline;

                barcode = ds.Tables[0].Rows[0]["serial"].ToString();

                //    var signature = getSignature(brcode);
                //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
                //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

                //    string deviceInfo =
                //  @"<DeviceInfo>
                //    <OutputFormat>PDF</OutputFormat>
                //    <EndPage>1</EndPage>
                //</DeviceInfo>";

                //Save
                string ls_path, path, mineType, encoding, filenameExtension;
                if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMIC");
                else ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "EVMI");

                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                //string[] streamids;
                //Warning[] warnings;
                string encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT4Copy", ds.Tables["VmiPolicyT4Table"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT4Copy.rdlc");
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

        public string VmiPolicyT5(string yr, string brcode, string polno, string sale_code)
        {
            string filename = string.Empty, ls_path = string.Empty, path = string.Empty;
            //string mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
            //    DataSet ds = getVmipolicyT5(yr, brcode, polno);
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }

            //    var signature = getSignature(brcode);
            //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
            //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

            //    string deviceInfo =
            //  @"<DeviceInfo>
            //    <OutputFormat>PDF</OutputFormat>
            //    <EndPage>1</EndPage>
            //</DeviceInfo>";

                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMI");
                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                var encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT5", ds.Tables["VmiPolicyT5Table"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT5.rdlc");
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
            catch (Exception ex)
            {
                return filename;
            }
        }

        public string VmiPolicyT5_Epol(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline)
        {
            string filename = string.Empty, ls_path = string.Empty, path = string.Empty;
            //string mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
            //    DataSet ds = getVmipolicyT5(yr, brcode, polno);
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }
            //    ds.Tables[0].Rows[0]["isCopy"] = isCopy;
            //    ds.Tables[0].Rows[0]["isOnline"] = isOnline;

            //    var signature = getSignature(brcode);
            //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
            //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;
            //    string deviceInfo =
            //  @"<DeviceInfo>
            //    <OutputFormat>PDF</OutputFormat>
            //    <EndPage>1</EndPage>
            //</DeviceInfo>";

                if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMIC");
                else ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "EVMI");

                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}

                string encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                //path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT5", ds.Tables[0]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("VmiPolicyT5Copy.rdlc");
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

        public string VmiPolicyT3_Epol(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline)
        {
            string filename = string.Empty, ls_path = string.Empty, path = string.Empty;
            //string mineType, encoding, filenameExtension;
            //string[] streamids;
            //Warning[] warnings;
            try
            {
            //    DataSet ds = getVmiPolicyT3(yr, brcode, polno);
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }
            //    ds.Tables[0].Rows[0]["isCopy"] = isCopy;
            //    ds.Tables[0].Rows[0]["isOnline"] = isOnline;

            //    var signature = getSignature(brcode);
            //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
            //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

            //    string deviceInfo =
            //  @"<DeviceInfo>
            //    <OutputFormat>PDF</OutputFormat>
            //    <EndPage>1</EndPage>
            //</DeviceInfo>";

                if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "VMIC");
                else ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "EVMI");

                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}

                string encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("VmiPolicyT3", ds.Tables["VmiPolicyT3Table"]);
                //rv.LocalReport.ReportPath = PathHelper.GetReportRdlcPath("VmiPolicyT3Copy.rdlc");
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

        public string CmiCopyrdlc(string yr, string brcode, string polno, string sale_code, string isCopy, string isOnline)
        {
            string filename = string.Empty;
            try
            {
            //    DataSet ds = getCMIPolicyCopy(yr, brcode, polno);
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }
            //    ds.Tables[0].Rows[0]["isCopy"] = isCopy;
            //    ds.Tables[0].Rows[0]["isOnline"] = isOnline;
            //    if (ds.Tables[0].Rows.Count == 0)
            //    {
            //        return filename;
            //    }

            //    var signature = getSignature(brcode);
            //    ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
            //    ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;
            //    string deviceInfo =
            //  @"<DeviceInfo>
            //    <OutputFormat>PDF</OutputFormat>
            //    <EndPage>1</EndPage>
            //</DeviceInfo>";

                //Save
                string ls_path, path, mineType, encoding, filenameExtension;
                if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "CMIC");
                else ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "ECMI");

                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                //string[] streamids;
                //Warning[] warnings;
                string encrypt_name = UtilityBusiness.MD5Hash("CMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("CmiPolicyCopy", ds.Tables["CmiPolicyCopyTable"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("CmiPolicyCopy.rdlc");
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

        public string Cmirdlc(string yr, string brcode, string polno, string sale_code)
        {
            string filename = string.Empty;
            try
            {
              //  DataSet ds = getCMIPolicyCopy(yr, brcode, polno);
              //  if (ds.Tables[0].Rows.Count == 0)
              //  {
              //      return filename;
              //  }

              //  var signature = getSignature(brcode);
              //  ds.Tables[0].Rows[0]["signature_picture"] = signature.signature_picture;
              //  ds.Tables[0].Rows[0]["signature_fullname"] = signature.signature_fullname;

              //  string deviceInfo =
              //  @"<DeviceInfo>
              //    <OutputFormat>PDF</OutputFormat>
              //    <EndPage>1</EndPage>
              //</DeviceInfo>";

                //Save
                string ls_path, path, mineType, encoding, filenameExtension;
                ls_path = UtilityBusiness.GetPolicyPath(brcode, sale_code, "CMI");
                //if (!Directory.Exists(ls_path))
                //{
                //    Directory.CreateDirectory(ls_path);
                //}
                //string[] streamids;
                //Warning[] warnings;
                string encrypt_name = UtilityBusiness.MD5Hash("CMIPOL" + yr.ToString() + brcode.ToString() + polno.ToString());
                filename = encrypt_name + ".pdf";
                path = ls_path + filename;
                //ReportViewer rv = new ReportViewer
                //{
                //    ProcessingMode = ProcessingMode.Local
                //};

                //ReportDataSource rds = new ReportDataSource("CmiPolicyCopy", ds.Tables["CmiPolicyCopyTable"]);
                //rv.LocalReport.ReportPath = UtilityBusiness.GetReportRdlcPath("CmiPolicy.rdlc");
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public DataSet getVmipolicyT5(string yr, string br_code, string pol_no)
        {
            string sql = "dbo.VmiPolicyT5Print";
            DataSet ds = new DataSet();

            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = sql
                };

                com.Parameters.Add(new SqlParameter("@yr", yr));
                com.Parameters.Add(new SqlParameter("@br", br_code));
                com.Parameters.Add(new SqlParameter("@pol_no", pol_no));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(ds);
                }
            }

            return ds;
        }

        public DataSet getVmiPolicyT3(string yr, string br_code, string pol_no)
        {
            string sql = "dbo.VmiPolicyT3Print";
            DataSet ds = new DataSet();

            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = sql
                };

                com.Parameters.Add(new SqlParameter("@yr", yr));
                com.Parameters.Add(new SqlParameter("@br", br_code));
                com.Parameters.Add(new SqlParameter("@pol_no", pol_no));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(ds, "VmiPolicyT3Table");
                }
            }

            return ds;
        }

        public DataSet getVmiPolicyT4(string sale_code, string yr, string br_code, string pol_no)
        {
            string sql = "dbo.VmiPolicyT4Print";
            DataSet ds = new DataSet();
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = sql
                };

                com.Parameters.Add(new SqlParameter("@sale_code", sale_code));
                com.Parameters.Add(new SqlParameter("@yr", yr));
                com.Parameters.Add(new SqlParameter("@br", br_code));
                com.Parameters.Add(new SqlParameter("@pol_no", pol_no));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(ds, "VmiPolicyT4Table");
                }
            }

            return ds;
        }

        public DataSet getCMIPolicyCopy(string yr, string br_code, string pol_no)
        {
            DataSet ds = new DataSet();
            string sql = "dbo.CmiPolicyPrint";
            DataSet dsCmi = new DataSet();

            string connectionString = _Configuration["ConnectionString:wscmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection (connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = sql
                };

                com.Parameters.Add(new SqlParameter("@yr", yr));
                com.Parameters.Add(new SqlParameter("@br", br_code));
                com.Parameters.Add(new SqlParameter("@pol_no", pol_no));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(dsCmi, "CmiPolicyCopyTable");
                }
            }

            return dsCmi;
        }

        public SignatureModel getSignature(string br_code)
        {
            DataSet ds = new DataSet();
            SignatureModel signatureModel = new SignatureModel();
            string sql = @"
                            select * from employee e
                            inner join signature_pic s on e.empcode = s.empcode
                            where s.br_code = @br_code and s.active_row = 'A'";

            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand com = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandText = sql
                };

                com.Parameters.Add(new SqlParameter("@br_code", br_code));

                using (SqlDataAdapter adap = new SqlDataAdapter(com))
                {
                    adap.Fill(ds, "SignatureDataTable");
                }
            }
            signatureModel = ds.Tables[0].AsEnumerable().Select(r => new SignatureModel()
            {
                pname = (string)r["pname"],
                fname = (string)r["fname"],
                lname = (string)r["lname"],
                br_code = (string)r["br_code"],
                empcode = (string)r["empcode"],
                position = (string)r["position"],
                signature_picture = (byte[])r["signature_picture"],
            }).FirstOrDefault();

            return signatureModel;
        }
    }
}
