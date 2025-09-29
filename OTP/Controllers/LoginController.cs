using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OTP.Model;
using OTP.Service;
using OtpNet;
using QRCoder;
using BCrypt.Net;
using System.Security.Claims;
using OTP.Interface;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public LoginController(LoginService loginService, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _loginService = loginService;
            //load JWT setting
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
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
        [HttpPost("/api/user")]
        public async Task<ActionResult<LoginModel?>> Login([FromBody] LoginRequest request)
        {
            //for testing purpose, not hash the password
            //In real case, the password should be hashed when store in DB
            //And the password here should be hashed before compare with DB
            //request.password = BCrypt.Net.BCrypt.HashPassword(request.password);
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
        [HttpPost("/api/loginCode")]
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
        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, "logged user")
            };
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<RefreshToken> GenerateRefreshToken(string requestIP,string id)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreateIP = requestIP,
                CreateAt = DateTime.UtcNow,
                Expire = DateTime.UtcNow.AddDays(7)
            };
            await _refreshTokenRepository.AddRefreshToken(id, refreshToken);
            return refreshToken;
        }
        private async Task<AuthResponse> Refresh(RefreshRequest request)
        {
            var user = await _loginService.GetUserByBid(request.id);
            DateTime time = DateTime.UtcNow;
        }
    }
}
