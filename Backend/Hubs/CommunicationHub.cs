using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class CommunicationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "hallo welt");
        }
    }
}