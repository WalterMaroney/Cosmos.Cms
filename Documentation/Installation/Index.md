# Cosmos Installation

The type of Cosmos installation depends on what your needs my be. For many situations a basic installation may suffice,
but if higher performance or higher availablility is required then Redis, CDN and other options might be considered.

Begin your installation by following our basic installation instructions. This will install a database, file storage and
two web applications--one for the "Publisher" and another for the "Editor."

## Basic Installation

The basic install requires four resources to be deloployed: a database, blob storage account, and a web application for the publisher and the editor.  Shown below are links
that show how to install these resources in Amazon Web Services (AWS) and Microsoft Azure.

### Database

Cosmos uses MS SQL Server as it's database. Below are links that show how to deploy a database to AWS and Azure:

* [Amazon RDS For SQL Server](https://aws.amazon.com/rds/sqlserver/)
* [Azure SQL Server](https://azure.microsoft.com/en-us/products/azure-sql/database/)

### File Storage

Files are stored in either AWS S3 or Azure Storage accounts. Make sure each is accessible to the "Editor" and each as the public website enabled.

* [AWS S3 with public website](https://docs.aws.amazon.com/AmazonS3/latest/userguide/HostingWebsiteOnS3Setup.html). Note you will need to [enable SSL for the S3 website](https://aws.amazon.com/premiumsupport/knowledge-center/cloudfront-serve-static-website/).
* [Azure Storage with public website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website).

### SendGrid Account

A [SendGrid account](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021#create-a-twilio-sendgrid-account) is needed by the Cosmos Editor for sending Emails related to user account management services.

### Web Applications

The following describes how to setup the editor and publisher web applications.

_Cosmos Editor Installation - Azure_

Step 1: Use the "Create a resource" to create a new Web App the dialog is shown below.

![Image of New Resource Dialog](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/CreateWebApp01.jpg)

Step 2: In the "Basics" dialog, make sure the choice for "Publish" is set to "Docker Container" and the "Operating System" is set to "Linux" as shown below.  Continue to the Docker tab.

![Image of Basics Dialog](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/CreateWebApp01b.jpg)

Step 3: For the docker settings, make sure "Options" is set to "Single Container" and that "Image source" is set to "Docker Hub," and "Image and tag" is set to "toiyabe/cosmoseditor:latest" as shown below.

![Image of Basics Dialog](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/CreateWebApp02.jpg)

Step 4: Now pick the App Service Plan tier.  Choose the one based on your expected load and performance, and budget.  Any will do even the "free" tier, but "B1" is recommenended as the minimum for good performance.  Finish the installation, and once complete open the website.

![Image of Spec Picker](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/CreateWebApp03.jpg)

Step 5: Open the website and you should see the setup screen as shown below.

![Image of Yaktocat](https://github.com/CosmosSoftware/Cosmos.Cms/blob/main/Documentation/Installation/CreateWebApp07.jpg)
