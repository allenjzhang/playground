import { createTypeSpecLibrary } from "@typespec/compiler";

export const $lib = createTypeSpecLibrary({
  name: "@azure-tools/portal-core",
  diagnostics: {},
} as const);

// Optional but convenient, those are meant to be used locally in your library.
export const { reportDiagnostic, createDiagnostic, createStateSymbol } = $lib;