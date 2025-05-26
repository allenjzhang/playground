import "@typespec/rest";
import "@typespec/versioning";
import "./widgets.tsp";
import "./flanges.tsp";

using TypeSpec.Versioning;

@service({
    title: "Microsoft.TenantWidget",
})
@versioned(ServiceVersions)
namespace Microsoft.Widgets;

enum ServiceVersions {
    `2022-05-10-preview`,
    `2022-05-12-preview`,
}
