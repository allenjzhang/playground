import "@typespec/rest";
import "./common.tsp";

namespace Microsoft.Widgets;
using TypeSpec.Rest;
using TypeSpec.Http;

model Flange {
  name: string;
  flavor?: string;
}

@route("/flanges/{name}")
namespace FlangeItemOperations {
  @get op get(@path name: string): Flange | Error;
  @put op create(@path name: string, @body flange: Flange): Flange | Error;
  @patch op update(
    @path name: string,
    @body body: {
      flavor: string;
    },
  ): Flange | Error;
  @delete op delete(@path name: string): NoContentResponse | Error;
}

namespace FlangeCollectionOperations {
  @route("/flanges")
  @get
  op list(): TypeSpec.Rest.Resource.CollectionWithNextLink<Flange> | Error;
}
