using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContosoPizza.Controllers
{
    [Route("/[controller]")]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor? _contextAccessor;
        private readonly HttpContext? _httpContext;

        public HomeController(IHttpContextAccessor httpContext)
        {
            _contextAccessor = httpContext;
            _httpContext = _contextAccessor.HttpContext;

        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public ActionResult Get()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/login/sweden")]
        public async Task<ActionResult> LoginAuthenticationAsync()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("usr", "mark"));
            claims.Add(new Claim("passport_type", "eu"));
            var identity = new ClaimsIdentity(claims, "cookie");
            var user = new ClaimsPrincipal(identity);
            await _httpContext.SignInAsync("cookie", user);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/login")]
        public async Task<ActionResult> Login()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("usr", "mark"));
            var identity = new ClaimsIdentity(claims, "cookie");
            var user = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("cookie", user);
            return Ok();
        }

        [Authorize]
        [HttpGet("/username")]
        public ActionResult Username()
        {
            var user = _contextAccessor.HttpContext.User;
            return Ok(user.FindFirst("usr").Value);
        }

        [Authorize("eu passport")]
        [HttpGet("/sweden")]
        public ActionResult Sweden()
        {
            return Ok();
        }

        [AllowAnonymous]
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
