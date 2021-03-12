#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MamRenewer/MamRenewer.csproj", "MamRenewer/"]
RUN dotnet restore "MamRenewer/MamRenewer.csproj"
COPY . .
WORKDIR "/src/MamRenewer"
RUN dotnet build "MamRenewer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MamRenewer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MamRenewer.dll"]