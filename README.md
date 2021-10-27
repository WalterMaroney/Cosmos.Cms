# [Cosmos Content Management Platform](https://cosmos.azureedge.net)

[![Build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml) [![ubunto build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/dotnet.yml)

Key features:

* High performance, "[headless](https://en.wikipedia.org/wiki/Headless_content_management_system)" design.
* Can host a single website simultaneously in AWS and Azure with realtime-synchronization.
* Built for web developers, also includes a rich WSYWIG editor for the non-technical users.
* "[Hybrid](https://en.wikipedia.org/wiki/Mashup_(web_application_hybrid))" design allows you to combine Cosmos HL with your own functionality.

Cosmos HL/CMS is a dynamic, high performance "[headless](https://en.wikipedia.org/wiki/Headless_content_management_system)" [web content management platform](https://en.wikipedia.org/wiki/Web_content_management_system). Tests have shown that it serves HTML pages two and a half times faster than a static website with blob storage--and it does this without the use of a CDN or  Redis or other third party cache systems.

It is also a "[hybrid](https://en.wikipedia.org/wiki/Mashup_(web_application_hybrid))" application because of its open architecture that allows you to "mashup," or combine the functionality of this CMS with your own web application.

At a minimum each Cosmos instance consists of two websites, a database and blob or file storage. The first is the "Editor." It's role is to create and maintain content for the website.  The second is the "Publisher." It is the website that "publishes" web content either by JSON feeds or as HTML web pages.

This split design means that the "Publisher" isn't burdened with the heavy load associated with content management.

Two repositories are associated with this procject:

* The [Cosmos](https://github.com/StateOfCalifornia/Cosmos) repository contains the "Editor," the "Publisher" and all the frameworks common to each.
* The "[Cosmos.Publisher](https://github.com/StateOfCalifornia/Cosmos.Publisher)" repository contains a "Publisher" website.  This is a "stock" out of the box Visual Studio application. It is turned into a "Publisher" by adding and configuring the [Cosmos.Common](https://www.nuget.org/packages/CDT.Cosmos.Cms.Common/) NuGet package.


## Cloud-first Design

C/CMS is built for [the cloud](https://cosmos.azureedge.net/), and makes use of services such as:

* Content Distribution Networks 
  * Akamai
  * Microsoft
  * Verizon
* Blob storage for assets
* Google Translate (v3)
* OAuth
  * Google
  * Microsoft

C/CMS also takes advantage of the cloud's ability to automatically scale, and, run simultaneously in multiple regions.

## Getting Started

This documentation is still under development, so check back for more topics as they become available.

* [Developing with Cosmos](/Documentation/DevelopingWithCosmos.md)
* [Main documentation page](https://cosmos.azureedge.net/documentation)
* [Installation](https://cosmos.azureedge.net/installation)
* [Configuration](https://cosmos.azureedge.net/configuration)
* [Website Setup](https://cosmos.azureedge.net/website_setup)
* [Create and edit web pages](https://cosmos.azureedge.net/edit_page)
* [Web page versioning](https://cosmos.azureedge.net/page_versions)
* [Scheduling page publishing](https://cosmos.azureedge.net/page_versions#ScheduleRelease)
* [Product videos](https://cosmos.azureedge.net/video)
* [File management](https://cosmos.azureedge.net/file_management)
