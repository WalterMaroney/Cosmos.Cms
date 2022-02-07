using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Hubs
{
    /// <summary>
    /// Chat hub
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Chat send method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Send(object sender, string message)
        {
            // Broadcast the message to all clients except the sender
            await Clients.Others.SendAsync("broadcastMessage", sender, message);
        }

        /// <summary>
        /// Send or broadcast message.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task SendTyping(object sender)
        {
            // Broadcast the typing notification to all clients except the sender
            await Clients.Others.SendAsync("typing", sender);
        }


        /// <summary>
        /// Joins users to an editing "room".
        /// </summary>
        /// <param name="articleRecordId"></param>
        /// <returns></returns>
        public async Task JoinRoom(string articleRecordId)
        {
            if (!string.IsNullOrEmpty(articleRecordId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, articleRecordId);
                var id = int.Parse(articleRecordId);
                var articleLock = await _dbContext.ArticleLocks.FirstOrDefaultAsync(a => a.ArticleId == id);
                if (articleLock == null)
                {
                    var identityUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == Context.User.Identity.Name);
                    articleLock = new ArticleLock()
                    {
                        Id = Guid.NewGuid(),
                        ArticleId = id,
                        IdentityUserId = identityUser.Id,
                        LockSetDateTime = System.DateTimeOffset.UtcNow,
                        ConnectionId = Context.ConnectionId
                    };
                    _dbContext.ArticleLocks.Add(articleLock);
                    await _dbContext.SaveChangesAsync();
                }
                await NotifyRoomOfLock(articleRecordId);
            }
        }

        /// <summary>
        /// Removes users from an editing "room"
        /// </summary>
        /// <param name="articleRecordId"></param>
        /// <returns></returns>
        public async Task LeaveRoom(string articleRecordId)
        {
            if (!string.IsNullOrEmpty(articleRecordId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, articleRecordId);
                await NotifyRoomOfLock(articleRecordId);
            }

        }

        /// <summary>
        /// Signal other members of the group that a page was just saved, and to reload page.
        /// </summary>
        /// <param name="articleRecordId"></param>
        /// <returns></returns>
        public async Task ArticleSaved(int articleRecordId)
        {
            await Clients.OthersInGroup(articleRecordId.ToString()).SendAsync("ArticleReload");
        }

        /// <summary>
        /// Notifies everyone on this editing room of any article lock.
        /// </summary>
        /// <param name="articleRecordId"></param>
        /// <returns></returns>
        private async Task NotifyRoomOfLock(string articleRecordId)
        {
            var id = int.Parse(articleRecordId);
            var model = await _dbContext.ArticleLocks.Where(w => w.ArticleId == id).Select(l => new ArticleLockViewModel()
            {
                ArticleRecordId = l.ArticleId,
                Id = l.Id,
                LockSetDateTime = l.LockSetDateTime,
                UserEmail = l.IdentityUser.Email,
                ConnectionId = Context.ConnectionId
            }).FirstOrDefaultAsync();
            string message = JsonConvert.SerializeObject(model);
            await Clients.Group(articleRecordId.ToString()).SendAsync("ArticleLock", message);
        }

        /// <summary>
        /// Handles when a client disconnects
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var articleLocks = await _dbContext.ArticleLocks.Where(w => w.ConnectionId == Context.ConnectionId).ToListAsync();

            if (articleLocks.Any())
            {
                var ids = articleLocks.Select(s => s.ArticleId);

                _dbContext.ArticleLocks.RemoveRange(articleLocks);

                await _dbContext.SaveChangesAsync();

                foreach (var id in ids)
                {
                    // Let everyone know of the lock releases
                    await NotifyRoomOfLock(id.ToString());
                }
            }


            await base.OnDisconnectedAsync(exception);
        }

    }
}
