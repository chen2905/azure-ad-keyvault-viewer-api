using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecretsController : ControllerBase
    {
        private readonly SecretClient _secretClient;
        private readonly string _env = "";
        public SecretsController(IConfiguration configuration)
        {
            // Get Key Vault name from appsettings.json
            var keyVaultName = configuration["AzureKeyVault:KeyVaultName"];
            var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";
            _env = configuration["Env"];
            // Initialize SecretClient using DefaultAzureCredential
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSecrets()
        {
            try
            {
                var secrets = new Dictionary<string, string>();
                var userName = User.Identity.Name; // This will get the username from the ClaimsPrincipal (context.User)
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { message = "User is not authorized to view secrets." });
                }
                string filter = string.Empty;

                if (userName.Contains("developer", StringComparison.OrdinalIgnoreCase))
                {
                    filter = "developer";
                }
                else if (userName.Contains("admin", StringComparison.OrdinalIgnoreCase))
                {
                    filter = "admin";
                }
                else
                {
                    filter = "user";
                }



                // List secrets in the Key Vault
                await foreach (var secretProperty in _secretClient.GetPropertiesOfSecretsAsync())
                {
                    // Fetch the secret using its name
                    var secretResponse = await _secretClient.GetSecretAsync(secretProperty.Name);

                    // Extract the KeyVaultSecret object from the response
                    var secret = secretResponse.Value;

                    // Use secret.Name and secret.Value
                    if (secret.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        secrets[secret.Name] = secret.Value + " from " +_env;
                    }
                }

                return Ok(secrets);
            }
            catch (Exception ex)
            {
                // Return error details
                return StatusCode(500, new { message = "Error retrieving secrets", details = ex.Message });
            }
        }

    }
}
