<template>
  <el-button
    size="small"
    v-for="(item, index) in tenantList"
    :key="index"
    @click="switchTenant(item)"
    ><span>{{ $t("title.tenants." + item.tenantId) }}</span></el-button
  >
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { IHttpConnectionOptions } from "@microsoft/signalr/src/IHttpConnectionOptions";

const currentTenant = localStorage.getItem("tenant");
const storageTenantList = localStorage.getItem("tenantList");
const tenantList = ref(<any>[]);

const switchTenant = (tenant: any) => {
  localStorage.setItem("tenant", tenant.tenantId.toString());
  localStorage.setItem("id_token", tenant.token.toString());
  const tenantInfo = {
    access_token: tenant.token,
  };
  localStorage.setItem("jwt_token", JSON.stringify(tenantInfo));
  window.location.reload();
};
// const initializeSignalR = (tenantList: any) => {
//   const token = tenantList[0].token;
//   const url = process.env.VUE_APP_API_URL + "/hub/client";
//   const connection = new HubConnectionBuilder()
//     .withUrl(url, {
//       withCredentials: false,
//       accessTokenFactory: () => token,
//     } as IHttpConnectionOptions)
//     .withAutomaticReconnect()
//     .configureLogging(LogLevel.Warning)
//     .build();
//   connection.start().then(() => {
//     console.log("tenantId", tenantList[0].tenantId, "Switch SignalR Connected");
//   });
// };
onMounted(() => {
  if (storageTenantList) {
    const _tenantList = JSON.parse(storageTenantList);
    tenantList.value = _tenantList.filter(
      (item) => item.tenantId != currentTenant
    );
  }
  // initializeSignalR(tenantList.value);
});
</script>
