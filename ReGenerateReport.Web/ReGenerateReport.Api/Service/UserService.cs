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
        private readonly IConfiguration _Configuration;

        public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _Configuration = configuration;
        }

        public bool ValidateCredentials(string username, string password)
        {
            var status = false;
            var User = _Configuration["Users:" + username + ""];
            if (User != null && User == "True")
            {
                status = true;
            }
            return status;
        }
    }
}
