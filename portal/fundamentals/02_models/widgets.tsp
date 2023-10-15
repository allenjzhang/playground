import "@typespec/rest";

namespace Microsoft.Widgets;
using TypeSpec.Rest;
using TypeSpec.Http;

@error
model Error {
  code: string;
  message: string;
}

model DomainName {
  @pattern("[a-z]\\w\\.{3}")
  domain: string;
}

@minValue(1)
@maxValue(1000000)
scalar Sku extends int32;

model Thingy {
  sku: Sku;

  @knownValues(Flavor)
  flavor: string;

  creationTime: utcDateTime;
  spinInterval?: duration;
  velocity?: float32;
  isSpinning?: boolean;
  ...DomainName;
  configuration?: Record<string>;
}

model Widget extends Thingy {
  @key
  @segment("widgets")
  name: string;

  color?: Color;
  accessories?: Accessory[];
}

model Accessory {
  id: string;
  description: string;
  type: "fancy" | "simple";
}

enum Flavor {
  Bitter,
  Sour,
  Salty,
  Sweet,
}

enum Color {
  Red,
  Orange,
  Yellow,
  Green,
  Blue,
  Indigo,
  Violet,
}

interface WidgetOperations extends Resource.ResourceOperations<Widget, Error> {}
