using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Models;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Data.Logic;
using CDT.Cosmos.Cms.Models;
using CDT.Cosmos.Cms.Services;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{
    [Authorize(Roles = "Reviewers, Administrators, Editors, Authors, Team Members")]
    public class EditorController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EditorController> _logger;
        private readonly IOptions<CosmosConfig> _options;
        private readonly UserManager<IdentityUser> _userManager;

        public EditorController(ILogger<EditorController> logger,
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            IOptions<CosmosConfig> options,
            SqlDbSyncContext syncContext
        ) :
            base(dbContext, userManager, articleLogic, options)
        {
            if (syncContext.IsConfigured())
                dbContext.LoadSyncContext(syncContext);

            _logger = logger;
            //_blobEndpointUrl = options.Value.BlobServiceConfigs.BlobPublicUrl.TrimEnd('/') + "/pub/";
            _dbContext = dbContext;
            _options = options;
            _userManager = userManager;
            _articleLogic = articleLogic;
        }

        private TeamIdentityLogic GetTeamIdentityLogic()
        {
            return User.IsInRole("Team Members") ? new TeamIdentityLogic(_dbContext, User) : null;
        }

        /// <summary>
        ///     Disposes of resources for this controller.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Edit home page, shows list of pages.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewData["PublisherUrl"] = _options.Value.SiteSettings.PublisherUrl;
            ViewData["TeamLogic"] = GetTeamIdentityLogic();
            return View();
        }

        /// <summary>
        ///     Creates a <see cref="CreatePageViewModel" /> used to create a new article.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Team Members"))
            {
                var identityUser = await _userManager.GetUserAsync(User);

                var teams = await _dbContext
                    .Teams
                    .Where(a => a.Members.Any(x => x.UserId == identityUser.Id))
                    .ToListAsync();

                ViewData["Teams"] = teams.Select(c => new TeamViewModel
                {
                    Id = c.Id,
                    TeamDescription = c.TeamDescription,
                    TeamName = c.TeamName
                }).OrderBy(o => o.TeamName).ToList();

                return View(new CreatePageViewModel
                {
                    Id = 0,
                    Title = string.Empty,
                    TeamId = teams.FirstOrDefault()?.Id,
                    Templates = await _dbContext.Templates.Select(s =>
                        new SelectListItem
                        {
                            Value = s.Id.ToString(),
                            Text = s.Title
                        }).ToListAsync()
                });
            }

            ViewData["Teams"] = null;
            return View(new CreatePageViewModel
            {
                Id = 0,
                Title = string.Empty,
                Templates = await _dbContext.Templates.Select(s =>
                    new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Title
                    }).ToListAsync()
            });
        }

        /// <summary>
        ///     Uses <see cref="ArticleEditLogic.Create(string, int?)" /> to create an <see cref="ArticleViewModel" /> that is
        ///     saved to
        ///     the database with <see cref="ArticleEditLogic.UpdateOrInsert" /> ready for editing.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePageViewModel model)
        {
            if (model == null) return NotFound();

            if (User.IsInRole("Team Members") && model.TeamId == null)
            {
                ModelState.AddModelError("TeamId", "Choose a team for this page.");

                var identityUser = await _userManager.GetUserAsync(User);

                var teams = await _dbContext
                    .Teams
                    .Where(a => a.Members.Any(x => x.UserId == identityUser.Id))
                    .ToListAsync();

                ViewData["Teams"] = teams.Select(c => new TeamViewModel
                {
                    Id = c.Id,
                    TeamDescription = c.TeamDescription,
                    TeamName = c.TeamName
                }).ToList();
            }

            if (await _dbContext.Articles.AnyAsync(a =>
                a.StatusCode != 2
                && a.Title.Trim().ToLower() == model.Title.Trim().ToLower()))
                ModelState.AddModelError("Title", $"Title {model.Title} is already taken.");

            // Check for conflict with blob storage root path.
            var blobRootPath = "pub";

            if (!string.IsNullOrEmpty(blobRootPath))
                if (model.Title.ToLower() == blobRootPath.ToLower())
                    ModelState.AddModelError("Title",
                        $"Title {model.Title} conflicts with the file folder \"{blobRootPath}/\".");

            if (!ModelState.IsValid)
            {
                model.Templates = await _dbContext.Templates.Select(s =>
                    new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Title
                    }).ToListAsync();

                return View(model);
            }

            var article = await _articleLogic.Create(model.Title, model.TemplateId);
            var result =
                await _articleLogic.UpdateOrInsert(article, _userManager.GetUserId(User), model.TeamId);

            return RedirectToAction("Edit", new { result.Model.Id });
        }

        /// <summary>
        ///     Creates a new version for an article and redirects to editor.
        /// </summary>
        /// <param name="id">Article ID</param>
        /// <param name="entityId">Entity Id to use as new version</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors")]
        public async Task<IActionResult> CreateVersion(int id, int? entityId = null)
        {
            IQueryable<Article> query;

            //
            // Are we basing this on an existing entity?
            //
            if (entityId == null)
            {
                //
                // If here, we are not. Clone the new version from the last version.
                //
                // Find the last version here
                var maxVersion = await _dbContext.Articles.Where(a => a.ArticleNumber == id)
                    .MaxAsync(m => m.VersionNumber);

                //
                // Now find that version.
                //
                query = _dbContext.Articles.Where(f =>
                    f.ArticleNumber == id &&
                    f.VersionNumber == maxVersion);
            }
            else
            {
                //
                // We are here because the new versison is being based on a
                // specific older version, not the latest version.
                //
                //
                // Create a new version based on a specific version
                //
                query = _dbContext.Articles.Where(f =>
                    f.Id == entityId.Value);
            }

            var article = await query.FirstOrDefaultAsync();

            // var defaultLayout = await ArticleLogic.GetDefaultLayout("en-US");
            var model = new ArticleViewModel
            {
                Id = article.Id, // This is the article we are going to clone as a new version.
                StatusCode = StatusCodeEnum.Active,
                ArticleNumber = article.ArticleNumber,
                UrlPath = article.UrlPath,
                VersionNumber = 0,
                Published = null,
                Title = article.Title,
                Content = article.Content,
                Updated = DateTime.UtcNow,
                HeaderJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                ReadWriteMode = false,
                PreviewMode = false,
                EditModeOn = false,
                CacheKey = null,
                CacheDuration = 0
            };

            var userId = _userManager.GetUserId(User);

            var result = await _articleLogic.UpdateOrInsert(model, userId);

            return RedirectToAction("Edit", "Editor", new { result.Model.Id });
        }

        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public IActionResult MonacoEditor()
        {
            return View();
        }

        /// <summary>
        ///     Creates a <see cref="CreatePageViewModel" /> used to create a new article.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> NewHome(int id)
        {
            var page = await _articleLogic.Get(id, EnumControllerName.Edit);
            return View(new NewHomeViewModel
            {
                Id = page.Id,
                ArticleNumber = page.ArticleNumber,
                Title = page.Title,
                IsNewHomePage = false,
                UrlPath = page.UrlPath
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> NewHome(NewHomeViewModel model)
        {
            if (model == null) return NotFound();
            await _articleLogic.NewHomePage(model.Id, _userManager.GetUserId(User));

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public IActionResult Trash()
        {
            return View();
        }

        /// <summary>
        ///     Publishes a website.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> Publish()
        {
            ViewData["EditModeOn"] = false;
            ViewData["MenuContent"] = _articleLogic.BuildMenu("en-US").Result;

            var layout = await _articleLogic.GetDefaultLayout("en-US");
            ViewData["AkamaiStatus"] = "Ready";
            var model = await _articleLogic.Create("Layout Preview");
            model.Layout = layout;
            model.EditModeOn = false;
            model.ReadWriteMode = false;
            model.PreviewMode = true;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators, Editors")]
        public override async Task<JsonResult> UpdateTimeStamps()
        {
            return await base.UpdateTimeStamps();
        }

        /// <summary>
        ///     Gets all the versions for an article
        /// </summary>
        /// <param name="id">Article number</param>
        /// <returns></returns>
        public async Task<IActionResult> Versions(int? id)
        {
            ViewData["EditModeOn"] = false;
            ViewData["MenuContent"] = _articleLogic.BuildMenu("en-US").Result;
            var article = await _dbContext.Articles.Where(a => a.ArticleNumber == id.Value)
                .Select(s => new { s.Title, s.Team }).FirstOrDefaultAsync();
            ViewData["ArticleTitle"] = article.Title;
            ViewData["TeamName"] = article.Team == null ? "" : article.Team.TeamName;

            if (id == null)
                return RedirectToAction("Index");

            ViewData["ArticleId"] = id.Value;

            ViewData["TeamLogic"] = GetTeamIdentityLogic();

            return View();
        }

        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> Logs()
        {
            var layout = await _articleLogic.GetDefaultLayout("en-US");
            var model = await _articleLogic.Create("Layout Preview");
            model.Layout = layout;
            model.EditModeOn = false;
            model.ReadWriteMode = false;
            model.PreviewMode = true;
            return View(model);
        }

        [Authorize(Roles = "Administrators, Editors")]
        public async Task<IActionResult> Read_Logs([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _dbContext.ArticleLogs
                .OrderByDescending(o => o.DateTimeStamp)
                .Include(i => i.IdentityUser)
                .Include(b => b.Article)
                .Select(s => new
                {
                    s.Id,
                    s.ActivityNotes,
                    s.DateTimeStamp,
                    s.Article.Title,
                    s.IdentityUser.Email
                }).ToListAsync();

            var result = await data.Select(s => new ArticleLogJsonModel
            {
                Id = s.Id,
                ActivityNotes = s.ActivityNotes,
                DateTimeStamp = s.DateTimeStamp.ToUniversalTime(),
                Title = s.Title,
                Email = s.Email
            }).ToDataSourceResultAsync(request);
            return Json(result);
        }

        #region SAVING CONTENT METHODS

        #endregion

        #region EDIT ARTICLE FUNCTIONS

        /// <summary>
        ///     Gets an article to edit by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                // Web browser may ask for favicon.ico, so if the ID is not a number, just skip the response.
                if (int.TryParse(id, out var pageId))
                {
                    ViewData["BlobEndpointUrl"] = _options.Value.SiteSettings.BlobPublicUrl;

                    //ViewData["Layouts"] = await GetLayoutListItems();
                    //
                    // Validate team member access.
                    //

                    ViewData["TeamLogic"] = GetTeamIdentityLogic();

                    return await BaseArticle_Get(pageId, EnumControllerName.Edit);
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }


        /// <summary>
        ///     Saves an article via HTTP POST.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        ///     For published articles, flushes Redis and CDN
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> SaveHtml(ArticleViewModel model)
        {
            if (model == null) return NotFound();

            // If this person is in the team member role, check edit access.
            if (!await Ensure_TeamMemberEditAccess(model)) return Unauthorized();

            // Kendo editor uses BOM, so strip that out here just in case 
            // any find there way here.
            //
            // Strip Byte Order Marks (BOM)
            model.Content = StripBOM(model.Content);

            //
            // The HTML editor edits the title and Content fields.
            // Next two lines detect any HTML errors with each.
            // Errors are saved in ModelState.
            model.Title = BaseValidateHtml("Title", model.Title, ModelState);
            model.Content = BaseValidateHtml("Content", model.Content, ModelState);

            // Make sure model state is valid
            if (ModelState.IsValid)
            {
                // Get the user's ID for logging.
                var user = await _userManager.GetUserAsync(User);

                //if (model.Published.HasValue)
                //{
                //    model.Content = MinifyHtml(model.Content);
                //    model.HeaderJavaScript = MinifyHtml(model.HeaderJavaScript);
                //    model.FooterJavaScript = MinifyHtml(model.FooterJavaScript);
                //}

                //
                // Now save the changes to the database here.
                var result = await _articleLogic.UpdateOrInsert(model, user.Id);
                model = result.Model;

                // Re-enable editable sections.
                model.Content = model.Content.Replace(" crx=\"", " contenteditable=\"",
                    StringComparison.CurrentCultureIgnoreCase);

                //
                // Flush Redis and CDN if required 
                // New: Delay CDN 10 seconds to allow for local memory cache(s) to drain
                //
                if (result.Urls.Any())
                {

                    // Now get all the languages that were flushed
                    // TODO: This needs to be improved.
                    // Not sure this is the best way to do this.
                    var paths = new List<string>();

                    paths.Add($"/{model.UrlPath}");
                    // Add the primary path
                    foreach (var url in result.Urls)
                    {
                        paths.Add($"/{url.TrimStart('/')}");
                    }

                    var json = await FlushCdn(paths.OrderBy(s => s).Distinct().ToArray());
                    var cdnResult = (CdnPurgeViewModel)json.Value;
                }

            }

            var errors = ModelState.Values
                .Where(w => w.ValidationState == ModelValidationState.Invalid)
                .ToList();

            var data = new SaveResultJsonModel
            {
                IsValid = ModelState.IsValid,
                ErrorCount = ModelState.ErrorCount,
                HasReachedMaxErrors = ModelState.HasReachedMaxErrors,
                ValidationState = ModelState.ValidationState,
                Model = model,
                Errors = errors
            };

            return Json(data);
        }

        /// <summary>
        ///     Ensures a team member has edit access to an article
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<bool> Ensure_TeamMemberEditAccess(ArticleViewModel model)
        {
            if (User.IsInRole("Team Members"))
            {
                var user = await _userManager.GetUserAsync(User);
                var teamMember = await _dbContext.TeamMembers
                    .Where(t => t.UserId == user.Id &&
                                // ReSharper disable once AccessToModifiedClosure
                                t.Team.Articles.Any(a => a.Id == model.Id))
                    .FirstOrDefaultAsync();

                if (teamMember == null ||
                    model.Published.HasValue && teamMember.TeamRole != (int)TeamRoleEnum.Editor)
                    return false;
            }
            else
            {
                if (model.Published.HasValue && User.IsInRole("Authors"))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Private, internal method that saves changes to an article.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>If a user is a member of the 'Team Members' role, ensures that user has ability to save article.</para>
        ///     <para>
        ///         If this is an article (or regular page) content being saved, the method
        ///         <see cref="Data.Logic.ArticleEditLogic.UpdateOrInsert" /> is used. Saving a template uses method
        ///         <see cref="SaveTemplateChanges" />.
        ///     </para>
        ///     <para>Errors are recorded using <see cref="ILogger" /> and with <see cref="ControllerBase.ModelState" />.</para>
        /// </remarks>
        //private async Task<ArticleEditViewModel> SaveArticleChanges3(ArticleViewModel model,
        //    EnumControllerName controllerName)
        //{

        //    CdnPurgeViewModel cdnResult = null;

        //    var user = await _userManager.GetUserAsync(User);

        //    // Get a new copy of the model
        //    var result = await _articleLogic.UpdateOrInsert(model, user.Id);

        //    model = result.Model;

        //    // Re-enable editable sections.
        //    model.Content = model.Content.Replace(" crx=\"", " contenteditable=\"",
        //        StringComparison.CurrentCultureIgnoreCase);

        //    //
        //    // Flush Redis and CDN if the article is published
        //    //
        //    if (model.Published.HasValue)
        //    {
        //        var result = await FlushRedis(model.UrlPath);

        //        // Now get all the languages that were flushed
        //        // TODO: This needs to be improved.
        //        // Not sure this is the best way to do this.
        //        var paths = new List<string>();
        //        foreach (var key in result.Keys)
        //        {
        //            if (key.Contains(model.UrlPath))
        //            {
        //                var parts = RedisCacheService.ParsePageCacheKey(key);

        //                paths.Add($"/{parts.UrlPath}?lang={parts.Language}");
        //                if (parts.Language.StartsWith("en"))
        //                {
        //                    paths.Add($"/{parts.UrlPath}");
        //                }
        //            }
        //        }

        //        var json = await FlushCdn(paths.ToArray());
        //        cdnResult = (CdnPurgeViewModel)json.Value;

        //    }

        //    var menu = await _articleLogic.BuildMenu("en-US");

        //    ViewData["MenuContent"] = menu;
        //    ViewData["PreviewMode"] = false;

        //    var resultModel = new ArticleEditViewModel(model, cdnResult);

        //    return resultModel;
        //}
        [HttpPost]
        [Authorize(Roles = "Administrators, Editors")]
        public async Task<JsonResult> FlushCdn(string[] paths = null)
        {
            return await FlushCdn(_logger, paths);
        }

        /// <summary>
        ///     This private method is used by <see cref="SaveArticleChanges" /> to save a <see cref="Template" />.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        ///     <para>Returns an <see cref="ArticleViewModel" /> where:</para>
        ///     <para></para>
        ///     <para>
        ///         * <see cref="ArticleViewModel.ReadWriteMode" /> is set using
        ///         <see cref="IOptions{SiteCustomizationsConfig}" /> injected into <see cref="BaseController" />.
        ///     </para>
        ///     <para>Errors are recorded using <see cref="ILogger" /> and with <see cref="ControllerBase.ModelState" />.</para>
        /// </returns>
        //private async Task<ArticleViewModel> SaveTemplateChanges(ArticleViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.Title = BaseValidateHtml("Title", model.Title, ModelState);
        //        model.Content = BaseValidateHtml("Content", model.Content, ModelState);
        //        //model.SubSection1 = ValidateHtml("SubSection1", model.SubSection1, ModelState);

        //        //var userId = UserManager.GetUserId(User);
        //        // Get a new copy of the model
        //        var template = await _dbContext.Templates.FindAsync(model.Id) ?? new Template();

        //        template.Content = model.Content;
        //        template.Title = model.Title;

        //        if (template.Id == 0) _dbContext.Templates.Add(template);

        //        await _dbContext.SaveChangesAsync();
        //    }

        //    model.ReadWriteMode = true;
        //    return model;
        //}
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> EditCode(int id)
        {
            var article = await _articleLogic.Get(id, EnumControllerName.Edit);
            if (article == null) return NotFound();

            // Validate security for authors before going further
            if (User.IsInRole("Team Members"))
            {
                var user = await _userManager.GetUserAsync(User);
                var teamMember = await _dbContext.TeamMembers
                    .Where(t => t.UserId == user.Id &&
                                t.Team.Articles.Any(a => a.Id == id))
                    .FirstOrDefaultAsync();

                if (teamMember == null || article.Published.HasValue &&
                    teamMember.TeamRole != (int)TeamRoleEnum.Editor)
                    return Unauthorized();
            }
            else
            {
                if (article.Published.HasValue && User.IsInRole("Authors"))
                    return Unauthorized();
            }

            ViewData["Version"] = article.VersionNumber;

            return View(new EditCodePostModel
            {
                Id = article.Id,
                ArticleNumber = article.ArticleNumber,
                EditorTitle = article.Title,
                EditorFields = new[]
                {
                    new EditorField
                    {
                        FieldId = "HeaderJavaScript",
                        FieldName = "Header Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "/images/seti-ui/icons/html.svg"
                    },
                    new EditorField
                    {
                        FieldId = "Content",
                        FieldName = "Html Content",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg"
                    },
                    new EditorField
                    {
                        FieldId = "FooterJavaScript",
                        FieldName = "Footer Block",
                        EditorMode = EditorMode.Html,
                        IconUrl = "~/images/seti-ui/icons/html.svg"
                    }
                },
                HeaderJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                Content = article.Content,
                EditingField = "HeaderJavaScript",
                CustomButtons = new[] { "Preview", "Html" }
            });
        }

        /// <summary>
        ///     Saves the code and html of the page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        ///     This method saves page code to the database.  <see cref="EditCodePostModel.Content" /> is validated using method
        ///     <see cref="BaseController.BaseValidateHtml" />.
        ///     HTML formatting errors that could not be automatically fixed are logged with
        ///     <see cref="ControllerBase.ModelState" /> and
        ///     the code is not saved in the database.
        /// </remarks>
        /// <exception cref="NotFoundResult"></exception>
        /// <exception cref="UnauthorizedResult"></exception>
        [HttpPost]
        [Authorize(Roles = "Administrators, Editors, Authors, Team Members")]
        public async Task<IActionResult> EditCode(EditCodePostModel model)
        {
            if (model == null) return NotFound();

            var article = await _dbContext.Articles.FirstOrDefaultAsync(f => f.Id == model.Id);

            if (article == null) return NotFound();

            // Validate security for authors before going further
            if (User.IsInRole("Team Members"))
            {
                var user = await _userManager.GetUserAsync(User);
                var teamMember = await _dbContext.TeamMembers
                    .Where(t => t.UserId == user.Id &&
                                // ReSharper disable once AccessToModifiedClosure
                                t.Team.Articles.Any(a => a.Id == article.Id))
                    .FirstOrDefaultAsync();

                if (teamMember == null || article.Published.HasValue &&
                    teamMember.TeamRole != (int)TeamRoleEnum.Editor)
                    return Unauthorized();
            }
            else
            {
                if (article.Published.HasValue && User.IsInRole("Authors"))
                    return Unauthorized();
            }

            // When we save to the database, remove content editable attribute.
            if (!string.IsNullOrEmpty(model.Content))
            {
                model.Content = model.Content.Replace(" contenteditable=\"", " crx=\"",
                            StringComparison.CurrentCultureIgnoreCase);
            }

            // Strip Byte Order Marks (BOM)
            model.Content = StripBOM(model.Content);
            model.HeaderJavaScript = StripBOM(model.HeaderJavaScript);
            model.FooterJavaScript = StripBOM(model.FooterJavaScript);

            // Validate HTML
            model.Content = BaseValidateHtml("Content", model.Content, ModelState);

            if (ModelState.IsValid) article.Content = model.Content;

            if (string.IsNullOrEmpty(model.HeaderJavaScript) ||
                string.IsNullOrWhiteSpace(model.HeaderJavaScript))
                article.HeaderJavaScript = string.Empty;
            else
                article.HeaderJavaScript = model.HeaderJavaScript.Trim();

            if (string.IsNullOrEmpty(model.FooterJavaScript) ||
                string.IsNullOrWhiteSpace(model.FooterJavaScript))
                article.FooterJavaScript = string.Empty;
            else
                article.FooterJavaScript = model.FooterJavaScript.Trim();


            // Check for validation errors...
            if (ModelState.IsValid)
                try
                {
                    // If no HTML errors were thrown, save here.
                    
                    await _dbContext.SaveChangesAsync();
                    //
                    // Pull back out of the database, so user can see exactly what was saved.
                    //
                    article = await _dbContext.Articles.FirstOrDefaultAsync(f => f.Id == model.Id);
                    if (article == null) throw new Exception("Could not retrieve saved code!");

                }
                catch (Exception e)
                {
                    var provider = new EmptyModelMetadataProvider();
                    ModelState.AddModelError("Save", e, provider.GetMetadataForType(typeof(string)));
                }

            // Now, prior to sending model back, re-enable the content editable attribute.
            if (!string.IsNullOrEmpty(article.Content))
            {
                article.Content = article.Content.Replace(" crx=\"", " contenteditable=\"",
                    StringComparison.CurrentCultureIgnoreCase);
            }

            // ReSharper disable once PossibleNullReferenceException
            ViewData["Version"] = article.VersionNumber;

            var jsonModel = new SaveCodeResultJsonModel
            {
                ErrorCount = ModelState.ErrorCount,
                IsValid = ModelState.IsValid
            };
            jsonModel.Errors.AddRange(ModelState.Values
                .Where(w => w.ValidationState == ModelValidationState.Invalid)
                .ToList());
            jsonModel.ValidationState = ModelState.ValidationState;

            DateTime? publishedDateTime = null;
            if (article.Published.HasValue)
            {
                publishedDateTime = article.Published.Value.ToUniversalTime();
            }

            return Json(jsonModel);
        }

        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public IActionResult Preload()
        {
            return View(new PreloadViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Preload(PreloadViewModel model, bool primaryOnly = false)
        {
            var activeCode = (int)StatusCodeEnum.Active;
            var query = _dbContext.Articles.Where(p => p.Published != null && p.StatusCode == activeCode);
            var articleList = await _articleLogic.GetArticleList(query);
            var publicUrl = _options.Value.SiteSettings.PublisherUrl.TrimEnd('/');

            model.PageCount = 0;

            var client = new HttpClient();

            // Get a list of editors that are outside the current cloud.
            var otherEditors = _options.Value.EditorUrls.Where(w => w.CloudName.Equals(_options.Value.PrimaryCloud, StringComparison.CurrentCultureIgnoreCase) == false).ToList();

            model.EditorCount++;

            //
            // If we are preloading CDN
            if (model.PreloadCdn)
            {
                foreach (var article in articleList)
                {
                    try
                    {
                        var response = await client.GetAsync($"{publicUrl}/{(article.UrlPath == "root" ? "" : article.UrlPath)}");
                        response.EnsureSuccessStatusCode();
                        _ = await response.Content.ReadAsStringAsync();
                        //model.PageCount++;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message, e);
                    }
                }
            }


            return View(model);
        }

        #endregion

        #region Data Services

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> CheckTitle(int articleNumber, string title)
        {
            var result = await _articleLogic.ValidateTitle(title, articleNumber);

            if (result) return Json(true);

            return Json($"Email {title} is already taken.");
        }

        /// <summary>
        ///     Get list of articles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Note: This method cannot retrieve articles that are in the trash.
        /// </remarks>
        public async Task<IActionResult> Get_Articles([DataSourceRequest] DataSourceRequest request)
        {
            List<ArticleListItem> list;
            var defaultSort = request.Sorts?.Any() == false && request.Filters?.Any() == false;

            if (User.IsInRole("Team Members"))
            {
                var identityUser = await _userManager.GetUserAsync(User);
                ViewData["TeamsLookup"] = await _dbContext.Teams
                    .Where(a => a.Members.Any(x => x.UserId == identityUser.Id))
                    .Select(s => new TeamViewModel
                    {
                        Id = s.Id,
                        TeamDescription = s.TeamDescription,
                        TeamName = s.TeamName
                    }).OrderBy(o => o.TeamName)
                    .ToListAsync();

                var userId = await _userManager.GetUserIdAsync(await _userManager.GetUserAsync(User));

                var query = _dbContext.Articles
                    .Include(i => i.Team)
                    .Where(w => w.Team.Members.Any(a => a.UserId == userId));

                list = await _articleLogic.GetArticleList(query, defaultSort);
            }
            else
            {
                list = await _articleLogic.GetArticleList(query: null, defaultSort);

                ViewData["TeamsLookup"] = null;
            }

            if (defaultSort)
            {
                if (request.Sorts == null) request.Sorts = new List<SortDescriptor>();
                if (request.Page < 2)
                    request.Sorts.Add(new SortDescriptor
                    {
                        Member = "IsDefault",
                        SortDirection = ListSortDirection.Descending
                    });
                request.Sorts.Add(new SortDescriptor
                {
                    Member = "Title",
                    SortDirection = ListSortDirection.Ascending
                });
            }
            else if (request.Page < 2)
            {
                if (request.Sorts == null) request.Sorts = new List<SortDescriptor>();
                request.Sorts.Insert(0, new SortDescriptor
                {
                    Member = "IsDefault",
                    SortDirection = ListSortDirection.Descending
                });
            }

            return Json(await list.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<IActionResult> Trash_Article([DataSourceRequest] DataSourceRequest request,
            ArticleListItem model)
        {
            if (model != null) await _articleLogic.TrashArticle(model.ArticleNumber);
            return Json(await new[] { model }.ToDataSourceResultAsync(request, ModelState));
        }

        /// <summary>
        ///     Get list of articles that are in the trash bin.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> Get_TrashedArticles([DataSourceRequest] DataSourceRequest request)
        {
            var list = await _articleLogic.GetArticleTrashList();
            return Json(await list.ToDataSourceResultAsync(request));
        }

        /// <summary>
        ///     Get all the versions of an article by article number.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Get_Versions([DataSourceRequest] DataSourceRequest request, int id)
        {
            var data = await _dbContext.Articles.OrderByDescending(o => o.VersionNumber)
                .Where(a => a.ArticleNumber == id).Select(s => new
                {
                    s.Id,
                    s.Published,
                    s.Title,
                    s.Updated,
                    s.VersionNumber,
                    s.Expires,
                    UsesHtmlEditor = true // s.Content.ToLower().Contains(" crx=")
                }).ToListAsync();

            //
            // Testing has detected a rare occurance of a duplicate version number.
            // Compensating for this error, is to first detect duplicate numbers,
            // and if one is detected, recorder the version numbers.
            //
            var check = (from a in data
                         group a by a.VersionNumber
                into g
                         select new
                         {
                             Version = g.Key,
                             Count = g.Count()
                         }).ToList();

            if (check.Any(c => c.Count > 1))
            {
                //
                // Duplicate versions detected. Fix now.
                //
                var ids = data.Select(s => s.Id).ToArray();
                var entities = await _dbContext.Articles.Where(a => ids.Contains(a.Id)).OrderBy(o => o.Id)
                    .ToListAsync();
                // Reorder version numbers.
                for (var i = 0; i < entities.Count; i++) entities[i].VersionNumber = i + 1;
                await _dbContext.SaveChangesAsync();
                //
                // Reload the version list.
                //
                data = await _dbContext.Articles.OrderByDescending(o => o.VersionNumber)
                    .Where(a => a.ArticleNumber == id).Select(s => new
                    {
                        s.Id,
                        s.Published,
                        s.Title,
                        s.Updated,
                        s.VersionNumber,
                        s.Expires,
                        UsesHtmlEditor = true // s.Content.ToLower().Contains(" crx=")
                    }).ToListAsync();
            }

            var model = data.Select(x =>
                new ArticleVersionInfo
                {
                    Id = x.Id,
                    VersionNumber = x.VersionNumber,
                    Title = x.Title,
                    Updated = x.Updated.ToUniversalTime(),
                    Published = x.Published?.ToUniversalTime(),
                    Expires = x.Expires?.ToUniversalTime(),
                    UsesHtmlEditor = x.UsesHtmlEditor
                });

            return Json(await model.ToDataSourceResultAsync(request));
        }

        /// <summary>
        ///     Gets a role list, and allows for filtering
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get_RoleList(string text)
        {
            var query = _dbContext.Roles.Select(s => new RoleItemViewModel
            {
                Id = s.Id,
                RoleName = s.Name,
                RoleNormalizedName = s.NormalizedName
            });

            if (!string.IsNullOrEmpty(text)) query = query.Where(w => w.RoleName.StartsWith(text));

            return Json(await query.OrderBy(r => r.RoleName).ToListAsync());
        }

        #endregion

        /// <summary>
        /// Recieves an encrypted signal from another editor to do something.
        /// </summary>
        /// <param name="data">Encrypted arguments</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public string Signal(string data)
        {
            var result = new SignalResult();

            try
            {
                var args = DecryptString(data).Split('|');

                switch (args[0])
                {
                    case "VERIFY":
                        result.JsonValue = JsonConvert.SerializeObject(new SignalVerifyResult { Echo = args[1], Stamp = DateTime.UtcNow });
                        break;
                    default:
                        throw new Exception($"Signal {args[0]} not supported.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                result.Exceptions.Add(e);
            }

            result.HasErrors = result.Exceptions.Any();

            var json = JsonConvert.SerializeObject(result);

            return EncryptString(json);
        }

    }
}