import "@typespec/rest";
import "./widgets.tsp";
import "./flanges.tsp";

@service({
    title: "Microsoft.TenantWidget",
    version: "2022-05-10-preview",
})
namespace Microsoft.Widgets;
