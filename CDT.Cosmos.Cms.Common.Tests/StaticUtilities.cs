using CDT.Cosmos.BlobService;
using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Services;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Controllers;
using CDT.Cosmos.Cms.Data.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Common.Tests
{
    public static class TestUsers
    {
        public const string Foo = "foo@foo.com";
        public const string Teamfoo1 = "teamfoo1@foo.com";
        public const string Teamfoo2 = "teamfoo2@foo.com";
    }

    public class Utilities
    {
        private IOptions<CosmosConfig> _cosmosOptions;

        public Utilities()
        {
            _cosmosOptions = GetCosmosConfigOptions();
        }

        /// <summary>
        ///     Gets the application Db context with sync context loaded.
        /// </summary>
        /// <returns></returns>
        public ApplicationDbContext GetApplicationDbContext(SqlConnectionString connectionString = null)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var cosmosConfig = GetCosmosConfigOptions();
            var primary = connectionString ?? cosmosConfig.Value.SqlConnectionStrings.FirstOrDefault(f => f.IsPrimary);

            Debug.Assert(primary != null, nameof(primary) + " != null");
            builder.UseSqlServer(primary.ToString());

            var context = new ApplicationDbContext(builder.Options);
            var syncContext = GetSqlDbSyncContext();
            context.LoadSyncContext(syncContext);

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //context.Database.Migrate();

            return context;
        }

        public ArticleLogic GetArticleLogic(ApplicationDbContext dbContext, bool isEditor = true)
        {
            var options = GetCosmosConfigOptions();
            options.Value.SiteSettings.IsEditor = false;
            return new ArticleLogic(
                dbContext,
                options);
        }

        public ArticleLogic GetArticleLogicNoRedis(ApplicationDbContext dbContext, bool isEditor = true)
        {
            var options = GetCosmosConfigOptions();
            options.Value.SiteSettings.IsEditor = false;
            return new ArticleLogic(
                dbContext,
                options);
        }

        public ArticleLogic GetArticleLogicNoRedisNoSql(bool isEditor = true)
        {
            var options = GetCosmosConfigOptions();
            options.Value.SqlConnectionStrings[0].InitialCatalog = "messedup";
            options.Value.SiteSettings.IsEditor = false;
            var dbContext = this.GetApplicationDbContext(options.Value.SqlConnectionStrings[0]);
            return new ArticleLogic(
                dbContext,
                options);
        }

        public ArticleEditLogic GetArticleEditLogic(ApplicationDbContext dbContext, bool allSetupOn = true)
        {
            //var siteOptions = Options.Create(new SiteSettings
            //{
            //    ReadWriteMode = readWriteModeOn,
            //    AllowSetup = allSetupOn
            //});

            var options = GetCosmosConfigOptions();

            options.Value.SiteSettings.AllowSetup = allSetupOn;
            options.Value.SiteSettings.AllowReset = allSetupOn;

            var syncContext = GetSqlDbSyncContext();

            return new ArticleEditLogic(
                dbContext,
                options, syncContext);
        }

        public IFormFile GetFormFile(string fileName)
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes($"This is a file for {fileName}."));
            return new FormFile(stream, 0, stream.Length, fileName, fileName);
        }

        public SqlDbSyncContext GetSqlDbSyncContext()
        {
            return new(GetCosmosConfigOptions());
        }

        /// <summary>
        ///     Use this context to test database install
        /// </summary>
        /// <returns></returns>
        public SqlDbSyncContext GetInstallTestSqlDbSyncContext()
        {
            var config = GetCosmosConfigOptions();

            config.Value.SqlConnectionStrings[0].InitialCatalog = "cosmos-mc-unittest-install";
            config.Value.SqlConnectionStrings[0].IsPrimary = true;

            config.Value.SqlConnectionStrings[1].InitialCatalog = "cosmos-mc-unittest-install-secondary";
            config.Value.SqlConnectionStrings[1].IsPrimary = false;

            return new SqlDbSyncContext(config);
        }

        /// <summary>
        ///     Compares date/times (not kind)
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <returns></returns>
        public bool DateTimesAreEqual(DateTimeOffset expected, DateTimeOffset actual)
        {
            //
            // The date/time should stay exactly the same after the save.
            //
            var isValid = expected.Year == actual.Year &&
                          expected.Month == actual.Month &&
                          expected.Day == actual.Day &&
                          expected.Hour == actual.Hour &&
                          expected.Minute == actual.Minute &&
                          expected.Second == actual.Second;

            return isValid;
        }

        #region CONFIGURATION MOCK UPS

        //private const string EditorRoleName = "Editors";
        private IConfiguration _configuration;

        private CosmosStartup _cosmosBootConfig;
        //private  ClientSecretCredential _clientSecretCredential;

        public IOptions<CosmosConfig> GetCosmosConfigOptions(bool awsSecrets = false, string secretKeyName = "")
        {
            if (_cosmosOptions == null)
            {
                var bootConfig = GetCosmosBootConfig();

                if (awsSecrets) bootConfig.UseAzureVault = false;

                if (!string.IsNullOrEmpty(secretKeyName)) bootConfig.SecretName = secretKeyName;

                var section = GetConfig().GetSection(bootConfig.SecretName);
                var model = JsonConvert.DeserializeObject<CosmosConfig>(section.Value);
                _cosmosOptions = Options.Create(model);
            }

            return _cosmosOptions;
        }

        public Guid GetCacheId()
        {
            return new("4dc3249c-64a9-4731-babc-fe5b2b1a7af7");
        }

        private string GetKeyValue(IConfigurationRoot config, string key)
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

        internal IConfiguration GetConfig()
        {
            if (_configuration != null) return _configuration;

            // the type specified here is just so the secrets library can 
            // find the UserSecretId we added in the csproj file
            var jsonConfig = Path.Combine(Environment.CurrentDirectory, "appsettings.json");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonConfig, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true); // User secrets override all - put here

            var config = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(1));

            // From either local secrets or app config, get connection info for Azure Vault.
            var tenantId = GetKeyValue(config, "CosmosAzureVaultTenantId");

            var clientId = GetKeyValue(config, "CosmosAzureVaultClientId");

            var key = GetKeyValue(config, "CosmosAzureVaultClientSecret");

            var vaultUrl = GetKeyValue(config, "CosmosAzureVaultUrl");

            var secretName = GetKeyValue(config, "CosmosSecretName");

            var strCosmosUseAwsSecretsMgr = GetKeyValue(config, "CosmosUseAwsSecretsMgr");

            _ = bool.TryParse(strCosmosUseAwsSecretsMgr, out var useAwsSecretsManager);

            var awsSecretsRegion = GetKeyValue(config, "CosmosAwsSecretsRegion");

            var awsKeyId = GetKeyValue(config, "CosmosAwsKeyId");

            var awsSecretAccessKey = GetKeyValue(config, "CosmosAwsSecretAccessKey");

            _cosmosBootConfig = new CosmosStartup
            {
                AllowConfigEdit = true,
                AzureVaultClientId = clientId,
                AzureVaultClientSecret = key,
                CosmosAzureVaultUrl = vaultUrl,
                SecretName = secretName,
                UseAzureVault = true,
                UseAwsSecretsMgr = useAwsSecretsManager,
                AwsSecretsRegion = awsSecretsRegion,
                AwsSecretAccessKey = awsSecretAccessKey,
                AwsKeyId = awsKeyId
            };


            //_clientSecretCredential = new ClientSecretCredential(
            //    _cosmosBootConfig.AzureVaultTenantId,
            //    _cosmosBootConfig.AzureVaultClientId,
            //    _cosmosBootConfig.AzureVaultClientSecret);

            builder.AddAzureKeyVault(vaultUrl, clientId, key);

            _configuration = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(1));

            return _configuration;
        }

        public CosmosStartup GetCosmosBootConfig()
        {
            if (_cosmosBootConfig == null)
                GetConfig();

            return _cosmosBootConfig;
        }

        //public  ClientSecretCredential GetClientSecretCredential()
        //{
        //    return _clientSecretCredential;
        //}

        //public  SimpleProxyConfigs GetSimpleProxyConfigs()
        //{
        //    return new SimpleProxyConfigs()
        //    {
        //        Configs = new ProxyConfig[]
        //         {
        //             new ProxyConfig()
        //                {
        //                    ContentType = "text/html; charset=UTF-8",
        //                    Method = "GET",
        //                    Name = "GoogleAnonymous",
        //                    Password = "",
        //                    UriEndpoint = "https://www.google.com",
        //                    UserName = "",
        //                    Roles = new string[] { "Anonymous" }
        //                },
        //             new ProxyConfig()
        //                {
        //                    ContentType = "application/x-www-form-urlencoded",
        //                    Method = "GET",
        //                    Name = "TableauAnonymous",
        //                    Password = "",
        //                    UriEndpoint = "https://worldtimeapi.org/api/timezone",
        //                    UserName = "",
        //                    Roles = new string[] { "Anonymous" }
        //                },
        //             new ProxyConfig()
        //                {
        //                    ContentType = "application/x-www-form-urlencoded",
        //                    Method = "GET",
        //                    Name = "TableauAuthenticated",
        //                    Password = "",
        //                    UriEndpoint = "https://worldtimeapi.org/api/timezone",
        //                    UserName = "",
        //                    Roles = new string[] { "Authenticated" }
        //                },
        //             new ProxyConfig()
        //                {
        //                    ContentType = "application/x-www-form-urlencoded",
        //                    Method = "GET",
        //                    Name = "TableauAdministrators",
        //                    Password = "",
        //                    UriEndpoint = "https://worldtimeapi.org/api/timezone",
        //                    UserName = "",
        //                    Roles = new string[] { "Administrators" }
        //                }
        //         }
        //    };
        //}

        #endregion

        #region CONTROLLER MOCK UPS

        #region CONTROLLER MOCK CONTEXT

        public ILogger<T> GetLogger<T>()
        {
            return new Logger<T>(new NullLoggerFactory());
        }

        public HttpContext GetMockContext(ClaimsPrincipal user)
        {
            return new DefaultHttpContext
            {
                FormOptions = new FormOptions(),
                Items = new Dictionary<object, object>(),
                RequestAborted = default,
                RequestServices = null!,
                ServiceScopeFactory = null!,
                Session = null!,
                TraceIdentifier = null!,
                User = user
            };
        }

        #endregion

        public EditorController GetEditorController(ClaimsPrincipal user, bool allowSetup = false,
            IOptions<CosmosConfig> config = null)
        {
            var logger = new Logger<EditorController>(new NullLoggerFactory());

            if (config == null)
            {
                config = GetCosmosConfigOptions();
                config.Value.SiteSettings.AllowSetup = allowSetup;
            }

            var dbContext = GetApplicationDbContext();

            var controller = new EditorController(
                logger,
                dbContext,
                GetUserManager(),
                GetArticleEditLogic(dbContext),
                config,
                GetSqlDbSyncContext())
            {
                ControllerContext = { HttpContext = GetMockContext(user) }
            };
            return controller;
        }

        public HomeController GetHomeController(ClaimsPrincipal user, bool allowSetup, IOptions<CosmosConfig> siteOptions = null)
        {
            if (siteOptions == null)
            {
                siteOptions = GetCosmosConfigOptions();
            }
            siteOptions.Value.SiteSettings.AllowSetup = allowSetup;

            var dbContext = GetApplicationDbContext();

            var logger = new Logger<HomeController>(new NullLoggerFactory());
            //var blobOptions = Options.Create(new AzureBlobServiceConfig());
            var controller = new HomeController(logger,
                siteOptions,
                Options.Create(GetCosmosBootConfig()),
                dbContext,
                GetArticleEditLogic(dbContext)
            )
            {
                ControllerContext = { HttpContext = GetMockContext(user) }
            };
            return controller;
        }

        public SetupController GetSetupController()
        {
            var logger = new Logger<SetupController>(new NullLoggerFactory());
            //var blobOptions = Options.Create(new AzureBlobServiceConfig());
            //var claimsPrincipal = GetPrincipal(TestUsers.Foo).Result;
            //var redisConfig = Options.Create(GetRedisContextConfig());
            //var dbContext = GetApplicationDbContext();

            var configuration = GetConfig();

            var startup = new CosmosStartup(configuration);

            _ = startup.TryRun(out var options);

            //if (startup.HasErrors || options == null || options.Value == null || options.Value.SqlConnectionStrings.Any() == false)
            //{
            //    var errors = startup.Diagnostics.Where(w => w.Success == false).ToArray();
            //    var builder = new StringBuilder();
            //    foreach (var error in errors)
            //    {
            //        builder.AppendLine(error.Message);
            //    }
            //    throw new Exception(builder.ToString());
            //}

            var controller = new SetupController(
                logger,
                options,
                startup.GetStatus()
                );

            return controller;
        }

        public TeamsController GetTeamsController()
        {
            //var logger = new Logger<TeamsController>(new NullLoggerFactory());
            //var blobOptions = Options.Create(new AzureBlobServiceConfig());
            var claimsPrincipal = GetPrincipal(TestUsers.Foo).Result;

            var siteOptions = GetCosmosConfigOptions();
            siteOptions.Value.SiteSettings.AllowSetup = false;

            var dbContext = GetApplicationDbContext();

            var controller = new TeamsController(
                GetCosmosConfigOptions(),
                dbContext,
                GetUserManager(),
                GetArticleEditLogic(dbContext),
                GetSqlDbSyncContext())
            {
                ControllerContext = { HttpContext = GetMockContext(claimsPrincipal) }
            };
            return controller;
        }

        #endregion

        #region SERVICE MOCK UPS

        public StorageContext GetStorageContext()
        {
            var service = new StorageContext(GetCosmosConfigOptions(), GetMemoryCache());
            return service;
        }

        public StorageContext GetStorageContext_AmazonPrimary()
        {
            var config = GetCosmosConfigOptions();
            config.Value.PrimaryCloud = "amazon";
            return new StorageContext(config, GetMemoryCache());
        }

        public StorageContext GetStorageContext_AzurePrimary()
        {
            var config = GetCosmosConfigOptions();
            config.Value.PrimaryCloud = "azure";
            return new StorageContext(config, GetMemoryCache());
        }

        private IMemoryCache GetMemoryCache()
        {
            var options = Options.Create(new MemoryCacheOptions()
            {
                SizeLimit = 20000000 // 20 megabytes decimal
            });
            return new MemoryCache(options);
        }

        public IEmailSender GetEmailSender()
        {
            var emailSender = new EmailSender(_cosmosOptions);
            return emailSender;
        }


        #endregion

        #region USER MOCK UPS

        public async Task<IdentityUser> GetIdentityUser(string emailAddress)
        {
            using var userManager = GetUserManager();
            var user = await userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                await userManager.CreateAsync(new IdentityUser(emailAddress)
                {
                    Email = emailAddress,
                    Id = Guid.NewGuid().ToString(),
                    EmailConfirmed = true
                });
                user = await userManager.FindByEmailAsync(emailAddress);
            }

            return user;
        }

        public async Task<ClaimsPrincipal> GetPrincipal(string emailAddress)
        {
            var user = await GetIdentityUser(emailAddress);
            using var userManager = GetUserManager();
            var claims = await userManager.GetClaimsAsync(user);

            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

            var roles = await userManager.GetRolesAsync(user);

            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));

            return principal;
        }

        public UserManager<IdentityUser> GetUserManager()
        {
            var userStore = new UserStore<IdentityUser>(GetApplicationDbContext());
            var userManager = new UserManager<IdentityUser>(userStore, null, new PasswordHasher<IdentityUser>(), null,
                null, null, null, null, GetLogger<UserManager<IdentityUser>>());
            return userManager;
        }

        public RoleManager<IdentityRole> GetRoleManager()
        {
            var userStore = new RoleStore<IdentityRole>(GetApplicationDbContext());
            var userManager = new RoleManager<IdentityRole>(userStore, null, null, null, GetLogger<RoleManager<IdentityRole>>());
            return userManager;
        }

        #endregion
    }
}