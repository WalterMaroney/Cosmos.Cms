using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Hubs
{
    /// <summary>
    /// Editing room hub
    /// </summary>
    [Authorize]
    public class EditingRoomHub : Hub
    {
        private ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public EditingRoomHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Joins users to an editing "room".
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task JoinRoom(int articleId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, articleId.ToString());
            var articleLock = await _dbContext.ArticleLocks.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            if (articleLock != null)
            {
                var identityUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == Context.User.Identity.Name);
                articleLock = new ArticleLock() { ArticleId = articleId, IdentityUserId = identityUser.Id, LockSetDateTime = System.DateTimeOffset.UtcNow };
                await _dbContext.SaveChangesAsync();
            }
            await NotifyRoomOfLock(articleId);
        }

        /// <summary>
        /// Removes users from an editing "room"
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task LeaveRoom(int articleId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, articleId.ToString());
            await NotifyRoomOfLock(articleId);
        }

        /// <summary>
        /// Signal other members of the group that a page was just saved, and to reload page.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task ArticleSaved(int articleId)
        {
            await Clients.Group(articleId.ToString()).SendAsync("ArticleReload");
        }

        /// <summary>
        /// Notifies everyone on this editing room of any article lock.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        private async Task NotifyRoomOfLock(int articleId)
        {
            var model = await _dbContext.ArticleLocks.Select(l => new ArticleLockViewModel()
            {
                ArticleId = l.ArticleId,
                Id = l.Id,
                LockSetDateTime = l.LockSetDateTime,
                UserEmail = l.IdentityUser.Email
            }).FirstOrDefaultAsync(a => a.ArticleId == articleId);

            await Clients.Group(articleId.ToString()).SendAsync("ArticleLock", model);
        }
    }
}
