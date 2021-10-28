#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# This Docker file is designed to run with GitHub Actions.
# Also, it means the docker container uses the dame distro as the file install.
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 80
EXPOSE 443
FROM base AS final
COPY /home/runner/work/app/cosmoseditor App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CDT.Cosmos.Cms.dll"]
