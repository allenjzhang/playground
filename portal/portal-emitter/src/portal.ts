import { EmitContext, NoTarget, listServices } from "@typespec/compiler";
import {
  ArmResourceOperations,
  getArmProviderNamespace,
  getArmResources,
} from "@azure-tools/typespec-azure-resource-manager";
import { reportDiagnostic } from "./lib.js";
import { defaultUxEntry } from "./types.js";
import { PortalCoreKeys } from "@azure-tools/portal-core";
import * as path from "path";

const StateKeys = {
  armResourceOperations: Symbol.for(
    "@azure-tools/typespec-azure-resource-manager.armResourceOperations"
  ),
};

export async function $onEmit(context: EmitContext) {
  // Validate Service is ARM via @armProviderNamespace
  const services = listServices(context.program).filter((x) =>
    getArmProviderNamespace(context.program, x.type)
  );

  if (services.length === 0) {
    reportDiagnostic(context.program, {
      code: "no-arm-services",
      format: {},
      target: NoTarget,
    });
    return;
  }

  let defaultUx: defaultUxEntry[] = [];

  // Generate default UX from all ARM resources in the program
  for (const resourceDetails of getArmResources(context.program)) {
    // extract custom decorator value
    const displayOptionValue = context.program
      .stateMap(PortalCoreKeys.displayOption)
      .get(resourceDetails.typespecType)?.value;

    // Get resource operations names
    let operations = context.program
      .stateMap(StateKeys.armResourceOperations)
      .get(resourceDetails.typespecType) as ArmResourceOperations;
    if (!operations) {
      operations = { lifecycle: {}, lists: {}, actions: {} };
    }
    const operationNames: string[] = [];
    operationNames.push(...Object.keys(operations.lifecycle));
    operationNames.push(...Object.keys(operations.lists));
    operationNames.push(...Object.keys(operations.actions));

    // Add entry to default UX
    defaultUx.push({
      name: resourceDetails.name,
      type: resourceDetails.kind,
      properties: resourceDetails.typespecType.properties,
      displayOption: displayOptionValue,
      operations: operationNames,
    });
  }

  const outputDir = path.join(context.emitterOutputDir, "portal-dx.json");
  await context.program.host.mkdirp(path.dirname(outputDir));
  await context.program.host.writeFile(
    outputDir,
    JSON.stringify(defaultUx, null, 2)
  );
}
