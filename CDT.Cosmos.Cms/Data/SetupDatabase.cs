﻿using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Services.Configurations;
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

            if (_options.Value.SqlConnectionStrings.Count > 1)
            {
                var primary = _options.Value.SqlConnectionStrings.FirstOrDefault(f => f.IsPrimary);

                var synContext = new SqlDbSyncContext(_options);
                _ = await synContext.SyncDatabases(Dotmim.Sync.Enumerations.SyncDirection.Bidirectional, new System.Threading.CancellationToken());
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

                if (!dbContext.Roles.Any())
                {
                    // Setup roles if they don't already exist. //
                    using var roleManager = GetRoleManager(dbContext);
                    await roleManager.CreateAsync(new IdentityRole("Administrators"));
                    await roleManager.CreateAsync(new IdentityRole("Editors"));
                    await roleManager.CreateAsync(new IdentityRole("Authors"));
                    await roleManager.CreateAsync(new IdentityRole("Reviewers"));
                    await roleManager.CreateAsync(new IdentityRole("Team Members"));
                }
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
               var dbContext = new ApplicationDbContext(sqlBulder.Options);
                dbContext.LoadSyncContext(new SqlDbSyncContext(_options));
            }

            return new ApplicationDbContext(sqlBulder.Options);
        }
    }
}
