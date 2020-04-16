// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzureVMCreateSample
{
    class Fluent
    {
        public static Task CreateVmAsync(
            string subscriptionId,
            string resourceGroupName,
            string location,
            string vmName)
        {
            var credentials = SdkContext.AzureCredentialsFactory
                .FromFile("azureauth.properties");

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            Console.WriteLine("Creating resource group...");
            azure.ResourceGroups.Define(resourceGroupName)
                .WithRegion(location)
                .Create();

            Console.WriteLine("Creating availability set...");
            var availabilitySet = azure.AvailabilitySets.Define("myAVSet")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithSku(AvailabilitySetSkuTypes.Aligned)
                .Create();

            Console.WriteLine("Creating public IP address...");
            var publicIPAddress = azure.PublicIPAddresses.Define("myPublicIP")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDynamicIP()
                .Create();

            Console.WriteLine("Creating virtual network...");
            var network = azure.Networks.Define("myVNet")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("mySubnet", "10.0.0.0/24")
                .Create();

            Console.WriteLine("Creating network interface...");
            var networkInterface = azure.NetworkInterfaces.Define("myNIC")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet("mySubnet")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                .Create();

            Console.WriteLine("Creating virtual machine...");
            azure.VirtualMachines.Define(vmName)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithExistingPrimaryNetworkInterface(networkInterface)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("azureuser")
                .WithAdminPassword("Azure12345678")
                .WithComputerName(vmName)
                .WithExistingAvailabilitySet(availabilitySet)
                .WithSize(VirtualMachineSizeTypes.BasicA0)
                .Create();

            return Task.CompletedTask;
        }
    }
}
