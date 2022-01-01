using CDT.Cosmos.Cms.Data.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Common.Tests
{
    [TestClass]
    public class C001LayoutUtilitiesTests
    {
        [TestMethod]
        public void A01_ReadCatalogSuccess()
        {
            var layoutUtilities = new LayoutUtilities();

            Assert.IsNotNull(layoutUtilities.CommunityCatalog);
        }

        [TestMethod]
        public async Task A02_GetDefaultLayout()
        {
            var layoutUtilities = new LayoutUtilities();

            var defaultLayout = await layoutUtilities.GetDefaultCommunityLayout();

            Assert.IsNotNull(defaultLayout);
        }

        [TestMethod]
        public async Task A03_GetLayouts()
        {
            var layoutUtilities = new LayoutUtilities();

            var layouts = await layoutUtilities.GetAllCommunityLayouts();

            Assert.IsTrue(layouts.Count > 1);
        }

        [TestMethod]
        public async Task A04_GetTemplates()
        {
            var layoutUtilities = new LayoutUtilities();

            var layouts = await layoutUtilities.GetAllCommunityLayouts();

            foreach(var layout in layouts)
            {
                var pageTemplates = await layoutUtilities.GetCommunityTemplatePages(layout.CommunityLayoutId);

                Assert.IsNotNull(pageTemplates);
                Assert.IsTrue(pageTemplates.Count > 0);
            }

        }
    }
}
