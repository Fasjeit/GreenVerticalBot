FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# fighting the cache...
#RUN rm -rf /app

# Copy everything
COPY src ./
WORKDIR /app/GreenVerticalBot
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
RUN apt-get update && \
    apt-get install -y software-properties-common && \
    rm -rf /var/lib/apt/lists/*
RUN add-apt-repository ppa:quamotion/ppa
RUN apt-get install -y libgdiplus
WORKDIR /app
COPY --from=build-env /app/GreenVerticalBot/out .
ENTRYPOINT ["dotnet","GreenVerticalBot.dll"]
