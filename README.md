# Cosmos HL

[![windows build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml) [![ubunto build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/codeql-analysis.yml)

Cosmos HL/CMS is a dynamic, high performance "[headless](https://en.wikipedia.org/wiki/Headless_content_management_system)"  [web content management system](https://en.wikipedia.org/wiki/Web_content_management_system). Tests have shown that it serves HTML pages two and a half times faster than a static website with blob storage--and it does this without the use of a CDN or  Redis or other third party cache systems.

Key features:

* High performance, "[headless](https://en.wikipedia.org/wiki/Headless_content_management_system)" design.
* Can host a single website simultaneously in AWS and Azure with realtime-synchronization.
* Built for web developers, also includes a rich WSYWIG editor for the non-technical users.
* "[Hybrid](https://en.wikipedia.org/wiki/Mashup_(web_application_hybrid))" design allows you to combine Cosmos HL with your own functionality.

At a minimum each Cosmos instance consists of two websites, a database and blob or file storage. The first is the "Editor." It's role is to create and maintain content for the website.  The second is the "Publisher." It is the website that "publishes" web content either by JSON feeds or as HTML web pages.  This minimulist design can run very inexpensively in either AWS or Azure for lightweight loads.

A more robust installation can have multiple instances running simulateously in AWS and Azure, and can support very high loads and be highly available.



Two repositories are associated with this procject:

* The [Cosmos](https://github.com/CosmosSoftware/Cosmos.Cms) repository contains the "Editor" and the "Publisher" and all the frameworks common to each.
* The "[Cosmos.Publisher](https://github.com/CosmosSoftware/Cosmos.Cms.Publisher)" repository contains a stand alone "Publisher" website.

The publisher repository is a "stock" out of the box Visual Studio application. It is turned into a "Publisher" by adding and configuring the [Cosmos.Common](https://www.nuget.org/packages/CDT.Cosmos.Cms.Common/) NuGet package.


## Cloud-first Design

Cosmos HL is built for the cloud and can integrate with services such as:

* Content Distribution Networks 
  * Akamai
  * Microsoft
  * Verizon
* Blob storage for static files (CSS, JavaScript, images, etc.)
* Google Translate (v3)
* OAuth
  * Google
  * Microsoft

## Getting Started

This documentation is still under development, so check back for more topics as they become available.

* Installation
  * Docker containers [Editor](https://hub.docker.com/repository/docker/toiyabe/cosmoseditor) | Publisher
  * Code deployment
* [Developing with Cosmos](/Documentation/DevelopingWithCosmos.md)

