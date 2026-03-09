import { Actions, Mutations } from "@/store/enums/StoreEnums";
import { Module, Action, Mutation, VuexModule } from "vuex-module-decorators";

@Module
export default class TenantModule extends VuexModule {
  iStat = {};

  @Action
  [Actions.PUT_TENANT_ISTAT](stat: any) {
    this.context.commit(Mutations.SET_TENANT_ISTAT, stat);
  }

  @Mutation
  [Mutations.SET_TENANT_ISTAT](stat: any) {
    this.iStat[stat.key] = stat.value;
  }
}
