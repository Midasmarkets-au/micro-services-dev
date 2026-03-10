import { createStore, Store, useStore as baseUseStore } from "vuex";
import { config } from "vuex-module-decorators";

import AuthModule, { UserAuthInfo } from "@/store/modules/AuthModule";
import BodyModule from "@/store/modules/BodyModule";
import BreadcrumbsModule from "@/store/modules/BreadcrumbsModule";
import ConfigModule from "@/store/modules/ConfigModule";
import BackendConfigModule from "@/store/modules/BackendConfigModule";
import ThemeModeModule from "@/store/modules/ThemeModeModule";
import NotifyModule from "@/store/modules/NotifyModule";
import { InjectionKey } from "vue";
import { AgentAccountInfo } from "@/projects/client/modules/ib/store/IbModule";
import { SalesAccountInfo } from "@/projects/client/modules/sales/store/SalesModule";
import { RepAccountInfo } from "@/projects/client/modules/rep/store/RepModule";

config.rawError = true;
export interface State {
  AgentModule: AgentAccountInfo;
  SalesModule: SalesAccountInfo;
  AuthModule: UserAuthInfo;
  RepModule: RepAccountInfo;
}

export const storeKey: InjectionKey<Store<State>> = Symbol();

const store = createStore<State>({
  modules: {
    AuthModule,
    BodyModule,
    BreadcrumbsModule,
    ConfigModule,
    BackendConfigModule,
    ThemeModeModule,
    NotifyModule,
  },
});

export const useStore = () => baseUseStore(storeKey);
export default store;
