import { InjectionKey } from "vue";

const PaymentInjectionKeys = {
  WITHDRAWAL_CRITERIA: Symbol("WITHDRAWAL_CRITERIA") as InjectionKey<any>,
  DEPOSIT_CRITERIA: Symbol("DEPOSIT_CRITERIA") as InjectionKey<any>,
};

export default PaymentInjectionKeys;
