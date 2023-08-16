using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public class SignHelperWarpper
    {
        public static void SignPdfToFile(string pathNonSign, string pathSigned, string signPassword, string referenceId)
        {
            string step = "Start";
            try
            {
                ESignRequest req = new ESignRequest()
                {
                    documentType = DocumentType.PDF,
                    referenceId = referenceId,
                    password = string.Empty
                };
                if (!string.IsNullOrEmpty(signPassword) && !string.IsNullOrWhiteSpace(signPassword))
                {
                    req.password = signPassword;
                }

                step = "Read pathNonSign";
                byte[] document_bytes = File.ReadAllBytes(pathNonSign);
                req.document = Convert.ToBase64String(document_bytes);

                step = "Send SignDocument";
                ESignResponse res = SignHelper.ESignDocument(req);
                if (res.statusCode == 200)
                {
                    step = "Convert pdf sign base 64 to byte";
                    byte[] res_bytes = Convert.FromBase64String(res.result.document);

                    step = "write pathSigned";
                    File.WriteAllBytes(pathSigned, res_bytes);

                    if (File.Exists(pathNonSign))
                    {
                        step = "Clear Non Sign";
                        File.Delete(pathNonSign);
                    }
                    else
                    {
                        step = "Not Clear Non Sign";
                    }
                }
                else
                {
                    throw new Exception("Error Ref Id:" + referenceId + " ,Response:" + res.statusDescription);
                }
            }
            catch (Exception ex)
            {
                throw ex;//new Exception("Error '" + step + "' Ref Id:" + referenceId + " ,Inner Message =>" + ex.Message, ex);
            }
        }
    }
}
