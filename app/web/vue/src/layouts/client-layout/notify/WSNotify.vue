<template>
  <div
    id="ws_notify"
    class="toast-container position-fixed t-top-60 end-0 p-3 z-index-3"
  ></div>
</template>

<script lang="ts">
import { defineComponent, onMounted, inject } from "vue";
import { useToast } from "vue-toastification";
// import { useStore } from "@/store";
import { WSSignalR } from "@/core/plugins/signalr";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

export default defineComponent({
  name: "bp-websockets",
  components: {},
  setup() {
    const messages = [];
    const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

    // const store = useStore();
    // const user = store.state.AuthModule.user;
    onMounted(() => {
      if (
        wsSignalR?.connection === null ||
        wsSignalR?.connection === undefined
      ) {
        return;
      }
      // wsSignalR.connection.start().then(() => {
      //   if (wsSignalR.connection == null) {
      //     console.log("Connection failed");
      //     return;
      //   }
      //   wsSignalR.connection.on("ReceiveMessage", (msg) => {
      //     const toast = useToast();
      //     toast.success(msg);
      //     console.log(msg);
      //   });
      // });

      // const channels = [
      //   // "private-User." + user.uid,
      //   // "private-User.Rebate." + user.uid,
      // ];
      // channels.forEach((channel) => {
      //   console.log(user);
      //   if (wsPusher === null || wsPusher.pusher === null) {
      //     return;
      //   }
      //   const channelPusher = wsPusher.pusher.subscribe(channel);
      //   channelPusher.bind("pusher:subscription_succeeded", () => {
      //     const toast = useToast();
      //     toast.success(channel, {
      //       timeout: 5000,
      //     });
      //   });
      //   channelPusher.bind(channel, (event) => {
      //     const toast = useToast();
      //     toast.success(event.msg, {
      //       timeout: false,
      //     });
      //     console.log("event", event);
      //   });
      // });
    });
    const hide = (msg) => {
      console.log(msg);
    };
    return {
      hide,
      messages,
    };
  },
});
</script>
<style scoped>
.t-top-60 {
  top: 60px;
}
</style>
