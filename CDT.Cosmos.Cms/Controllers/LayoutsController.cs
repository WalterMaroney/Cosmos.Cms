using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Models;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Data.Logic;
using CDT.Cosmos.Cms.Models;
using HtmlAgilityPack;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Z.EntityFramework.Plus;

namespace CDT.Cosmos.Cms.Controllers
{
    [Authorize(Roles = "Administrators, Editors")]
    public class LayoutsController : BaseController
    {
        private readonly ArticleEditLogic _articleLogic;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LayoutsController> _logger;

        public LayoutsController(ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ArticleEditLogic articleLogic,
            SqlDbSyncContext syncContext,
            IOptions<CosmosConfig> options,
            ILogger<LayoutsController> logger) : base(dbContext, userManager, articleLogic, options)
        {
            if (syncContext.IsConfigured())
                dbContext.LoadSyncContext(syncContext);

            _dbContext = dbContext;
            _articleLogic = articleLogic;
            _logger = logger;
        }

        // GET: Layouts
        public async Task<IActionResult> Index()
        {
            if (!await _dbContext.Layouts.AnyAsync())
            {
                _dbContext.Layouts.AddRange(LayoutDefaults.GetStarterLayouts());
                await _dbContext.SaveChangesAsync();
            }

            var model = await _articleLogic.Create("Layouts");
            model.Title = "Layouts";
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var layout = LayoutDefaults.GetOceanside();
            layout.IsDefault = false;
            layout.LayoutName = "New Layout " + await _dbContext.Layouts.CountAsync();
            layout.Notes = "New layout created. Please customize using code editor.";
            _dbContext.Layouts.Add(layout);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("EditCode", new { layout.Id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            return await GetLayoutWithHomePage(id);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("id,header,footer")] int id, string header, string footer)
        {
            var layout = await _dbContext.Layouts.FirstOrDefaultAsync(i => i.Id == id);

            // Make editable
            header = header.Replace(" contenteditable=\"", " crx=\"", StringComparison.CurrentCultureIgnoreCase);
            footer = footer.Replace(" contenteditable=\"", " crx=\"", StringComparison.CurrentCultureIgnoreCase);

            layout.HtmlHeader = header;
            layout.FooterHtmlContent = footer;

            await _dbContext.SaveChangesAsync();

            return await GetLayoutWithHomePage(id);
        }

        /// <summary>
        /// Gets the home page with the specified layout (may not be the default layout)
        /// </summary>
        /// <param name="id">Layout Id (default layout if null)</param>
        /// <returns>ViewResult with <see cref="ArticleViewModel"/></returns>
        private async Task<IActionResult> GetLayoutWithHomePage(int? id)
        {
            // Get the home page
            var model = await _articleLogic.GetByUrl("");

            // Specify layout if given.
            if (id.HasValue)
            {
                var layout = await _dbContext.Layouts.FirstOrDefaultAsync(i => i.Id == id.Value);
                model.Layout = new LayoutViewModel(layout);
            }

            // Make its editable
            model.Layout.HtmlHeader = model.Layout.HtmlHeader.Replace(" crx=\"", " contenteditable=\"", StringComparison.CurrentCultureIgnoreCase);
            model.Layout.FooterHtmlContent = model.Layout.FooterHtmlContent.Replace(" crx=\"", " contenteditable=\"", StringComparison.CurrentCultureIgnoreCase);

            return View(model);
        }

        public async Task<IActionResult> EditNotes(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var model = await _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            }).FirstOrDefaultAsync(f => f.Id == id.Value);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditNotes([Bind(include: "Id,IsDefault,LayoutName,Notes")] LayoutIndexViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model != null)
            {
                var layout = await _dbContext.Layouts.FindAsync(model.Id);
                layout.LayoutName = model.LayoutName;
                var contentHtmlDocument = new HtmlDocument();
                contentHtmlDocument.LoadHtml(HttpUtility.HtmlDecode(model.Notes));
                if (contentHtmlDocument.ParseErrors.Any())
                    foreach (var error in contentHtmlDocument.ParseErrors)
                        ModelState.AddModelError("Notes", error.Reason);

                var remove = "<div style=\"display:none;\"></div>";
                layout.Notes = contentHtmlDocument.ParsedText.Replace(remove, "").Trim();
                //layout.IsDefault = model.IsDefault;
                if (model.IsDefault)
                {
                    var layouts = await _dbContext.Layouts.Where(w => w.Id != model.Id).ToListAsync();
                    foreach (var layout1 in layouts) layout1.IsDefault = false;
                }

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: Layouts/Edit/5
        public async Task<IActionResult> EditCode(int? id)
        {
            if (id == null) return NotFound();

            var layout = await _dbContext.Layouts.FindAsync(id);
            if (layout == null) return NotFound();

            var model = new LayoutCodeViewModel
            {
                Id = layout.Id,
                EditorTitle = layout.LayoutName,
                EditorFields = new List<EditorField>
                {
                    new()
                    {
                        FieldId = "Head",
                        FieldName = "Head",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "BodyHtmlAttributes",
                        FieldName = "Body Html Attributes",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "BodyHeaderHtmlAttributes",
                        FieldName = "Header Html Attributes",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "HtmlHeader",
                        FieldName = "Header Content",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "FooterHtmlAttributes",
                        FieldName = "Footer Html Attributes",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "FooterHtmlContent",
                        FieldName = "Footer Content",
                        EditorMode = EditorMode.Html
                    },
                    new()
                    {
                        FieldId = "PostFooterBlock",
                        FieldName = "Post Footer Block",
                        EditorMode = EditorMode.Html
                    }
                },
                CustomButtons = new List<string> { "Preview", "Layouts" },
                Head = layout.Head,
                BodyHtmlAttributes = layout.BodyHtmlAttributes,
                BodyHeaderHtmlAttributes = layout.BodyHeaderHtmlAttributes,
                HtmlHeader = layout.HtmlHeader,
                FooterHtmlAttributes = layout.FooterHtmlAttributes,
                FooterHtmlContent = layout.FooterHtmlContent,
                PostFooterBlock = layout.PostFooterBlock,
                EditingField = ""
            };
            return View(model);
        }

        /// <summary>
        ///     Saves the code and html of the page.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         This method saves page code to the database. The following properties are validated with method
        ///         <see cref="BaseController.BaseValidateHtml" />:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.Head" />
        ///         </item>
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.HtmlHeader" />
        ///         </item>
        ///         <item>
        ///             <see cref="LayoutCodeViewModel.FooterHtmlContent" />
        ///         </item>
        ///     </list>
        ///     <para>
        ///         HTML formatting errors that could not be automatically fixed by <see cref="BaseController.BaseValidateHtml" />
        ///         are logged with <see cref="ControllerBase.ModelState" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="NotFoundResult"></exception>
        /// <exception cref="UnauthorizedResult"></exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCode(int id, LayoutCodeViewModel layout)
        {
            if (id != layout.Id) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    // Strip out BOM
                    layout.Head = StripBOM(layout.Head);
                    layout.HtmlHeader = StripBOM(layout.HtmlHeader);
                    layout.BodyHeaderHtmlAttributes = StripBOM(layout.BodyHeaderHtmlAttributes);
                    layout.BodyHtmlAttributes = StripBOM(layout.BodyHtmlAttributes);
                    layout.FooterHtmlAttributes = StripBOM(layout.FooterHtmlAttributes);
                    layout.FooterHtmlContent = StripBOM(layout.FooterHtmlContent);
                    layout.PostFooterBlock = StripBOM(layout.PostFooterBlock);

                    //
                    // This layout now is the default, make sure the others are set to "false."

                    var entity = await _dbContext.Layouts.FindAsync(layout.Id);
                    entity.BodyHeaderHtmlAttributes = layout.BodyHeaderHtmlAttributes;
                    entity.BodyHtmlAttributes = layout.BodyHtmlAttributes;
                    entity.FooterHtmlAttributes = layout.FooterHtmlAttributes;
                    entity.FooterHtmlContent =
                        BaseValidateHtml("FooterHtmlContent", layout.FooterHtmlContent);
                    entity.Head = BaseValidateHtml("Head", layout.Head);
                    entity.HtmlHeader = BaseValidateHtml("HtmlHeader", layout.HtmlHeader);
                    entity.PostFooterBlock = layout.PostFooterBlock;

                    // Check validation again after validation of HTML
                    if (entity.IsDefault)
                    {
                        await _dbContext.SaveChangesAsync();

                        // Make sure everything is refreshed.
                        await MakeGlobalChange();
                    }
                    else
                    {
                        await _dbContext.SaveChangesAsync();
                    }

                    var jsonModel = new SaveCodeResultJsonModel
                    {
                        ErrorCount = ModelState.ErrorCount,
                        IsValid = ModelState.IsValid
                    };
                    jsonModel.Errors.AddRange(ModelState.Values
                        .Where(w => w.ValidationState == ModelValidationState.Invalid)
                        .ToList());
                    jsonModel.ValidationState = ModelState.ValidationState;

                    return Json(jsonModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LayoutExists(layout.Id)) return NotFound();
                    throw;
                }

            return View(layout);
        }

        public async Task<IActionResult> Preview(int id)
        {
            var layout = await _dbContext.Layouts.FindAsync(id);
            var model = await _articleLogic.Create("Layout Preview");
            model.Layout = new LayoutViewModel(layout);
            model.EditModeOn = false;
            model.ReadWriteMode = false;
            model.PreviewMode = true;

            return View("~/Views/Home/CosmosIndex.cshtml", model);
        }

        public async Task<IActionResult> EditPreview(int id)
        {
            var layout = await _dbContext.Layouts.FindAsync(id);
            var model = await _articleLogic.Create("Layout Preview");
            model.Layout = new LayoutViewModel(layout);
            model.EditModeOn = true;
            model.ReadWriteMode = true;
            model.PreviewMode = true;
            return View("~/Views/Home/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> SetLayoutAsDefault(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var model = await _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            }).FirstOrDefaultAsync(f => f.Id == id.Value);

            if (model == null) return NotFound();

            return View(model);
        }

        /// <summary>
        ///     Sets a layout as the default layout
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SetLayoutAsDefault([Bind(include: "Id,IsDefault,LayoutName,Notes")] LayoutIndexViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model == null) return RedirectToAction("Index");

            var layout = await _dbContext.Layouts.FindAsync(model.Id);
            layout.IsDefault = model.IsDefault;
            if (model.IsDefault)
            {
                await _dbContext.SaveChangesAsync();
                await _dbContext.Layouts.Where(w => w.Id != model.Id)
                    .UpdateAsync(u => new Layout
                    {
                        IsDefault = false
                    });
                int[] validCodes =
                {
                    (int) StatusCodeEnum.Active,
                    (int) StatusCodeEnum.Inactive
                };

                await _dbContext.Articles.Where(w => validCodes.Contains(w.StatusCode))
                    .UpdateAsync(u => new Article
                    {
                        LayoutId = layout.Id
                    });

                // Make sure everything is refreshed.
                await MakeGlobalChange();

                return RedirectToAction("Publish", "Editor");
            }

            return RedirectToAction("Index", "Layouts");
        }

        private bool LayoutExists(int id)
        {
            return _dbContext.Layouts.Any(e => e.Id == id);
        }

        /// <summary>
        ///     Gets a list of layouts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> Read_Layouts([DataSourceRequest] DataSourceRequest request)
        {
            var model = _dbContext.Layouts.Select(s => new LayoutIndexViewModel
            {
                Id = s.Id,
                IsDefault = s.IsDefault,
                LayoutName = s.LayoutName,
                Notes = s.Notes
            }).OrderByDescending(o => o.IsDefault).ThenBy(t => t.LayoutName);

            return Json(await model.ToDataSourceResultAsync(request));
        }

        /// <summary>
        ///     Updates a layout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Update_Layout([DataSourceRequest] DataSourceRequest request,
            LayoutIndexViewModel model)
        {
            var entity = await _dbContext.Layouts.FindAsync(model.Id);
            entity.IsDefault = model.IsDefault;
            entity.LayoutName = model.LayoutName;
            entity.Notes = model.Notes;
            await _dbContext.SaveChangesAsync();

            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }

        /// <summary>
        ///     Removes a layout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Destroy_Layout([DataSourceRequest] DataSourceRequest request,
            LayoutIndexViewModel model)
        {
            if (model != null)
            {
                var entity = await _dbContext.Layouts.FindAsync(model.Id);

                if (!entity.IsDefault)
                {
                    _dbContext.Layouts.Remove(entity);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("Id", "Cannot delete the default layout.");
                }
            }

            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }

        /// <summary>
        /// After a layout change, refresh everything!
        /// </summary>
        /// <returns></returns>
        private async Task MakeGlobalChange()
        {
            _ = await base.UpdateTimeStamps();
            _ = await FlushCdn(_logger, new[] { "/*" });
        }
    }
}