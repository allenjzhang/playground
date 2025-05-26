import { ArmOperationKind } from "@azure-tools/typespec-azure-resource-manager";

export interface defaultUxEntry {
  name: string;
  type: string;
  properties: Record<string, any>;
  displayOption: string;
  operations: string[];
}
