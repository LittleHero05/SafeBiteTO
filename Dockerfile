# ---- Build stage: has the .NET SDK, compiles the app ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SafeBite.Api/SafeBite.Api.csproj SafeBite.Api/
RUN dotnet restore SafeBite.Api/SafeBite.Api.csproj

COPY SafeBite.Api/ SafeBite.Api/
COPY data/seed/ data/seed/
RUN dotnet publish SafeBite.Api/SafeBite.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage: only the small ASP.NET runtime + your published app ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SafeBite.Api.dll"]