using CDT.Cosmos.Cms.Common.Services.Configurations;
using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Common.Data
{
    /// <summary>
    ///     SQL Database Synchronization Context
    /// </summary>
    /// <remarks>This context keeps multiple databases in synchronization with the primary database.</remarks>
    public class SqlDbSyncContext
    {
        private readonly IOptions<CosmosConfig> _cosmosOptions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="options"></param>
        public SqlDbSyncContext(IOptions<CosmosConfig> options)
        {
            _cosmosOptions = options;
        }

        /// <summary>
        ///     Determine if this service is configured
        /// </summary>
        /// <returns></returns>
        public bool IsConfigured()
        {
            return
                _cosmosOptions.Value !=
                null // There must be a Cosmos cproviders           && _cosmosOptions.Value.SqlConnectionStrings != null // There must be SQL connection strings
                && _cosmosOptions.Value.SqlConnectionStrings.Count > 0 &&
                _cosmosOptions.Value.SqlConnectionStrings.Count > 1 &&
                _cosmosOptions.Value.SqlConnectionStrings.Any(a =>
                    a.IsPrimary); // For more than one connection, there must be at least one primary.
        }

        /// <summary>
        ///     Detects if migrations need to be applied
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> PendingMigrations()
        {
            var migrations = new List<string>();
            foreach (var connection in _cosmosOptions.Value.SqlConnectionStrings)
            {
                var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

                builder.UseSqlServer(connection.ToString());

                using (var context = new ApplicationDbContext(builder.Options))
                {
                    var pending = await context.Database.GetPendingMigrationsAsync();

                    if (pending != null && pending.Any()) migrations.AddRange(pending);
                }
            }

            return migrations.Distinct().ToList();
        }

        /// <summary>
        ///     Applies pending migrations.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> ApplyPendingMigrations()
        {
            var migrations = new List<string>();

            if (_cosmosOptions.Value.SiteSettings.AllowSetup ?? false)
                foreach (var connection in _cosmosOptions.Value.SqlConnectionStrings)
                {
                    var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

                    builder.UseSqlServer(connection.ToString());

                    using (var context = new ApplicationDbContext(builder.Options))
                    {
                        var pending = context.Database.GetPendingMigrations().ToList();

                        if (pending.Any()) await context.Database.MigrateAsync();
                    }
                }

            return migrations.Distinct().ToList();
        }

        /// <summary>
        ///     Gets the synchronization setup
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private SyncSetup GetSyncSetup(string[] entities, SyncDirection direction)
        {
            var syncSetup = new SyncSetup(entities);

            foreach (var table in syncSetup.Tables) table.SyncDirection = direction;

            return syncSetup;
        }

        /// <summary>
        ///     Gets a list of all the tables that need to be synchronized
        /// </summary>
        /// <returns></returns>
        public static string[] GetSyncAllTables()
        {
            var list = new List<string> { "__EFMigrationsHistory" };
            list.AddRange(GetSyncAccountTables());
            list.AddRange(GetSyncCosmosTables());
            return list.ToArray();
        }

        /// <summary>
        ///     Gets a list of user account tables in database
        /// </summary>
        /// <returns></returns>
        public static string[] GetSyncAccountTables()
        {
            return new[]
            {
                "AspNetRoleClaims",
                "AspNetRoles",
                "AspNetUserClaims",
                "AspNetUserLogins",
                "AspNetUserRoles",
                "AspNetUsers",
                "AspNetUserTokens"
            };
        }

        /// <summary>
        ///     Gets a list of Cosmos specific tables that need to be synchronized
        /// </summary>
        /// <returns></returns>
        public static string[] GetSyncCosmosTables()
        {
            return new[]
            {
                "ArticleLogs",
                "ArticleLocks",
                "Articles",
                "Layouts",
                "MenuItems",
                "TeamMembers",
                "Teams",
                "Templates"
            };
        }


        /// <summary>
        ///     Get primary provider
        /// </summary>
        /// <returns></returns>
        public SqlSyncProvider GetPrimaryProvider()
        {
            if (_cosmosOptions.Value == null || !_cosmosOptions.Value.SqlConnectionStrings.Any())
                return null;

            var primaryConnection = _cosmosOptions.Value.SqlConnectionStrings.FirstOrDefault(f => f.IsPrimary);

            if (primaryConnection == null)
                throw new Exception("A primary SQL database connect has not been defined.");

            return new SqlSyncProvider(primaryConnection.ToString());
        }

        /// <summary>
        ///     Gets secondary providers
        /// </summary>
        /// <returns></returns>
        public List<SqlSyncProvider> GetSecondaryProviders()
        {
            return _cosmosOptions.Value.SqlConnectionStrings.Where(f => f.IsPrimary == false)
                .Select(s => new SqlSyncProvider(s.ToString())).ToList();
        }

        /// <summary>
        ///     Synchronizes the primary database with the secondaries.
        /// </summary>
        /// <param name="direction">
        ///     <see cref="SyncDirection" />
        /// </param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<SyncResult>> SyncDatabases(SyncDirection direction, CancellationToken token)
        {
            if (_cosmosOptions.Value == null || !_cosmosOptions.Value.SqlConnectionStrings.Any())
                return new List<SyncResult>();

            return await SyncDatabases(GetSyncAllTables(), direction, token);
        }

        /// <summary>
        ///     Executes the synchronization
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="direction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<SyncResult>> SyncDatabases(string[] entities, SyncDirection direction,
            CancellationToken cancellationToken)
        {
            if (_cosmosOptions.Value == null || !_cosmosOptions.Value.SqlConnectionStrings.Any())
                return new List<SyncResult>();

            var syncSetup = GetSyncSetup(entities, direction);

            var serverProvider = GetPrimaryProvider();
            var secondaries = GetSecondaryProviders();

            var results = new List<SyncResult>();

            foreach (var provider in secondaries)
            {
                using var agent = new SyncAgent(provider, serverProvider, syncSetup);

                //agent.RemoteOrchestrator.OnTableChangesApplying(e =>
                //{
                //    var command = e.Connection.CreateCommand();
                //    if (e.Transaction != null)
                //    {
                //        command.Transaction = e.Transaction;
                //    }
                //    command.CommandText = $"SET IDENTITY_INSERT {e.Table.TableName} ON";
                //    try
                //    {
                //        //var t = command.ExecuteScalarAsync(cancellationToken);
                //        //t.Wait(cancellationToken);
                //    }
                //    catch (Exception e1)
                //    {
                //        var d = e1;
                //    }
                //});
                //agent.RemoteOrchestrator.OnTableChangesApplied(e =>
                //{
                //    var command = e.Connection.CreateCommand();
                //    if (e.Transaction != null)
                //    {
                //        command.Transaction = e.Transaction;
                //    }
                //    command.CommandText = $"SET IDENTITY_INSERT {e.TableChangesApplied.TableName} OFF";
                //    try
                //    {
                //        //var t = command.ExecuteScalarAsync(cancellationToken);
                //        //t.Wait(cancellationToken);
                //    }
                //    catch (Exception e2)
                //    {
                //        var d = e2;
                //    }
                //});

                results.Add(await agent.SynchronizeAsync());
                agent.Dispose();
            }

            return results;
        }
    }
}