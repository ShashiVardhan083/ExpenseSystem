# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ExpenseSystem.sln .

# Copy project files first (for better caching)
COPY ExpenseSystem.Web/ExpenseSystem.Web.csproj ExpenseSystem.Web/
COPY ExpenseSystem.Application/ExpenseSystem.Application.csproj ExpenseSystem.Application/
COPY ExpenseSystem.Infrastructure/ExpenseSystem.Infrastructure.csproj ExpenseSystem.Infrastructure/
COPY ExpenseSystem.API/ExpenseSystem.API.csproj ExpenseSystem.API/
COPY ExpenseSystem.Domain/ExpenseSystem.Domain.csproj ExpenseSystem.Domain/
# Restore
RUN dotnet restore ExpenseSystem.Web/ExpenseSystem.Web.csproj

# Copy everything else
COPY . .

# Publish
WORKDIR /src/ExpenseSystem.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseSystem.Web.dll"]