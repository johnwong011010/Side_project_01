using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace OTP.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [EnableCors("policy")]
    public class AtmController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
