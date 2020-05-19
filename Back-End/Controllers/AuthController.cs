using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using Back_End.Models;
using System.Text.Json;

namespace Sample.Server.WebAuthenticator
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string _appScheme = "onemoretree";
        private const string _facebookDebugTokenUrl = "https://graph.facebook.com/v7.0/debug_token";

        private readonly List<string> _providers = new List<string>() { "Facebook" };
        private readonly DatabaseContext _databaseContext;

        private readonly string _facebookAppId;
        private readonly string _facebookAppToken;

        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly string _jwtSignKey;

        public AuthController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            _databaseContext = databaseContext;

            _facebookAppId = configuration["FACEBOOK_APP_ID"];
            _facebookAppToken = configuration["FACEBOOK_APP_ACCESS_TOKEN"];

            _jwtIssuer = configuration["JWT_ISSUER"];
            _jwtAudience = configuration["JWT_AUDIENCE"];
            _jwtSignKey = configuration["JWT_SIGN_KEY"];
        }


        [HttpGet]
        [Route("login/{provider}")]
        public async Task<ActionResult> Get(string provider, bool mobile)
        {
            // Check if the provider is registered
            if (!_providers.Contains(provider, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            // Capitalize first letter
            provider = provider.First().ToString().ToUpper() + provider.Substring(1);

            // Authenticate the user or ask for authentication through provider
            AuthenticateResult auth = await Request.HttpContext.AuthenticateAsync();

            if (!auth.Succeeded
                || auth?.Principal == null
                || !auth.Principal.Identities.Any(id => id.IsAuthenticated)
                || string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
            {
                return Challenge(provider);
            }

            // Build the user model
            User user;
            string refreshToken;

            if (provider == "Facebook")
            {
                string facebookId = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;

                user = new User()
                {
                    Email = User.Claims.Where(claim => claim.Type == ClaimTypes.Email).First().Value,
                    Name = User.Claims.Where(claim => claim.Type == ClaimTypes.Name).First().Value,
                    FacebookId = facebookId,
                    Picture = $"https://graph.facebook.com/{facebookId}/picture"
                };

                refreshToken = auth.Properties.GetTokenValue("access_token");
            }
            else
            {
                return BadRequest();
            }

            // Check if the user is new or already present in database
            IEnumerable<User> users = from registeredUser in _databaseContext.Users
                                      where registeredUser.Email == user.Email
                                      select registeredUser;

            // Store the details in database if the user hasn't registered yet
            if (users.Count() == 0)
            {
                _databaseContext.Users.Add(user);
                await _databaseContext.SaveChangesAsync();
            }
            else
            {
                user = users.First();
            }

            // Provide the authentication token
            JwtSecurityToken accessToken = IssueJwtToken(user.Id.ToString(), out DateTime expiration);

            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "access_token", new JwtSecurityTokenHandler().WriteToken(accessToken) },
                { "expiration", expiration.ToString("o") },
                { "refresh_token", refreshToken },
                { "provider", provider }
            };

            if (mobile)
            {
                string parameters = string.Join("&", tokens.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}"));
                return Redirect($"{_appScheme}://#{parameters}");
            }
            else
            {
                return Ok(tokens);
            }
        }
        
        [HttpPost]
        [Route("refresh/{provider}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Refresh(string provider, [FromBody] JsonElement payload)
        {
            string refreshToken = payload.GetString("refresh_token");

            // Check that the provider is registered and a token has been provided
            if (!_providers.Contains(provider, StringComparer.OrdinalIgnoreCase) || refreshToken == null || refreshToken == string.Empty)
            {
                return BadRequest();
            }

            if (string.Equals(provider, "Facebook", StringComparison.OrdinalIgnoreCase))
            {
                using (HttpClient client = new HttpClient())
                {
                    // Query facebook for token validity
                    HttpResponseMessage response = await client.GetAsync($"{_facebookDebugTokenUrl}?input_token={refreshToken}&access_token={_facebookAppToken}");
                    dynamic json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    
                    if (json.data.app_id != _facebookAppId || json.data.is_valid == false)
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        // Issue a new JWT
                        string facebookId = json.data.user_id;

                        IEnumerable<User> users = from user in _databaseContext.Users
                                                  where user.FacebookId == facebookId
                                                  select user;

                        JwtSecurityToken accessToken = IssueJwtToken(users.First().Id.ToString(), out DateTime expiration);

                        Dictionary<string, string> token = new Dictionary<string, string>
                        {
                            { "access_token", new JwtSecurityTokenHandler().WriteToken(accessToken) },
                            { "expiration", expiration.ToString("o") }
                        };

                        return Ok(token);
                    }
                }
            }
            else
            {
                return BadRequest();
            }
        }
        
        private JwtSecurityToken IssueJwtToken(string userId, out DateTime expiration)
        {
            expiration = DateTime.UtcNow.AddHours(2);

            Claim[] claims = new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSignKey));
            SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(_jwtIssuer, _jwtAudience, claims, expires: expiration, signingCredentials: signIn);
        }

    }
}
