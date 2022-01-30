using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Data.Logic;
using CDT.Cosmos.Cms.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{
    /// <summary>
    /// Menu or navigation controller
    /// </summary>

    [Authorize(Roles = "Administrators,Editors")]
    public class MenuController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<MenuController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        /// <param name="userManager"></param>
        /// <param name="articleLogic"></param>
        /// <param name="distributedCache"></param>
        /// <param name="syncContext"></param>
        /// <param name="options"></param>
        /// <exception cref="Exception"></exception>
        public MenuController(
            ILogger<MenuController> logger,
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            IDistributedCache distributedCache,
            SqlDbSyncContext syncContext,
            IOptions<CosmosConfig> options) :
            base(dbContext, userManager, articleLogic, options)
        {
            if (syncContext.IsConfigured())
                dbContext.LoadSyncContext(syncContext);

            if (options.Value.SiteSettings.AllowSetup ?? true)
            {
                throw new Exception("Permission denied. Website in setup mode.");
            }
            _logger = logger;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
            _articleLogic = articleLogic;
        }

        /// <summary>
        /// Index page method
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var pageModel = (await _articleLogic.GetArticleList()).OrderBy(o => o.Title).Select(s =>
                new SelectListItem
                {
                    Text = s.Title,
                    Value = "/" + s.UrlPath
                }).ToList();
            ViewData["PageUrls"] = pageModel;
            await BaseLoadMenuIntoViewData();

            return View();
        }

        /// <summary>
        /// Destroy a meny item
        /// </summary>
        /// <param name="request"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IActionResult> Destroy([DataSourceRequest] DataSourceRequest request, MenuItemViewModel item)
        {
            if (ModelState.IsValid)
            {
                var entity = await _dbContext.MenuItems.FindAsync(item.Id);
                if (entity == null)
                {
                    _logger.LogError($"Could not destroy menu item ID: {item.Id}.");
                    return NotFound();
                }

                _dbContext.MenuItems.Remove(entity);
                await _dbContext.SaveChangesAsync();
                await FlushMenuFromRedis(entity.Guid);
            }

            return Json(await new[] { item }.ToTreeDataSourceResultAsync(request, ModelState));
        }

        /// <summary>
        /// Create a menu item
        /// </summary>
        /// <param name="request"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IActionResult> Create([DataSourceRequest] DataSourceRequest request, MenuItemViewModel item)
        {
            if (ModelState.IsValid)
            {
                var entity = item.ToEntity();
                _dbContext.MenuItems.Add(entity);

                await _dbContext.SaveChangesAsync();
                item.Id = entity.Id; // Send back the ID number

                await FlushMenuFromRedis(entity.Guid);
            }

            return Json(new[] { item }.ToTreeDataSourceResult(request, ModelState));
        }

        /// <summary>
        /// Read menu items
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> Read([DataSourceRequest] DataSourceRequest request)
        {
            var items = await _dbContext.MenuItems.OrderBy(o => o.SortOrder).Select(s =>
                new MenuItemViewModel
                {
                    Id = s.Id,
                    hasChildren = s.HasChildren,
                    ParentId = s.ParentId,
                    SortOrder = s.SortOrder,
                    MenuText = s.MenuText,
                    Url = s.Url,
                    IconCode = s.IconCode
                }).ToListAsync();
            var result = items.ToTreeDataSourceResult(request,
                e => e.Id,
                e => e.ParentId,
                e => e
            );
            return Json(result);
        }

        /// <summary>
        /// Update menu items
        /// </summary>
        /// <param name="request"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IActionResult> Update([DataSourceRequest] DataSourceRequest request, MenuItemViewModel item)
        {
            var entity = await _dbContext.MenuItems.FindAsync(item.Id);
            if (entity == null)
            {
                _logger.LogError($"Could not destroy menu item ID: {item.Id}.");
                return NotFound();
            }

            var oldGuid = entity.Guid;

            entity.SortOrder = item.SortOrder;
            entity.MenuText = item.MenuText;
            entity.IconCode = item.IconCode;
            entity.Url = item.Url;
            entity.ParentId = item.ParentId;
            entity.HasChildren = item.hasChildren;

            await _dbContext.SaveChangesAsync();
            await FlushMenuFromRedis(oldGuid);

            return Json(new[] { item }.ToTreeDataSourceResult(request, ModelState));
        }

        private async Task FlushMenuFromRedis(Guid guid)
        {
            await _distributedCache.RemoveAsync(guid.ToString());
        }
    }
}