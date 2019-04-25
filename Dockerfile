FROM microsoft/dotnet:2.2-sdk AS installer-env

COPY . /src/AzPlatformMonitor.Functions
RUN cd /src/AzPlatformMonitor.Functions && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish src/AzPlatformMonitor.Functions/AzPlatformMonitor.Functions.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]

# Install Curl
RUN apt-get update && apt-get install -y --no-install-recommends curl

# Thanks https://jpmelos.com/articles/how-use-chrome-selenium-inside-docker-container-running-python/
# Install Chrome for Selenium
RUN curl https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb -o /chrome.deb
RUN dpkg -i /chrome.deb || apt-get install -yf
RUN rm /chrome.deb
