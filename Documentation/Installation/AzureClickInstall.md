# Azure Automated Install

Before clicking the button below please [create a SendGrid account](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021) and [create an API key](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021#api-keys) and save it. You will need it later.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FCosmosSoftware%2FCosmos.Cms%2Fmain%2FAutomation%2FAzure%2Fazuredeploy.json)

This button installs the following:

* Azure SQL Server and Database (SKU Basic 5)
* Storage Account (Premium LRS)
* Linux App Plan (SKU B1)
  * Cosmos Editor Website (container from Docker Hub)
  * Cosmos Publisher Website (container from Docker Hub)

# Required Post Installation Steps

* [Enable the static website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website-host#configure-static-website-hosting) on the storage account.
* Run the Editor Configuration Tool to create your configuration.
* Install the configuration for both Editor and Publisher websites.
* Setup your website using the Editor.
  

# Recommended Post Installation Steps

* This install chooses DNS names automatically for the editor, publisher, and the storage websites. It does this so that the install will not conflict with other websites already installed in Azure.  Now you may want your own custom DNS names for each of these. After adding your own DNS names, you will have to update the configuration of your Cosmos install.
  * [Setup custom domain for Editor and Publisher App Services](https://docs.microsoft.com/en-us/Azure/app-service/app-service-web-tutorial-custom-domain?tabs=cname)
  * [Setup custom domain for static website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-custom-domain-name?tabs=azure-portal#map-a-custom-domain-with-https-enabled).
* The automated install deploys a low-cost implementation of Cosmos which is not suitable for moderate or high loads.  It is recommended that you [scale up](https://docs.microsoft.com/en-us/azure/app-service/manage-scale-up) and [scale out](https://docs.microsoft.com/en-us/azure/azure-monitor/autoscale/autoscale-get-started?toc=/azure/app-service/toc.json) the App Service Plan and [Azure SQL instance](https://docs.microsoft.com/en-us/azure/azure-sql/database/scale-resources) to meet your needs.
