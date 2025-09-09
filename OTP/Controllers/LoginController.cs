using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OTP.Model;
using OTP.Service;
using OtpNet;
using QRCoder;
using System.Threading.Tasks;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }
        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class VerifyRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string otpCode { get; set; }
        }
        [HttpPost("/api/login")]
        public async Task<ActionResult<LoginModel?>> Login([FromBody] LoginRequest request)
        {
            var data = await _loginService.GetUser(request.username, request.password);
            if (data == null)
            {
                return NotFound(data);
            }
            var message = new
            {
                username = data.Username,
                password = request.password,
                isDeleted = data.isDeleted,
                status = "need verify"
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(message);
            return Ok(jsonString);
        }
        [HttpPost("/api/generate")]
        public async Task<ActionResult> GenerateCode([FromBody] LoginRequest request)
        {
            var userSecret = await _loginService.GetSecret(request.username, request.password);
            if (userSecret == null) 
            { 
                var secret = KeyGeneration.GenerateRandomKey();
                var user = new UserOTP
                {
                    Issuer = "Bank",
                    Label = "TestOTP",
                    Secret = Base32Encoding.ToString(secret)
                };
                await _loginService.WriteSecret(request.username, request.password, user.Secret);
                var qrCode = user.GenQRcode();
                var code = Convert.ToBase64String(qrCode);
                return Ok(code);
            }
            else
            {
                var user = new UserOTP
                {
                    Issuer = "Bank",
                    Label = "TestOTP",
                    Secret = userSecret
                };
                var qrCode = user.GenQRcode();
                var code = Convert.ToBase64String(qrCode);
                return Ok(code);
            }
        }
        [HttpPost("/api/verify")]
        public async Task<IActionResult> VerifyLogin([FromBody]VerifyRequest request)
        {
            var secret = await _loginService.GetSecret(request.username, request.password);
            //if 2FA is an option, then need this code
            //if (string.IsNullOrEmpty(secret)) return BadRequest("This user not need 2FA");
            Totp instance = new Totp(Base32Encoding.ToBytes(secret));
            var isValid = instance.VerifyTotp(request.otpCode, out long timeStepMatched, new VerificationWindow(2, 2));
            if (isValid)
            {
                return Ok("Verify success");
            }
            else
            {
                return BadRequest("Verify fail");
            }
        }
        [HttpPost("/api/noCard")]
        public async Task<IActionResult> GetCode([FromBody] LoginRequest request)
        {
            var secret = await _loginService.GetSecret(request.username, request.password);
            //create instance
            //using created user screct,1 min per step and 8 totpsize
            var totp = new Totp(Base32Encoding.ToBytes(secret), step: 60, totpSize: 8);
            //recommed to use computeTotp(DateTime.UtcNow);
            //or just use its overload -> computeTotp();
            string fundCode = totp.ComputeTotp(DateTime.UtcNow);
            return Ok(fundCode);
        }
    }
}
