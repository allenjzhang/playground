// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureVMCreateSample
{
    static class ArmTemplate
    {
        public static async Task CreateVM()
        {
            try
            {
                var credentials = SdkContext.AzureCredentialsFactory
                    .FromFile("azureauth.properties");

                var azure = Microsoft.Azure.Management.Fluent.Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

                var groupName = "azhang-rg1";
                var location = Region.USWest;

                var resourceGroup = azure.ResourceGroups.Define(groupName)
                    .WithRegion(location)
                    .Create();

                string storageAccountName = SdkContext.RandomResourceName("st", 10);

                Console.WriteLine("Creating storage account...");
                var storage = azure.StorageAccounts.Define(storageAccountName)
                    .WithRegion(Region.USWest)
                    .WithExistingResourceGroup(resourceGroup)
                    .Create();

                var storageKeys = storage.GetKeys();
                string storageConnectionString = "DefaultEndpointsProtocol=https;"
                                                 + "AccountName=" + storage.Name
                                                 + ";AccountKey=" + storageKeys[0].Value
                                                 + ";EndpointSuffix=core.windows.net";

                var account = CloudStorageAccount.Parse(storageConnectionString);
                var serviceClient = account.CreateCloudBlobClient();

                Console.WriteLine("Creating container...");
                var container = serviceClient.GetContainerReference("templates");
                container.CreateIfNotExistsAsync().Wait();
                var containerPermissions = new BlobContainerPermissions()
                { PublicAccess = BlobContainerPublicAccessType.Container };
                container.SetPermissionsAsync(containerPermissions).Wait();

                Console.WriteLine("Uploading template file...");
                var templateblob = container.GetBlockBlobReference("CreateVMTemplate.json");
                templateblob.UploadFromFileAsync("CreateVMTemplate.json").ConfigureAwait(false).GetAwaiter().GetResult();

                Console.WriteLine("Uploading parameters file...");
                var paramblob = container.GetBlockBlobReference("Parameters.json");
                paramblob.UploadFromFileAsync("Parameters.json").ConfigureAwait(false).GetAwaiter().GetResult();

                var templatePath = "https://" + storageAccountName + ".blob.core.windows.net/templates/CreateVMTemplate.json";
                var paramPath = "https://" + storageAccountName + ".blob.core.windows.net/templates/Parameters.json";
                var deployment = azure.Deployments.Define("myDeployment")
                    .WithExistingResourceGroup(groupName)
                    .WithTemplateLink(templatePath, "1.0.0.0")
                    .WithParametersLink(paramPath, "1.0.0.0")
                    .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
                    .Create();
                Console.WriteLine("Press enter to delete the resource group...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
