using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ReGenerateReport.Api.Helper;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public class WsService : IWsService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _Configuration;
        private readonly IAbsoluteUriService _absoluteUriService;
        private readonly IPolicyReportService _policyReportService;
        private readonly ITaxinvoiceService _taxinvoiceService;

        public WsService(IHttpContextAccessor httpContextAccessor,
                         IConfiguration configuration,
                         IAbsoluteUriService absoluteUriService,
                         IPolicyReportService policyReportService,
                         ITaxinvoiceService taxinvoiceService)
        {
            _httpContextAccessor = httpContextAccessor;
            _Configuration = configuration;
            _absoluteUriService = absoluteUriService;
            _policyReportService = policyReportService;
            _taxinvoiceService = taxinvoiceService;
        }

        public GetPolicy getPolicyType(string policy)
        {
            GetPolicy pol = new GetPolicy();
            string[] policyS = policy.Split("/กธ/");
            if (policyS[policyS.Count() - 1].Length == 6)
            {
                pol.polType = "VMI";
                pol.polNo = policyS[policyS.Count() - 1];
                pol.polYr = policyS[0].Substring(0, 2);
                pol.polBrCode = policyS[0].Substring(2, 3);

                //Get Value Code
                string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var getDataCode = connection.QueryFirst($"select a.sale_code, LEFT(policy_type,1) as policy_type, am.channel_code, a.flag_online  " +
                                                            $" from vmi_policy v left join applicants a ON v.apply_year = a.apply_year and v.apply_branch = a.apply_branch and v.apply_no = a.apply_no " +
                                                            $"                   right join applicants_motors am ON a.apply_year = am.apply_year and a.apply_branch = am.apply_branch and a.apply_no = am.apply_no " +
                                                            $" where policy_year = '{pol.polYr}' and policy_branch = '{pol.polBrCode}' and policy_no = '{pol.polNo}'");
                    if (getDataCode != "")
                    {
                        pol.saleCode = $"{getDataCode.sale_code}";
                        pol.productClass = $"{getDataCode.policy_type}";
                        pol.channelCode = $"{getDataCode.channel_code}";
                        pol.flagOnline = $"{getDataCode.flag_online}";
                    }
                }
            }
            else
            {
                pol.polType = "CMI";
                pol.polNo = policyS[policyS.Count() - 1];
                pol.polYr = policyS[0].Substring(0, 2);
                pol.polBrCode = policyS[0].Substring(2, 3);

                //Get Value Code
                string connectionString = _Configuration["ConnectionString:wscmimotordbConstr"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var getDataCode = connection.QueryFirst($"select sale_code, channel_code, flag_Online as flag_online  " +
                                                            $" from maspol_tesco " +
                                                            $" where yr = '{pol.polYr}' and br_code = '{pol.polBrCode}' and pol_no = '{pol.polNo}'");
                    if (getDataCode != "")
                    {
                        pol.saleCode = $"{getDataCode.sale_code}";
                        pol.productClass = "";
                        pol.channelCode = $"{getDataCode.channel_code}";
                        pol.flagOnline = $"{getDataCode.flag_online}";
                    }
                }
            }
            return pol;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType3(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM vmi_policy " +
                                                              $" WHERE policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            //2. Check File PDF ใน Server
            if (flagOnline == "2" || flagOnline == "5")
            {
                if (channel == "001" || channel == "002")
                {
                    filename = _policyReportService.VmiPolicyT3_Epol(polyr, polbr, polno, sale_code, "false", "true");
                }
                else
                {
                    filename = _policyReportService.VmiPolicyT3_Epol(polyr, polbr, polno, sale_code, "false", "false");
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "true");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                if (File.Exists(pathToFilespdfSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkPolicy = policyUrl;
                }
                else
                {
                    if (File.Exists(pathToFilespdf))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                        link.linkPolicy = policyUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                if (File.Exists(pathToFilestaxSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkTax = receiptUrl;
                }
                else
                {
                    if (File.Exists(pathToFilestax))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                        link.linkTax = receiptUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.OK.ToString();
                        link.Status = "Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
            }
            else
            {
                filename = _policyReportService.VmiPolicyT3(polyr, polbr, polno, sale_code);

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "Cannot create file policy " + polno;
                    return link;
                }

                if (sale_code == "15350")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false");
                }
                else if (sale_code == "04580" || sale_code == "04473" || sale_code == "16438")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false", false);
                }
                else
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "false");
                }

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType4(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM vmi_policy " +
                                                              $" WHERE policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            //2. Check File PDF ใน Server
            if (flagOnline == "2" || flagOnline == "5")
            {
                if (channel == "001" || channel == "002")
                {
                    filename = _policyReportService.VmiPolicyT4Copy(out string barcode, polyr, polbr, polno, sale_code, "false", "true");
                }
                else
                {
                    filename = _policyReportService.VmiPolicyT4Copy(out string barcode, polyr, polbr, polno, sale_code, "false", "false");
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "true");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                if (File.Exists(pathToFilespdfSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkPolicy = policyUrl;
                }
                else
                {
                    if (File.Exists(pathToFilespdf))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                        link.linkPolicy = policyUrl;
                    }
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Not Found Policy File PDF " + polno;
                    return link;
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                if (File.Exists(pathToFilestaxSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkTax = receiptUrl;
                }
                else
                {
                    if (File.Exists(pathToFilestax))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                        link.linkTax = receiptUrl;
                    }
                    link.StatusCode = HttpStatus.OK.ToString();
                    link.Status = "Not Found Tax File PDF " + polno;
                    return link;
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
            }
            else
            {
                filename = _policyReportService.VmiPolicyT4(out string barcode, polyr, polbr, polno, sale_code);

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                if (sale_code == "15350")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false");
                }
                else if (sale_code == "04580" || sale_code == "04473" || sale_code == "16438")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false", false);
                }
                else
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "false");
                }

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType4Copy(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM vmi_policy " +
                                                              $" WHERE policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            //2. Check File PDF ใน Server
            if (flagOnline == "2")
            {
                if (channel == "001" || channel == "002")
                {
                    filename = _policyReportService.VmiPolicyT4Copy(out string barcode, polyr, polbr, polno, sale_code, "true", "true");
                }
                else
                {
                    filename = _policyReportService.VmiPolicyT4Copy(out string barcode, polyr, polbr, polno, sale_code, "true", "false");
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiCopyRdlc(polyr, polbr, polno, sale_code, "true");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = reportPath + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = reportPath + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                if (File.Exists(pathToFilespdfSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkPolicy = policyUrl;
                }
                else
                {
                    if (File.Exists(pathToFilespdf))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                        link.linkPolicy = policyUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = reportPath + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = reportPath + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                if (File.Exists(pathToFilestaxSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkTax = receiptUrl;
                }
                else
                {
                    if (File.Exists(pathToFilestax))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                        link.linkTax = receiptUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.OK.ToString();
                        link.Status = "Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
            }
            else
            {
                filename = _policyReportService.VmiPolicyT4Copy(out string barcode, polyr, polbr, polno, sale_code, "true", "false");

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiCopyRdlc(polyr, polbr, polno, sale_code, "false");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType5noPrint(string polyr, string polbr, string polno, string sale_code, string flagOnline, string productClass)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM vmi_policy " +
                                                              $" WHERE policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            //2. Check File PDF ใน Server
            if (flagOnline == "2")
            {
                if (productClass == "5")
                {
                    filename = _policyReportService.VmiPolicyT5_Epol(polyr, polbr, polno, sale_code, "true", "true");
                }
                else
                {
                    filename = _policyReportService.VmiPolicyT3_Epol(polyr, polbr, polno, sale_code, "true", "true");
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiCopyRdlc(polyr, polbr, polno, sale_code, "true");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = reportPath + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = reportPath + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                if (File.Exists(pathToFilespdfSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkPolicy = policyUrl;
                }
                else
                {
                    if (File.Exists(pathToFilespdf))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                        link.linkPolicy = policyUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = reportPath + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = reportPath + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                if (File.Exists(pathToFilestaxSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkTax = receiptUrl;
                }
                else
                {
                    if (File.Exists(pathToFilestax))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                        link.linkTax = receiptUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.OK.ToString();
                        link.Status = "Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
            }
            else
            {
                if (productClass == "5")
                {
                    filename = "";
                }
                else
                {
                    filename = "";
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = "";

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType5(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM vmi_policy " +
                                                              $" WHERE policy_year = '{polyr}' and policy_branch = '{polbr}' and policy_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            //2. Check File PDF ใน Server
            if (flagOnline == "2" || flagOnline == "5")
            {
                if (channel == "001" || channel == "002")
                {
                    filename = _policyReportService.VmiPolicyT5_Epol(polyr, polbr, polno, sale_code, "false", "true");
                }
                else
                {
                    filename = _policyReportService.VmiPolicyT5_Epol(polyr, polbr, polno, sale_code, "false", "false");
                }

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "true");

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = reportPath + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                if (File.Exists(pathToFilespdfSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkPolicy = policyUrl;
                }
                else
                {
                    if (File.Exists(pathToFilespdf))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                        link.linkPolicy = policyUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = reportPath + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                if (File.Exists(pathToFilestaxSign))
                {
                    //ข้อมูล Sign เรียบร้อย
                    link.linkTax = receiptUrl;
                }
                else
                {
                    if (File.Exists(pathToFilestax))
                    {
                        SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                        link.linkTax = receiptUrl;
                    }
                    else
                    {
                        link.StatusCode = HttpStatus.OK.ToString();
                        link.Status = "Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
            }
            else
            {
                filename = _policyReportService.VmiPolicyT5(polyr, polbr, polno, sale_code);

                if (string.IsNullOrEmpty(filename))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                    return link;
                }

                if (sale_code == "15350")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false");
                }
                else if (sale_code == "04580" || sale_code == "04473" || sale_code == "16438")
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiStickerRdlc(polyr, polbr, polno, sale_code, "false", false);
                }
                else
                {
                    filename_tax = _taxinvoiceService.TaxInvoiceVmiRdlc(polyr, polbr, polno, sale_code, "false");
                }

                if (string.IsNullOrEmpty(filename_tax))
                {
                    link.StatusCode = "300";
                    link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                    return link;
                }

                string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "Successfully";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyCMI(string actionName, string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var cmi_taxno = string.Empty;
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

            var dataYearTax = string.Empty;
            var dataMonthTax = string.Empty;
            var dataDayTax = string.Empty;

            //Get Report Path 
            string reportPath = _Configuration["Report:reportpath"];

            //Replace Name API   
            string replaceApi = _Configuration["ProjectName"];

            //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
            string connectionString = _Configuration["ConnectionString:wscmimotordbConstr"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var chkDataDB = connection.QueryFirst<string>($"select COUNT(*) as CountData from maspol_tesco where yr = '{polyr}' and br_code = '{polbr}' and pol_no = '{polno}'");
                if (chkDataDB == "0")
                {
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "Data Not Found In Database";
                    return link;
                }
            }

            // Check ข้อมูล Tax
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataTaxno = connection.QueryFirst<string>($"select cmi_taxno from cmi_taxinvoice where yr = '{polyr}' and br_code = '{polbr}' and pol_no = '{polno}'");
                if (dataTaxno != null)
                {
                    cmi_taxno = dataTaxno;
                }
            }

            //Get Day Month Year Policy
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDate = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,insert_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,insert_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,insert_date))),2) as dataday" +
                                                              $" FROM maspol_tesco " +
                                                              $" WHERE yr = '{polyr}' and br_code = '{polbr}' and pol_no = '{polno}'");
                if (dataDate != "")
                {
                    dataYear = $"{dataDate.datayear}";
                    dataMonth = $"{dataDate.datamonth}";
                    dataDay = $"{dataDate.dataday}";
                }
            }

            //Get Day Month Year Tax
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var dataDateTax = connection.QueryFirst($"SELECT RIGHT(CONVERT(VARCHAR(4), YEAR(DATEADD(YEAR,543,req_date))),2) as datayear, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),MONTH(DATEADD(YEAR,543,req_date))),2) as datamonth, " +
                                                              $" RIGHT('0'+CONVERT(VARCHAR(2),DAY(DATEADD(YEAR,543,req_date))),2) as dataday" +
                                                              $" FROM cmi_taxinvoice " +
                                                              $" WHERE yr = '{polyr}' and br_code = '{polbr}' and pol_no = '{polno}'");
                if (dataDateTax != "")
                {
                    dataYearTax = $"{dataDateTax.datayear}";
                    dataMonthTax = $"{dataDateTax.datamonth}";
                    dataDayTax = $"{dataDateTax.dataday}";
                }
            }

            var filename = string.Empty;
            var filename_tax = string.Empty;
            var isCopy = string.Empty;
            var parentDirectory_path = string.Empty;
            var parentDirectory_path_Tax = string.Empty;

            try
            {
                if (actionName == "PolicyCMInoPrint")
                {
                    if (flagOnline == "2")
                    {
                        isCopy = "false";
                        parentDirectory_path = "/report/EPolicyFormCMI/";
                    }
                    else
                    {
                        isCopy = "true";
                        parentDirectory_path = "/report/PolicyFormCMICopy/";
                    }
                }
                else if (actionName == "PolicyCMI")
                {
                    if (flagOnline == "2" || flagOnline == "5")
                    {
                        isCopy = "false";
                        parentDirectory_path = "/report/EPolicyFormCMI/";
                    }
                    else
                    {
                        isCopy = "true";
                        parentDirectory_path = "/report/PolicyFormCMI/";
                    }
                }

                if (flagOnline == "2" || flagOnline == "5")
                {
                    if (channel == "001" || channel == "002")
                    {
                        filename = _policyReportService.CmiCopyrdlc(polyr, polbr, polno, sale_code, isCopy, "true");
                    }
                    else
                    {
                        filename = _policyReportService.CmiCopyrdlc(polyr, polbr, polno, sale_code, isCopy, "false");
                    }

                    if (string.IsNullOrEmpty(filename))
                    {
                        link.StatusCode = "300";
                        link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                        return link;
                    }

                    if (flagOnline == "2")
                    {
                        parentDirectory_path_Tax = "/report/ETaxinvoiceFormCMI/";
                        filename_tax = _taxinvoiceService.TaxinvoiceCmiRdlc(polyr, cmi_taxno, sale_code, "true");
                    }
                    else
                    {
                        parentDirectory_path_Tax = "/report/ETaxinvoiceFormCMI/";
                        filename_tax = _taxinvoiceService.TaxinvoiceCmiRdlc(polyr, cmi_taxno, sale_code, "false");
                    }

                    if (string.IsNullOrEmpty(filename_tax))
                    {
                        link.StatusCode = "300";
                        link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file taxinvoice " + polno;
                        return link;
                    }

                    string originalPath = _absoluteUriService.GetAbsoluteUri();  //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                    string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                    string policyUrl = parentDirectory + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                    string receiptUrl = parentDirectory + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/Sign_" + filename_tax;

                    string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                    //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                    //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                    pathToFilespdf = reportPath + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                    pathToFilespdfSign = reportPath + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

                    if (File.Exists(pathToFilespdfSign))
                    {
                        //ข้อมูล Sign เรียบร้อย
                        link.linkPolicy = policyUrl;
                    }
                    else
                    {
                        if (File.Exists(pathToFilespdf))
                        {
                            SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, polyr + polbr + "/กธ/" + polno);
                            link.linkPolicy = policyUrl;
                        }
                        else
                        {
                            link.StatusCode = HttpStatus.NotFound.ToString();
                            link.Status = "Not Found Policy File PDF " + polno;
                            return link;
                        }
                    }

                    //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                    //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                    pathToFilestax = reportPath + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/" + filename_tax;
                    pathToFilestaxSign = reportPath + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/Sign_" + filename_tax;

                    if (string.IsNullOrEmpty(cmi_taxno))
                    {
                        link.linkTax = "";
                    }
                    else
                    {
                        if (File.Exists(pathToFilestaxSign))
                        {
                            //ข้อมูล Sign เรียบร้อย
                            link.linkTax = receiptUrl;
                        }
                        else
                        {
                            if (File.Exists(pathToFilestax))
                            {
                                SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, polbr + "-" + polno + "/" + polyr);
                                link.linkTax = receiptUrl;
                            }
                            else
                            {
                                link.StatusCode = HttpStatus.OK.ToString();
                                link.Status = "Not Found Tax File PDF " + polno;
                                return link;
                            }
                        }
                    }

                    link.StatusCode = HttpStatus.OK.ToString();
                    link.Status = "Successfully";
                    return link;
                }
                else
                {
                    if (actionName == "PolicyCMInoPrint")
                    {
                        filename = _policyReportService.CmiCopyrdlc(polyr, polbr, polno, sale_code, "true", "false");
                    }
                    else if(actionName == "PolicyCMI")
                    {
                        if (sale_code != "08582")
                        {
                            filename = _policyReportService.Cmirdlc(polyr, polbr, polno, sale_code);
                        }
                        else
                        {
                            filename = "";
                        }
                    }

                    if (string.IsNullOrEmpty(filename))
                    {
                        link.StatusCode = "300";
                        link.Status = "เกิดความผิดพลาดทางระบบ : Cannot create file policy " + polno;
                        return link;
                    }

                    if (flagOnline == "2")
                    {
                        parentDirectory_path_Tax = "/report/ETaxinvoiceFormCMI/";
                        filename_tax = _taxinvoiceService.TaxinvoiceCmiRdlc(polyr, cmi_taxno, sale_code, "true");
                    }
                    else
                    {
                        parentDirectory_path_Tax = "/report/ETaxinvoiceFormCMI/";
                        filename_tax = _taxinvoiceService.TaxinvoiceCmiRdlc(polyr, cmi_taxno, sale_code, "false");
                    }

                    string originalPath = _absoluteUriService.GetAbsoluteUri();   //new Uri(HttpContext.Current.Request.Url.AbsoluteUri).OriginalString;
                    string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("api", replaceApi);

                    string policyUrl = parentDirectory + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                    string receiptUrl = parentDirectory + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/" + filename_tax;

                    link.StatusCode = HttpStatus.OK.ToString();
                    link.Status = "Successfully";
                    link.linkPolicy = policyUrl;
                    link.linkTax = receiptUrl;
                    return link;
                }
            }
            catch (Exception ex)
            {
                link.StatusCode = HttpStatus.InternalServerError.ToString();
                link.Status = "Exception : " + ex.Message;
                link.linkPolicy = "";
                link.linkTax = "";
                return link;
            }
        }
    }
}
