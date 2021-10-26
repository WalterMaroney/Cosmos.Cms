using System.Linq;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CDT.Cosmos.Cms.Common.Tests
{
    [TestClass]
    public class A10AwsSecretsMgrTests
    {
        private static Utilities utils;

        //private static CosmosBootConfig bootConfig;
        private static IOptions<CosmosConfig> config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            utils = new Utilities();
            config = utils.GetCosmosConfigOptions(true, "HelloWorld");
        }

        [TestMethod]
        public void A_01_TestConnection()
        {
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.Value);
            Assert.IsTrue(config.Value.StorageConfig.AzureConfigs.Any());
            Assert.IsTrue(config.Value.SqlConnectionStrings.Any());
        }
    }
}