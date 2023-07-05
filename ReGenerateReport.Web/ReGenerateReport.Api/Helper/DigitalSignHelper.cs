using ReGenerateReport.Api.LogApiAnalytic;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public class DigitalSignHelper
    {
        public static async Task<SignPdfResponse> InvokeESign(Byte[] fileImage, SpGetDataPolicyNonMotor requestSignReport, Guid logId, DateTime dateLog)
        {
            try
            {
                SignPdfResponse signPdfResponse = new SignPdfResponse();

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start sign document : InvokeESign");

                //Convert Byte Array To Base64String
                var fileContent = Convert.ToBase64String(fileImage, 0, fileImage.Length);
                string password = string.Empty;

                //Binding Parameter ESing
                ESignRequest requestData = new ESignRequest
                {
                    document = fileContent,
                    documentType = DocumentType.PDF,
                    referenceId = requestSignReport.PolicyNo,
                    password = password
                };

                //Call VIRESignDocument
                var reponse = await SignHelper.ESignDocument(requestData, logId, dateLog);

                if (reponse.status == "success")
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    signPdfResponse.signedPdf = reponse.result.document;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document successful");
                }
                else
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + reponse.statusDescription);
                }

                return signPdfResponse;
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + ex.Message);
                throw ex;
            }
        }

        public static async Task<SignPdfResponse> InvokeESignVmi(Byte[] fileImage, SpGetDataPolicyVmi requestSignReport, Guid logId, DateTime dateLog)
        {
            try
            {
                SignPdfResponse signPdfResponse = new SignPdfResponse();

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start sign document : InvokeESign");

                //Convert Byte Array To Base64String
                var fileContent = Convert.ToBase64String(fileImage, 0, fileImage.Length);
                string password = string.Empty;

                //Binding Parameter ESing
                ESignRequest requestData = new ESignRequest
                {
                    document = fileContent,
                    documentType = DocumentType.PDF,
                    referenceId = requestSignReport.PolicyNo,
                    password = password
                };

                //Call VIRESignDocument
                var reponse = await SignHelper.ESignDocument(requestData, logId, dateLog);

                if (reponse.status == "success")
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    signPdfResponse.signedPdf = reponse.result.document;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document successful");
                }
                else
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + reponse.statusDescription);
                }

                return signPdfResponse;
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + ex.Message);
                throw ex;
            }
        }

        public static async Task<SignPdfResponse> InvokeESignCmi(Byte[] fileImage, SpGetDataPolicyCmi requestSignReport, Guid logId, DateTime dateLog)
        {
            try
            {
                SignPdfResponse signPdfResponse = new SignPdfResponse();

                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"Start sign document : InvokeESign");

                //Convert Byte Array To Base64String
                var fileContent = Convert.ToBase64String(fileImage, 0, fileImage.Length);
                string password = string.Empty;

                //Binding Parameter ESing
                ESignRequest requestData = new ESignRequest
                {
                    document = fileContent,
                    documentType = DocumentType.PDF,
                    referenceId = requestSignReport.PolicyNo,
                    password = password
                };

                //Call VIRESignDocument
                var reponse = await SignHelper.ESignDocument(requestData, logId, dateLog);

                if (reponse.status == "success")
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    signPdfResponse.signedPdf = reponse.result.document;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document successful");
                }
                else
                {
                    signPdfResponse.respStatus = reponse.status;
                    signPdfResponse.respDesc = reponse.statusDescription;
                    HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + reponse.statusDescription);
                }

                return signPdfResponse;
            }
            catch (Exception ex)
            {
                HelperLogFile.CreateLog(dateLog, DateTime.Now.ToString(), logId != Guid.Empty ? logId.ToString() : null, null, null, null, $"InvokeESign document fail : " + ex.Message);
                throw ex;
            }
        }
    }
}
