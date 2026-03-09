import { Actions, Mutations } from "@/store/enums/StoreEnums";
import { Module, Action, Mutation, VuexModule } from "vuex-module-decorators";

export interface RepAccount {
  uid: number;
  name?: string;
  type?: number;
  createdOn?: string;
}

export interface RepAccountInfo {
  repAccount: RepAccount;
}

@Module
export default class RepModule extends VuexModule implements RepAccountInfo {
  repAccount = JSON.parse(window.localStorage.getItem("repAccount") || "null");

  get currentRepAccount(): RepAccount | null {
    return this.repAccount;
  }

  @Action
  [Actions.PUT_REP_ACCOUNT](account: RepAccount | null) {
    this.context.commit(Mutations.SET_REP_ACCOUNT, account);
  }

  @Mutation
  [Mutations.SET_REP_ACCOUNT](account: RepAccount) {
    // console.log("SET: ", account);
    this.repAccount = account;
    if (!account) {
      window.localStorage.removeItem("repAccount");
      return;
    }
    window.localStorage.setItem("repAccount", JSON.stringify(account));
  }
}
