FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /work

COPY ./Directory.Build.props ./
COPY ./src/*/*.csproj ./
RUN for projectFile in $(ls *.csproj); \
do \
  mkdir -p ${projectFile%.*}/ && mv $projectFile ${projectFile%.*}/; \
done

ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

RUN dotnet restore /work/Hotel.Api/Hotel.Api.csproj

COPY src .

FROM build AS publish
WORKDIR /work/Hotel.Api

ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

RUN dotnet publish -c Release -o /app --no-restore



FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app .

ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

HEALTHCHECK --interval=5m --timeout=3s --start-period=10s --retries=1 \
  CMD curl --fail http://localhost:80/health || exit 1

EXPOSE 80

ENTRYPOINT ["dotnet", "Hotel.Api.dll"]
