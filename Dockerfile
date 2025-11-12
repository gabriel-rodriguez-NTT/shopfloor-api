# ========== BUILD STAGE ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar primero los archivos de proyecto (mejora el cacheo de dependencias)
COPY ["ShopfloorAssistant.Core/ShopfloorAssistant.Core.csproj", "ShopfloorAssistant.Core/"]
COPY ["ShopfloorAssistant.AppService/ShopfloorAssistant.AppService.csproj", "ShopfloorAssistant.AppService/"]
COPY ["ConsoleApp2/ConsoleApp2.csproj", "ConsoleApp2/"]

# Restaurar dependencias

# Copiar el resto del código fuente
COPY . .
RUN dotnet restore "ConsoleApp2/ConsoleApp2.csproj"

# Publicar la API en modo Release
WORKDIR /src/ConsoleApp2
RUN dotnet publish "ConsoleApp2.csproj" -c Release -o /app/publish --no-restore

# ========== RUNTIME STAGE ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Variables de entorno recomendadas
ENV ASPNETCORE_URLS=http://+:8080
#ENV ASPNETCORE_ENVIRONMENT=Production
#ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copiar archivos publicados
COPY --from=build /app/publish .

# Exponer el puerto HTTP
EXPOSE 8080

# Comando de entrada
ENTRYPOINT ["dotnet", "ConsoleApp2.dll"]
