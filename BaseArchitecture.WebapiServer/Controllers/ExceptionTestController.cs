using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseArchitecture.WebapiServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExceptionTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Exception()
        {
            throw new Exception("this is a custom test Exception!");
        }
    }
}