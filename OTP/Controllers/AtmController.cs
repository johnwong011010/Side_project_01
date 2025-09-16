using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OTP.Service;
using Confluent.Kafka;
using OtpNet;
using OTP.Model;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class AtmController : Controller
    {
        private readonly LoginService _loginService;
        private readonly RequestService _requestService;
        private readonly IProducer<Null, string> _producer;
        private readonly IConsumer<Null, string> _consumer;
        private readonly ILogger<AtmController> _logger;
        public AtmController(LoginService loginService, RequestService requestService, IProducer<Null, string> producer, IConsumer<Null, string> consumer, ILogger<AtmController> logger)
        {
            _loginService = loginService;
            _requestService = requestService;
            _producer = producer;
            _consumer = consumer;
            _logger = logger;
        }
        public class GenerateRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string account { get; set; }
            public int denominations { get; set; }
        }
        public class MachineData
        {
            public string machine_code { get; set; } = null!;
            public string machine_name { get; set; } = null!;
            public string location { get; set; } = null!;
        }
        public class MessageFormat
        {
            public string Key { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string account { get; set; }
            public int denomination { get; set; }
            public string code { get; set; }
            public DateTime requestTime { get; set; }
        }
        [HttpPost("/api/[controller]/Code")]
        public async Task<ActionResult> GenerateCode([FromBody] GenerateRequest request)
        {
            var secret = await _loginService.GetSecret(request.username, request.password);
            if (secret == null) 
            { 
                _logger.LogWarning("User not found or secret not set for user: {Username}", request.username);
                return BadRequest("User not found or secret not set");
            }
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
                _logger.LogInformation("Produced message to Kafka topic 'pending' for user: {Username}", request.username);
                await _requestService.AddRequest(new Model.NoCardRequest
                {
                    username = request.username,
                    account = request.account,
                    denomination = request.denominations,
                    verify_code = code,
                    Finished = false
                });
                return Ok(new { Code = code, Status = "pending" });
            }
            catch (ProduceException<string,string> e)
            {
                _logger.LogError(e, "Kafka produce error for user: {Username}", request.username);
                Console.WriteLine($"Kafka produce error: {e.Error.Reason}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("/api/[controller]/message")]
        public async Task<ActionResult> StartConsume([FromBody] MachineData data)
        {
            // Validate machine data
            // in real solution, it should have a format check.
            if (data.machine_code == null || data.machine_name == null || data.location == null)
            {
                _logger.LogError("Not ATM request received.");
                return Unauthorized("Not ATM request");
            }
            else
            {
            }
        }
    }
}
