using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

/* Use Builder */
var builder = WebApplication.CreateBuilder(args);

/* Add Services to the builder */
builder.Services.AddDbContext<ContosoPizzaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ContosoPizzaContext") ?? throw new InvalidOperationException("Connection string 'ContosoPizzaContext' not found.")));

/* DI Controllers */
builder.Services.AddControllers()
     .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

/* DI HttpContext
 * HttpContext Allows access to Request and Response blocks
 * Use IHttpContextAccessor to access HttpContext
 */
builder.Services.AddHttpContextAccessor();

/* DI Swagger */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* DI Authentication */
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth("github", options =>
    {
        options.SignInScheme = "cookie";
        options.ClientId = "1075a54aa8c782584bf0";
        options.ClientSecret = "d3f5a890e895e7a155a427dca547e40febbc74c5";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.CallbackPath = "/oauth/github/callback";
        options.SaveTokens = true;
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.ClaimActions.MapJsonKey("sub", "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        options.Events.OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            var result = await context.Backchannel.SendAsync(request);
            var user = await result.Content.ReadFromJsonAsync<JsonElement>();
            context.RunClaimActions(user);
        };
    })
    .AddOpenIdConnect("google", options =>
    {
        options.Authority = "https://accounts.google.com";
        options.ClientId = "325233773683-etk0p3gv1glrnfghhhes2vvhev5msksm.apps.googleusercontent.com";
        options.ClientSecret = "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";
        options.CallbackPath = "/oauth/google/callback";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
    });

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("eu passport", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
        .AddAuthenticationSchemes("cookie")
        .RequireClaim("passport_type", "eu");
    });
});

var app = builder.Build();

// Run seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

/* Use Swagger Middleware in Development */
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* Use HttpsRedirection Middleware */
app.UseHttpsRedirection();

/* Use Authentication Middleware */
app.UseAuthentication();
/* Use Authorization Middleware */
app.UseAuthorization();

/* Use Controllers Middleware */
app.MapControllers();

/* Start Application */
app.Run();
