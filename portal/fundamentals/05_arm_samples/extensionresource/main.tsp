import "@typespec/rest";
import "@typespec/versioning";
import "@azure-tools/typespec-providerhub";
import "@azure-tools/typespec-azure-core";
import "@azure-tools/typespec-azure-resource-manager";

using TypeSpec.Http;
using TypeSpec.Rest;
using TypeSpec.Versioning;
using Azure.Core;
using Azure.ResourceManager;

@armProviderNamespace
@service({
  title: "ContosoProviderHubClient",
  version: "2021-10-01-preview",
})
@doc("Contoso Resource Provider management API.")
@useDependency(Azure.ResourceManager.Versions.v1_0_Preview_1)
namespace Microsoft.ContosoProviderHub;

@doc("A ContosoProviderHub resource")
model RoleAssignment is ExtensionResource<RoleAssignmentProperties> {
  @doc("Name of roleAssignment")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("roleAssignmentName")
  @path
  @segment("roleAssignments")
  @visibility("read")
  name: string;
}

@doc("RoleAssignment properties")
model RoleAssignmentProperties {
  @doc("Profile of roleAssignment")
  @encode("base64url")
  profile?: bytes;

  @visibility("read")
  @doc("The status of the last operation.")
  provisioningState?: ProvisioningState;
}

@doc("The provisioning state of a resource.")
@lroStatus
enum ProvisioningState {
  ...ResourceProvisioningState,

  @doc("The resource is being provisioned")
  Provisioning,

  @doc("The resource is updating")
  Updating,

  @doc("The resource is being deleted")
  Deleting,

  @doc("The resource create request has been accepted")
  Accepted,
}

interface Operations extends Azure.ResourceManager.Operations {}

@armResourceOperations
interface RoleAssignments {
  get is ArmResourceRead<RoleAssignment>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<RoleAssignment>;
  update is ArmResourcePatchSync<RoleAssignment, RoleAssignmentProperties>;
  delete is ArmResourceDeleteSync<RoleAssignment>;
}
