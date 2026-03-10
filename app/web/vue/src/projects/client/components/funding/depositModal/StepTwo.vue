<template>
  <div
    v-if="isLoading"
    class="d-flex align-items-center justify-content-center h-400px"
  >
    <LoadingRing />
  </div>

  <div v-else>
    <div class="step-title">
      {{ $t("title.walletNoteOn") }}{{ $t("title.deposit") }}
    </div>
    <div
      class="fs-7 text-gray lh-lg"
      v-html="paymentRequireData.groupInfo.policy"
      style="height: 100%"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, inject } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const paymentRequireData = inject<any>("paymentRequireData");
const isLoading = inject<any>("isLoading");

onMounted(async () => {
  isLoading.value = true;
  try {
    paymentRequireData.value.groupInfo =
      await clientGlobalService.getDepositGroupInfo(
        paymentRequireData.value.account.uid,
        {
          group: paymentRequireData.value.group,
        }
      );

    if (paymentRequireData.value.group == "Credit Card") {
      paymentRequireData.value.request =
        paymentRequireData.value.groupInfo.requestValues;
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
});
</script>

<style lang="scss" scoped></style>
