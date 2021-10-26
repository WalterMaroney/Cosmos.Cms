using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{

    public class UpgradeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EditorController> _logger;
        private readonly IOptions<CosmosConfig> _options;
        private readonly SqlDbSyncContext _syncContext;

        public UpgradeController(IConfiguration configuration, ILogger<EditorController> logger,
            IOptions<CosmosConfig> options,
            SqlDbSyncContext syncContext)
        {
            _configuration = configuration;
            _logger = logger;
            _options = options;
            _syncContext = syncContext;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (_options.Value.SiteSettings.AllowSetup ?? false)
            {
                var model = new UpgradeIndexViewModel
                {
                    PendingMigrations = await _syncContext.PendingMigrations()
                };
                return View(model);
            }

            return Unauthorized();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(UpgradeIndexViewModel model)
        {
            if (_options.Value.SiteSettings.AllowSetup ?? false)
                try
                {
                    if (ModelState.IsValid && model.Authorized && model.IsBackedUp & model.SoftwareUpgrade)
                    {
                        await _syncContext.ApplyPendingMigrations();
                        return RedirectToAction("UpgradeComplete", "Upgrade");
                    }

                    return View(model);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    throw;
                }

            return Unauthorized();
        }

        [AllowAnonymous]
        public IActionResult UpgradeComplete()
        {
            if (_options.Value.SiteSettings.AllowSetup ?? false) return View();
            return Unauthorized();
        }
    }
}