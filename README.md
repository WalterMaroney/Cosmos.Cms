# Cosmos HL/CMS

[![ubunto build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml)

 Cosmos HL/CMS is a dynamic, high performance "[headless](https://en.wikipedia.org/wiki/Headless_content_management_system)"  [web content management system](https://en.wikipedia.org/wiki/Web_content_management_system) whose hybrid design combines a static website with a dynamic one.  The static website hosts CSS, JavaScript, images and other files that are static in nature while the dynamic website hosts HTML pages that can change at any time. A separate "Editor" website maintains content for both.
 
Key features:

* A dynamic website that performs like a state website.
* Can host a single website simultaneously in AWS and Azure with realtime-synchronization.
* Code editor for web developers and a WSYWIG editor for the non-technical.
* Cost effective to run.
* Optionally integrates with Akamai, Microsoft, and Verizon CDNs.

Two repositories are associated with this procject:

* The [Cosmos](https://github.com/CosmosSoftware/Cosmos.Cms) repository contains the "Editor" and the "Publisher" and all the frameworks common to each.
* The "[Cosmos Publisher](https://github.com/CosmosSoftware/Cosmos.Cms.Publisher)" repository contains a stand alone "Publisher" website.

The publisher repository is a "stock" out of the box Visual Studio application. It is turned into a "Publisher" by adding and configuring the [Cosmos Common](https://www.nuget.org/packages/CDT.Cosmos.Cms.Common/) NuGet package.

## Origin

If you would like to know why Cosmos HL was created and how it was architected, please read [the origin story](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Origin.md).

## Getting Started

This documentation is still under development, so check back for more topics as they become available.

* Setup Help
  * [Installation instructions](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/Index.md)
  * Links to Docker containers containers: [Editor](https://hub.docker.com/repository/docker/toiyabe/cosmoseditor) | [Publisher](https://hub.docker.com/repository/docker/toiyabe/cosmospublisher)
* [Developing with Cosmos](/Documentation/DevelopingWithCosmos.md)

