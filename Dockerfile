FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# fighting the cache...
#RUN rm -rf /app

# Copy everything
COPY src ./
WORKDIR /app/GvBot
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/GvBot/out .
ENTRYPOINT ["dotnet","GvBot.dll"]
