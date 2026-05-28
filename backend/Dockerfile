# Use it as sdk for project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# We will work in /src
WORKDIR /src

# Copy csproj to /src
COPY ["LibraryPlatform.csproj", "./"]
RUN dotnet restore "LibraryPlatform.csproj"

# Build app in /app/build using /src
COPY . .
RUN dotnet build "LibraryPlatform.csproj" -c Release -o /app/build

# Publish app in /app/build using /src
FROM build AS publish
RUN dotnet publish "LibraryPlatform.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Go to /app to make last steps
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose ports
ENV ASPNETCORE_URLS="http://+:5000"
EXPOSE 5000

# We need this files from /app/publish to preserve them in /app!
COPY --from=publish /app/publish .

# Run!
ENTRYPOINT ["dotnet", "LibraryPlatform.dll"]
