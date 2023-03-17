using ContosoPizza.Services;
using Hangfire;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HomeController(IHttpContextAccessor httpContext, IBackgroundJobClient backgroundJobClient)
        {
            _contextAccessor = httpContext;
            _httpContext = _contextAccessor.HttpContext;
            _backgroundJobClient = backgroundJobClient;
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
            HangfireTest job = new HangfireTest(_backgroundJobClient);
            job.Print();
            var user = _contextAccessor.HttpContext.User;
            return Ok(user.Claims.ToList());
        }

        [Authorize("eu passport")]
        [HttpGet("/sweden")]
        public ActionResult Sweden()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/login/oauth/github")]
        public IResult GithubLogin()
        {
            return Results.Challenge(
                new AuthenticationProperties()
                {
                    RedirectUri = "https://localhost:3000/username"
                },
                authenticationSchemes: new List<string>() { "github" });
        }

        [AllowAnonymous]
        [HttpGet("/oauth/github/callback")]
        public ActionResult GithubLoginCallback()
        {
            return Ok();
        }

        [HttpGet("/login/oauth/google")]
        public IResult GoogleLogin()
        {
            return Results.Challenge(
                new AuthenticationProperties()
                {
                    RedirectUri = "https://localhost:3000/username"
                },
                authenticationSchemes: new List<string>() { "google" });
        }

        [AllowAnonymous]
        [HttpPost("/oauth/google/callback")]
        public ActionResult GoogleLoginCallback()
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/login/oauth/zoho")]
        public IResult ZohoLogin()
        {
            return Results.Challenge(
                new AuthenticationProperties()
                {
                    RedirectUri = "https://localhost:3000/username"
                },
                authenticationSchemes: new List<string>() { "zoho" });
        }

        [AllowAnonymous]
        [HttpGet("/oauth/zoho/callback")]
        public ActionResult ZohoLoginCallback()
        {
            return Ok();
        }
    }
}
