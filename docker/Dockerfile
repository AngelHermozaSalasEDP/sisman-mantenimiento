FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Sisman.Mantenimiento.slnx ./
COPY src/ src/
RUN dotnet publish src/Sisman.Mantenimiento.Api -c Release -o /app --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Sisman.Mantenimiento.Api.dll"]
