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

            var defaultLayout = layoutUtilities.GetDefault();

            Assert.IsNotNull(defaultLayout);
        }

        [TestMethod]
        public void A03_GetTemplates()
        {
            var layoutUtilities = new LayoutUtilities();

            var layout = layoutUtilities.Catalogs.LayoutCatalog.FirstOrDefault();

            var pageTemplate = layoutUtilities.GetTemplates(layout.Id);

            Assert.IsNotNull(pageTemplate);
            Assert.IsTrue(pageTemplate.Count > 0);
        }
    }
}
