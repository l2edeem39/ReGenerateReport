using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public static class GlobalHelper
    {
        public static string ValueToNumericOnly(string _Input)
        {
            if (IsNotNull(_Input))
            {
                return Regex.Replace(_Input, "[^0-9a-zA-Z]+", string.Empty);
            }
            else
            {
                return string.Empty;
            }
        }
        public static bool IsNumeric(string stringToTest)
        {
            int result;

            return int.TryParse(stringToTest, out result);

        }
        public static bool IsNotNull(object chkobj)
        {
            if (chkobj == null)
            {
                return false;
            }
            if (chkobj.GetType() == typeof(Guid))
            {
                if (Guid.Parse(chkobj.ToString()) == Guid.Empty)
                {
                    return false;
                }
            }
            else
            {
                if (chkobj.ToString().ToUpper() == "NULL" || chkobj.ToString() == string.Empty || chkobj.ToString() == Guid.Empty.ToString())
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsNull(object chkobj)
        {
            if (chkobj == null)
            {
                return true;
            }

            if (chkobj.GetType() == typeof(Guid))
            {
                if (Guid.Parse(chkobj.ToString()) == Guid.Empty)
                {
                    return true;
                }
            }
            else
            {
                if (chkobj.ToString().ToUpper() == "NULL" || chkobj.ToString() == string.Empty || chkobj.ToString() == Guid.Empty.ToString())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ValueContains<T>(this T item, params T[] items)
        {
            if (items == null) return false;
            return items.Contains(item);
        }
        public static bool chkIDCardNo(string idcard)
        {
            if (Regex.IsMatch(idcard, @"^\d+$"))
            {
                int checkDigit = Convert.ToInt16(idcard.Substring(12, 1));
                string insuredcardNo = idcard.Substring(0, 12);
                int numOrder = 13, sumCheck = 0;
                char[] c = insuredcardNo.ToCharArray();
                for (int i = 0; i < c.Length; i++)
                {
                    sumCheck += Convert.ToInt16(c[i].ToString()) * numOrder;
                    numOrder--;
                }

                int sumMod = 11 - (sumCheck % 11);
                if (sumMod >= 10)
                {
                    sumMod = Convert.ToInt16(sumMod.ToString().Substring(1, 1));
                }
                if (sumMod != checkDigit)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
