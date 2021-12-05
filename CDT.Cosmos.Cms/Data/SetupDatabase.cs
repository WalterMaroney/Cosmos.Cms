using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Data.Logic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Data
{
    /// <summary>
    /// Sets up the database(s) for a Cosmos installation.
    /// </summary>
    public class SetupDatabase
    {
        private IOptions<CosmosConfig> _options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public SetupDatabase(IOptions<CosmosConfig> options)
        {
            _options = options;
        }

        /// <summary>
        /// Create database schema or applies database migrations for an upgrade.
        /// </summary>
        /// <returns>Number of migrations applied.</returns>
        public async Task<int> CreateOrUpdateSchema()
        {
            var migrations = new List<string>();

            if (_options.Value.SiteSettings.AllowSetup ?? false)
            {
                foreach (var connectionString in _options.Value.SqlConnectionStrings)
                {
                    using (var dbContext = GetDbContext(connectionString.ToString()))
                    {
                        migrations.AddRange(await dbContext.Database.GetPendingMigrationsAsync());
                        // Create database if it does not exist, and create schema
                        await dbContext.Database.MigrateAsync();
                    }
                }
            }

            return migrations.Distinct().Count();
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Items are added to the database if not already present: page templates,
        /// layouts, font icons, standard roles.
        /// </remarks>
        public async Task SeedDatabase()
        {
            if (_options.Value.SiteSettings.AllowSetup ?? false)
            {
                var conn = _options.Value.SqlConnectionStrings.FirstOrDefault();

                using var dbContext = GetDbContext(conn.ToString(), true);

                if (!dbContext.FontIcons.Any())
                {
                    var tblSeeding = new TableSeeding(dbContext);

                    // Load fonts if they don't already exist.
                    await tblSeeding.LoadFontIcons();
                }

                // Load default layouts if they don't already exist.
                if (!dbContext.Layouts.Any())
                {
                    // Load default layout and pages
                    var layoutUtilities = new LayoutUtilities();

                    var layouts = layoutUtilities.GetCommunityLayouts();
                    dbContext.Layouts.AddRange(layouts);
                    await dbContext.SaveChangesAsync();

                    foreach (var layout in layouts)
                    {
                        var templates = layoutUtilities.GetCommunityTemplatePages(layout.CommunityLayoutId);

                        foreach (var t in templates)
                        {
                            t.LayoutId = layout.Id;
                        }

                        dbContext.Templates.AddRange(templates);

                        await dbContext.SaveChangesAsync();
                    }
                }

                // Add home page if they don't already exist.
                if (!dbContext.Articles.Any())
                {
                    var defaultLayoutId = dbContext.Layouts.FirstOrDefault(f => f.IsDefault).Id;

                    var homeTemplate = dbContext.Templates.FirstOrDefault(f => f.LayoutId == defaultLayoutId && f.Title.ToLower() == "home page");

                    dbContext.Articles.Add(new Article()
                    {
                        ArticleNumber = 1,
                        Content = homeTemplate.Content,
                        LayoutId = defaultLayoutId,
                        Published = DateTime.UtcNow.AddMinutes(-5),
                        StatusCode = (int)StatusCodeEnum.Active,
                        Title = "Home Page",
                        UrlPath = "root",
                        Updated = DateTime.UtcNow.AddMinutes(-5),
                        VersionNumber = 1
                    });

                    await dbContext.SaveChangesAsync();
                }
                

                // Setup roles if they don't already exist. //
                using var roleManager = GetRoleManager(dbContext);

                if (!await roleManager.RoleExistsAsync("Administrators"))
                    await roleManager.CreateAsync(new IdentityRole("Administrators"));

                if (!await roleManager.RoleExistsAsync("Editors"))
                    await roleManager.CreateAsync(new IdentityRole("Editors"));

                if (!await roleManager.RoleExistsAsync("Authors"))
                    await roleManager.CreateAsync(new IdentityRole("Authors"));

                if (!await roleManager.RoleExistsAsync("Reviewers"))
                    await roleManager.CreateAsync(new IdentityRole("Reviewers"));

                if (!await roleManager.RoleExistsAsync("Team Members"))
                    await roleManager.CreateAsync(new IdentityRole("Team Members"));
            }
        }

        private RoleManager<IdentityRole> GetRoleManager(ApplicationDbContext dbContext)
        {
            var userStore = new RoleStore<IdentityRole>(dbContext);
            var userManager = new RoleManager<IdentityRole>(userStore, null, null, null, null);
            return userManager;
        }

        private ApplicationDbContext GetDbContext(string connectionString, bool loadSyncContext = false)
        {
            var sqlBulder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqlBulder.UseSqlServer(connectionString,
                opts =>
                    opts.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));

            if (loadSyncContext)
            {
                return new ApplicationDbContext(sqlBulder.Options, _options);
            }

            return new ApplicationDbContext(sqlBulder.Options);
        }
    }
}
