# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ExpenseSystem.API/ExpenseSystem.API.csproj", "ExpenseSystem.API/"]
COPY ["ExpenseSystem.Application/ExpenseSystem.Application.csproj", "ExpenseSystem.Application/"]
COPY ["ExpenseSystem.Domain/ExpenseSystem.Domain.csproj", "ExpenseSystem.Domain/"]
COPY ["ExpenseSystem.Infrastructure/ExpenseSystem.Infrastructure.csproj", "ExpenseSystem.Infrastructure/"]

RUN dotnet restore "ExpenseSystem.API/ExpenseSystem.API.csproj"

COPY . .
WORKDIR "/src/ExpenseSystem.API" 
RUN dotnet publish -c Release -o /app/publish 

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ExpenseSystem.API.dll"]