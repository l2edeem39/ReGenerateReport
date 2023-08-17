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
                var data = new WsGetLinkResponse();
                var pol = _wsService.getPolicyType(req.PolicyNo);
                string poltype = pol.polType;
                string polyr = pol.polYr;
                string polbr = pol.polBrCode;
                string polno = pol.polNo;
                string saleCode = pol.saleCode.Trim();
                string productClass = pol.productClass.Trim();
                string flagOnline = pol.flagOnline.Trim();
                string channel = pol.channelCode.Trim();
                if (poltype == "VMI")
                {

                    if (productClass == "5")
                    {
                        if (saleCode == "15445" || saleCode == "15841" || saleCode == "16184")
                        {
                            data = await _wsService.PolicyVMIType5noPrint(polyr, polbr, polno, saleCode, flagOnline, productClass);
                        }
                        else
                        {
                            data = await _wsService.PolicyVMIType5(polyr, polbr, polno, saleCode, flagOnline, channel);
                        }
                    }
                    else if (productClass == "2" || productClass == "3")
                    {
                        if (saleCode == "15445" || saleCode == "15841" || saleCode == "16184")
                        {
                            data = await _wsService.PolicyVMIType5noPrint(polyr, polbr, polno, saleCode, flagOnline, productClass);
                        }
                        else
                        {
                            data = await _wsService.PolicyVMIType3(polyr, polbr, polno, saleCode, flagOnline, channel);
                        }
                    }
                    else if (productClass == "4")
                    {
                        if (saleCode == "15445" || saleCode == "15841" || saleCode == "16184")
                        {
                            data = await _wsService.PolicyVMIType4Copy(polyr, polbr, polno, saleCode, flagOnline, channel);
                        }
                        else
                        {
                            data = await _wsService.PolicyVMIType4(polyr, polbr, polno, saleCode, flagOnline, channel);
                        }
                    }
                }
                else if (poltype == "CMI")
                {
                    if (saleCode == "15841")
                    {
                        data = await _wsService.PolicyCMI("PolicyCMInoPrint", polyr, polbr, polno, saleCode, flagOnline, channel);
                    }
                    else
                    {
                        data = await _wsService.PolicyCMI("PolicyCMI", polyr, polbr, polno, saleCode, flagOnline, channel);
                    }
                }

                if (data == null || data.linkPolicy == null)
                {
                    statusCode = HttpStatus.NotFound;
                    Result.StatusCode = statusCode.ToString();
                    Result.Status = "NotFound";
                    return StatusCode(statusCode, Result);
                }

                //Result.StatusCode = statusCode.ToString();
                //Result.Status = "OK";
                //Result.linkPolicy = data.linkPolicy;
                //Result.linkTax = data.linkTax;
                return StatusCode(statusCode, data);
            }
            catch (Exception ex)
            {
                statusCode = HttpStatus.InternalServerError;
                Result.StatusCode = statusCode.ToString();
                Result.Status = ex.Message;
                return StatusCode(statusCode, Result);
            }
        }
    }
}
