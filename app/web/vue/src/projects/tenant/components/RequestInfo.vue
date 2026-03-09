<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.requestInfo')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 380px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div class="d-flex align-items-center rounded p-5 mb-1 bg-light-primary">
        <span class="svg-icon svg-icon-warning me-5">
          <span class="svg-icon svg-icon-1"> </span>
        </span>
        Request {{ partyName }} ({{ partyId }}) info.
      </div>
      <div>info: {{ info }}</div>
    </div>
  </SimpleForm>
</template>
<script setup lang="ts">
import { defineProps, onMounted, ref, inject } from "vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

const props = defineProps({
  partyName: { type: String, required: true },
  partyId: { type: Number, required: true },
});

const info = ref("");

const isLoading = ref(false);

onMounted(async () => {
  wsSignalR?.connection?.on("ReceiveViewInfoRequest", (msg) => showInfo(msg));
});

const submit = async () => {
  await requestInfo();
};

const showInfo = (msg) => {
  info.value = msg;
  const outerObj = JSON.parse(msg);
  console.log(outerObj);
};

const requestInfo = async () => {
  isLoading.value = true;
  await wsSignalR?.connection?.invoke("RequestReport", props.partyId, "", "");
  isLoading.value = false;
};
</script>
