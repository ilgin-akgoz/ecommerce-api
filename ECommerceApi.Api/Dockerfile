FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerceApi.Api/*.csproj ./ECommerceApi.Api/
RUN dotnet restore ./ECommerceApi.Api/ECommerceApi.Api.csproj

COPY ECommerceApi.Api/. ./ECommerceApi.Api/
WORKDIR /src/ECommerceApi.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerceApi.Api.dll"]