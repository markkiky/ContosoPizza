using Newtonsoft.Json.Linq;
using System.Text;

namespace ContosoPizza.Services.Zoho
{
    public class OauthClient
    {
        private string ClientId { get; set; }

        private string ClientSecret { get; set; }

        private static readonly List<string> scopes = new()
        {
           "ZohoBooks.fullaccess.all",
           "ZohoInventory.fullaccess.all",
           "zohobackstage.order.read",
           "zohobackstage.order.UPDATE",
           "zohobackstage.attendee.read","zohobackstage.event.read",
           "zohobackstage.portal.read",
           "zohobackstage.paymentoption.read",
           "zohobackstage.eventticket.READ"
        };

        private static readonly string AccessType = "offline";
        private static readonly string Prompt = "Consent";
        private static readonly string ResponseType = "code";
        private static readonly string AuthorizeUrl = "https://accounts.zoho.com/oauth/v2/auth";

        public OauthClient()
        {
            ClientId = "1000.SY4COZVK72MVM83ZZZJZ4DAU8HGHZF";
            ClientSecret = "d61f96400fe7fee9c9d85d013a7718842a8a9cf78a";
        }

        public OauthClient(string client_id, string client_secret)
        {
            ClientId = client_id;
            ClientSecret = client_secret;
        }

        public string AuthenticationUrl(string redirect_url)
        {
            var url =  Uri.EscapeDataString(redirect_url);
            return $"{AuthorizeUrl}?client_id={ClientId}&client_secret={ClientSecret}&scope={ScopesToString(scopes)}&access_type={AccessType}&response_type={ResponseType}&prompt={Prompt}&redirect_url={url}";
        }

        public string ScopesToString(List<string> scopes)
        {
            
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < scopes.Count; i++)
            {
                sb.Append(scopes[i]);

                if (i != scopes.Count - 1)
                {
                    sb.Append("+");
                }
            }
            return sb.ToString();
        }


    }
}
