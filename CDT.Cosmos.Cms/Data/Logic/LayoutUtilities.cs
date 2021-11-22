using CDT.Cosmos.Cms.Common.Data;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;

namespace CDT.Cosmos.Cms.Data.Logic
{
    /// <summary>
    /// Loads external layouts and associated page templates into Comsos CMS
    /// </summary>
    public class LayoutUtilities
    {
        private const string COSMOSLAYOUTSREPO = "https://cosmos-layouts.azurewebsites.net";

        public LayoutUtilities()
        {
            using var client = new WebClient();
            var data = client.DownloadString($"{COSMOSLAYOUTSREPO}/catalog.json");
            Catalogs = JsonConvert.DeserializeObject<Root>(data);
        }

        /// <summary>
        /// Default layout ID
        /// </summary>
        /// <remarks>Default is mdb-cfc</remarks>
        public string DefaultLayoutId { get; set; } = "mdb-cfc";

        public Root Catalogs { get; private set; }

        public Layout GetDefault()
        {
            return GetLayout(DefaultLayoutId, true);
        }

        public Layout GetLayout(string layoutId, bool isDefault)
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
                LayoutName = layout.Name,
                Notes = layout.Description,
                Head = head.InnerHtml,
                BodyHtmlAttributes = ParseAttributes(body.Attributes),
                HtmlHeader = bodyHeader.InnerHtml,
                FooterHtmlAttributes = ParseAttributes(bodyFooter.Attributes),
                PostFooterBlock = postFooter,
                FooterHtmlContent = bodyFooter.InnerHtml
            };

            return cosmosLayout;
        }

        private string ParseAttributes(HtmlAttributeCollection collection)
        {
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
        /// <param name="layoutId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<Template> GetTemplates(string layoutId = "")
        {
            var tempates = new List<Template>();

            if (string.IsNullOrEmpty(layoutId))
            {
                layoutId = DefaultLayoutId;
            }
            var layout = Catalogs.LayoutCatalog.FirstOrDefault(f => f.Id == layoutId);

            using var client = new WebClient();

            foreach (var page in layout.Pages)
            {
                var url = $"{COSMOSLAYOUTSREPO}/Layouts/{layoutId}/{page.Path}";
                var data = client.DownloadString(url);

                tempates.Add(new Template()
                {
                    Content = data,
                    Description = page.Description,
                    Title = page.Type == "home" ? "Home Page" : page.Title
                });
            }

            return tempates;
        }
    }

    public class Page
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    public class LayoutCatalog
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string License { get; set; }
        public List<Page> Pages { get; set; }
    }

    public class Root
    {
        public List<LayoutCatalog> LayoutCatalog { get; set; }
    }


}
