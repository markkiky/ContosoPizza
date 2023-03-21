using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Hangfire;
using ContosoPizza.Models.Zoho;
using System.Security.AccessControl;
using Newtonsoft.Json;

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
        options.ClientId = "1000.SY4COZVK72MVM83ZZZJZ4DAU8HGHZF";
        options.ClientSecret = "d61f96400fe7fee9c9d85d013a7718842a8a9cf78a";
        options.AuthorizationEndpoint = "https://accounts.zoho.com/oauth/v2/auth?access_type=offline&prompt=Consent";
        options.TokenEndpoint = "https://accounts.zoho.com/oauth/v2/token";
        options.CallbackPath = "/oauth/zoho/callback";
        options.SaveTokens = true;
        options.UserInformationEndpoint = "https://www.zohoapis.com/books/v3/organizations";
        options.AccessDeniedPath = "";
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
            //var companyJson = JsonConvert.DeserializeObject(content);


            var companies = JsonConvert.DeserializeObject<List<Organization>>(companyJson.GetProperty("organizations").ToString());

            foreach (var company in companies)
            {
                var existing_company = database.Company.Where(comp => comp.LedgerId == company.organization_id).SingleOrDefault();

                if (existing_company != null)
                {
                    var access_token = database.Token.SingleOrDefault(comp => comp.Type == "AccessToken" && comp.CompanyId == existing_company.Id);
                    if (access_token != null)
                    {
                        access_token.Name = context.AccessToken;
                        access_token.ExpiryDate = DateTime.UtcNow.AddSeconds(int.Parse(context.TokenResponse.ExpiresIn));
                    }
                    else
                    {
                        Token token = new Token
                        {
                            Name = context.AccessToken,
                            ExpiryDate = DateTime.UtcNow.AddSeconds(int.Parse(context.TokenResponse.ExpiresIn)),
                            CompanyId = existing_company.Id,
                            Type = "AccessToken"
                        };
                        database.Token.Add(token);
                    }

                    var refresh_token = database.Token.SingleOrDefault(comp => comp.Type == "RefreshToken" && comp.CompanyId == existing_company.Id);
                    if (refresh_token != null)
                    {
                        refresh_token.Name = context.AccessToken;
                    }
                    else
                    {
                        Token token = new Token
                        {
                            Type = "RefreshToken",
                            Name = context.RefreshToken,
                            CompanyId = existing_company.Id
                        };

                        database.Token.Add(token);

                    }

                    database.SaveChanges();
                }
                else
                {
                    Company new_company = new()
                    {
                        Name = company.name,
                        LedgerId = company.organization_id
                    };

                    database.Company.Add(new_company);
                    database.SaveChanges();

                    var saved_company = database.Company.SingleOrDefault(c => c.LedgerId == company.organization_id);

                    Token access_token = new()
                    {
                        Name = context.AccessToken,
                        ExpiryDate = DateTime.UtcNow.AddSeconds(int.Parse(context.TokenResponse.ExpiresIn)),
                        CompanyId = saved_company.Id,
                        Type = "AccessToken"
                    };

                    Token refresh_token = new()
                    {
                        Name = context.AccessToken,
                        CompanyId = saved_company.Id,
                        Type = "RefreshToken"
                    };

                    database.Token.Add(refresh_token); database.SaveChanges();
                    database.Token.Add(access_token); database.SaveChanges();
                }

            }

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
