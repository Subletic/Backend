using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;
using SignalRChat.Hubs;
using System.Threading.Tasks;

namespace SignalRChat.Tests
{
    [TestFixture]
    public class ChatHubTests
    {
        [Test]
        public async Task SendMessage_Should_Send_Message_To_All_Clients()
        {
            // Arrange
            var hub = new ChatHub();
            var mockClients = new HubCallerClients();
            var mockContext = new HubCallerContext(null, null, null, null);
            mockContext.ConnectionId = "connectionId";
            mockClients.All = new MockClientProxy();

            hub.Context = new HubContext(mockContext, mockClients);

            string expectedUser = "John";
            string expectedMessage = "Hello, world!";

            // Act
            await hub.SendMessage(expectedUser, expectedMessage);

            // Assert
            Assert.AreEqual(1, mockClients.All.InvokeCount);
            Assert.AreEqual("ReceiveMessage", mockClients.All.MethodName);
            Assert.AreEqual(new object[] { expectedUser, expectedMessage }, mockClients.All.Parameters);
        }
    }

    public class MockClientProxy : IClientProxy
    {
        public int InvokeCount { get; private set; }
        public string MethodName { get; private set; }
        public object[] Parameters { get; private set; }

        public Task SendCoreAsync(string methodName, object[] args)
        {
            InvokeCount++;
            MethodName = methodName;
            Parameters = args;
            return Task.CompletedTask;
        }

        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            MethodName = methodName;
            Parameters = args;
            return Task.CompletedTask;
        }

    }
}
