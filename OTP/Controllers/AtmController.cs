using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OTP.Service;
using Confluent.Kafka;
using OtpNet;
using OTP.Model;
using System.Reflection.Metadata.Ecma335;

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
        public class AtmInput 
        {
            public string username { get; set; } = null!;
            public string account { get; set; } = null!;
            public string code { get; set; } = null!;
        }
        [HttpPost("/api/[controller]/Code")]
        public async Task<ActionResult> GenerateCode([FromBody] GenerateRequest request)
        {
            var secret = await _loginService.GetSecret(request.username, request.password);
            var user = await _loginService.GetUser(request.username, request.password);
            string id = user._id;
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
                ID = id,
                Username = request.username,
                Password = request.password,
                Account = request.account,
                Denominations = request.denominations,
                Code = code,
                Status = "pending"
            };
            var messageString = System.Text.Json.JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = messageString };
            try
            {
                await _producer.ProduceAsync("pending", kafkaMessage);
                _logger.LogInformation("Produced message to Kafka topic 'pending' for user: {Username}", request.username);
                return Ok(new { Code = code, Status = "pending" });
            }
            catch (ProduceException<string,string> e)
            {
                _logger.LogError(e, "Kafka produce error for user: {Username}", request.username);
                Console.WriteLine($"Kafka produce error: {e.Error.Reason}");
                return StatusCode(500, "Internal server error");
            }
        }
        //in machine login
        [HttpPost("/api/[controller]/userdata")]
        public async Task<ActionResult> Login(string username, string password)
        {
            var searchResult = await _loginService.GetUser(username, password);
            if (searchResult == null)
            {
                return NotFound("User not found");
            }
            else
            {
                return Ok("choose action");
            }
        }
        [HttpPost("/api/[controller]/codeVaildation")]
        public async Task<ActionResult> NoCardFund([FromBody] AtmInput input)
        {
            //check the input vaild code 
            if (String.IsNullOrEmpty(input.code))
            {
                _logger.LogWarning("User enter Invaild code: {code}", input.code);
                return BadRequest("No verify code detected");
            }
            else if (input.code.Length == 0 || input.code.Length < 8 || !input.code.All(char.IsDigit))
            {
                _logger.LogCritical("User input a worth code");
                return BadRequest("Invaild code");
            }
            var request = await _requestService.GetOneRequest(input.account, input.username, input.code);
            if (request == null)
            {
                _logger.LogWarning("User doing Invaild request");
                return BadRequest("Invaild request");
            }
            else
            {
                //check the verify code first
                var user = await _loginService.GetUserByBid(request.uID);
                Totp totp = new Totp(Base32Encoding.ToBytes(user.Secret));
                var isVaild = totp.VerifyTotp(input.code, out long timeStepMatched);
                if (isVaild)
                {
                    _logger.LogCritical("User {Username} successfully withdraw {Denominations} from account {Account}", input.username, request.denomination, input.account);
                    return Ok($"Success withdraw {request.denomination} from account {input.account}");
                }
                else
                {
                    _logger.LogInformation("User enter a outdate code");
                    return BadRequest("Outdate code");
                }
            }
        }
    }
}
