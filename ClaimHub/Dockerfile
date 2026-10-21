FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ClaimHub.sln ./
COPY src/ClaimHub.Domain/ClaimHub.Domain.csproj src/ClaimHub.Domain/
COPY src/ClaimHub.Infrastructure/ClaimHub.Infrastructure.csproj src/ClaimHub.Infrastructure/
COPY src/ClaimHub.Web/ClaimHub.Web.csproj src/ClaimHub.Web/
RUN dotnet restore src/ClaimHub.Web/ClaimHub.Web.csproj

COPY src/ src/
RUN dotnet publish src/ClaimHub.Web/ClaimHub.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClaimHub.Web.dll"]
