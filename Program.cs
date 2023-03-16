using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

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
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://accounts.google.com";
        options.ClientId = "325233773683-etk0p3gv1glrnfghhhes2vvhev5msksm.apps.googleusercontent.com";
        options.ClientSecret = "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";
        options.CallbackPath = "/LoginCallback";
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
