FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el .csproj y restaurar
COPY ["LOGIN.csproj", "."]
RUN dotnet restore

# Copiar todo y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Imagen final CON .NET 10 (no 8)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LOGIN.dll"]