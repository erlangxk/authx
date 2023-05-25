FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine  AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["authx/authx.fsproj", "authx/"]
RUN dotnet restore --use-current-runtime "authx/authx.fsproj"
COPY . .
WORKDIR "/src/authx"
RUN dotnet build "authx.fsproj" --use-current-runtime -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "authx.fsproj" --use-current-runtime --no-restore -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./authx"]
