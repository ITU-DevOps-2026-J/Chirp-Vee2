# Makefile for .NET SonarScanner

# Project settings
ORGANIZATION_KEY := itu-devops-2026-j
PROJECT_KEY := ITU-DevOps-2026-J_Chirp-Vee2
SONAR_HOST_URL := https://sonarcloud.io
SONAR_TOKEN := $(SONAR_TOKEN)
CONFIG := Debug
SOLUTION := ./Chirp.sln

# Default target
all: build

# Clean build
clean:
	dotnet clean $(SOLUTION)

# Restore packages
restore:
	dotnet restore $(SOLUTION)

# Build the project
build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIG) --no-incremental

# SonarScanner: begin analysis
sonar-begin:
	dotnet sonarscanner begin \
	    /o:"$(ORGANIZATION_KEY)" \
		/k:"$(PROJECT_KEY)" \
		/d:sonar.host.url="$(SONAR_HOST_URL)" \
		/d:sonar.token="$(SONAR_TOKEN)" \
		/d:sonar.verbose=true

# SonarScanner: end analysis
sonar-end:
	dotnet sonarscanner end \
		/d:sonar.token="$(SONAR_TOKEN)"

# Run full sonar analysis
sonar: sonar-begin build sonar-end

.PHONY: all clean restore build sonar-begin sonar-end sonar