using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Rest.Azure.Authentication;

namespace TagsSamples
{
    class Program
    {
        private static string DefaultSubscription = "XXXX";
        private static string TenantDomain = "XXXX";
        private static string ClientId = "XXXX";
        private static string ClientSecret = "XXXX";

        static async Task Main(string[] args)
        {
            var resourceId = $"/subscriptions/{DefaultSubscription}/resourceGroups/XXX";

            var credentials = await ApplicationTokenProvider.LoginSilentAsync(
                TenantDomain,
                ClientId,
                ClientSecret);

            using (var client = new ResourceManagementClient(credentials))
            {
                client.SubscriptionId = DefaultSubscription;

                try
                {
                    // Create Predefined Tag name and value. Predefined Tags are not automatically
                    // applied to existing or newly created sub resources.
                    // Requires contributor permission at subscription level
                    //var t = await client.Tags.CreateOrUpdateAsync("PredefinedTag1");
                    //await client.Tags.CreateOrUpdateValueAsync("PredefinedTag1", "DefaultValue");

                    // Tags operations, only require Tags Contributor
                    // 1. Create/Update Tags on subscription,
                    var tags = new TagsResource(
                        new Tags(
                            new Dictionary<string, string>
                            {
                                { "environment", DateTimeOffset.Now.ToString() },
                                { "department", DateTimeOffset.Now.ToString() },
                                { "PredefinedTag1", "override" }
                            }));
                    var result = await client.Tags.CreateOrUpdateAtScopeAsync(resourceId, tags);

                    // 2. Update Tags on resource
                    var patchTags = new TagsPatchResource(
                        "Merge", // Replace, Delete
                        new Tags(
                            new Dictionary<string, string>
                            {
                                { "environment", DateTimeOffset.Now.ToString() },
                                { "department", DateTimeOffset.Now.ToString() },
                                { "newTag", DateTimeOffset.Now.ToString() },
                            }));

                    result = await client.Tags.UpdateAtScopeAsync(resourceId, patchTags);

                    // 3. Delete all Tags on the resource
                    await client.Tags.DeleteAtScopeAsync(resourceId);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
        }
    }
}
