import "@typespec/rest";

namespace Microsoft.Widgets;

using TypeSpec.Rest;

@error
model Error {
  code: string;
  message: string;
}
