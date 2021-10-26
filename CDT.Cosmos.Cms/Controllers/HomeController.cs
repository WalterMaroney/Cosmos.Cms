using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Models;
using CDT.Cosmos.Cms.Common.Services;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Common.Services.Configurations.BootUp;
using CDT.Cosmos.Cms.Data.Logic;
using CDT.Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly IOptions<CosmosStartup> _bootConfigOptions;
        private readonly IOptions<CosmosConfig> _cosmosConfigOptions;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,
            IOptions<CosmosConfig> cosmosConfig,
            IOptions<CosmosStartup> bootConfigOptions,
            ApplicationDbContext dbContext,
            ArticleEditLogic articleLogic)
        {
            _logger = logger;
            _cosmosConfigOptions = cosmosConfig;
            _articleLogic = articleLogic;
            _bootConfigOptions = bootConfigOptions;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(string id, string lang)
        {
            try
            {
                // ViewData["EditModeOn"] = false;
                //
                // Check for configuration setup
                //

                if (await _dbContext.IsConfigured())
                {
                }

                // Make sure this is Url Encoded, because this is the way it is stored in DB.
                if (!string.IsNullOrEmpty(id))
                    id = ArticleLogic.HandleUrlEncodeTitle(id);

                ArticleViewModel article;

                if (string.IsNullOrEmpty(lang)) lang = "en";

                ViewData["UseGoogleTranslate"] =
                    string.IsNullOrEmpty(_cosmosConfigOptions?.Value?.GoogleCloudAuthConfig?.ClientId) == false;

                ViewData["AllowReset"] = _cosmosConfigOptions.Value.SiteSettings.AllowReset;
                ViewData["AllowConfig"] = _bootConfigOptions.Value.AllowConfigEdit;


                if (User.Identity?.IsAuthenticated == false)
                {
                    //
                    // See if we need to register a new user.
                    //
                    if (await _dbContext.Users.AnyAsync()) return Redirect("~/Identity/Account/Login");
                    return Redirect("~/Identity/Account/Register");
                }

                if (!await _dbContext.Users.AnyAsync(u => u.UserName.Equals(User.Identity.Name)))
                    return RedirectToAction("SignOut", "Users");

                if (_cosmosConfigOptions.Value.SiteSettings.AllowSetup.HasValue &&
                    _cosmosConfigOptions.Value.SiteSettings.AllowSetup.Value &&
                    await _dbContext.Roles.AnyAsync(r => r.Name.Contains("Administrators")) == false)
                    return RedirectToAction("Index", "Setup");


                if (!User.IsInRole("Reviewers") && !User.IsInRole("Authors") && !User.IsInRole("Editors") &&
                    !User.IsInRole("Administrators") &&
                    !User.IsInRole("Team Members")) return RedirectToAction("AccessPending");

                if (!await _dbContext.Articles.AnyAsync()) return RedirectToAction("Index", "Editor");

                //
                // If yes, do NOT include headers that allow caching.
                //
                Response.Headers[HeaderNames.CacheControl] = "no-store";
                Response.Headers[HeaderNames.Pragma] = "no-cache";

                article = await _articleLogic.GetByUrl(id, lang); // ?? await _articleLogic.GetByUrl(id, langCookie);

                // Article not found?
                // try getting a version not published.

                if (article == null)
                {
                    //
                    // Create your own not found page for a graceful page for users.
                    //
                    article = await _articleLogic.GetByUrl("/not_found", lang);

                    HttpContext.Response.StatusCode = 404;

                    if (article == null) return NotFound();
                }

                article.EditModeOn = false;
                article.ReadWriteMode = true;

                return View("Index", article);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Gets an article by its ID (or row key).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Reviewers,Authors,Editors,Administrators")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Preview(string id)
        {
            try
            {
                if (int.TryParse(id, out var pageId))
                {
                    ViewData["EditModeOn"] = false;
                    var article = await _articleLogic.Get(pageId, EnumControllerName.Home);

                    if (article != null)
                    {
                        article.ReadWriteMode = false;
                        article.EditModeOn = false;

                        return View("CosmosIndex", article);
                    }
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.Headers[HeaderNames.CacheControl] = "no-store";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            ViewData["EditModeOn"] = false;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        ///     Gets a list of languages supported for translation
        /// </summary>
        /// <param name="lang">language code</param>
        /// <returns></returns>
        public async Task<JsonResult> GetSupportedLanguages(string lang = "en-US")
        {
            var translationServices = new TranslationServices(_cosmosConfigOptions);
            var result = await translationServices.GetSupportedLanguages(lang);

            var model = result.Languages.Select(s => new LangItemViewModel
            {
                DisplayName = s.DisplayName,
                LanguageCode = s.LanguageCode
            }).ToList();

            return Json(model);
        }

        #region STATIC WEB PAGES

        [Authorize]
        public IActionResult AccessPending()
        {
            var model = new ArticleViewModel
            {
                Id = 0,
                ArticleNumber = 0,
                UrlPath = null,
                VersionNumber = 0,
                Published = null,
                Title = "Access Pending",
                Content = null,
                Updated = default,
                HeaderJavaScript = null,
                FooterJavaScript = null,
                Layout = null,
                ReadWriteMode = false,
                PreviewMode = false,
                EditModeOn = false
            };
            return View(model);
        }

        #endregion

        #region SIMPLE PROXIES

        ///// <summary>
        ///// Returns a proxy result as a <see cref="JsonResult"/>.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> SimpleProxyJson(string id)
        //{
        //    if (_proxyConfigs.Value == null) return Json(string.Empty);
        //    var proxy = new SimpleProxyService(_proxyConfigs);
        //    return Json(await proxy.CallEndpoint(id, new UserIdentityInfo(User)));
        //}

        ///// <summary>
        ///// Returns a proxy as a simple string.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public async Task<string> SimpleProxy(string id)
        //{
        //    if (_proxyConfigs.Value == null) return string.Empty;
        //    var proxy = new SimpleProxyService(_proxyConfigs);
        //    return await proxy.CallEndpoint(id, new UserIdentityInfo(User));
        //}

        #endregion
    }
}