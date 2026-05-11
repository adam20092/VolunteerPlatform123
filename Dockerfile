# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копиране само на .csproj файла за по-бързо възстановяване на пакетите
COPY ["volunteerplatform/volunteerplatform.csproj", "volunteerplatform/"]
RUN dotnet restore "volunteerplatform/volunteerplatform.csproj"

# Копиране на останалите файлове и компилиране
COPY . .
WORKDIR "/src/volunteerplatform"
RUN dotnet build "volunteerplatform.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "volunteerplatform.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Конфигурация за Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "volunteerplatform.dll"]
