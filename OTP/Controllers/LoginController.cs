using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class LoginController : Controller
    {
        [HttpPost("/api/login")]
        public Task<ActionResult> Login()
        {
            
        }
    }
}
