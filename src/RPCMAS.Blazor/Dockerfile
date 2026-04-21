FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RPCMAS.Blazor/RPCMAS.Blazor.csproj", "src/RPCMAS.Blazor/"]
COPY ["src/RPCMAS.Core/RPCMAS.Core.csproj", "src/RPCMAS.Core/"]
RUN dotnet restore "src/RPCMAS.Blazor/RPCMAS.Blazor.csproj"
COPY . .
WORKDIR "/src/src/RPCMAS.Blazor"
RUN dotnet publish "RPCMAS.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RPCMAS.Blazor.dll"]
