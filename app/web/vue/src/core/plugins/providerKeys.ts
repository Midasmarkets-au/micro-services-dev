import { InjectionKey } from "vue";
import { ApiFuncType } from "./api";

export const apiProviderKey = Symbol() as InjectionKey<Array<ApiFuncType>>;
