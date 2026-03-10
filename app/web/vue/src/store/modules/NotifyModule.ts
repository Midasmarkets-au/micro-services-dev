import { Mutations } from "@/store/enums/StoreEnums";
import { Module, Mutation, VuexModule } from "vuex-module-decorators";

export interface SocketState {
  isConnected: boolean;
  message: string;
  reconnectError: boolean;
  heartBeatInterval: number;
  heartBeatTimer: number;
}

@Module
export default class NotifyModule extends VuexModule implements SocketState {
  isConnected = false;
  message = "";
  reconnectError = false;
  heartBeatInterval = 5000;
  heartBeatTimer = 0;

  @Mutation
  [Mutations.SOCKET_ONOPEN](state, event) {
    const socket = event.currentTarget;
    state.socket.isConnected = true;
    // 连接成功时启动定时发送心跳消息，避免被服务器断开连接
    state.socket.heartBeatTimer = setInterval(() => {
      const message = "PinPon";
      state.socket.isConnected &&
        socket.sendObj({
          code: 200,
          msg: message,
        });
    }, state.socket.heartBeatInterval);
  }

  @Mutation
  [Mutations.SOCKET_ONCLOSE](state, event) {
    state.socket.isConnected = false;
    // 连接关闭时停掉心跳消息
    clearInterval(state.socket.heartBeatTimer);
    state.socket.heartBeatTimer = 0;
    console.log("连接已断开: " + new Date());
    console.log(event);
  }

  // 发生错误
  @Mutation
  [Mutations.SOCKET_ONERROR](state, event) {
    console.error(state, event);
  }

  // 收到服务端发送的消息
  @Mutation
  [Mutations.SOCKET_ONMESSAGE](state, message) {
    state.socket.message = message;
  }

  // 自动重连
  @Mutation
  [Mutations.SOCKET_RECONNECT](state, count) {
    console.info("消息系统重连中...", state, count);
  }

  // 重连错误
  @Mutation
  [Mutations.SOCKET_RECONNECT_ERROR](state) {
    state.socket.reconnectError = true;
  }
}
