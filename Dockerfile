FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY UnfathomablePayLink.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish UnfathomablePayLink.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data && chown -R app:app /app/data

COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ConnectionStrings__PayLinkDb="Data Source=/app/data/PayLink.db"

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "UnfathomablePayLink.dll"]
