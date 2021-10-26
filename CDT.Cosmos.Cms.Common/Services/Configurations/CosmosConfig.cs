using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Common.Services.Configurations
{
    /// <summary>
    ///     Cosmos configuration model
    /// </summary>
    public class CosmosConfig
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public CosmosConfig()
        {
            AuthenticationConfig = new AuthenticationConfig();
            CdnConfig = new CdnConfig();
            SqlConnectionStrings = new List<SqlConnectionString>();
            EditorUrls = new List<EditorUrl>();
            SiteSettings = new SiteSettings();
            StorageConfig = new StorageConfig();
        }

        /// <summary>
        ///     Primary cloud for this installation.
        /// </summary>
        [Display(Name = "Primary Cloud")]
        [UIHint("CloudProvider")]
        public string PrimaryCloud { get; set; }

        /// <summary>
        ///     Authentication
        /// </summary>
        public AuthenticationConfig AuthenticationConfig { get; set; }

        /// <summary>
        ///     CDN Configuration
        /// </summary>
        public CdnConfig CdnConfig { get; set; }

        /// <summary>
        ///     Editor Urls
        /// </summary>
        public List<EditorUrl> EditorUrls { get; set; }

        /// <summary>
        ///     Database connection strings
        /// </summary>
        public List<SqlConnectionString> SqlConnectionStrings { get; set; }

        /// <summary>
        ///     Google Cloud service authentication configuration
        /// </summary>
        public GoogleCloudAuthConfig GoogleCloudAuthConfig { get; set; }

        /// <summary>
        ///     SendGrid configuration
        /// </summary>
        public SendGridConfig SendGridConfig { get; set; }

        /// <summary>
        ///     Site-wide settings
        /// </summary>
        public SiteSettings SiteSettings { get; set; }

        /// <summary>
        ///     Blob service configuration
        /// </summary>
        public StorageConfig StorageConfig { get; set; }

        /// <summary>
        ///     Environment Variable Name
        /// </summary>
        public string EnvironmentVariable { get; set; } = "";

        /// <summary>
        ///     Secret key used for JWT authenticated communication between editors.
        /// </summary>
        [RegularExpression(@"^[0-9, a-z, A-Z]{32,32}$",
            ErrorMessage = "Must have at least 32 random numbers and letters.")]
        public string SecretKey { get; set; }
    }
}