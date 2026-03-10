import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { App, inject } from "vue";
import { IHttpConnectionOptions } from "@microsoft/signalr/src/IHttpConnectionOptions";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

interface WSSignalR {
  install: (app: App) => void;
  setup: (token: string | null | undefined) => void;
  connection: HubConnection | null;
  disconnect: () => void;
  url: string;
}

const createSignalR = (url: string): WSSignalR => ({
  url: url,
  connection: null,
  install(app: App) {
    app.provide(TenantGlobalInjectionKeys.WS_SIGNAL_R, this);
  },
  disconnect() {
    this.connection?.stop().then(() => {
      console.log("ws disconnect");
    });
  },
  setup(token: string | null | undefined) {
    if (!token) {
      return;
    }
    this.connection = new HubConnectionBuilder()
      .withUrl(this.url, {
        withCredentials: false,
        accessTokenFactory: () => token,
      } as IHttpConnectionOptions)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
  },
});

export { createSignalR, WSSignalR };
