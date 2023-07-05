using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public static class CommonHelper
    {
        public static IConfiguration _Configuration;
        public static string GetItext7TemplateConfigs(string policyType, string policyLanguage)
        {
            try
            {
                string result = string.Empty;

                if (policyLanguage == "T")
                {
                    switch (policyType.Trim())
                    {
                        case "1":
                            result = _Configuration["TemplateITextVmi:PT_13_TH"];  //ConfigurationManager.AppSettings["PT_13_TH"].ToString();
                            break;
                        case "2":
                            result = _Configuration["TemplateITextVmi:PT_13_TH"];  //ConfigurationManager.AppSettings["PT_13_TH"].ToString();
                            break;
                        case "3":
                            result = _Configuration["TemplateITextVmi:PT_13_TH"];  //ConfigurationManager.AppSettings["PT_13_TH"].ToString();
                            break;
                        case "4":
                            result = _Configuration["TemplateITextVmi:PT_4_TH"];  //ConfigurationManager.AppSettings["PT_4_TH"].ToString();
                            break;
                        case "5":
                            result = _Configuration["TemplateITextVmi:PT_5_TH"];  //ConfigurationManager.AppSettings["PT_5_TH"].ToString();
                            break;
                    }
                }

                if (policyLanguage == "E")
                {
                    switch (policyType.Trim())
                    {
                        case "1":
                            result = _Configuration["TemplateITextVmi:PT_13_EN"];  //ConfigurationManager.AppSettings["PT_13_EN"].ToString();
                            break;
                        case "2":
                            result = _Configuration["TemplateITextVmi:PT_13_EN"];  //ConfigurationManager.AppSettings["PT_13_EN"].ToString();
                            break;
                        case "3":
                            result = _Configuration["TemplateITextVmi:PT_13_EN"];  //ConfigurationManager.AppSettings["PT_13_EN"].ToString();
                            break;
                        case "5":
                            result = _Configuration["TemplateITextVmi:PT_5_EN"];  //ConfigurationManager.AppSettings["PT_5_EN"].ToString();
                            break;
                    }
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
