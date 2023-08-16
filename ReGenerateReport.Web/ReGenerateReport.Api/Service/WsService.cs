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
            }
            else
            {
                pol.polType = "CMI";
                pol.polNo = policyS[policyS.Count() - 1];
                pol.polYr = policyS[0].Substring(0, 2);
                pol.polBrCode = policyS[0].Substring(2, 3);
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
        public async Task<WsGetLinkResponse> CheckFilePDF(WsGetLinkResponse req, string sale_code, string isCopy)
        {
            WsGetLinkResponse resLink = new WsGetLinkResponse();
            try
            {
                string filename = string.Empty, ls_path = string.Empty, path = string.Empty, path_Sign = string.Empty;
                string filename_Tax = string.Empty, ls_path_Tax = string.Empty, path_Tax = string.Empty, path_Tax_Sign = string.Empty;
                string encrypt_name = string.Empty;
                if (req.polType == "VMI")
                {
                    //Policy
                    if (isCopy == "true") ls_path = UtilityBusiness.GetPolicyPath(req.polBr, sale_code, "VMIC");
                    else ls_path = UtilityBusiness.GetPolicyPath(req.polBr, sale_code, "EVMI");
                    encrypt_name = UtilityBusiness.MD5Hash("VMIPOL" + req.polYr.ToString() + req.polBr.ToString() + req.polNo.ToString());
                    filename = encrypt_name + ".pdf";
                    path = ls_path + filename;
                    path_Sign = ls_path + "Sign_" + filename;

                    //Tax
                    ls_path_Tax = UtilityBusiness.GetTaxinvoicePath(req.polBr, sale_code, "EVMI");

                    var encrypt_name_Tax = UtilityBusiness.MD5Hash("VMITAX" + req.polYr.ToString() + req.polBr.ToString() + req.polNo.ToString());
                    filename_Tax = encrypt_name_Tax + ".pdf";
                    path_Tax = ls_path_Tax + filename_Tax;
                    path_Tax_Sign = ls_path_Tax + "Sign_" + filename_Tax;

                    if (!File.Exists(path))
                    {
                        if (!File.Exists(path_Sign))
                        {
                            resLink.StatusCode = "404";
                            resLink.Status = "ไม่พบ File Policy";
                            resLink.linkTax = "";
                            resLink.linkPolicy = "";
                            return resLink;
                        }
                    }

                    if (!File.Exists(path_Tax))
                    {
                        if (!File.Exists(path_Tax_Sign))
                        {
                            resLink.StatusCode = "404";
                            resLink.Status = "ไม่พบ File Tax";
                            resLink.linkTax = "";
                            resLink.linkPolicy = "";
                            return resLink;
                        }
                    }

                    resLink.polType = req.polType;
                    resLink.polYr = req.polYr;
                    resLink.polBr = req.polBr;
                    resLink.polNo = req.polNo;
                    resLink.StatusCode = "200";
                    resLink.FileNamePolicy = filename;
                    resLink.FileNameTax = filename_Tax;
                    resLink.Status = "";
                    resLink.linkTax = "";
                    resLink.linkPolicy = "";
                }
                else if (req.polType == "CMI")
                {

                }

                return resLink;
            }
            catch (Exception ex)
            {
                resLink.StatusCode = "500";
                resLink.Status = "Exception Check File PDF : " + ex.Message;
                resLink.linkTax = "";
                resLink.linkPolicy = "";
                return resLink;
            }
        }

        //Check File PDF ว่าทำการ Sign หรือยัง
        public async Task<WsGetLinkResponse> CheckFileSign(WsGetLinkResponse req, string sale_code)
        {
            WsGetLinkResponse resLink = new WsGetLinkResponse();
            try
            {
                if (req.polType == "VMI")
                {
                    string originalPath = _absoluteUriService.GetAbsoluteUri(); // new Uri(HttpContextAccessor.Current.Request.Url.AbsoluteUri).OriginalString;
                    string parentDirectory = originalPath.Substring(0, originalPath.LastIndexOf("/"));
                    string policyUrl = string.Empty, receiptUrl = string.Empty;

                    //string parentDirectory = originalPath.Substring(0, 5) == "https" ? originalPath.Substring(0, originalPath.LastIndexOf("/")) : originalPath.Substring(0, originalPath.LastIndexOf("/")).Replace("http", "https");
                    policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy;
                    receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax;

                    string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                    //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNamePolicy);
                    //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy);
                    pathToFilespdf = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNamePolicy;
                    pathToFilespdfSign = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNamePolicy;

                    if (File.Exists(pathToFilespdfSign))
                    {
                        //ข้อมูล Sign เรียบร้อย
                        resLink.linkPolicy = policyUrl;
                    }
                    else
                    {
                        if (File.Exists(pathToFilespdf))
                        {
                            SignHelperWarpper.SignPdfToFile(pathToFilespdf, pathToFilespdfSign, string.Empty, req.polYr + req.polBr + "/กธ/" + req.polNo);
                            resLink.linkPolicy = policyUrl;
                        }
                    }

                    //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNameTax);
                    //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax);

                    pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + req.FileNameTax;
                    pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + req.polBr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + req.FileNameTax;

                    if (File.Exists(pathToFilestaxSign))
                    {
                        //ข้อมูล Sign เรียบร้อย
                        resLink.linkTax = receiptUrl;
                    }
                    else
                    {
                        if (File.Exists(pathToFilestax))
                        {
                            SignHelperWarpper.SignPdfToFile(pathToFilestax, pathToFilestaxSign, string.Empty, req.polBr + "-" + req.polNo + "/" + req.polYr);
                            resLink.linkTax = receiptUrl;
                        }
                    }

                    resLink.StatusCode = "200";
                    resLink.Status = "OK";
                }
                else if (req.polType == "CMI")
                {

                }



                return resLink;
            }
            catch (Exception ex)
            {
                resLink.StatusCode = "500";
                resLink.Status = "Exception File PDF Sign : " + ex.Message;
                resLink.linkTax = "";
                resLink.linkPolicy = "";
                return resLink;
            }
        }


        ///
        public async Task<WsGetLinkResponse> PolicyVMIType5noPrint(string polyr, string polbr, string polno, string sale_code, string flagOnline, string productClass)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();

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

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename);
                pathToFilespdf = "~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                pathToFilespdfSign = "~/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;

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
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;
                pathToFilestaxSign = "~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

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

                string policyUrl = parentDirectory + "/report/PolicyFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;

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

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename);
                pathToFilespdf = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                pathToFilespdfSign = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;

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
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;
                pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

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

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }

        public async Task<WsGetLinkResponse> PolicyVMIType3(string polyr, string polbr, string polno, string sale_code, string flagOnline, string channel)
        {
            WsGetLinkResponse link = new WsGetLinkResponse();

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

                string policyUrl = parentDirectory + "/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;
                string receiptUrl = parentDirectory + "/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

                string pathToFilespdf = string.Empty, pathToFilespdfSign = string.Empty, pathToFilestax = string.Empty, pathToFilestaxSign = string.Empty;

                //pathToFilespdf = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename);
                //pathToFilespdfSign = HttpContext.Current.Server.MapPath("~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename);
                pathToFilespdf = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                pathToFilespdfSign = "~/report/EPolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename;

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
                }

                //pathToFilestax = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax);
                //pathToFilestaxSign = HttpContext.Current.Server.MapPath("~/report/TaxinvoiceFormVMICopy/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax);
                pathToFilestax = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;
                pathToFilestaxSign = "~/report/ETaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/Sign_" + filename_tax;

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

                string policyUrl = parentDirectory + "/report/PolicyFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename;
                string receiptUrl = parentDirectory + "/report/TaxinvoiceFormVMI/" + DateHelper.GetCurYear() + "/" + polbr + "/" + sale_code + "/" + DateHelper.GetCurMonth() + "/" + DateHelper.GetCurDay() + "/" + filename_tax;

                link.StatusCode = HttpStatus.OK.ToString();
                link.Status = "OK";
                link.linkPolicy = policyUrl;
                link.linkTax = receiptUrl;
            }

            return link;
        }
    }
}
