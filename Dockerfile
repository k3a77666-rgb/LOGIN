# Usa la imagen SDK 10.0 para compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia el archivo del proyecto y restaura las dependencias
COPY ["LOGIN.csproj", "."]
RUN dotnet restore

# Copia el resto del código y compila la aplicación
COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- CAMBIO IMPORTANTE AQUÍ ---
# Usa la imagen ASP.NET Runtime 10.0 (NO 8.0) para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copia los archivos publicados desde la etapa de compilación
COPY --from=build /app/publish .

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "LOGIN.dll"]