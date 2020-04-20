// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Management.Compute;
using Azure.Management.Compute.Models;
using Azure.Management.Network;
using Azure.Management.Resource;
using Azure.Management.Resource.Models;
using AzureVMCreateSample.extensions;
using IPVersion = Azure.Management.Network.IPVersion;
using NetworkProfile = Azure.Management.Compute.Models.NetworkProfile;
using SubResource = Azure.Management.Compute.Models.SubResource;
using Sku = Azure.Management.Compute.Models.Sku; 

namespace AzureVMCreateSample
{
    static class Track2WithHelpers
    {
        public static async Task CreateVmAsync(
            string subscriptionId,
            string resourceGroup,
            string location,
            string vmName)
        {
            // Initialize Client
            var resourceClient = new ResourceClient(subscriptionId, new DefaultAzureCredential(true));
            var networkClient = new NetworkClient(subscriptionId, new DefaultAzureCredential(true));
            var computeClient = new ComputeClient(subscriptionId, new DefaultAzureCredential(true));

            // Create Resource Group
            await resourceClient.ResourceGroups.CreateOrUpdateAsync(resourceGroup, new ResourceGroup(location));

            // Create AvailabilitySet
            // TODO: Review initializer
            var availabilitySet = new AvailabilitySet(location);
            availabilitySet.PlatformFaultDomainCount = 2;
            availabilitySet.PlatformUpdateDomainCount = 5;
            availabilitySet.Sku = new Sku() { Name = "Aligned" };

            availabilitySet = await computeClient.AvailabilitySets
                .CreateOrUpdateAsync(resourceGroup, vmName + "_aSet", availabilitySet);

            // Create IP Address
            // TODO: Review other initializers
            var ipAddress = new PublicIPAddress();
            ipAddress.PublicIPAddressVersion = IPVersion.IPv4;
            ipAddress.PublicIPAllocationMethod = IPAllocationMethod.Dynamic;
            ipAddress.Location = location;

            ipAddress = await networkClient
                .PublicIPAddresses.StartCreateOrUpdate(resourceGroup, vmName + "_ip", ipAddress)
                .WaitForCompletionAsync();

            // Create VNet
            var vnet = new VirtualNetwork();
            vnet.Location = location;
            vnet.AddAddressSpace("10.0.0.0/16");
            vnet.AddNewSubnet("mySubnet", "10.0.0.0/24");
            vnet.AddNewSubnet("mySubnet1", "10.0.1.0/24");

            vnet = await networkClient.VirtualNetworks
                .StartCreateOrUpdate(resourceGroup, vmName + "_vent", vnet)
                .WaitForCompletionAsync();

            // Create Network interface
            var nic = new NetworkInterface();
            nic.Location = location;
            nic.UseExistingVirtualNetwork(vnet, "mySubnet");
            nic.UseExistingPublicIP(ipAddress);

            nic = await networkClient.NetworkInterfaces
                .StartCreateOrUpdate(resourceGroup, vmName + "_nic", nic)
                .WaitForCompletionAsync();

            var vm = new VirtualMachine(location);
            vm.AddExistingNetworkInterface(nic.Id, true);
            vm.SetAvailabilitySet(availabilitySet.Id);
            vm.ConfigureLinuxWithPassword(
                VirtualMachineSizeTypes.StandardB1Ms,
                "testVM", 
                "azureUser", 
                "azure12345QWE!",
                configuration: new LinuxConfiguration { DisablePasswordAuthentication = false, ProvisionVMAgent = true });

            await computeClient.VirtualMachines
                .StartCreateOrUpdate(resourceGroup, vmName, vm)
                .WaitForCompletionAsync();
        }
    }
}