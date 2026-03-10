import { inject } from "vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { WSSignalR } from "@/core/plugins/signalr";

const wrapper = async (
  func: (wsSignalR: WSSignalR) => Promise<any> | undefined
) => {
  const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);
  if (!wsSignalR || !wsSignalR.connection) return false;
  try {
    await func(wsSignalR);
  } catch {
    return false;
  }
  return true;
};

export default {
  getInstance: function () {
    return inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);
  },
  addToTradeAccountGroup: (accountUid: number) =>
    wrapper((wsSignalR) =>
      wsSignalR.connection?.invoke("AddToTradeAccountGroup", accountUid)
    ),
};
