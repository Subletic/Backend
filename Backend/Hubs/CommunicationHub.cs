using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    /// <summary>
    /// Used for SignalR communication.
    /// </summary>
    public class CommunicationHub : Hub
    {
        /// <summary>
        /// Not actually used.
        /// Should contain Methods which are called from the Frontend.
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "hallo welt");
        }
    }
}