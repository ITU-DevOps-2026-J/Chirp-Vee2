FROM mcr.microsoft.com/dotnet/sdk:8.0

#RUN useradd -m appuser

WORKDIR /App
ENV CI=true

COPY Chirp-Project .

RUN dotnet restore
RUN dotnet build -c Debug --no-restore

#  install system dependencies as root
RUN pwsh /App/test/End2EndTests/bin/Debug/net8.0/playwright.ps1 install-deps

# fix permissions before switching user
# RUN chown -R appuser:appuser /App

# USER appuser

# install browsers as appuser
RUN pwsh /App/test/End2EndTests/bin/Debug/net8.0/playwright.ps1 install

ENTRYPOINT ["dotnet", "test", "-c", "Debug", "--no-build"]