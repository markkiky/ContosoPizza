using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers
{
    [Route("/[controller]")]
    public class HomeController : Controller
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public ActionResult Get()
        {
            return Ok();
        }

        
        [HttpPost("/LoginCallback")]
        public async Task<IActionResult> LoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                // handle authentication failure
            }

            // handle authentication success

            return Ok();
        }
    }
}
