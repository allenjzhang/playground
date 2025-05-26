import "@typespec/rest";
import "@typespec/versioning";

namespace Microsoft.Widgets;
using TypeSpec.Rest;
using TypeSpec.Http;
using TypeSpec.Versioning;

model Widget {
  @key
  @segment("widgets")
  name: string;

  @madeOptional(ServiceVersions.`2022-05-12-preview`)
  color?: string;

  @added(ServiceVersions.`2022-05-12-preview`)
  description?: string;

  @removed(ServiceVersions.`2022-05-12-preview`)
  weightInPounds?: int32;

  @added(ServiceVersions.`2022-05-12-preview`)
  weightInKilograms?: int32;
}

interface WidgetOperations extends Resource.ResourceOperations<Widget, Error> {
  @added(ServiceVersions.`2022-05-12-preview`)
  @route("/widgets/{name}/upgrade")
  @post
  @action
  upgrade(
    ...Resource.ResourceParameters<Widget>,
    newVersion: int32,
  ): Widget | Error;
}
