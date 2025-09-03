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
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }
        [HttpPost("/api/login")]
        public async Task<ActionResult<LoginModel?>> Login(string username, string password)
        {
            var data = await _loginService.GetUser(username, password);
            if (data == null)
            {
                return NotFound(data);
            }
            var message = new
            {
                user = data,
                status = "need verify"
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(message);
            return Ok(jsonString);
        }
        [HttpGet("/api/generate")]
        public async Task<ActionResult> GenerateCode(string username, string password)
        {
            var secret = KeyGeneration.GenerateRandomKey();
            var user = new UserOTP
            {
                Issuer = "Bank",
                Label = "TestOTP",
                Secret = Base32Encoding.ToString(secret)
            };
            await _loginService.WriteSecret(username, password, user.Secret);
            var qrCode = user.GenQRcode();
            var code = Convert.ToBase64String(qrCode);
            return Ok(code);
        }
        [HttpPost("/api/verify")]
        public async Task<IActionResult> VerifyLogin(string username, string password, [FromBody] string optstring)
        {
            var secret = await _loginService.GetSecret(username, password);
            if (string.IsNullOrEmpty(secret)) return BadRequest("This user not need 2FA");
            Totp instance = new Totp(Base32Encoding.ToBytes(secret));
            var isValid = instance.VerifyTotp(optstring, out long timeStepMatched, new VerificationWindow(2, 2));
            if (isValid)
            {
                return Ok("Verify success");
            }
            else
            {
                return BadRequest("Verify fail");
            }
        }
    }
}
