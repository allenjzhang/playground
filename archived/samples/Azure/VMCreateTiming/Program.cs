// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace VMCreateTiming
{
    public class Program
    {
        private const string DefaultLocation = "West US";
        private const string ClientId = "Azure_ClientId";
        private const string ClientSecret = "Azure_ClientSecret";
        private const string SubscriptionId = "Subscription_ID";


        public static async Task<int> Main(params string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Argument<string>("Name prefix to be used for the resources"),
                new Option<int>(
                    aliases: new string[] { "-c", "--count" },
                    getDefaultValue: () => 10,
                    "Number of VM to be created"),
                new Option<bool>(
                    aliases: new string[] { "-s", "--is-sequential" },
                    "Execute VM creation in parallel order. Default is True")
            };

            rootCommand.Description = "My sample app\r\n\r\nNote: Please set following ENV variables:\r\n\tAzure_ClientId, \r\n\tAzure_ClientSecret, \r\n\tSubscription_Id.";

            rootCommand.Handler = CommandHandler.Create<string, int, bool>(async (name, count, isSequential) =>
            {
                await ScenarioHandler(name, count, isSequential);
            });

            return await rootCommand.InvokeAsync(args);
        }

        static async Task ScenarioHandler(
            string namePrefix,
            int count,
            bool isSequential)
        {
            var clientId = Environment.GetEnvironmentVariable(ClientId);
            var clientSecret = Environment.GetEnvironmentVariable(ClientSecret);
            var subid = Environment.GetEnvironmentVariable(SubscriptionId);
            var rgName = namePrefix + "_rg";

            if (CheckMissingParam(namePrefix, nameof(namePrefix)) ||
                CheckMissingParam(clientId, ClientId) ||
                CheckMissingParam(clientSecret, ClientSecret) ||
                CheckMissingParam(subid, SubscriptionId))
            {
                return;
            }

            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var sshKeyFile = Path.Combine(userFolder, ".ssh/id_rsa.pub");
            if (!File.Exists(sshKeyFile))
            {
                Console.WriteLine($"SSH public key file {sshKeyFile} not found. Please follow https://docs.microsoft.com/en-us/azure/virtual-machines/linux/create-ssh-keys-detailed to create the SSH key file");

                return;
            }
            var sshKeyData = File.ReadAllText(sshKeyFile);

            Console.Write($"Logging in with Cliend Id: {clientId} ...");
            var credentials = await ApplicationTokenProvider.LoginSilentAsync("microsoft.com", clientId, clientSecret);
            Console.WriteLine("\t Done");

            Console.Write($"Creating/Update ResourceGroup {rgName} ...");
            await ResourceHelper.CreateResourceGroupAsync(credentials, subid, rgName, DefaultLocation);
            Console.WriteLine("\t Done");

            Console.WriteLine($"Start VM creation in {(isSequential ? "Sequential" : "Parallel")} mode.");

            var overallStart = DateTime.UtcNow;
            var taskList = new List<Task>();
            var result = new List<LatencyInfo>();
            for (int i = 0; i < count; i++)
            {
                var name = $"{namePrefix}_{i}";
                var latency = new LatencyInfo() { Name = name };

                if (isSequential)
                {
                    await Worker(credentials, subid, rgName, name, sshKeyData, latency);
                }
                else
                {

                    taskList.Add(Task.Run(async () => await Worker(credentials, subid, rgName, name, sshKeyData, latency)));
                }

                result.Add(latency);
            }

            await Task.WhenAll(taskList);

            // Dump result
            Console.WriteLine("-------------- Overall Summary ---------------");
            Console.WriteLine($"\t Started at {overallStart} taking {(DateTime.UtcNow-overallStart).TotalSeconds:F2} seconds.");
            Console.WriteLine("-------------- Latency Result ---------------");
            Console.WriteLine("Name\t\tLatency(sec)\tState");
            foreach (var l in result)
            {
                Console.WriteLine($"{l.Name}\t\t{l.Latency.TotalSeconds:F2}\t\t{l.ProvisioningState}");
            }
        }

        static async Task W()
        {
            Console.WriteLine($"{Guid.NewGuid()}{DateTime.UtcNow}.");
            await Task.Delay(TimeSpan.FromSeconds(20));
            Console.WriteLine($"{Guid.NewGuid()}{DateTime.UtcNow}..");
        }

        static async Task Worker(
            ServiceClientCredentials credentials,
            string subid,
            string rgName,
            string namePrefix,
            string sshKeyData,
            LatencyInfo latencyInfo)
        {
            Console.WriteLine($"Creating/Update Network {namePrefix}_nic...");
            var nic = await ResourceHelper.CreateNicAsync(credentials, subid, rgName, DefaultLocation, namePrefix + "_nic");

            Console.WriteLine($"Creating/Update AvailabilitySet {namePrefix}_aSet ...");
            var aset = await ResourceHelper.CreateAvailabilitySetAsync(credentials, subid, rgName, DefaultLocation, namePrefix + "_aSet");

            var startTime = DateTime.UtcNow;
            Console.WriteLine($"Creating/Update VM {namePrefix}_vm...\t\t{DateTime.UtcNow.ToLongTimeString()}");
            var response = await ResourceHelper.CreateVmAsync(credentials, subid, rgName, DefaultLocation, $"{namePrefix}_vm", aset.Id, nic.Id, sshKeyData);
            latencyInfo.Latency = DateTime.UtcNow - startTime;
            latencyInfo.ProvisioningState = response.Body.ProvisioningState;
            Console.WriteLine($"\t Done Creating/Update VM {namePrefix}_vm. \t\t{latencyInfo.Latency.TotalSeconds} sec");
        }

        static bool CheckMissingParam(string param, string paramName)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                Console.WriteLine($"Required parameter {paramName} was not specified. Please run with /? for details.");

                return true;
            }

            return false;
        }
    }
}
