# Backend

Contains an ASP.NET Core project for our Subletic-Backend. It provides a WebSocket-Service/Server, that transcribes an audio-stream an returns a Subtitle. If the Frontend is connected, additional correction of the Subtitles can be made, before sending the subtitle back to the Client.

## Usage

| Description | Command |
|---|---|
| Installation of FFmpeg | `winget install Gyan.FFmpeg` |
| Check if FFmpeg installation was successful | `ffmpeg -version` |
| Installation of .NET SDK | `winget install Microsoft.DotNet.SDK.7` |
| Check if .NET-SDK installation was successful | `dotnet --version` |
| Load all dependency's | `dotnet restore` |
| Start Backend | `dotnet run` |
| Run UnitTests | `dotnet test` |

## Connection

To start the software a few environment-variables have to be set:

| Variable-Name | Value | Development | Production |
|---|---|---|---|
| SPEECHMATICS_API_KEY | >is located in the credentials location< | ✅ | ✅ |
| FRONTEND_URL | http://d.projekte.swe.htwk-leipzig.de:40110 | ❌ | ✅ |

## Ports

| Software    | Port  |
|-------------|-------|
| Frontend    | 40110 |
| Backend     | 40114 |
| Mock Server | 40118 |

