# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.csproj ./
COPY Heloilo.Application/*.csproj ./Heloilo.Application/
COPY Heloilo.Domain/*.csproj ./Heloilo.Domain/
COPY Heloilo.Infrastructure/*.csproj ./Heloilo.Infrastructure/

# Restore dependencies
RUN dotnet restore Heloilo.Backend.csproj

# Copy everything else and build
COPY . .
WORKDIR /src
RUN dotnet build Heloilo.Backend.csproj -c Release --no-restore -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src
RUN dotnet publish Heloilo.Backend.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create directory for SQLite database
RUN mkdir -p /app/data

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
# Render will provide PORT environment variable, ASP.NET Core will use it automatically
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port (Render uses PORT env var, default to 10000 for local testing)
EXPOSE 10000

# Run the application
ENTRYPOINT ["dotnet", "Heloilo.Backend.dll"]

