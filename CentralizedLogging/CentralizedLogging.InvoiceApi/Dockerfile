#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["CentralizedLogging.InvoiceApi/CentralizedLogging.InvoiceApi.csproj", "CentralizedLogging.InvoiceApi/"]
COPY ["CentralizedLogging.Core/CentralizedLogging.Core.csproj", "CentralizedLogging.Core/"]
RUN dotnet restore "CentralizedLogging.InvoiceApi/CentralizedLogging.InvoiceApi.csproj"
COPY . .
WORKDIR "/src/CentralizedLogging.InvoiceApi"
RUN dotnet build "CentralizedLogging.InvoiceApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CentralizedLogging.InvoiceApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CentralizedLogging.InvoiceApi.dll"]