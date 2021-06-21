// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using IPVersion = Microsoft.Azure.Management.Compute.Models.IPVersion;
using NetworkProfile = Microsoft.Azure.Management.Compute.Models.NetworkProfile;
using Sku = Microsoft.Azure.Management.Compute.Models.Sku;
using SubResource = Microsoft.Azure.Management.Compute.Models.SubResource;

namespace VMCreateTiming
{
    static class ResourceHelper
    {
        public static async Task CreateResourceGroupAsync(
            ServiceClientCredentials credentials,
            string subscriptionId,
            string resourceGroup,
            string location)
        {
            var resourceClient = new ResourceManagementClient(credentials);
            resourceClient.SubscriptionId = subscriptionId;

            await resourceClient.ResourceGroups.CreateOrUpdateAsync(resourceGroup, new ResourceGroup(location));
        }

        public static async Task<NetworkInterface> CreateNicAsync(
            ServiceClientCredentials credentials,
            string subscriptionId,
            string resourceGroup,
            string location,
            string name)
        {
            var networkClient = new NetworkManagementClient(credentials);
            networkClient.SubscriptionId = subscriptionId;

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
                .BeginCreateOrUpdateAsync(resourceGroup, name + "_vent", vnet);

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
                        PrivateIPAllocationMethod = IPAllocationMethod.Dynamic
                    }
                }
            };

            return await networkClient.NetworkInterfaces.CreateOrUpdateAsync(resourceGroup, name + "_nic", nic);
        }

        public static async Task<AvailabilitySet> CreateAvailabilitySetAsync(
            ServiceClientCredentials credentials,
            string subscriptionId,
            string resourceGroup,
            string location,
            string asetName)
        {
            var computeClient = new ComputeManagementClient(credentials);
            computeClient.SubscriptionId = subscriptionId;

            // Create Availability Set
            var availabilitySet = new AvailabilitySet(location)
            {
                PlatformUpdateDomainCount = 5,
                PlatformFaultDomainCount = 2,
                Sku = new Sku("Aligned"),
            };

            return await computeClient.AvailabilitySets
                .CreateOrUpdateAsync(resourceGroup, asetName, availabilitySet);
        }

        public static async Task<AzureOperationResponse<VirtualMachine>> CreateVmAsync(
            ServiceClientCredentials credentials,
            string subscriptionId,
            string resourceGroup,
            string location,
            string vmName,
            string availabilitySetId,
            string nicId,
            string sshKeyData)
        {
            var computeClient = new ComputeManagementClient(credentials);
            computeClient.SubscriptionId = subscriptionId;

            var vm = new VirtualMachine()
            {
                Location = location,
                NetworkProfile = new NetworkProfile { NetworkInterfaces = new[] { new NetworkInterfaceReference() { Id = nicId } } },
                AvailabilitySet = new SubResource { Id = availabilitySetId },
                OsProfile = new OSProfile
                {
                    ComputerName = "testVM",
                    AdminUsername = "azureUser",
                    LinuxConfiguration = new LinuxConfiguration
                    {
                        DisablePasswordAuthentication = true,
                        ProvisionVMAgent = true,
                        Ssh = new SshConfiguration(new List<SshPublicKey> { new SshPublicKey(path: $"/home/azureUser/.ssh/authorized_keys", keyData: sshKeyData)})
                    }
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

            return await computeClient.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync(resourceGroup, vmName, vm);
        }
    }
}
