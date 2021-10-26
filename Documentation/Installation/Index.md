# Cosmos Installation

The type of Cosmos installation depends on what your needs my be. For many situations a basic installation may suffice,
but if higher performance or higher availablility is required then Redis, CDN and other options might be considered.

Begin your installation by following our basic installation instructions. This will install a database, file storage and
two web applications--one for the "Publisher" and another for the "Editor."

## Basic Installation

The basic install requires four resources to be deloployed: a database, blob storage account, and a web application for the publisher and the editor.  Shown below are links
that show how to install these resources in Amazon Web Services (AWS) and Microsoft Azure.

### Database

Cosmos uses MS SQL Server as it's database. Any SQL server database will do so long as the Editor and Publisher websites can connect. Below are links that show how to deploy a database to AWS and Azure:

* [Amazon RDS For SQL Server](https://aws.amazon.com/rds/sqlserver/)
* [Azure SQL Server](https://azure.microsoft.com/en-us/products/azure-sql/database/)

### File Storage

Currently Cosmos will store files in AWS S3 or Azure Storage accounts. Make sure each is accessible to the "Editor" and each as the public website enabled.

* [AWS S3 with public website](https://docs.aws.amazon.com/AmazonS3/latest/userguide/HostingWebsiteOnS3Setup.html). Note you will need to [enable SSL for the S3 website](https://aws.amazon.com/premiumsupport/knowledge-center/cloudfront-serve-static-website/).
* [Azure Storage with public website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website).

### SendGrid Account

A [SendGrid account](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021#create-a-twilio-sendgrid-account) is needed by the Cosmos Editor for sending Emails related to user account management services.

### Web Applications

You will need to install two web applications. The first is the Publisher and the second is the Editor.

* [AWS Elastic Beanstalk](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/dotnet-core-tutorial.html)

There are two options for installing the websites.  You can download executable packages for Publisher and Editor from the links shown below:

* Publisher Zip File
* Editor Zip File

Here are links to documentation on how to deploy the zip files to each cloud:

* [AWS Elastic Beanstalk](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/dotnet-core-tutorial.html#dotnet-core-tutorial-deploy)

Cosmos is also distributed as Linux containers.  They can be obtained from the links below:

* Publisher Container
* Editor Container



* AWS
  * 

## Redis Option

## CDN Options

## Single Cloud-Multi Region Option

## Multi-cloud Installation Option

