// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Azure.Management.Compute.Models;

namespace AzureVMCreateSample.extensions
{
    public static partial class VirtualNetworkExtensions
    {
        public static void AddExistingNetworkInterface(this VirtualMachine vm, string nicID, bool isPrimary)
        {
            if (vm.NetworkProfile == null)
                vm.NetworkProfile = new NetworkProfile();

            if (vm.NetworkProfile.NetworkInterfaces == null)
                vm.NetworkProfile.NetworkInterfaces = new List<NetworkInterfaceReference>();

            vm.NetworkProfile.NetworkInterfaces.Add(new NetworkInterfaceReference() { Id = nicID, Primary = isPrimary });
        }

        public static void SetAvailabilitySet(this VirtualMachine vm, string availabilitySetID)
        {
            if (vm.AvailabilitySet == null)
                vm.AvailabilitySet = new SubResource();

            vm.AvailabilitySet.Id = availabilitySetID;
        }

        public static void ConfigureLinuxWithPassword(
            this VirtualMachine vm,
            VirtualMachineSizeTypes vmSize,
            string machineName,
            string userName,
            string password,
            string linuxOffer = "UbuntuServer",
            string publisher = "Canonical",
            string sku = "18.04-LTS",
            string version = "latest",
            LinuxConfiguration configuration = null)
        {
            vm.OsProfile = new OSProfile
            {
                // TODO User name, password, SSH should have helpers
                ComputerName = machineName,
                AdminUsername = userName,
                AdminPassword = password,
                LinuxConfiguration = configuration,
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

            vm.HardwareProfile = new HardwareProfile() {VmSize = VirtualMachineSizeTypes.StandardB1Ms};
        }

        public static void AddDataDisk(
            this VirtualMachine vm,
            string diskImageUrl,
            int sizeInGB)
        {
        }

        public static void AttachDataDisk(
            this VirtualMachine vm,
            string vhdUrl)
        {
        }

    }
}
