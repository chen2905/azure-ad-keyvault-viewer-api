using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace azure_ad_keyvault_viewer_api.Middlewares
{
    public class AzureAdJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AzureAdJwtMiddleware> _logger;
        private readonly string _azureAdIssuer = "https://sts.windows.net/{tenant-id}/"; // Replace with your actual tenant ID

        public AzureAdJwtMiddleware(RequestDelegate next, ILogger<AzureAdJwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation($"JWT Token: {token}");

                 var principal = GetClaimsPrincipalFromToken(token);
                    if (principal != null)
                    {
                        context.User = principal;  // Set the user context with authenticated user
                        _logger.LogInformation($"User authenticated: {principal.Identity.Name}");
                    }
                    else
                    {
                        _logger.LogWarning("Invalid token, unable to extract claims.");
                    }
                
              
            }
            else
            {
                _logger.LogWarning("Authorization header missing or invalid.");
            }

            await _next(context);
        }

        

        private ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, jwtToken.Payload["unique_name"].ToString()), // Subject is usually unique_name or user identifier
                   
                    // You can extract other claims here as needed
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract claims: {ex.Message}");
                return null;
            }
        }
    }
}
