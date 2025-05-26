import "@typespec/rest";
import "@typespec/versioning";
import "@azure-tools/typespec-azure-core";
import "@azure-tools/typespec-azure-resource-manager";
import "@azure-tools/portal-core";

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

// For more information about the proxy vs tracked,
// see https://armwiki.azurewebsites.net/rp_onboarding/tracked_vs_proxy_resources.html?q=proxy%20resource
@doc("A ContosoProviderHub resource")
@displayOption("bold")
model Employee is TrackedResource<EmployeeProperties> {
  @doc("Name of employee")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("employeeName")
  @path
  @segment("employees")
  @visibility("read")
  name: string;
}

@doc("Employee properties")
model EmployeeProperties {
  @doc("Age of employee")
  age?: int32;

  @doc("City of employee")
  city?: string;

  @doc("Profile of employee")
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
interface Employees {
  get is ArmResourceRead<Employee>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<Employee>;
  update is ArmResourcePatchSync<Employee, EmployeeProperties>;
  delete is ArmResourceDeleteSync<Employee>;
  listByResourceGroup is ArmResourceListByParent<Employee>;
  listBySubscription is ArmListBySubscription<Employee>;
}

@armResourceOperations
interface Dependents {
  get is ArmResourceRead<Dependent>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<Dependent>;
  update is ArmResourcePatchSync<Dependent, DependentProperties>;
  delete is ArmResourceDeleteSync<Dependent>;
}

@parentResource(Employee)
model Dependent is ProxyResource<DependentProperties> {
  @doc("Name of dependent")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("dependentName")
  @path
  @segment("dependents")
  @visibility("read")
  name: string;
}

@doc("Depedent properties")
model DependentProperties {
  @doc("Age of depedent")
  age: int32;

  @doc("Gender of depedent")
  gender: string;
}
