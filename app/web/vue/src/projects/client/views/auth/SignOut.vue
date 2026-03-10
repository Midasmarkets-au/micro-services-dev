<template>
  <div></div>
</template>
<script lang="ts" setup>
import { onMounted, inject } from "vue";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import { Actions } from "@/store/enums/StoreEnums";
import { WSSignalR } from "@/core/plugins/signalr";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const router = useRouter();
const store = useStore();
const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

onMounted(() => {
  wsSignalR?.disconnect();
  store.dispatch(Actions.LOGOUT).then(() => router.push({ name: "sign-in" }));
});
</script>
