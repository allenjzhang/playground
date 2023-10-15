import "@typespec/rest";
import "./common.tsp";

namespace Microsoft.Widgets;
using TypeSpec.Rest;

model Widget {
  @key
  @segment("widgets")
  name: string;

  color: string;
}

interface WidgetOperations extends Resource.ResourceOperations<Widget, Error> {}
