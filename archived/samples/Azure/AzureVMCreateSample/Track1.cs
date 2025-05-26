// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Models;
using IPVersion = Microsoft.Azure.Management.Network.Models.IPVersion;
using ResourceManagementClient = Microsoft.Azure.Management.ResourceManager.ResourceManagementClient;
using Sku = Microsoft.Azure.Management.Compute.Models.Sku;
using SubResource = Microsoft.Azure.Management.Compute.Models.SubResource;

namespace AzureVMCreateSample
{
    static class Track1
    {
        public static async Task CreateVmAsync(
            string subscriptionId,
            string resourceGroup,
            string location,
            string vmName)
        {
            var credentials = SdkContext.AzureCredentialsFactory
                .FromFile("azureauth.properties");

            var resourceClient = new ResourceManagementClient(credentials);
            var networkClient = new NetworkManagementClient(credentials);
            var computeClient = new ComputeManagementClient(credentials);

            resourceClient.SubscriptionId = subscriptionId;
            networkClient.SubscriptionId = subscriptionId;
            computeClient.SubscriptionId = subscriptionId;

            // Create Resource Group
            await resourceClient.ResourceGroups.CreateOrUpdateAsync(resourceGroup, new ResourceGroup(location));

            // Create Availability Set
            var availabilitySet = new AvailabilitySet(location)
            {
                PlatformUpdateDomainCount = 5,
                PlatformFaultDomainCount = 2,
                Sku = new Sku("Aligned"),
            };

            availabilitySet = await computeClient.AvailabilitySets
                .CreateOrUpdateAsync(resourceGroup, vmName + "_aSet", availabilitySet);

            // Create IP Address
            var ipAddress = new PublicIPAddress()
            {
                PublicIPAddressVersion = IPVersion.IPv4,
                PublicIPAllocationMethod = IPAllocationMethod.Dynamic,
                Location = location,
            };

            ipAddress = await networkClient
                .PublicIPAddresses.BeginCreateOrUpdateAsync(resourceGroup, vmName + "_ip", ipAddress);

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
                .BeginCreateOrUpdateAsync(resourceGroup, vmName + "_vent", vnet);

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
                .BeginCreateOrUpdateAsync(resourceGroup, vmName + "_nic", nic);

            var vm = new VirtualMachine(location)
            {
                NetworkProfile = new NetworkProfile { NetworkInterfaces = new[] { new NetworkInterfaceReference() { Id = nic.Id } } },
                AvailabilitySet = new SubResource { Id = availabilitySet.Id },
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
                HardwareProfile = new HardwareProfile() { VmSize = VirtualMachineSizeTypes.StandardB1ms },
            };

            await computeClient.VirtualMachines.BeginCreateOrUpdateAsync(resourceGroup, vmName, vm);
        }
    }
}
