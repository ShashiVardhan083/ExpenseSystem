# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 8080

# SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution file
COPY ["ExpenseSystem.sln", "./"]

# Copy project files
COPY ["ExpenseSystem.API/ExpenseSystem.API.csproj", "ExpenseSystem.API/"]
COPY ["ExpenseSystem.Application/ExpenseSystem.Application.csproj", "ExpenseSystem.Application/"]
COPY ["ExpenseSystem.Infrastructure/ExpenseSystem.Infrastructure.csproj", "ExpenseSystem.Infrastructure/"]
COPY ["ExpenseSystem.Domain/ExpenseSystem.Domain.csproj", "ExpenseSystem.Domain/"]
COPY ["ExpenseSystem.Web/ExpenseSystem.Web.csproj", "ExpenseSystem.Web/"]

# Restore NuGet packages
RUN dotnet restore

# Copy remaining source code
COPY . .

# Move into API project
WORKDIR "/src/ExpenseSystem.API"

# Publish application
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ExpenseSystem.API.dll"]