<template>
  <div
    v-if="isLoading"
    class="d-flex align-items-center justify-content-center h-400px"
  >
    <LoadingRing />
  </div>

  <div v-else>
    <div class="step-title">
      <!-- {{ paymentRequireData.selectedServiceName }} -->
      {{ $t("title.walletNoteOn") }}{{ $t("title.withdraw") }}
    </div>
    <div
      class="fs-6 text-gray-600 h-100"
      v-html="paymentRequireData.groupInfo.policy"
      style="overflow: scroll"
    ></div>
  </div>
</template>
<script lang="ts" setup>
import { inject, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const isLoading = inject<any>("isLoading");
const paymentRequireData = inject<any>("paymentRequireData");

onMounted(async () => {
  isLoading.value = true;
  try {
    if (paymentRequireData.value.isAccount) {
      paymentRequireData.value.groupInfo =
        await clientGlobalService.getAccountWithdrawGroupInfo(
          paymentRequireData.value.detail.uid,
          paymentRequireData.value.selectedServiceHashId
        );
    } else {
      paymentRequireData.value.groupInfo =
        await clientGlobalService.getWalletWithdrawGroupInfo(
          paymentRequireData.value.detail.hashId,
          paymentRequireData.value.selectedServiceHashId
        );
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
});
</script>
<style lang="scss" scoped></style>
