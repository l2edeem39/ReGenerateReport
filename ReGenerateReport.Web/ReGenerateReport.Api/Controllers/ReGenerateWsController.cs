using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ReGenerateReport.Api.Helper;
using ReGenerateReport.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Controllers
{
    [ApiController]
    public class ReGenerateWsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Service.IUserService _userRepository;
        private readonly Service.IWsService _wsService;

        public ReGenerateWsController(IConfiguration config, IHttpContextAccessor httpContextAccessor, Service.IUserService IUserService, Service.IWsService WsService)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = IUserService;
            _wsService = WsService;
        }

        [Route("api/GenerateAndGetLinkWs")]
        [HttpPost]
        public async Task<IActionResult> GenerateAndGetLinkWs([FromBody] ReqPolicy req)
        {
            WsGetLinkResponse Result = new WsGetLinkResponse();
            int statusCode = HttpStatus.OK;

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Result.Status = "Unauthorized";
                Result.StatusCode = "401";
                return StatusCode(401, Result);
            }
            string authHeader = Request.Headers["Authorization"];
            byte[] authBytes = Convert.FromBase64String(authHeader.Replace("Authorization", "").Substring("Basic ".Length));
            string authString = Encoding.UTF8.GetString(authBytes);
            string[] authArray = authString.Split(':');
            string username = authArray[0];
            string password = authArray[1];

            if (!_userRepository.ValidateCredentials(username, password))
            {
                Result.Status = "Unauthorized";
                Result.StatusCode = "401";
                return StatusCode(401, Result);
            }

            try
            {
                var data = await _wsService.getLinkPolicy(req.PolicyNo, req.SaleCode);
                if (data == null || data.linkPolicy == null)
                {
                    statusCode = HttpStatus.NotFound;
                    Result.StatusCode = statusCode.ToString();
                    Result.Status = "NotFound";
                    return StatusCode(statusCode, new { Result });
                }

                //Result.StatusCode = statusCode.ToString();
                //Result.Status = "OK";
                //Result.linkPolicy = data.linkPolicy;
                //Result.linkTax = data.linkTax;
                return StatusCode(statusCode, new { data });
            }
            catch (Exception ex)
            {
                statusCode = HttpStatus.InternalServerError;
                Result.StatusCode = statusCode.ToString();
                Result.Status = ex.Message;
                return StatusCode(statusCode, new { Result });
            }
        }
    }
}
