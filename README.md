# Cosmos CMS README

This project is Sponsored by  [![Tek Yantra Logo](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/tekyantra.png?raw=true)](https://tekyantra.com/)

[![ubunto build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml) 
[![NuGet Badge](https://buildstats.info/nuget/CDT.Cosmos.Cms.Common)](https://www.nuget.org/packages/CDT.Cosmos.Cms.Common/)

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://cosmos.moonrise.net/get_started/install)

Quick links: [Developer Help](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/DevelopingWithCosmos.md) | [Manual Docker Install](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/README.md#docker-container-manual-install)

## About Cosmos CMS

### Status

Cosmos CMS began as a project within the [State of California's](https://www.ca.gov/) [California Department of Technology](https://cdt.ca.gov/), Office of Enterprise Technology from 2019 through 2021 and was built as a collaboration between it's software engineering and DevOps teams.

Starting in December of 2021 management at the Department of Technology decided to make this project was made open-source.

In January of 2022 [Tek Yantra](https://tekyantra.com/) offered to sponsor this project and become a contributor.  Tek Yantra staff have been involved with this project from the begining in 2019.

Now the project is in a phase where we are trying to grow our developer and user community.

### Why a CMS

This project began out of a need for a lean, high performance Content Management System that has the capacity to handle extremely high number of users and be highly available.  It hosted websites built for the fire response of 2019 and COVID 19 response of 2020.

[Please see our blog article](https://cosmos.moonrise.net/blog) that describes the unique architecture of this system.

# Cosmos Repos

Three repositories are associated with this project:

* The [Cosmos](https://github.com/CosmosSoftware/Cosmos.Cms) repository contains the "Editor" and the "Publisher" and all the frameworks common to each.
* The "[Cosmos Publisher](https://github.com/CosmosSoftware/Cosmos.Cms.Publisher)" repository contains a stand alone "Publisher" website.
* [Open-source layouts](https://github.com/CosmosSoftware/Cosmos.Starter.Layouts) made ready for use with Cosmos CMS.

The publisher repository is a "stock" out of the box Visual Studio application. It is turned into a "Publisher" by adding and configuring the [Cosmos Common](https://www.nuget.org/packages/CDT.Cosmos.Cms.Common/) NuGet package.

Would you like to fork one of our repos and contribute? See our [contributing guidelines](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/CONTRIBUTING.md) to get started.

## Developing with Cosmos

If you would like to download and work with the code in this repository, [please see our documentation on how to get started](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/DevelopingWithCosmos.md).

## Getting Started

You can deploy Cosmos to Azure by clicking the following button (recommended) or manually install by following the directions below.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](/Documentation/Installation/AzureClickInstall.md)

This documentation is still under development, so check back for more topics as they become available.

## Docker Container Manual Install

* Setup Help
  * [Manual installation instructions](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/Index.md)
  * Links to Docker containers: [Editor](https://hub.docker.com/repository/docker/toiyabe/cosmoseditor) | [Publisher](https://hub.docker.com/repository/docker/toiyabe/cosmospublisher)
* [Developing with Cosmos](/Documentation/DevelopingWithCosmos.md)

