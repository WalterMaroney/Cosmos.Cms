using CDT.Cosmos.Cms.Common.Data;
using Dotmim.Sync.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Common.Tests
{
    [TestClass]
    public class A00EnvironmentTests
    {
        public static Utilities utils;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            utils = new Utilities();
        }

        [TestMethod]
        public async Task A00_SyncDatabases()
        {
            //
            // Setup context.
            //
            var dbContext = utils.GetApplicationDbContext();

            //
            // Wipe clean the database before starting.
            //
            if (dbContext.Database.GetAppliedMigrations().Any())
            {
                dbContext.TeamMembers.RemoveRange(dbContext.TeamMembers.ToList());
                dbContext.Teams.RemoveRange(dbContext.Teams.ToList());
                dbContext.ArticleLogs.RemoveRange(dbContext.ArticleLogs.ToList());
                dbContext.Articles.RemoveRange(dbContext.Articles.ToList());
                dbContext.Users.RemoveRange(dbContext.Users.ToList());
                dbContext.Roles.RemoveRange(dbContext.Roles.ToList());
                await dbContext.SaveChangesAsync();

                var syncContext = utils.GetSqlDbSyncContext();
                var results = await syncContext.SyncDatabases(SyncDirection.DownloadOnly, new CancellationToken());

                // The databases might be in sync, so there may be no results, but it should not be null.
                Assert.IsNotNull(results);
            }
        }

        [TestMethod]
        public async Task A01_InstallDatabases_Success()
        {
            var setupController = utils.GetSetupController();

            // Now recreate the database and schema
            await setupController.Database("NewInstall");

            var options = utils.GetCosmosConfigOptions();

            foreach (var connection in options.Value.SqlConnectionStrings)
            {
                var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
                builder.UseSqlServer(connection.ToString());
                using var dbContext = new ApplicationDbContext(builder.Options);
                // Create database if it does not exist, and schema
                var migrations = await dbContext.Database.GetAppliedMigrationsAsync();
                Assert.IsTrue(migrations.Count() > 1);
            }
        }

        [TestMethod]
        public async Task A02_TestSetup()
        {
            using (var roleManager = utils.GetRoleManager())
            {
                Assert.IsTrue(await roleManager.RoleExistsAsync("Editors"));
                Assert.IsTrue(await roleManager.RoleExistsAsync("Authors"));
                Assert.IsTrue(await roleManager.RoleExistsAsync("Reviewers"));
                Assert.IsTrue(await roleManager.RoleExistsAsync("Administrators"));
                Assert.IsTrue(await roleManager.RoleExistsAsync("Team Members"));
            }

            var foo = await utils.GetIdentityUser(TestUsers.Foo);

            using var userManager = utils.GetUserManager();

            var result = await userManager.AddToRoleAsync(foo, "Administrators");

            Assert.IsTrue(result.Succeeded);

            Assert.IsTrue(await userManager.IsInRoleAsync(foo, "Administrators"));
        }


        [TestMethod]
        public async Task A03_TestApplicationDbContext()
        {
            var syncContext = utils.GetSqlDbSyncContext();
            Assert.IsTrue(syncContext.IsConfigured());
            ;
            var dbContext = utils.GetApplicationDbContext();
            Assert.IsNotNull(dbContext);
            Assert.IsTrue(await dbContext.Database.CanConnectAsync());
        }

        [TestMethod]
        public void A04_TestPublisherHealth_Redis_Success()
        {
            var articleLogic = utils.GetArticleLogic(utils.GetApplicationDbContext());

            Assert.IsTrue(articleLogic.GetPublisherHealth());
        }

        [TestMethod]
        public void A05_TestPublisherHealth_SqlOnly_Success()
        {
            var articleLogic = utils.GetArticleLogicNoRedis(utils.GetApplicationDbContext());

            Assert.IsTrue(articleLogic.GetPublisherHealth());
        }
        
    }
}