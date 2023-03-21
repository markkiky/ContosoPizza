using ContosoPizza.Data;
using ContosoPizza.Models;

namespace ContosoPizza.Services.Zoho
{
    public class TokenRefresher : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public TokenRefresher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ContosoPizzaContext>();

                    var tokens = database.Token.Where(t => t.Company.erp == "Zoho" && t.Type == "AccessToken");

                    foreach (var token in tokens)
                    {
                        if (token.ExpiryDate.Value.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(30))
                        {

                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
}

public class RefreshTokenContext
{
    private readonly IHttpClientFactory _httpClientFactory;
    public RefreshTokenContext(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task RefreshToken (Token token)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            {"client_id", "" },
            {"client_secret", "" },
            {"grant_type", "refresh_token" },
            {"refresh_token", token.Name }
        };

        var requestContent = new FormUrlEncodedContent(tokenRequestParameters);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accounts.zoho.com/oauth/v2/token");
        requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        requestMessage.Content = requestContent;
        
    }
}

   
