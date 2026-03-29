FROM dhi.io/dotnet:10-sdk

# Install curl + dependencies
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    wget \
    gnupg \
    apt-transport-https \
    && rm -rf /var/lib/apt/lists/*

# Install Microsoft PowerShell repo + pwsh
RUN wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell \
    && rm -rf /var/lib/apt/lists/* \
    && rm packages-microsoft-prod.deb


#RUN useradd -m appuser

WORKDIR /App
ENV CI=true

COPY Chirp-Project .

RUN dotnet restore
RUN dotnet build -c Debug --no-restore

#  install system dependencies as root
RUN pwsh /App/test/End2EndTests/bin/Debug/net10.0/playwright.ps1 install-deps

# fix permissions before switching user
# RUN chown -R appuser:appuser /App

# USER appuser

# install browsers as appuser
RUN pwsh /App/test/End2EndTests/bin/Debug/net10.0/playwright.ps1 install

ENTRYPOINT ["dotnet", "test", "-c", "Debug", "--no-build"]