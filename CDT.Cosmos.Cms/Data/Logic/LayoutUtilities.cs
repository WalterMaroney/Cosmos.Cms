using CDT.Cosmos.Cms.Common.Data;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CDT.Cosmos.Cms.Data.Logic
{
    /// <summary>
    /// Loads external layouts and associated page templates into Comsos CMS
    /// </summary>
    /// <remarks>Layouts are loaded from here: <see href="https://cosmos-layouts.azurewebsites.net"/>.</remarks>
    public class LayoutUtilities
    {
        private const string COSMOSLAYOUTSREPO = "https://cosmos-layouts.azurewebsites.net";

        /// <summary>
        /// Constructor
        /// </summary>
        public LayoutUtilities()
        {
            using var client = new HttpClient();
            var data = client.GetStringAsync($"{COSMOSLAYOUTSREPO}/catalog.json").Result;
            Catalogs = JsonConvert.DeserializeObject<Root>(data);
        }

        /// <summary>
        /// Default layout ID
        /// </summary>
        /// <remarks>Default is mdb-cfc</remarks>
        public string DefaultLayoutId { get; set; } = "mdb-cfc";

        /// <summary>
        /// Catalogs of layouts
        /// </summary>
        public Root Catalogs { get; private set; }

        /// <summary>
        /// Loads the default layout
        /// </summary>
        /// <returns></returns>
        /// <remarks>This is used when setting up Cosmos.</remarks>
        public Layout LoadDefaultLayout()
        {
            return LoadLayout(DefaultLayoutId, true);
        }

        /// <summary>
        /// Loads a specified layout
        /// </summary>
        /// <param name="layoutId"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public Layout LoadLayout(string layoutId, bool isDefault)
        {
            var layout = Catalogs.LayoutCatalog.FirstOrDefault(f => f.Id == layoutId);
            using var client = new WebClient();
            var url = $"{COSMOSLAYOUTSREPO}/Layouts/{layoutId}/layout.html";
            var data = client.DownloadString(url);

            var contentHtmlDocument = new HtmlDocument();
            contentHtmlDocument.LoadHtml(data);

            var head = contentHtmlDocument.DocumentNode.SelectSingleNode("//head");
            var body = contentHtmlDocument.DocumentNode.SelectSingleNode("//body");
            var bodyHeader = contentHtmlDocument.DocumentNode.SelectSingleNode("//body/header");
            var bodyFooter = contentHtmlDocument.DocumentNode.SelectSingleNode("//body/footer");

            var start = data.IndexOf("<!--Post-Footer-Begin-->") + 24;
            var end = data.LastIndexOf("<!--Post-Footer-End-->");

            var postFooter = data.Substring(start, end - start);

            var cosmosLayout = new Layout()
            {
                IsDefault = isDefault,
                CommunityLayoutId = layoutId,
                LayoutName = layout.Name,
                Notes = layout.Description,
                Head = head.InnerHtml,
                BodyHtmlAttributes = ParseAttributes(body?.Attributes),
                HtmlHeader = bodyHeader.InnerHtml,
                FooterHtmlAttributes = ParseAttributes(bodyFooter?.Attributes),
                PostFooterBlock = postFooter,
                FooterHtmlContent = bodyFooter.InnerHtml
            };

            return cosmosLayout;
        }


        private string ParseAttributes(HtmlAttributeCollection collection)
        {
            if (collection == null)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var attribute in collection)
            {
                builder.Append($"{attribute.Name}=\"{attribute.Value}\" ");
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Gets a page template
        /// </summary>
        /// <param name="communityLayoutId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<Template> GetCommunityTemplatePages(string communityLayoutId = "")
        {
            var tempates = new List<Template>();

            if (string.IsNullOrEmpty(communityLayoutId))
            {
                communityLayoutId = DefaultLayoutId;
            }

            var layout = Catalogs.LayoutCatalog.FirstOrDefault(f => f.Id == communityLayoutId);

            using var client = new WebClient();

            foreach (var page in layout.Pages)
            {
                var url = $"{COSMOSLAYOUTSREPO}/Layouts/{page.Path}";
                var uglifiedUrl = NUglify.Uglify.Html(url);
                var html = client.DownloadString(uglifiedUrl.Code);

                var template = ParseTemplate(html);
                template.Description = page.Description;
                template.Title = page.Type == "home" ? "Home Page" : page.Title;
                template.CommunityLayoutId = communityLayoutId;
                tempates.Add(template);
            }

            return tempates.Distinct().ToList();
        }

        /// <summary>
        /// Gets all the layouts in the community catalog
        /// </summary>
        /// <returns></returns>
        public List<Layout> GetCommunityLayouts()
        {
            var layoutIds = Catalogs.LayoutCatalog.Select(s => s.Id).ToList();
            var layouts = new List<Layout>();

            foreach (var id in layoutIds)
            {
                layouts.Add(LoadLayout(id, id == DefaultLayoutId));
            }

            return layouts;
        }

        /// <summary>
        /// Parses an HTML page and loads it into Cosmos
        /// </summary>
        /// <param name="html"></param>
        /// <returns>Body content of a page without layout elements.</returns>
        public Template ParseTemplate(string html)
        {
            var builder = new StringBuilder();

            var contentHtmlDocument = new HtmlDocument();

            contentHtmlDocument.LoadHtml(html);

            //var head = contentHtmlDocument.DocumentNode.SelectSingleNode("//head");
            //var body = contentHtmlDocument.DocumentNode.SelectSingleNode("//body");
            var bodyHeader = contentHtmlDocument.DocumentNode.SelectSingleNode("//body/header");

            var headerComments = bodyHeader.ChildNodes.Where(w => w.NodeType == HtmlNodeType.Comment).ToList();
            foreach (var headerComment in headerComments)
            {
                headerComment.Remove();
            }

            var bodyFooter = contentHtmlDocument.DocumentNode.SelectSingleNode("//body/footer");

            // Break out the HEADER tag
            // Start removing things
            var nav = contentHtmlDocument.DocumentNode.SelectSingleNode("//body/header/nav");
            nav.Remove(); // nav bar
            // Anything in the header other than nav bar needs to be pulled out and put below
            // the header.
            builder.AppendLine(bodyHeader.InnerHtml);

            // Now remove header element from the body of the document.
            bodyHeader.Remove();

            // Save what remains in the body
            var body = contentHtmlDocument.DocumentNode.SelectSingleNode("//body");

            var childNodes = body.ChildNodes.Where(t => t.NodeType != HtmlNodeType.Comment);

            foreach (var node in childNodes)
            {
                if (node.Name.Equals("footer", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
                builder.AppendLine(node.OuterHtml);
            }
                        
            return new Template()
            {
                Content = builder.ToString(),
                Description = string.Empty,
                Title = string.Empty
            };
        }
    }

    /// <summary>
    /// Template page
    /// </summary>
    public class Page
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    /// <summary>
    /// Layout catalog item
    /// </summary>
    public class LayoutCatalog
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string License { get; set; }
        public List<Page> Pages { get; set; }
    }

    /// <summary>
    /// Catalog feed root
    /// </summary>
    public class Root
    {
        public List<LayoutCatalog> LayoutCatalog { get; set; }
    }


}
