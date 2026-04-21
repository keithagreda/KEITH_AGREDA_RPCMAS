FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RPCMAS.API/RPCMAS.API.csproj", "src/RPCMAS.API/"]
COPY ["src/RPCMAS.Core/RPCMAS.Core.csproj", "src/RPCMAS.Core/"]
COPY ["src/RPCMAS.Infrastructure/RPCMAS.Infrastructure.csproj", "src/RPCMAS.Infrastructure/"]
RUN dotnet restore "src/RPCMAS.API/RPCMAS.API.csproj"
COPY . .
WORKDIR "/src/src/RPCMAS.API"
RUN dotnet publish "RPCMAS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RPCMAS.API.dll"]
