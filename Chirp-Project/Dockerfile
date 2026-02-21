FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /App

# Copy everything
COPY ./src ./
# Restore as distinct layers
RUN dotnet restore Web/Web.csproj
# Build and publish a release
RUN dotnet publish Web/Web.csproj -c Release -o /App/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "Web.dll"]