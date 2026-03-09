import { Actions, Mutations } from "@/store/enums/StoreEnums";
import { Module, Action, Mutation, VuexModule } from "vuex-module-decorators";

export interface SalesAccount {
  uid: number;
  name?: string;
  type?: number;
  salesSelfGroupName?: string;
  createdOn?: string;
  siteId?: number;
}

export interface SalesAccountInfo {
  salesAccount: SalesAccount;
}

@Module
export default class SalesModule
  extends VuexModule
  implements SalesAccountInfo
{
  salesAccount = JSON.parse(
    window.localStorage.getItem("salesAccount") || "null"
  );

  get currentSalesAccount(): SalesAccount | null {
    return this.salesAccount;
  }

  @Action
  [Actions.PUT_SALES_ACCOUNT](account: SalesAccount | null) {
    this.context.commit(Mutations.SET_SALES_ACCOUNT, account);
  }

  @Mutation
  [Mutations.SET_SALES_ACCOUNT](account: SalesAccount) {
    // console.log("SET: ", account);
    this.salesAccount = account;
    if (!account) {
      window.localStorage.removeItem("salesAccount");
      return;
    }
    window.localStorage.setItem("salesAccount", JSON.stringify(account));
  }
}
