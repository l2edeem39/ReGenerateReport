using System;
using System.Globalization;

namespace ReGenerateReport.Api.Helper
{
    public class DateHelper
    {
        public static string GetCurYear()
        {
            string ls_year = DateTime.Now.ToString("yy", new CultureInfo("th-TH"));
            return ls_year;
        }
        public static string GetCurMonth()
        {
            string ls_month = DateTime.Now.ToString("MM", new CultureInfo("th-TH"));
            return ls_month;
        }
        public static string GetCurDay()
        {
            string ls_day = DateTime.Now.ToString("dd", new CultureInfo("th-TH"));
            return ls_day;
        }
        public static DateTime conv2DateTime(string data)
        {
            string ls_yyyy = data.Substring(0, 4);
            int li_yyyy = int.Parse(ls_yyyy) + 543;
            string ls_date = li_yyyy.ToString() + "/" + data.Substring(4, 2) + "/" + data.Substring(6, 2);
            //DateTime p_from = DateTime.Parse("2556/09/19");
            DateTime p_from = DateTime.Parse(ls_date);
            return p_from;
        }
        public static DateTime convDateVIB(string data)
        {
            DateTime p_from = DateTime.ParseExact(data, "dd-MM-yyyy", new CultureInfo("en-GB"));
            return p_from;
        }
        public static double caldays(string ls_startdate, string ls_enddate)
        {
            DateTime ldt_startdate, ldt_enddate;
            CultureInfo culture = new CultureInfo("en-GB");
            ldt_startdate = DateTime.ParseExact(ls_startdate, "yyyyMMdd", culture);
            ldt_enddate = DateTime.ParseExact(ls_enddate, "yyyyMMdd", culture);

            double days = (ldt_enddate - ldt_startdate).TotalDays;
            return days + 1;
        }
        public static bool chkdate(string input, string inputFormat)
        {
            DateTime dateValue;

            if (!DateTime.TryParseExact(input, inputFormat, new CultureInfo("en-US"), DateTimeStyles.AllowLeadingWhite, out dateValue))
                return false;

            return true;
        }

        public static string ToUtcTime(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2) return string.Empty;

            return input.Substring(0, 2) + ":" + input.Substring(2);

        }

        public static DateTime GetTimeZoneByCity(string cityCode)
        {

            var allTimeZone = TimeZoneInfo.GetSystemTimeZones();
            var remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var remoteTime = TimeZoneInfo.ConvertTime(DateTime.Now, remoteTimeZone);


            return remoteTime;
        }
        public static DateTime GetDate(DateTime datetime)
        {
            if (datetime < DateTime.MaxValue && datetime > DateTime.MinValue)
            {
                //if (datetime.Millisecond > 0 || datetime.Minute > 0 || datetime.Second > 0)
                //{
                //    return Convert.ToDateTime(datetime).Date;
                //}
                return Convert.ToDateTime(datetime);
            }
            return DateTime.Now; //.Today;
        }
    }
}
