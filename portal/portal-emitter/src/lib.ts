import { createTypeSpecLibrary, paramMessage } from "@typespec/compiler";

export const $lib = createTypeSpecLibrary({
  name: "@azure-tools/typespec-portal-emitter",
  diagnostics: {
    "no-arm-services": {
      severity: "warning",
      messages: {
        default: paramMessage`Unable to find any ARM resources in the TypeSpec. Please ensure that it is an ARM spec.`,
      },
    },
  },
} as const);
export const { reportDiagnostic, getTracer, createStateSymbol } = $lib;
