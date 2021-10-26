using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CDT.Cosmos.BlobService.Drivers;
using CDT.Cosmos.BlobService.Models;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CDT.Cosmos.BlobService.Tests
{
    [TestClass]
    public class A02_AzureDriverTests
    {
        private static CosmosConfig _cosmosConfig;

        private static string _fullPathTestFile;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new CosmosConfig
            {
                StorageConfig = new StorageConfig()
            };

            config.StorageConfig = StaticUtilities.GetCosmosConfig().Value.StorageConfig;

            _cosmosConfig = config;

            _fullPathTestFile = Path.Combine(context.DeploymentDirectory, DriverTestConstants.TestFile1);
        }

        [TestMethod]
        public async Task A01_GetList()
        {
            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            var blobs = await driver.GetObjectsAsync("");

            Assert.IsNotNull(blobs);
        }

        [TestMethod]
        public async Task A02_DeleteItems()
        {
            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            // Delete all blobs
            var blobs = await driver.DeleteFolderAsync("");

            // See if they are deleted
            var results = await driver.GetObjectsAsync("");

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public async Task A03_CreateFolderSuccess()
        {
            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            await driver.CreateFolderAsync(DriverTestConstants.FolderHelloWorld1);

            // Get all blobs
            var items = await driver.GetObjectsAsync("");
            Assert.AreEqual(1, items.Count);
        }

        [TestMethod]
        public async Task A04_CreateSubFolders()
        {
            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            await driver.CreateFolderAsync(DriverTestConstants.FolderHelloWorld1);

            await driver.CreateFolderAsync(DriverTestConstants.HelloWorld1SubDirectory1);

            await driver.CreateFolderAsync(DriverTestConstants.HelloWorldSubDirectory2);

            await driver.CreateFolderAsync(DriverTestConstants.HelloWorldSubdirectory2Subdirectory3);

            // Get all blobs
            var items = await driver.GetBlobItemsByPath("");
            Assert.AreEqual(4, items.Count);
        }

        [TestMethod]
        public async Task A05_GetSubFolders()
        {
            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            // Get all blobs
            var blobs = await driver.GetObjectsAsync("/hello-world-1/");

            Assert.AreEqual(3, blobs.Count);

            var subBlobs1 = await driver.GetObjectsAsync(DriverTestConstants.HelloWorld1SubDirectory1 + "/");
            Assert.AreEqual(1, subBlobs1.Count);

            var subBlobs2 = await driver.GetObjectsAsync(DriverTestConstants.HelloWorldSubDirectory2 + "/");
            Assert.AreEqual(2, subBlobs2.Count);
        }

        [TestMethod]
        public async Task A06_UploadFile()
        {
            await using var memStream = new MemoryStream();
            await using var fileStream = File.OpenRead(_fullPathTestFile);
            await fileStream.CopyToAsync(memStream);
            memStream.Position = 0;

            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            var fullPath = DriverTestConstants.HelloWorldSubdirectory2Subdirectory3 + "/" +
                           DriverTestConstants.TestFile1;

            var fileUploadMetadata = new FileUploadMetaData
            {
                UploadUid = Guid.NewGuid().ToString(),
                FileName = DriverTestConstants.TestFile1,
                RelativePath = fullPath.TrimStart('/'),
                ContentType = "image/jpeg",
                ChunkIndex = 0,
                TotalChunks = 1,
                TotalFileSize = memStream.Length
            };


            await driver.AppendBlobAsync(memStream.ToArray(), fileUploadMetadata, DateTimeOffset.UtcNow);

            var blob = await driver.GetBlobAsync(fileUploadMetadata.RelativePath);

            Assert.IsTrue(await blob.ExistsAsync());
        }

        [TestMethod]
        public async Task A07_GetAndCopyFile()
        {
            var source = DriverTestConstants.HelloWorldSubdirectory2Subdirectory3 + "/" + DriverTestConstants.TestFile1;
            var destination = DriverTestConstants.HelloWorldSubDirectory2 + "/" + DriverTestConstants.TestFile1;

            var driver = new AzureStorage(_cosmosConfig.StorageConfig.AzureConfigs.FirstOrDefault());

            var sourceObject = await driver.GetBlobAsync(source);

            await driver.CopyBlobAsync(source, destination);

            var destObject = await driver.GetBlobAsync(destination);

            Assert.IsNotNull(await destObject.ExistsAsync());

            Assert.AreNotEqual(sourceObject.Name, destObject.Name);

            var copiedBlob = await driver.GetBlobAsync(destination);

            var prop1 = await sourceObject.GetPropertiesAsync();
            var prop2 = await copiedBlob.GetPropertiesAsync();

            Assert.AreEqual(prop1.Value.ContentLength, prop2.Value.ContentLength);
        }
    }
}