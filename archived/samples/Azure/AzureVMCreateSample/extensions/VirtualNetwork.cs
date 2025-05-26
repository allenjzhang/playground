// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Azure.Management.Network;

namespace AzureVMCreateSample.extensions
{
    public static partial class VirtualNetworkExtensions
    {
        public static void AddAddressSpace(this VirtualNetwork vnet, string addressSpace)
        {
            vnet.AddressSpace = new AddressSpace { AddressPrefixes = new List<string>() { addressSpace } };
        }

        public static void AddAddressSpaces(this VirtualNetwork vnet, string[] addressSpaces)
        {
            vnet.AddressSpace = new AddressSpace { AddressPrefixes = addressSpaces.ToList() };
        }

        public static void AddNewSubnet(this VirtualNetwork vnet, string name, string addressPrefix)
        {
            // TODO: Review why Subnets is not initialized as empty collection
            if (vnet.Subnets == null)
            {
                vnet.Subnets = new List<Subnet>();
            }

            vnet.Subnets.Add(new Subnet() { Name = name, AddressPrefix = addressPrefix});
        }

        public static void AddNewSubnet(this VirtualNetwork vnet, string name, string[] addressPrefixes)
        {
            vnet.Subnets.Add(new Subnet() { Name = name, AddressPrefixes = addressPrefixes });
        }
    }
}
