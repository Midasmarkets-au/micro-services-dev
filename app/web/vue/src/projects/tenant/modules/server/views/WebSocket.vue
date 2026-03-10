<template>
  <div class="card">
    <div class="card-header"></div>
    <div class="card-body">
      <div class="d-flex gap-6">
        <el-input
          v-model="baseUrl"
          type="text"
          placeholder="Enter WebSocket URL"
          :disabled="isLoading"
        />
        <el-button
          type="success"
          @click="testWsSignalR"
          :disabled="!baseUrl || isLoading"
        >
          Connect</el-button
        >
        <el-button
          :disabled="!baseUrl || isLoading"
          type="danger"
          @click="disconnectWs"
        >
          Disconnect</el-button
        >
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { createSignalR, disconnect } from "@/core/plugins/signalr";
import { ref } from "vue";
import notification from "@/core/plugins/notification";

const isLoading = ref(false);
const mySignalR = ref<any>(null);
const baseUrl = ref(
  "https://thebcr.com/live/trade/symbol-group-hub?accountTypes=Advantage&accountTypes=Affilicate&accountTypes=Real"
);
const testWsSignalR = async () => {
  console.log("<<<<<<testWsSignalR");
  isLoading.value = true;
  const accountTypes = ["Advantage", "Affilicate", "Real"];
  // apply symbols as query string
  var queryString = new URLSearchParams();
  accountTypes.forEach((x) => queryString.append("accountTypes", x));
  // queryString.append("symbol", "EURUSD");
  try {
    const url = baseUrl.value;
    console.log("<<<<<<testWsSignalR>>>>>url", url);

    mySignalR.value = createSignalR(url);
    console.log("<<<<<<testWsSignalR>>>>>mySignalR", mySignalR.value);
    mySignalR.value.setup("xxxxx");

    await mySignalR?.value.connection?.start();
    console.log(
      "<<<<<<testWsSignalR>>>>>connection",
      mySignalR?.value.connection
    );
    notification.success();
  } catch (error) {
    console.error("Error connecting to WebSocket:", error);
    notification.danger();
  }

  isLoading.value = false;
};

const disconnectWs = async () => {
  console.log("<<<<<<disconnectWs");
  isLoading.value = true;
  if (mySignalR.value) {
    await mySignalR.value.connection.stop();
    mySignalR.value = null;
    notification.success();
  }
  isLoading.value = false;
};
</script>
