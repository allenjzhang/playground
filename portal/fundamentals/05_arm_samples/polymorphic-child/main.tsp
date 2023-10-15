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

interface Operations extends Azure.ResourceManager.Operations {}

@doc("A ContosoProviderHub resource")
model VirtualMachine is TrackedResource<VirtualMachineProperties> {
  @doc("Name of virtualMachine")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("virtualMachineName")
  @path
  @segment("virtualMachines")
  name: string;
}

@doc("VirtualMachine properties")
model VirtualMachineProperties {
  @doc("Profile of virtualMachine")
  @encode("base64url")
  profile?: bytes;

  @visibility("read")
  @doc("The status of the last operation.")
  provisioningState?: ProvisioningState;
}

@armResourceOperations
interface VirtualMachines {
  get is ArmResourceRead<VirtualMachine>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<VirtualMachine>;
  update is ArmResourcePatchSync<VirtualMachine, VirtualMachineProperties>;
  delete is ArmResourceDeleteSync<VirtualMachine>;
  listByResourceGroup is ArmResourceListByParent<VirtualMachine>;
  listBySubscription is ArmListBySubscription<VirtualMachine>;
}

@doc("A ContosoProviderHub resource")
model VirtualMachineScaleSet is TrackedResource<VirtualMachineScaleSetProperties> {
  @doc("Name of virtualMachine scale set")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("virtualMachineScaleSetName")
  @path
  @segment("virtualMachineScaleSets")
  name: string;
}

@doc("VirtualMachine properties")
model VirtualMachineScaleSetProperties {
  @doc("Profile of virtualMachine")
  @encode("base64url")
  profile?: bytes;

  @visibility("read")
  @doc("The status of the last operation.")
  provisioningState?: ProvisioningState;
}

@armResourceOperations
interface VirtualMachinesScaleSets {
  get is ArmResourceRead<VirtualMachineScaleSet>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<VirtualMachineScaleSet>;
  update is ArmResourcePatchSync<VirtualMachineScaleSet, VirtualMachineScaleSetProperties>;
  delete is ArmResourceDeleteSync<VirtualMachineScaleSet>;
  listByResourceGroup is ArmResourceListByParent<VirtualMachineScaleSet>;
  listBySubscription is ArmListBySubscription<VirtualMachineScaleSet>;
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

@friendlyName("{name}RunCommand", TParent)
@parentResource(TParent)
model RunCommandBase<
  TParent extends TypeSpec.Reflection.Model,
  TProperties extends TypeSpec.Reflection.Model = RunCommandProperties
> extends ProxyResource<TProperties> {
  @doc("Name of runCommand")
  @pattern("^[a-zA-Z0-9-]{3,24}$")
  @key("runCommandName")
  @path
  @segment("runCommands")
  @visibility("read")
  name: string;
}

@doc("The RunCommandProperties.")
model RunCommandProperties {
  @doc("Run Command")
  command: string;

  @doc("Profile of runCommand")
  @encode("base64url")
  profile?: bytes;
}

// ---- 1.
@doc("A RunCommand for the VirtualMachine type.")
model VmRunCommand is RunCommandBase<VirtualMachine>;

@armResourceOperations
interface VmRunCommands {
  get is ArmResourceRead<VmRunCommand>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<VmRunCommand>;
  update is ArmResourcePatchSync<VmRunCommand, RunCommandProperties>;
  delete is ArmResourceDeleteSync<VmRunCommand>;
}

// ---- 2.
@doc("A RunCommand for the VirtualMachineScaleSet type.")
model VmssRunCommand is RunCommandBase<VirtualMachineScaleSet>;

@armResourceOperations
interface VmssRunCommands {
  get is ArmResourceRead<VmssRunCommand>;
  createOrUpdate is ArmResourceCreateOrUpdateAsync<VmssRunCommand>;
  update is ArmResourcePatchSync<VmssRunCommand, RunCommandProperties>;
  delete is ArmResourceDeleteSync<VmssRunCommand>;
}
