using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using NuGet.Packaging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ContosoPizzaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ContosoPizzaContext") ?? throw new InvalidOperationException("Connection string 'ContosoPizzaContext' not found.")));

// Add services to the container.
// Add Controllers with NewtonSoft
builder.Services.AddControllers()
     .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Cookies")
    .AddCookie()
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

var app = builder.Build();


// Run seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
