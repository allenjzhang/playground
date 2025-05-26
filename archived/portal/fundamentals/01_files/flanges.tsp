import "@typespec/rest";
import "./common.tsp";

namespace Microsoft.Widgets;
using TypeSpec.Rest;

model Flange {
  @key
  @segment("flanges")
  name: string;
  flavor: string;
}

interface FlangeOperations extends Resource.ResourceOperations<Flange, Error> {}