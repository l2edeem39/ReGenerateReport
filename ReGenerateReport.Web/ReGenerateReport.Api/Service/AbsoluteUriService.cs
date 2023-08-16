using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Service
{
    public class AbsoluteUriService: IAbsoluteUriService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AbsoluteUriService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetAbsoluteUri()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var absoluteUri = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            return absoluteUri;
        }
    }
}
