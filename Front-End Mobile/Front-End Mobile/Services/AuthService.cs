using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Front_End_Mobile.Services
{
    public class AuthService
    {
        private static readonly string _authenticationUrl = "https://onemoretree.azurewebsites.net/api/auth/login/{0}?mobile=true";
        private static readonly string _callback = "onemoretree://";
        private static readonly string _refreshUrl = "https://onemoretree.azurewebsites.net/api/auth/refresh/{0}";

        public async Task<string> GetAccessToken()
        {
            // Check if the token doesn't exist
            if (!IsAuthenticated())
            {
                throw new AuthenticationException();
            }

            // Check if the token isn't valid and try to refresh it
            if (DateTime.Now > Preferences.Get("expiration", DateTime.MinValue))
            {
                await RefreshToken();
            }
            
            return Preferences.Get("access_token", null);
        }

        public async Task Authenticate(string scheme)
        {
            // Check if the user is already authenticated
            if (IsAuthenticated())
            {
                throw new AuthenticationException();
            }

            Uri authUrl = new Uri(string.Format(_authenticationUrl, scheme));
            Uri callbackUrl = new Uri(_callback);

            // Initiate browser based flow and wait until the callback is received
            WebAuthenticatorResult result = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);

            // Check if the user cancels the flow at any point
            if (result == null)
            {
                throw new AuthenticationException();
            }

            // Store token informations in a key/value store
            Preferences.Set("access_token", result.Properties["access_token"]);
            Preferences.Set("refresh_token", result.Properties["refresh_token"]);
            Preferences.Set("provider", result.Properties["provider"]);
            Preferences.Set("expiration", DateTime.Parse(result.Properties["expiration"]));
        }

        // Try to refresh the token
        private async Task RefreshToken()
        {
            // Check if the user isn't authenticated
            if (!IsAuthenticated())
            {
                throw new AuthenticationException();
            }

            HttpClient httpClient = new HttpClient();
            JObject payload = new JObject();

            payload["refresh_token"] = Preferences.Get("refresh_token", null);

            // Call API to get a new refresh token
            HttpResponseMessage response = await httpClient.PostAsync(
                string.Format(_refreshUrl, Preferences.Get("provider", null)),
                new StringContent(payload.ToString(), Encoding.UTF8, "application/json"));

            // Check if the refresh token has expired
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            // Save the new token
            string access_token = json.access_token;
            DateTime expiration = json.expiration;
            
            Preferences.Set("access_token", access_token);
            Preferences.Set("expiration", expiration);
        }

        public bool IsAuthenticated()
        {
            if (Preferences.ContainsKey("access_token") && 
                Preferences.ContainsKey("refresh_token") &&
                Preferences.ContainsKey("provider") &&
                Preferences.ContainsKey("expiration"))
            {
                return true;
            }

            return false;
        }

    }
}
