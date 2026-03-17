FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /App

ENV CI=true

COPY . .

RUN dotnet restore
RUN dotnet build -c Debug --no-restore

RUN pwsh /App/test/End2EndTests/bin/Debug/net8.0/playwright.ps1 install
RUN pwsh /App/test/End2EndTests/bin/Debug/net8.0/playwright.ps1 install-deps

ENTRYPOINT ["dotnet", "test", "-c", "Debug", "--no-build"]
