// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Management.Compute;
using Azure.Management.Compute.Models;
using Azure.Management.Network;
using Azure.Management.Resource;
using Azure.Management.Resource.Models;
using IPVersion = Azure.Management.Network.IPVersion;
using NetworkProfile = Azure.Management.Compute.Models.NetworkProfile;
using Sku = Azure.Management.Compute.Models.Sku;
using SubResource = Azure.Management.Compute.Models.SubResource;

namespace AzureVMCreateSample
{
    static class Track2_2
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
            var availabilitySet = new AvailabilitySet(location);
            //// Following are optional, has default, probably no need for helpers as there are simple types.
            //// For other complex properties, need to evaluate common usage scenarios
            availabilitySet.PlatformFaultDomainCount = 5;
            availabilitySet.PlatformUpdateDomainCount = 2;
            availabilitySet.Sku = new Sku() { Name = "Aligned" };

            availabilitySet = await computeClient.AvailabilitySets
                .CreateOrUpdateAsync(resourceGroup, vmName + "_aSet", availabilitySet);

            // Create IP Address
            var ipAddress = new PublicIPAddress();
            // TODO Need to review. Downside of using string instead of enum is intellisense is messed up. But extensibility may trump this.
            // While . is easier to see intellisense, still does not give sense of necessary required properties.
            // Initializer helper should be added for initialization.
            ipAddress.PublicIPAddressVersion = IPVersion.IPv4;
            ipAddress.PublicIPAllocationMethod = IPAllocationMethod.Dynamic;
            ipAddress.Location = location;

            ipAddress = await networkClient
                .PublicIPAddresses.StartCreateOrUpdate(resourceGroup, vmName + "_ip", ipAddress)
                .WaitForCompletionAsync();

            // Create VNet
            var vnet = new VirtualNetwork();
            vnet.Location = location;
            vnet.Subnets.Add(new Subnet(){ Name = "name", AddressPrefix = "10.0.0.0/1" });
            vnet.AddressSpace.AddressPrefixes.Add("10.0.0.0/16");
            // TODO: vnet.Subnets.AddExistingSubnet("existing id");

            vnet = await networkClient.VirtualNetworks
                .StartCreateOrUpdate(resourceGroup, vmName + "_vent", vnet)
                .WaitForCompletionAsync();

            // Create Network interface
            var nic = new NetworkInterface();
            nic.Location = location;
            // TODO may need helper extensions for existing and new
            nic.IpConfigurations.Add(new NetworkInterfaceIPConfiguration()
            {
                Name = "Primary",
                Primary = true,
                Subnet = new Subnet() { Id = vnet.Subnets.First().Id },
                PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                PublicIPAddress = new PublicIPAddress() { Id = ipAddress.Id }
            });

            nic = await networkClient.NetworkInterfaces
                .StartCreateOrUpdate(resourceGroup, vmName + "_nic", nic)
                .WaitForCompletionAsync();

            var vm = new VirtualMachine(location);
            vm.NetworkProfile = new NetworkProfile {NetworkInterfaces = new[] {new NetworkInterfaceReference() {Id = nic.Id}}};
            vm.AvailabilitySet.Id = availabilitySet.Id;
            vm.OsProfile = new OSProfile
            {
                // TODO User name, password, SSH should have helpers
                ComputerName = "testVM",
                AdminUsername = "azureUser",
                AdminPassword = "azure12345QWE!",
                LinuxConfiguration = new LinuxConfiguration
                    {DisablePasswordAuthentication = false, ProvisionVMAgent = true}
            };
            vm.StorageProfile = new StorageProfile()
            {
                ImageReference = new ImageReference()
                {
                    Offer = "UbuntuServer",
                    Publisher = "Canonical",
                    Sku = "18.04-LTS",
                    Version = "latest"
                },
                DataDisks = new List<DataDisk>()
            };
            // vm.SetLatestWindowsImage();

            await computeClient.VirtualMachines
                .StartCreateOrUpdate(resourceGroup, vmName, vm)
                .WaitForCompletionAsync();
        }
    }
}