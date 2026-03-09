import { InjectionKey } from "vue";
import { PaymentInfoTenantModal } from "@/core/models/PaymentInfos";

export default {
  ACCOUNT_DETAILS: Symbol("ACCOUNT_DETAILS") as InjectionKey<any>,
  USER_EMAIL: Symbol("USER_EMAIL") as InjectionKey<any>,
  GET_ACCOUNT_DETAILS: Symbol("GET_ACCOUNT_DETAILS") as InjectionKey<
    () => Promise<any>
  >,
  APPLICATION_DETAILS: Symbol("APPLICATION_DETAILS"),
  GET_USER_INFOS: Symbol("USER_INFOS"),
  PLATFORM_SERVICES: Symbol("PLATFORM_SERVICES"),
  GET_READ_ONLY_CODE: Symbol("GET_READ_ONLY_CODE"),
  GET_ACCOUNT_WIZARD_STATUS: Symbol("GET_ACCOUNT_WIZZARD_STATUS"),
  GET_SYSTEM_PAYMENT_SERVICES: Symbol("GET_SYSTEM_PAYMENT_SERVICES"),
  GET_ACCOUNT_PAYMENT_SERVICES_SELECTIONS: Symbol(
    "GET_ACCOUNT_PAYMENT_SERVICES"
  ),
  GET_ACCOUNT_AVAILABLE_PAYMENT_SERVICES: Symbol(
    "GET_ACCOUNT_AVAILABLE_PAYMENT_SERVICES"
  ),
  INIT_PAYMENT_SERVICES: Symbol("INIT_PAYMENT_SERVICES") as InjectionKey<
    () => void
  >,
  GET_USER_PAYMENT_INFOS: Symbol("GET_USER_PAYMENT_INFOS") as InjectionKey<
    () => Promise<Array<PaymentInfoTenantModal>>
  >,
  HANDLE_REFRESH_ACCOUNT_CLIENT_TABLE: Symbol(
    "HANDLE_REFRESH_ACCOUNT_CLIENT_TABLE"
  ) as InjectionKey<() => void>,
};
