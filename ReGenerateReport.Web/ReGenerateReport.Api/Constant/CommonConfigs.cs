using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Constant
{
    public class CommonConfigs
    {
        private readonly IConfiguration _Configuration;

        public CommonConfigs(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        //public static string Mode = ConfigurationManager.AppSettings["Mode"].ToString();

        //public static string StartDate = ConfigurationManager.AppSettings["StartDate"].ToString();
        //public static string EndDate = ConfigurationManager.AppSettings["EndDate"].ToString();

        //static readonly string strPreDay = ConfigurationManager.AppSettings["PreDay"].ToString() == "" ? "0" : ConfigurationManager.AppSettings["PreDay"].ToString();
        //public static int PreDay = int.Parse(strPreDay);

        //public static string IsLimit = ConfigurationManager.AppSettings["IsLimit"].ToString();
        //static readonly string strLimit = ConfigurationManager.AppSettings["Limit"].ToString() == "" ? "0" : ConfigurationManager.AppSettings["Limit"].ToString();
        //public static int Limit = int.Parse(strLimit);
    }
}
