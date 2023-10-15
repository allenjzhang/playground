import type { DecoratorContext, Enum, Model } from "@typespec/compiler";
import { PortalCoreKeys } from "./keys.js";

/**
 * This is a sample decorator that will be used to decorate a property.
 * @param target The model that is being decorated.
 * @param name Display name of the property.
 */
export function $displayOption(context: DecoratorContext, target: Model, name: string) {
  const { program } = context;

  program.stateMap(PortalCoreKeys.displayOption).set(target, name);
}