using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class CommunicationHub : Hub
    {
        /// <summary>
        /// Not actually used.
        /// Hub needs content to work.
        /// Should contain Methods which are called from the Frontend.
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "hallo welt");
        }
    }
}