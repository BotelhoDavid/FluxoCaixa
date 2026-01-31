FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto e restaurar
COPY ["src/FluxoCaixa.API/FluxoCaixa.API.csproj", "src/FluxoCaixa.API/"]
COPY ["src/FluxoCaixa.Application/FluxoCaixa.Application.csproj", "src/FluxoCaixa.Application/"]
COPY ["src/FluxoCaixa.Domain/FluxoCaixa.Domain.csproj", "src/FluxoCaixa.Domain/"]
COPY ["src/FluxoCaixa.Infra.Data/FluxoCaixa.Infra.Data.csproj", "src/FluxoCaixa.Infra.Data/"]
COPY ["src/FluxoCaixa.Infra.CrossCutting.ExceptionHandler/FluxoCaixa.Infra.CrossCutting.ExceptionHandler.csproj", "src/FluxoCaixa.Infra.CrossCutting.ExceptionHandler/"]

RUN dotnet restore "src/FluxoCaixa.API/FluxoCaixa.API.csproj"

# Copiar todo o código e buildar
COPY . .
WORKDIR "/src/src/FluxoCaixa.API"
RUN dotnet build "FluxoCaixa.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FluxoCaixa.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluxoCaixa.API.dll"]
