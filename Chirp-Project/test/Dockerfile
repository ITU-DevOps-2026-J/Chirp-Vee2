FROM dhi.io/dotnet:10-sdk

RUN apt-get update && apt-get install -y curl ca-certificates gnupg \
    && curl -sSL https://packages.microsoft.com/config/debian/12/prod.list \
    -o /etc/apt/sources.list.d/microsoft-prod.list \
    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc \
    | gpg --dearmor \
    | tee /usr/share/keyrings/microsoft-prod.gpg > /dev/null \
    && apt-get update \
    && apt-get install -y powershell \
    && rm -rf /var/lib/apt/lists/*

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