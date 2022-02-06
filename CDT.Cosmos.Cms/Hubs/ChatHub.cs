using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Hubs
{
    /// <summary>
    /// Chat hub
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
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

    }
}
