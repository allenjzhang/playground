using System;
using System.Threading.Tasks;

namespace AzureVMCreateSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var subscriptionId = "***REMOVED***";
            var location = "westus2";

            //await Track1.CreateVmAsync(subscriptionId, "azhang-rg-track1", location, "testVM1");

            await Track2.CreateVmAsync(subscriptionId, "azhang-rg", location, "testVM2");

            //await Fluent.CreateVmAsync(subscriptionId, "azhang-rg-fluent", location, "testVMfluent");

            //ArmTemplate.CreateVM();
        }
    }
}
