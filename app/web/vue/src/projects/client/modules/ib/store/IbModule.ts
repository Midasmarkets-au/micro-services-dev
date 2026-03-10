import { Actions, Mutations } from "@/store/enums/StoreEnums";
import { Module, Action, Mutation, VuexModule } from "vuex-module-decorators";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { FundTypes } from "@/core/types/FundTypes";

export interface AgentAccount {
  uid: number;
  currencyId: CurrencyTypes;
  fundType: FundTypes;
  role: AccountRoleTypes;
  hasLevelRule: boolean;
  name?: string;
  type?: number;
  siteId?: number;
  createdOn?: string;
  agentSelfGroupName?: string;
  salesGroupName?: string;
  alias?: string;
}

export interface AgentAccountInfo {
  agentAccount: AgentAccount | null;
  agentAccountList: AgentAccount[];
}

@Module
export default class IbModule extends VuexModule implements AgentAccountInfo {
  agentAccount = JSON.parse(
    window.localStorage.getItem("ibCurrentAccount") || "null"
  );

  agentAccountList = JSON.parse(
    window.localStorage.getItem("ibAccounts") || "[]"
  );

  /**
   * Get current account object
   * @returns number
   */
  get currentAccount(): AgentAccount | null {
    return this.agentAccount;
  }

  /**
   * Get account object
   * @returns [number]
   */
  get ibAccounts(): AgentAccount[] {
    return this.agentAccountList;
  }

  @Action
  [Actions.PUT_IB_CURRENT_ACCOUNT](account: AgentAccount | null) {
    this.context.commit(Mutations.SET_IB_CURRENT_ACCOUNT, account);
  }

  @Action
  [Actions.PUT_IB_ACCOUNTS](accounts: AgentAccount[] | null) {
    this.context.commit(Mutations.SET_IB_ACCOUNTS, accounts);
  }

  @Action
  [Actions.GET_IB_ACCOUNT_LIST]() {
    return Object.assign(
      [],
      JSON.parse(window.localStorage.getItem("ibAccounts") || "[]")
    );
  }

  @Mutation
  [Mutations.SET_IB_CURRENT_ACCOUNT](account: AgentAccount) {
    // console.log("SET: ", account);
    this.agentAccount = account;
    if (!account) {
      window.localStorage.removeItem("ibCurrentAccount");
      return;
    }
    window.localStorage.setItem("ibCurrentAccount", JSON.stringify(account));
  }

  @Mutation
  [Mutations.SET_IB_ACCOUNTS](accounts: AgentAccount[]) {
    this.agentAccountList = accounts;
    if (!accounts) {
      window.localStorage.removeItem("ibAccounts");
      return;
    }
    window.localStorage.setItem("ibAccounts", JSON.stringify(accounts));
  }

  @Mutation
  [Mutations.GET_CURRENT_IB_ACCOUNT]() {
    return (
      this.currentAccount ??
      Object.assign(
        {},
        JSON.parse(window.localStorage.getItem("ibCurrentAccount") || "{}")
      )
    );
  }

  //GET_IB_ACCOUNT_LIST
}
