FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EcommerceApi.Api/*.csproj ./EcommerceApi.Api/
RUN dotnet restore ./EcommerceApi.Api/EcommerceApi.Api.csproj

COPY EcommerceApi.Api/. ./EcommerceApi.Api/
WORKDIR /src/EcommerceApi.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "EcommerceApi.Api.dll"]