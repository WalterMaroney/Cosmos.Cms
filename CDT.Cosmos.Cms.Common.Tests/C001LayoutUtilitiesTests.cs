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

            Assert.IsNotNull(layoutUtilities.Catalogs);
        }

        [TestMethod]
        public void A02_GetDefaultLayout()
        {
            var layoutUtilities = new LayoutUtilities();

            var defaultLayout = layoutUtilities.LoadDefaultLayout();

            Assert.IsNotNull(defaultLayout);
        }

        [TestMethod]
        public void A03_GetLayouts()
        {
            var layoutUtilities = new LayoutUtilities();

            var layouts = layoutUtilities.GetCommunityLayouts();

            Assert.IsTrue(layouts.Count > 1);
        }

        [TestMethod]
        public void A04_GetTemplates()
        {
            var layoutUtilities = new LayoutUtilities();

            var layouts = layoutUtilities.GetCommunityLayouts();

            foreach(var layout in layouts)
            {
                var pageTemplates = layoutUtilities.GetCommunityTemplatePages(layout.CommunityLayoutId);

                Assert.IsNotNull(pageTemplates);
                Assert.IsTrue(pageTemplates.Count > 0);
            }

        }
    }
}
