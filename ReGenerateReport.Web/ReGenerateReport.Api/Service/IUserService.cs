using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public interface IUserService
    {
        bool ValidateCredentials(string username, string password);
    }
}
