<template>
  <div>
    <div class="step-title">
      {{ $t("title.selectWithdrawChannel") }}
    </div>

    <div
      v-if="isLoading"
      class="d-flex align-items-center justify-content-center h-400px"
    >
      <LoadingRing />
    </div>

    <div
      v-else-if="!isLoading && services.length > 0"
      class="row mb-4 max-h-400px overflow-y-auto"
      style="max-height: 400px; overflow-y: auto"
    >
      <div
        class="col-lg-6 col-6 mt-3"
        v-for="(item, index) in services"
        :key="index"
        @click="
          (paymentRequireData.selectedServiceName = item.name),
            (paymentRequireData.selectedServiceHashId = item.hashId)
        "
      >
        <PaymentGroupCard
          :item="item"
          :logo="item.logo"
          :groupName="item.name"
          :showName="item.name"
          :selectedGroup="paymentRequireData.selectedServiceName"
        />
      </div>
    </div>

    <div v-else class="w-100 h-100 mt-5 amount-tip">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("tip.unavailablePleaseContact") }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import PaymentGroupCard from "../PaymentGroupCard.vue";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const services = ref({} as any);
const isLoading = inject<any>("isLoading");
const paymentRequireData = inject<any>("paymentRequireData");

onMounted(async () => {
  isLoading.value = true;
  try {
    if (paymentRequireData.value.isAccount) {
      services.value = await clientGlobalService.getAccountWithdrawGroups(
        paymentRequireData.value.detail.uid
      );
    } else {
      services.value = await clientGlobalService.getWalletWithdrawGroups(
        paymentRequireData.value.detail.hashId
      );
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
});
</script>

<style lang="scss" scoped></style>
