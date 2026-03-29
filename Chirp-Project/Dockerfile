FROM dhi.io/dotnet:10-sdk AS build

WORKDIR /App

# Copy everything
COPY ./src ./

# Create volume directory
RUN mkdir -p /App/data \
# Restore as distinct layers
    && dotnet restore Web/Web.csproj \
# Build and publish a release
    && dotnet publish Web/Web.csproj -c Release -o /App/out

# Build runtime image
FROM dhi.io/aspnetcore:10
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "Web.dll"]