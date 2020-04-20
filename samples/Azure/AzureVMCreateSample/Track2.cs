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
    static class Track2
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
            var availabilitySet = new AvailabilitySet(location)
            {
                PlatformUpdateDomainCount = 5,
                PlatformFaultDomainCount = 2,
                Sku = new Sku() { Name = "Aligned" }  // TODO. Verify new codegen on AvailabilitySetSkuTypes.Aligned
            };

            availabilitySet = await computeClient.AvailabilitySets
                .CreateOrUpdateAsync(resourceGroup, vmName + "_aSet", availabilitySet);

            // Create IP Address
            // TODO verify why lack of (location) ctor.
            var ipAddress = new PublicIPAddress()
            {
                PublicIPAddressVersion = IPVersion.IPv4,
                PublicIPAllocationMethod = IPAllocationMethod.Dynamic,
                Location = location,
            };

            ipAddress = await networkClient
                .PublicIPAddresses.StartCreateOrUpdate(resourceGroup, vmName + "_ip", ipAddress)
                .WaitForCompletionAsync();

            // Create VNet
            var vnet = new VirtualNetwork()
            {
                Location = location,
                AddressSpace = new AddressSpace() { AddressPrefixes = new List<string>() { "10.0.0.0/16" } },
                Subnets = new List<Subnet>()
                {
                    new Subnet()
                    {
                        Name = "mySubnet",
                        AddressPrefix = "10.0.0.0/24",
                    }
                },
            };

            vnet = await networkClient.VirtualNetworks
                .StartCreateOrUpdate(resourceGroup, vmName + "_vent", vnet)
                .WaitForCompletionAsync();

            // Create Network interface
            var nic = new NetworkInterface()
            {
                Location = location,
                IpConfigurations = new List<NetworkInterfaceIPConfiguration>()
                {
                    new NetworkInterfaceIPConfiguration()
                    {
                        Name = "Primary",
                        Primary = true,
                        Subnet = new Subnet() { Id = vnet.Subnets.First().Id },
                        PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                        PublicIPAddress = new PublicIPAddress() { Id = ipAddress.Id }
                    }
                }
            };

            nic = await networkClient.NetworkInterfaces
                .StartCreateOrUpdate(resourceGroup, vmName + "_nic", nic)
                .WaitForCompletionAsync();

            var vm = new VirtualMachine(location)
            {
                NetworkProfile = new NetworkProfile { NetworkInterfaces = new [] { new NetworkInterfaceReference() { Id = nic.Id } } },
                OsProfile = new OSProfile
                {
                    ComputerName = "testVM",
                    AdminUsername = "azureUser",
                    AdminPassword = "azure12345QWE!",
                    LinuxConfiguration = new LinuxConfiguration { DisablePasswordAuthentication = false, ProvisionVMAgent = true }
                },
                StorageProfile = new StorageProfile()
                {
                    ImageReference = new ImageReference()
                    {
                        Offer = "UbuntuServer",
                        Publisher = "Canonical",
                        Sku = "18.04-LTS",
                        Version = "latest"
                    },
                    DataDisks = new List<DataDisk>()
                },
                HardwareProfile = new HardwareProfile() { VmSize = VirtualMachineSizeTypes.StandardB1Ms },
            };
            vm.AvailabilitySet.Id = availabilitySet.Id;

            await computeClient.VirtualMachines
                .StartCreateOrUpdate(resourceGroup, vmName, vm)
                .WaitForCompletionAsync();
        }
    }
}