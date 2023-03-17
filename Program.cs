using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Hangfire;
using ContosoPizza.Models.Zoho;

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
    .AddOAuth("zoho", options =>
    {
        options.SignInScheme = "cookie";
        options.ClientId = "1000.SY4COZVK72MVM83ZZZJZ4DAU8HGHZF";
        options.ClientSecret = "d61f96400fe7fee9c9d85d013a7718842a8a9cf78a";
        options.AuthorizationEndpoint = "https://accounts.zoho.com/oauth/v2/auth";
        options.TokenEndpoint = "https://accounts.zoho.com/oauth/v2/token";
        options.CallbackPath = "/oauth/zoho/callback";
        options.SaveTokens = true;
        options.UserInformationEndpoint = "https://www.zohoapis.com/books/v3/organizations";
        options.Scope.Add("ZohoBooks.fullaccess.all");
        options.Scope.Add("ZohoInventory.fullaccess.all");
        options.Scope.Add("zohobackstage.order.read");
        options.Scope.Add("zohobackstage.order.UPDATE");
        options.Scope.Add("zohobackstage.attendee.read");
        options.Scope.Add("zohobackstage.event.read");
        options.Scope.Add("zohobackstage.portal.read");
        options.Scope.Add("zohobackstage.paymentoption.read");
        options.Scope.Add("zohobackstage.eventticket.READ");
        options.Events.OnCreatingTicket = async context =>
        {
            var database = context.HttpContext.RequestServices.GetRequiredService<ContosoPizzaContext>();

            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

            var result = await context.Backchannel.SendAsync(request);
            var content = await result.Content.ReadAsStringAsync();

            var companyJson = JsonDocument.Parse(content).RootElement;

           
            var companies = companyJson.GetProperty("organizations");

           

            List<Organization> orgs = new List<Organization>()
            {

            };


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

/* Configure Cookie Authorization */
builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("eu passport", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
        .AddAuthenticationSchemes("cookie")
        .RequireClaim("passport_type", "eu");
    });
});

/* Hangfire */
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("ContosoPizzaContext"), new Hangfire.SqlServer.SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    }
    ));

builder.Services.AddHangfireServer();

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

app.UseHangfireDashboard();


/* Use Controllers Middleware */
app.MapControllers();

/* Start Application */
app.Run();
