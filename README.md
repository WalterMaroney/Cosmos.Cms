# [Cosmos Content Management Platform](https://cosmos.azureedge.net)

[![Build](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml/badge.svg)](https://github.com/CosmosSoftware/Cosmos.Cms/actions/workflows/main-publish.yml)

Cosmos HL/CMS (C/CMS) is a dynamic, high performance "headless" [web content management platform](https://en.wikipedia.org/wiki/Web_content_management_system). Tests have shown that it serves HTML pages two and a half times faster than a static website with blob storage.

It is a "[hybrid](https://en.wikipedia.org/wiki/Mashup_(web_application_hybrid))"  because of its open architecture allows you to "mashup," or combine the functionality of this CMS with your own web application.

Each Cosmos instance has at least two websites, a database and blob or file storage. One website is the "publisher," and it's job is to host the public website.  The second is the "Editor," and it's role is to create and maintain content for the website.  The two are split out so that the "publisher" isn't burdened with the heavy load associated with content management--which ranges from editing HTML and other code, to uploading files such as images, videos, or JavaScript files or Cascading Style Sheet files.

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
