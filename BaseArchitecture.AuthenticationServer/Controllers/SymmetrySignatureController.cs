using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseArchitecture.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BaseArchitecture.AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SymmetrySignatureController : ControllerBase
    {
        private IConfiguration _configuration;
        private IJWTService _jWTService;
        public SymmetrySignatureController(IConfiguration configuration, IJWTService jWTService)
        {
            this._configuration = configuration;
            this._jWTService = jWTService;
        }
        [Route("GetToken")]
        [HttpPost]
        public IActionResult GetSignature(string userName, string password)
        {
            if ("nextload".Equals(userName, StringComparison.InvariantCultureIgnoreCase) &&
                "123456".Equals(password))
            {
                Claim[] claims = new[] {
                    new Claim("role","Adminitrastor"),
                    new Claim("userName",userName)
                };
                string token = _jWTService.GetToken(claims);
                return Ok(new { UserName = userName, Token = token });
            }
            else
            {
                return Ok(new { UserName = userName, Token = string.Empty });
            }
        }
    }
}