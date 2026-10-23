# Imagen de la API o de la web, según PROYECTO (Sisman.Mantenimiento.Api o Sisman.Mantenimiento.Web).
ARG PROYECTO=Sisman.Mantenimiento.Api

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROYECTO
WORKDIR /src
COPY Sisman.Mantenimiento.slnx ./
COPY src/ src/
RUN dotnet publish src/${PROYECTO} -c Release -o /app --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG PROYECTO
ENV PROYECTO=${PROYECTO}
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["sh", "-c", "exec dotnet ${PROYECTO}.dll"]
