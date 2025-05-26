// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Management.Network;

namespace AzureVMCreateSample.extensions
{
    public static partial class NetworkInterfaceExtensions
    {
        public static void UseExistingVirtualNetwork(this NetworkInterface nic, VirtualNetwork vnet, string subnetName)
        {
            EnsureIpConfigrationsCollectionInitialized(nic);

            if (nic.IpConfigurations.Count == 0)
            {
                nic.IpConfigurations.Add(new NetworkInterfaceIPConfiguration()
                {
                    Name = "Primary",
                    Primary = true,
                });
            }

            var matchingSubnet = vnet.Subnets.FirstOrDefault(s => s.Name.Equals(subnetName));
            if (matchingSubnet == null)
                throw new ArgumentException("Subnet with name not found", subnetName);

            nic.IpConfigurations.First().Subnet = new Subnet() {Id = matchingSubnet.Id};
        }

        public static void UseExistingPublicIP(this NetworkInterface nic, PublicIPAddress publicIp)
        {
            EnsureIpConfigrationsCollectionInitialized(nic);

            if (nic.IpConfigurations.Count == 0)
            {
                nic.IpConfigurations.Add(new NetworkInterfaceIPConfiguration()
                {
                    Name = "Primary",
                    Primary = true,
                });
            }

            nic.IpConfigurations.First().PublicIPAddress = new PublicIPAddress() {Id = publicIp.Id};
        }

        private static void EnsureIpConfigrationsCollectionInitialized(NetworkInterface nic)
        {
            if (nic.IpConfigurations == null)
            {
                nic.IpConfigurations = new List<NetworkInterfaceIPConfiguration>();
            }
        }
    }
}
