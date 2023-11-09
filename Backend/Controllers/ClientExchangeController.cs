using Backend.Data;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace Backend.Controllers
{
    /// <summary>
    /// The ClientExchangeController receives a transcription request from a client via a WebSocket
    /// and returns the transcribed, corrected and converted substitles.
    /// </summary>
    [ApiController]
    public class ClientExchangeController : ControllerBase
    {
        /// <summary>
        /// Dependency Injection for accessing needed Services.
        /// </summary>
        private readonly IAvReceiverService avReceiverService;
        private readonly ISubtitleExporterService subtitleExporterService;

        /// <summary>
        /// Constructor for ClientExchangeController.
        /// Gets instances of services via Dependency Injection.
        /// </summary>
        public ClientExchangeController(ISubtitleExporterService subtitleExporterService,
            IAvReceiverService avReceiverService)
        {
            this.avReceiverService = avReceiverService;
            this.subtitleExporterService = subtitleExporterService;
        }


        [Route("/transcribe")]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) {
                Console.WriteLine ("Rejecting invalid transcription request");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            Console.WriteLine ("Accepting transcription request");
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            CancellationTokenSource ctSource = new CancellationTokenSource();

            Task subtitleExportTask = subtitleExporterService.Start (webSocket, ctSource); // write at end of pipeline
            Task avReceiveTask = avReceiverService.Start (webSocket, ctSource); // read at start of pipeline

            await avReceiveTask;
            try
            {
                await subtitleExportTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine ("Cancellation handled");
            }

            await webSocket.CloseAsync (WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
    }
}
