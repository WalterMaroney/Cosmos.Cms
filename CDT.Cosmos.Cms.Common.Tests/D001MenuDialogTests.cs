using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Common.Tests
{
    [TestClass]
    public class D001MenuDialogTests
    {
        public static Utilities utils;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            utils = new Utilities();
        }

        [TestMethod] 
        public async Task A001TestMenuDialog()
        {
            var user = await utils.GetIdentityUser(TestUsers.Foo);

            var articleLogic = utils.GetArticleEditLogic(utils.GetApplicationDbContext(), false);

            var blog = await articleLogic.Create("Blog");
            var blogFoo = await articleLogic.Create("Blog/Foo");
            var blogBoat = await articleLogic.Create("Blog/Boat");
            var blogBoatPort = await articleLogic.Create("Blog/Boat/Port");
            var blogBoatStarboard = await articleLogic.Create("Blog/Boat/Starboard");

            await articleLogic.UpdateOrInsert(blog, user.Id);
            await articleLogic.UpdateOrInsert(blogFoo, user.Id);
            await articleLogic.UpdateOrInsert(blogBoat, user.Id);
            await articleLogic.UpdateOrInsert(blogBoatPort, user.Id);
            await articleLogic.UpdateOrInsert(blogBoatStarboard, user.Id);

            //var result1 = await articleLogic.GetTOC("/");
            //var result2 = await articleLogic.GetTOC(blog.UrlPath);
            //var result3 = await articleLogic.GetTOC(blogFoo.UrlPath);
            //var result4 = await articleLogic.GetTOC(blogBoat.UrlPath);
            //var result5 = await articleLogic.GetTOC(blogBoatPort.UrlPath);
            //var result6 = await articleLogic.GetTOC(blogBoatStarboard.UrlPath);

            //Assert.IsTrue(result1.Count > 0);

        }
    }
}
