using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OTP.Service;
using Confluent.Kafka;
using OtpNet;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class AtmController : Controller
    {
        private readonly LoginService _loginService;
        private readonly IProducer<Null, string> _producer;
        public AtmController(LoginService loginService, IProducer<Null, string> producer)
        {
            _loginService = loginService;
            _producer = producer;
        }
        public class GenerateRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string account { get; set; }
            public int denominations { get; set; }
        }
        [HttpPost("/api/[controller]/Code")]
        public async Task<ActionResult> GenerateCode([FromBody] GenerateRequest request)
        {
            var secret = await _loginService.GetSecret(request.username, request.password);
            if (secret == null) return BadRequest("User not found or secret not set");
            var bytes = Base32Encoding.ToBytes(secret);
            //cause we not need change the funding code so often but keep it security, so set step to 600s and code length to 8
            var totp = new Totp(bytes,step:600,totpSize:8);
            var code = totp.ComputeTotp();
            var message = new
            {
                Key = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Username = request.username,
                Password = request.password,
                Account = request.account,
                Denominations = request.denominations,
                Code = code
            };
            var messageString = System.Text.Json.JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = messageString };
            try
            {
                await _producer.ProduceAsync("pending", kafkaMessage);
                return Ok(new { Code = code, Status = "pending" });
            }
            catch (ProduceException<string,string> e)
            {
                Console.WriteLine($"Kafka produce error: {e.Error.Reason}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
