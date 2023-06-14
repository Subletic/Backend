# Basisimage mit dem .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Setze das Arbeitsverzeichnis
WORKDIR /app

# Kopiere den csproj-Datei in das Arbeitsverzeichnis
COPY Backend/*.csproj ./

# Restore der NuGet-Pakete
RUN dotnet restore

# Kopiere den Rest des Codes in das Arbeitsverzeichnis
COPY ./Backend ./

# Build der Anwendung
RUN dotnet publish -c Release -o out

# Nächste Phase des Dockerfiles
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Setze das Arbeitsverzeichnis
WORKDIR /app

# Kopiere das Build-Ergebnis der vorherigen Phase in das Arbeitsverzeichnis
COPY --from=build-env /app/out .

# Setze den Port, auf dem die API lauscht
EXPOSE 80

# Starte die ASP.NET API
ENTRYPOINT ["dotnet", "Backend.dll"]
