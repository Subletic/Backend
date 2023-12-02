# Backend

Contains an ASP.NET Core project for our Subletic-Backend. It provides a WebSocket-Service/Server, that transcribes an audio-stream an returns a subtitle. If the Frontend is connected, additional correction of the subtitles can be made, before sending the subtitle back to the client.

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

Please also ensure that the `ffmpeg.exe` lies on the PATH of your OS. In Windows you can find it in `Environment Variables` > `System variables` > `PATH` > `Edit`

## Connection

To start the software a few **environment-variables** have to be set. When the software is run for development purpose a **`launchSettings.json`** can be used to set these values. Also note the port **`40114`** the Backend is started on.

| Variable-Name | Value | Development | Production |
|---|---|---|---|
| SPEECHMATICS_API_KEY | >is located in the credentials location< | ✅ | ✅ |
| FRONTEND_URL | http://d.projekte.swe.htwk-leipzig.de:40110 | ❌ | ✅ |
| BACKEND_URL | http://0.0.0.0:40114 | ❌ | ✅ |

**`Properties/launchSettings.json`:**
```json
{
    "iisSettings": {
        "windowsAuthentication": false,
        "anonymousAuthentication": true,
        "iisExpress": {
            "applicationUrl": "http://localhost:36373",
            "sslPort": 44325
        }
    },
    "profiles": {
        "http": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "http://localhost:40114",
            "environmentVariables": {
                "SPEECHMATICS_API_KEY": "<INSERT SPEECHMATICS KEY>",
                "FRONTEND_URL": "http://localhost:40110"
            }
        },
        "IIS Express": {
            "commandName": "IISExpress",
            "launchBrowser": true
        }
    }
}
```

## Ports

| Software    | Port  |
|-------------|-------|
| Frontend    | 40110 |
| Backend     | 40114 |
| Mock Server | 40118 |

