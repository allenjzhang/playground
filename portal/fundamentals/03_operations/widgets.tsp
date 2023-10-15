import "@typespec/rest";

namespace Microsoft.Widgets;
using TypeSpec.Rest;
using TypeSpec.Http;

model Widget {
  @key
  @segment("widgets")
  name: string;

  color: string;
}

interface WidgetOperations extends Resource.ResourceOperations<Widget, Error> {
  @action upgrade(...Resource.ResourceParameters<Widget>): Widget | Error;
}
