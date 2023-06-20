using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool ValidateCredentials(string username, string password)
        {
            SqlConnection connection = null;
            var status = false;
            //try
            //{
            //    var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            //    var configuration = configBuilder.Build();

            //    var connectionString = configuration.GetConnectionString("DB");

            //    var sql = "select br_code from loginWS where login = '" + username.Trim() + "' and password = '" + password.Trim() + "'";

            //    using (connection = new SqlConnection(connectionString))
            //    {
            //        connection.Open();

            //        using (var command = new SqlCommand(sql, connection))
            //        {
            //            var reader = command.ExecuteReader();

            //            while (reader.Read())
            //            {
            //                HttpContext context = _httpContextAccessor.HttpContext;
            //                context.Session.SetString("UserBranch", reader.GetValue(0).ToString());

            //                status = true;
            //                break;
            //            }
            //        }
            //    }

            //    if (connection != null && connection.State == ConnectionState.Open)
            //    {
            //        connection.Close();
            //    }
            //    return status;
            //}
            //catch (Exception ex)
            //{
            //    if (connection != null && connection.State == ConnectionState.Open)
            //    {
            //        connection.Close();
            //    }
            //    return status;
            //}

            //return username.Equals("admin") && password.Equals("Pa$$WoRd");
            status = true;
            return status;
        }
    }
}
