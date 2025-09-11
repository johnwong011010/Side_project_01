using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OTP.Service;
using OtpNet;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class AtmController : Controller
    {
        private readonly LoginService _loginService;
        public AtmController(LoginService loginService)
        {
            _loginService = loginService;
        }
        public class GenerateRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }
    }
}
