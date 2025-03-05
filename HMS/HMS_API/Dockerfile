# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for the main API and its dependencies
COPY ["HMS_API/HMS_API.csproj", "HMS_API/"]
COPY ["DBLayer/DBLayer.csproj", "DBLayer/"]
COPY ["HMSModels/HMSModels.csproj", "HMSModels/"]
COPY ["UniComponents/UniComponents.csproj", "UniComponents/"]

# Restore dependencies for the API project
RUN dotnet restore "HMS_API/HMS_API.csproj"

COPY HMS_API/.env HMS_API/.env

# Copy the remaining source files
COPY . .

WORKDIR /src/HMS_API

# Publish the API (builds and publishes in one command)
RUN dotnet publish "HMS_API.csproj" -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "HMS_API.dll"]