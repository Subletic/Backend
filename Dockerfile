# https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-7.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY Backend/*.csproj ./Backend/
WORKDIR /source/Backend
RUN dotnet restore

# copy everything else and build app
WORKDIR /source
COPY Backend/. ./Backend/
WORKDIR /source/Backend
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
COPY Backend/ssl/server.pfx ./ssl/server.pfx

# install ffmpeg
RUN apt-get update && \
    apt-get -y upgrade && \
    apt-get install -y --fix-missing ffmpeg

# expose port 40114
EXPOSE 40114

# start the app
ENTRYPOINT ["dotnet", "Backend.dll"]
