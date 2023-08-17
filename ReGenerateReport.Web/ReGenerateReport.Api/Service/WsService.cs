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

        //public async Task<WsGetLinkResponse> getLinkPolicy(string policy, string sale_code)
        //{
        //    WsGetLinkResponse link = new WsGetLinkResponse();
        //    //1. Check ข้อมูล DB ว่ามีข้อมูลไหม
        //    var pol = getPolicyType(policy);
        //    link.polType = pol.polType;
        //    link.polYr = pol.polYr;
        //    link.polBr = pol.polBrCode;
        //    link.polNo = pol.polNo;

        //    var chkdb = await CheckDataDB(link);
        //    if (chkdb.StatusCode != HttpStatus.OK.ToString())
        //    {
        //        return chkdb;
        //    }

        //    //2. Check File PDF ใน Server
        //    var chkFile = await CheckFilePDF(link, sale_code, "");
        //    if (chkFile.StatusCode != HttpStatus.OK.ToString())
        //    {
        //        return chkFile;
        //    }
        //    else
        //    {
        //        link = chkFile;
        //    }

        //    //3. Check File PDF ว่าทำการ Sign
        //    link = await CheckFileSign(link, sale_code);

        //    return link;
        //}

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

        //Check ข้อมูล DB ว่ามีข้อมูลไหม
        //public async Task<WsGetLinkResponse> CheckDataDB(WsGetLinkResponse req)
        //{
        //    WsGetLinkResponse resLink = new WsGetLinkResponse();
        //    try
        //    {
        //        var data = "";
        //        if (req.polType == "VMI")
        //        {
        //            string connectionString = _Configuration["ConnectionString:wsvmimotordbConstr"];
        //            using (SqlConnection connection = new SqlConnection(connectionString))
        //            {
        //                data = connection.QueryFirst<string>($"select COUNT(*) as CountData from vmi_policy where policy_year = '{req.polYr}' and policy_branch = '{req.polBr}' and policy_no = '{req.polNo}'");
        //            }
        //        }
        //        else
        //        {
        //            string connectionString = _Configuration["ConnectionString:wscmimotordbConstr"];
        //            using (SqlConnection connection = new SqlConnection(connectionString))
        //            {
        //                data = connection.QueryFirst<string>($"select COUNT(*) as CountData from maspol_tesco where yr = '{req.polYr}' and br_code = '{req.polBr}' and pol_no = '{req.polNo}'");
        //            }
        //        }

        //        if (data == "0")
        //        {
        //            resLink.StatusCode = "404";
        //            resLink.Status = "ไม่พบข้อมูล Database";
        //            resLink.linkTax = "";
        //            resLink.linkPolicy = "";
        //        }
        //        else
        //        {
        //            resLink.StatusCode = "200";
        //            resLink.Status = "";
        //            resLink.linkTax = "";
        //            resLink.linkPolicy = "";
        //        }
        //        return resLink;
        //    }
        //    catch (Exception ex)
        //    {
        //        resLink.StatusCode = "500";
        //        resLink.Status = "Exception Check Data DB : " + ex.Message;
        //        resLink.linkTax = "";
        //        resLink.linkPolicy = "";
        //        return resLink;
        //    }
        //}

        //Check File PDF ใน Server ว่ามีอยู่ไหม
        //public async Task<WsGetLinkResponse> CheckFilePDF(WsGetLinkResponse req, string sale_code, string isCopy)
        //{
        //    WsGetLinkResponse resLink = new WsGetLinkResponse();
        //    try
        //    {
        //        string filename = string.Empty, ls_path = string.Empty, path = string.Empty, path_Sign = string.Empty;
        //        string filename_Tax = string.Empty, ls_path_Tax = string.Empty, path_Tax = string.Empty, path_Tax_Sign = string.Empty;
        //        string encrypt_name = string.Empty;
        //        if (req.polType == "VMI")
        //        {
        //            //Policy
        //            if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(req.polBr, sale_code, "VMIC");
        //            else ls_path = UtilityBusiness.GetPolicyPath(req.polBr, sale_code, "EVMI");
        //            encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + req.polYr.ToString() + req.polBr.ToString() + req.polNo.ToString());
        //            filename = encrypt_name + ".pdf";
        //            path = ls_path + filename;
        //            path_Sign = ls_path + "Sign_" + filename;

        //            //Tax
        //            ls_path_Tax = UtilityBusiness.GetTaxinvoicePath(req.polBr, sale_code, "EVMI");

        //            var encrypt_name_Tax = UtilityBusiness.MD5Hash("VMITAX" + req.polYr.ToString() + req.polBr.ToString() + req.polNo.ToString());
        //            filename_Tax = encrypt_name_Tax + ".pdf";
        //            path_Tax = ls_path_Tax + filename_Tax;
        //            path_Tax_Sign = ls_path_Tax + "Sign_" + filename_Tax;

        //            if (!File.Exists(path))
        //            {
        //                if (!File.Exists(path_Sign))
        //                {
        //                    resLink.StatusCode = "404";
        //                    resLink.Status = "ไม่พบ File Policy";
        //                    resLink.linkTax = "";
        //                    resLink.linkPolicy = "";
        //                    return resLink;
        //                }
        //            }

        //            if (!File.Exists(path_Tax))
        //            {
        //                if (!File.Exists(path_Tax_Sign))
        //                {
        //                    resLink.StatusCode = "404";
        //                    resLink.Status = "ไม่พบ File Tax";
        //                    resLink.linkTax = "";
        //                    resLink.linkPolicy = "";
        //                    return resLink;
        //                }
        //            }

        //            resLink.polType = req.polType;
        //            resLink.polYr = req.polYr;
        //            resLink.polBr = req.polBr;
        //            resLink.polNo = req.polNo;
        //            resLink.StatusCode = "200";
        //            resLink.FileNamePolicy = filename;
        //            resLink.FileNameTax = filename_Tax;
        //            resLink.Status = "";
        //            resLink.linkTax = "";
        //            resLink.linkPolicy = "";
        //        }
        //        else if (req.polType == "CMI")
        //        {

        //        }

        //        return resLink;
        //    }
        //    catch (Exception ex)
        //    {
        //        resLink.StatusCode = "500";
        //        resLink.Status = "Exception Check File PDF : " + ex.Message;
        //        resLink.linkTax = "";
        //        resLink.linkPolicy = "";
        //        return resLink;
        //    }
        //}

        //Check File PDF ว่าทำการ Sign หรือยัง
        //public async Task<WsGetLinkResponse> CheckFileSign(WsGetLinkResponse req, string sale_code)
        //{
        //    WsGetLinkResponse resLink = new WsGetLinkResponse();
        //    try
        //    {
        //        if (req.polType == "VMI")
        //        {
        //            string originalPath = _absoluteUriService.GetAbsoluteUri(); // new Uri(HttpContextAccessor.Current.Request.Url.AbsoluteUri).OriginalString;
        //            string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));
        //            string policyUrl = string.Empty, receiptUrl = string.Empty;

        //            //string parentDirectory = originalPath.Substring(0, 5) == "https" ? originalPath.Substring(0, originalPath.LastIndexOf("/")) : originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("http", "https");
        //            policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy;
        //            receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax;

        //            string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

        //            //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNamePolicy);
        //            //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy);
        //            pathToFilespdf = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNamePolicy;
        //            pathToFilespdfSign = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy;

        //            if (File.Exists(pathToFilespdfSign))
        //            {
        //                //ข้อมูล Sign เรียบร้อย
        //                resLink.linkPolicy = policyUrl;
        //            }
        //            else
        //            {
        //                if (File.Exists(pathToFilespdf))
        //                {
        //                    SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, req.polYr + req.polBr + "/กธ/" + req.polNo);
        //                    resLink.linkPolicy = policyUrl;
        //                }
        //            }

        //            //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNameTax);
        //            //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax);

        //            pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNameTax;
        //            pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax;

        //            if (File.Exists(pathToFilestaxSign))
        //            {
        //                //ข้อมูล Sign เรียบร้อย
        //                resLink.linkTax = receiptUrl;
        //            }
        //            else
        //            {
        //                if (File.Exists(pathToFilestax))
        //                {
        //                    SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, req.polBr + "-" + req.polNo + "/" + req.polYr);
        //                    resLink.linkTax = receiptUrl;
        //                }
        //            }

        //            resLink.StatusCode = "200";
        //            resLink.Status = "OK";
        //        }
        //        else if (req.polType == "CMI")
        //        {

        //        }



        //        return resLink;
        //    }
        //    catch (Exception ex)
        //    {
        //        resLink.StatusCode = "500";
        //        resLink.Status = "Exception File PDF Sign : " + ex.Message;
        //        resLink.linkTax = "";
        //        resLink.linkPolicy = "";
        //        return resLink;
        //    }
        //}


        ///
        public async Task<WsGetLinkResponse> PolicyVMIType3(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();
            var dataYear = string.Empty;
            var dataMonth = string.Empty;
            var dataDay = string.Empty;

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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

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
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
            }
            else
            {
                filename = _policyReportService.VmiPolicyT3(polyr, polbr, polno, sale_code);

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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                    link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                    return link;
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

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
                    link.StatusCode = HttpStatus.NotFound.ToString();
                    link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                    return link;
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = "~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = "~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = "~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

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
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = "~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = "~/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = "~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

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
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                pathToFilespdf = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                pathToFilespdfSign = "~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                        return link;
                    }
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;
                pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax;

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
                        link.StatusCode = HttpStatus.NotFound.ToString();
                        link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                        return link;
                    }
                }

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
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
                    string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                    string policyUrl = parentDirectory + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;
                    string receiptUrl = parentDirectory + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/Sign_" + filename_tax;

                    string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                    //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename);
                    //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename);
                    pathToFilespdf = "~" + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                    pathToFilespdfSign = "~" + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename;

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
                            link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Policy File PDF " + polno;
                            return link;
                        }
                    }

                    //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename_tax);
                    //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/Sign_" + filename_tax);
                    pathToFilestax = "~"+ parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/" + filename_tax;
                    pathToFilestaxSign = "~"+ parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/Sign_" + filename_tax;

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
                                link.StatusCode = HttpStatus.NotFound.ToString();
                                link.Status = "เกิดความผิดพลาดทางระบบ : Not Found Tax File PDF " + polno;
                                return link;
                            }
                        }
                    }

                    link.StatusCode = HttpStatus.OK.ToString();
                    link.Status = "OK";
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
                    string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));

                    string policyUrl = parentDirectory + parentDirectory_path + dataYear + "/" + polbr + "/" + sale_code + "/" + dataMonth + "/" + dataDay + "/" + filename;
                    string receiptUrl = parentDirectory + parentDirectory_path_Tax + dataYearTax + "/" + polbr + "/" + sale_code + "/" + dataMonthTax + "/" + dataDayTax + "/" + filename_tax;

                    link.StatusCode = HttpStatus.OK.ToString();
                    link.Status = "OK";
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
