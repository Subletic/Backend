namespace BackendTests;

using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Backend.Controllers;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Serilog;
using Serilog.Events;

public class SpeechmaticsConnectionServiceTests
{
    private const string TEST_ENVVAR_NAME = "SpeechmaticsApiKeyTest";

    private readonly ILogger logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Debug)
        .WriteTo.Console()
        .CreateLogger();

    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("SpeechmaticsConnectionService:SPEECHMATICS_API_URL_AUTHORITY", "test"),
                new KeyValuePair<string, string?>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS", "5.0"),
            },
        })
        .Build();

    private void assertServiceState(ISpeechmaticsConnectionService service, bool shouldBeConnected)
    {
        // Should always work
        Assert.That(service.JsonOptions, Is.Not.Null);
        Assert.That(service.AudioFormat, Is.Not.Null);

        // Result is state-specific
        Assert.That(service.Connected, Is.EqualTo(shouldBeConnected));
        Assert.That(service.CancellationToken.IsCancellationRequested, Is.EqualTo(!shouldBeConnected));

        // Behaviour is state-specific
        if (shouldBeConnected)
        {
            Assert.DoesNotThrow(() => service.ThrowIfNotConnected());
            Assert.DoesNotThrow(() => { WebSocket test = service.Socket; });
            Assert.That(service.Socket.State, Is.EqualTo(WebSocketState.Open));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => service.ThrowIfNotConnected());
            Assert.Throws<InvalidOperationException>(() => { WebSocket test = service.Socket; });
        }
    }

    [Test]
    public void Constructor_CorrectUnconnectedState()
    {
        ISpeechmaticsConnectionService service = new SpeechmaticsConnectionService(
            configuration,
            logger);

        assertServiceState(service, false);
    }

    [Test]
    public async Task Connect_HandlesNoKeyCorrectly()
    {
        ISpeechmaticsConnectionService service = new SpeechmaticsConnectionService(
            configuration,
            logger);

        bool result = await service.Connect(CancellationToken.None);

        Assert.That(result, Is.EqualTo(false));
        assertServiceState(service, false);
    }

    [Test]
    public async Task Disconnect_HandlesWrongStateCorrectly()
    {
        ISpeechmaticsConnectionService service = new SpeechmaticsConnectionService(
            configuration,
            logger);

        bool result = await service.Disconnect(true, CancellationToken.None);

        Assert.That(result, Is.EqualTo(false));
        assertServiceState(service, false);
    }

    [Test]
    public async Task RegisterApiKey_HandlesMissingKeyCorrectly()
    {
        ISpeechmaticsConnectionService service = new SpeechmaticsConnectionService(
            configuration,
            logger);

        Environment.SetEnvironmentVariable(TEST_ENVVAR_NAME, null);

        bool result = await service.RegisterApiKey(TEST_ENVVAR_NAME);

        Assert.That(result, Is.EqualTo(false));
        assertServiceState(service, false);
    }

    [Test]
    public async Task RegisterApiKey_HandlesNoConnectionCorrectly()
    {
        ISpeechmaticsConnectionService service = new SpeechmaticsConnectionService(
            configuration,
            logger);

        Environment.SetEnvironmentVariable(TEST_ENVVAR_NAME, "test");

        bool result = await service.RegisterApiKey(TEST_ENVVAR_NAME);

        Assert.That(result, Is.EqualTo(false));
        assertServiceState(service, false);
    }
}
