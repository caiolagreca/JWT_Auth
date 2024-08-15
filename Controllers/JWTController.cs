using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JWTController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult PrivateAPI()
        {
            var list = new[]
            {
                new {Code = 1, Name= "This endpoint is restricted"},
                new {Code = 2, Name = "you have to login to see this"}
            }.ToList();
            
            return Ok(list);
        }

        [HttpGet]
        public IActionResult PublicAPI()
        {
            var list = new[]
            {
                new {Code = 1, Name = "everybody can see this endpoint"},
            new {Code = 2, Name = "Whatver"}
            }.ToList();

            return Ok(list);
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
