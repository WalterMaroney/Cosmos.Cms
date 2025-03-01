﻿using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Models;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{
    /// <summary>
    /// Templates controller
    /// </summary>
    [Authorize(Roles = "Administrators, Editors")]
    public class TemplatesController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        /// <param name="options"></param>
        /// <param name="userManager"></param>
        /// <param name="articleLogic"></param>
        /// <param name="syncContext"></param>
        /// <exception cref="Exception"></exception>
        public TemplatesController(ILogger<TemplatesController> logger, ApplicationDbContext dbContext,
            IOptions<CosmosConfig> options, UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            SqlDbSyncContext syncContext) :
            base(dbContext, userManager, articleLogic, options)
        {
            if (syncContext.IsConfigured())
                dbContext.LoadSyncContext(syncContext);

            if (options.Value.SiteSettings.AllowSetup ?? true)
            {
                throw new Exception("Permission denied. Website in setup mode.");
            }
            _dbContext = dbContext;
            _articleLogic = articleLogic;
        }

        /// <summary>
        /// Index view model
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
            ViewData["EditModeOn"] = false; // Used by page views
            ViewData["Layouts"] = await BaseGetLayoutListItems();

            return View();
        }

        /// <summary>
        /// Create a template method
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            var entity = new Template
            {
                Id = 0,
                Title = "New Template " + await _dbContext.Templates.CountAsync(),
                Description = "<p>New template, please add descriptive and helpful information here.</p>",
                Content = "<p>" + LoremIpsum.SubSection1 + "</p>"
            };
            _dbContext.Templates.Add(entity);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("EditCode", "Templates", new { entity.Id });
        }

        /// <summary>
        /// Edit template code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCode(int id)
        {
            var entity = await _dbContext.Templates.FindAsync(id);

            var model = new TemplateCodeEditorViewModel
            {
                Id = entity.Id,
                EditorTitle = entity.Title,
                EditorFields = new List<EditorField>
                {
                    new()
                    {
                        EditorMode = EditorMode.Html,
                        FieldName = "Html Content",
                        FieldId = "Content",
                        IconUrl = "~/images/seti-ui/icons/html.svg"
                    }
                },
                EditingField = "Content",
                Content = entity.Content,
                Version = 0,
                CustomButtons = new List<string>
                {
                    "Preview"
                }
            };
            return View(model);
        }

        /// <summary>
        /// Save edited template code
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCode(TemplateCodeEditorViewModel model)
        {
            var entity = await _dbContext.Templates.FindAsync(model.Id);

            entity.Content = model.Content;

            await _dbContext.SaveChangesAsync();

            model = new TemplateCodeEditorViewModel
            {
                Id = entity.Id,
                EditorTitle = entity.Title,
                EditorFields = new List<EditorField>
                {
                    new()
                    {
                        EditorMode = EditorMode.Html,
                        FieldName = "Html Content",
                        FieldId = "Content",
                        IconUrl = "~/images/seti-ui/icons/html.svg"
                    }
                },
                EditingField = "Content",
                Content = entity.Content,
                CustomButtons = new List<string>
                {
                    "Preview"
                },
                IsValid = true
            };
            return Json(model);
        }

        /// <summary>
        /// Preview a template
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(int Id)
        {
            var template = await _dbContext.Templates.FindAsync(Id);

            var model = await _articleLogic.Create("Layout Preview");
            model.Content = template?.Content;
            model.EditModeOn = false;
            model.ReadWriteMode = true;
            model.PreviewMode = true;
            ViewData["UseGoogleTranslate"] = false;

            return View(model);
        }

        /// <summary>
        /// Preview edit mode for a template
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PreviewEdit(int id)
        {
            var template = await _dbContext.Templates.FindAsync(id);
            var model = await _articleLogic.Create("Layout Preview");
            model.Content = template.Content;
            model.EditModeOn = true;
            model.ReadWriteMode = true;
            model.PreviewMode = false;
            return View("~/Views/Home/Index.cshtml", model);
        }

        /// <summary>
        ///     Creates a new template
        /// </summary>
        /// <param name="request"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Templates_Create([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<TemplateIndexViewModel> templates)
        {
            var results = new List<Template>();

            var layout = await _dbContext.Layouts.FirstOrDefaultAsync(f => f.IsDefault);

            if (templates != null && ModelState.IsValid)
                foreach (var template in templates)
                {
                    var entity = new Template
                    {
                        Id = 0,
                        Content = "",
                        Description = template.Description,
                        Title = template.Title,
                        LayoutId = layout.Id,
                        CommunityLayoutId = layout.CommunityLayoutId
                    };
                    _dbContext.Templates.Add(entity);
                    await _dbContext.SaveChangesAsync();
                    results.Add(entity);
                }

            return Json(await results.Select(s => new TemplateIndexViewModel
            {
                Description = s.Description,
                Id = s.Id,
                Title = s.Title
            }).ToDataSourceResultAsync(request, ModelState));
        }

        /// <summary>
        ///     Reads the list of templates
        /// </summary>
        /// <param name="request">Data source request</param>
        /// <returns>JsonResult</returns>
        public async Task<IActionResult> Templates_Read([DataSourceRequest] DataSourceRequest request)
        {
            var defautLayout = await _dbContext.Layouts.FirstOrDefaultAsync(f => f.IsDefault);

            var model = await _dbContext.Templates.Include(i => i.Layout).OrderBy(t => t.Title)
                .Where(l => l.LayoutId == null ||  l.LayoutId == defautLayout.Id)
                .Select(s => new TemplateIndexViewModel
                {
                    Id = s.Id,
                    LayoutName = s.Layout.LayoutName,
                    Description = s.Description,
                    Title = s.Title
                }).ToListAsync();
            return Json(await model.ToDataSourceResultAsync(request));
        }

        /// <summary>
        /// Update template information
        /// </summary>
        /// <param name="request"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Templates_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<TemplateIndexViewModel> templates)
        {
            if (templates != null && ModelState.IsValid)
            {
                foreach (var template in templates)
                {
                    var entity = await _dbContext.Templates.FindAsync(template.Id);
                    entity.Description = template.Description;
                    entity.Title = template.Title;
                }

                await _dbContext.SaveChangesAsync();
            }

            return Json(await templates.ToDataSourceResultAsync(request, ModelState));
        }

        /// <summary>
        /// Delete templates
        /// </summary>
        /// <param name="request"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Templates_Destroy([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<TemplateIndexViewModel> templates)
        {
            if (templates.Any())
            {
                foreach (var template in templates)
                {
                    var entity = await _dbContext.Templates.FindAsync(template.Id);
                    _dbContext.Templates.Remove(entity);
                }

                await _dbContext.SaveChangesAsync();
            }

            return Json(await templates.ToDataSourceResultAsync(request, ModelState));
        }

        /// <summary>
        /// Real a list of templates
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Layouts_Read([DataSourceRequest] DataSourceRequest request)
        {
            var layoutUtils = new LayoutUtilities();
            var model = layoutUtils.CommunityCatalog.LayoutCatalog.Select(s => new LayoutCatalogViewModel()
            {
                Id = s.Id,
                Description = s.Description,
                License = s.License,
                Name = s.Name
            }
            ).ToList();

            return Json(model.ToDataSourceResult(request));
        }

        /// <summary>
        /// Gets the page templates for a given layout.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LayoutPages_Read([DataSourceRequest] DataSourceRequest request, string id)
        {
            var layoutUtils = new LayoutUtilities();

            var model = await layoutUtils.GetPageTemplates(id);

            return Json(model.ToDataSourceResult(request));
        }
    }
}