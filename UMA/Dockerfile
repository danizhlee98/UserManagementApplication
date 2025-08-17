#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UMA/UMA.csproj", "UMA/"]
RUN dotnet restore "UMA/UMA.csproj"
COPY . .
WORKDIR "/src/UMA"
RUN dotnet build "UMA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UMA.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UMA.dll"]