using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CDT.Akamai.Tests
{
    internal class ConfigUtilities
    {
        //private const string EditorRoleName = "Editors";
        private static IConfiguration _configuration;

        private static string GetKeyValue(IConfigurationRoot config, string key)
        {
            var data = config[key];
            if (string.IsNullOrEmpty(data))
            {
                data = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(data))
                {
                    data = Environment.GetEnvironmentVariable(key.ToUpper());
                }
            }
            return data;
        }

        internal static IConfiguration GetConfig()
        {
            if (_configuration != null) return _configuration;

            // the type specified here is just so the secrets library can 
            // find the UserSecretId we added in the csproj file
            var jsonConfig = Path.Combine(Environment.CurrentDirectory, "appsettings.json");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonConfig, true) // Lowest priority - put here
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true); // User secrects override all - put here

            var config = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(2), 10);

            // From either local secrets or app config, get connection info for Azure Vault.
            var clientId = GetKeyValue(config, "CosmosAzureVaultClientId");

            var key = GetKeyValue(config, "CosmosAzureVaultClientSecret");

            var vaultUrl = GetKeyValue(config, "CosmosAzureVaultUrl");

            builder.AddAzureKeyVault(vaultUrl, clientId, key);
            _configuration = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(2), 10);

            return _configuration;
        }
    }
}