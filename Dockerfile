# https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-7.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY Backend/*.csproj ./Backend
RUN dotnet restore

# copy everything else and build app
COPY Backend/. ./Backend/
WORKDIR /source/Backend
RUN dotnet publish -c release -o app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./

# Installiere FFmpeg Dependency
RUN apt-get -y update
RUN apt-get -y upgrade
RUN apt-get install -y ffmpeg

# Setze den Port, auf dem die API lauscht
EXPOSE 40114

# Starte die ASP.NET API
ENTRYPOINT ["dotnet", "Backend.dll"]
