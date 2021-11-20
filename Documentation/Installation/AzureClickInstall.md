# Azure Automated Install

Before clicking the button below please [create a SendGrid account](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021) and [create and save an API key](https://docs.sendgrid.com/for-developers/partners/microsoft-azure-2021#api-keys). You will need the key during the install.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FCosmosSoftware%2FCosmos.Cms%2Fmain%2FAutomation%2FAzure%2Fazuredeploy.json)

This button installs the following:

* [Azure SQL Database](https://azure.microsoft.com/en-us/products/azure-sql/database/) (SKU Basic)
* Storage Account ([Standard RA-GRS](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview))
* Linux [App Service Plan](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans) (SKU B1)
  * Cosmos Editor Website ([container from Docker Hub](https://hub.docker.com/repository/docker/toiyabe/cosmoseditor))
  * Cosmos Publisher Website ([container from Docker Hub](https://hub.docker.com/repository/docker/toiyabe/cosmospublisher))

# Required Post Installation Steps

* After the template finishes, click the "Outputs" tab on left. Copy the list of IP addresses and [add them your SendGrid account](https://docs.sendgrid.com/ui/account-and-settings/ip-access-management).
* Open the Editor website and create your admin account.
  * Note: The Editor and Publisher websites may take a minute or two to spin up as the Docker containers are being installed upon first use.
* After creating the admin account, remove the configuration variable "CosmosAllowSetup" from the Editor website.
* [Enable the static website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website-host#configure-static-website-hosting) on the storage account.

 

# Optional (yet recommended) Post Installation Steps

* This install chooses DNS names automatically for the editor, publisher, and the storage websites. Now you may want your own custom DNS names for each. After adding your own DNS names, you will have to update the configuration of your Cosmos install.
  * [Setup custom domain for Editor and Publisher App Services](https://docs.microsoft.com/en-us/Azure/app-service/app-service-web-tutorial-custom-domain?tabs=cname)
  * [Setup custom domain for static website](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-custom-domain-name?tabs=azure-portal#map-a-custom-domain-with-https-enabled).
* The automated install deploys a low-cost implementation of Cosmos which is suitable for very light traffic.  It is recommended that you [scale up](https://docs.microsoft.com/en-us/azure/app-service/manage-scale-up) and [scale out](https://docs.microsoft.com/en-us/azure/azure-monitor/autoscale/autoscale-get-started?toc=/azure/app-service/toc.json) the App Service Plan and [Azure SQL instance](https://docs.microsoft.com/en-us/azure/azure-sql/database/scale-resources) to meet your needs.
