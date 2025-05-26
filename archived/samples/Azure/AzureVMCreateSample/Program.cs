using System;
using System.Threading.Tasks;

namespace AzureVMCreateSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var subscriptionId = "db1ab6f0-4769-4b27-930e-01e2ef9c123c";
            var location = "westus2";

            //await Track1.CreateVmAsync(subscriptionId, "azhang-rg-track1", location, "testVM1");

            await Track2WithHelpers.CreateVmAsync(subscriptionId, "azhang-rg", location, "testVM2");

            //await Fluent.CreateVmAsync(subscriptionId, "azhang-rg-fluent", location, "testVMfluent");

            //ArmTemplate.CreateVM();
        }
    }
}
