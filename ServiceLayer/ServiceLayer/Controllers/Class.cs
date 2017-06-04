using Microsoft.AspNetCore.Mvc;

namespace ServiceLayer.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        [Route("")]
        public IActionResult Get()
        {
            return Ok(new { Data = "Shawn Wildermuth", FavoriteColor = "Blue" });
        }
    }
}
