#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# This Docker file is designed to run with GitHub Actions.
# Notice there is no copy command here? That is because the build step is done outside
# of the container. This allows private Nuget Packages to be installed much easier.
# Also, it means the docker container uses the dame distro as the file install.
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 80
EXPOSE 443
FROM base AS final
COPY /home/runner/work/Cosmos.Cms/Cosmos.Cms/CDT.Cosmos.Cms.Website/bin/Release/net5.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CDT.Cosmos.Cms.Website.dll"]
