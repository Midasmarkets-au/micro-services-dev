<template>
  <div class="step1-title">
    {{ $t("title.selectDepostiChannel") }}
  </div>

  <div
    v-if="isLoading"
    class="d-flex align-items-center justify-content-center h-400px"
  >
    <LoadingRing />
  </div>

  <div
    v-else-if="!isLoading && services && services.length > 0"
    class="row mb-4 max-h-400px overflow-y-auto"
  >
    <div
      class="col-lg-6 col-6 mt-9"
      v-for="(item, index) in services"
      :key="index"
      @click="item.isActive ? selectedGroup(item) : null"
    >
      <PaymentGroupCard
        :item="item"
        :groupName="item.group"
        :showName="item.paymentMethodName"
        :logo="item.logo"
        :selectedGroup="paymentRequireData.group"
      />
    </div>
  </div>

  <div v-else class="mt-5 amount-tip">
    <inline-svg src="/images/icons/general/gen066.svg" />
    <span class="ms-2 me-1">{{ $t("tip.unavailablePleaseContact") }}</span>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, inject } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import PaymentGroupCard from "../PaymentGroupCard.vue";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const paymentRequireData = inject<any>("paymentRequireData");
const isLoading = inject<any>("isLoading");
const services = ref({} as any);

function selectedGroup(_item: any) {
  paymentRequireData.value.group = _item.group;
  paymentRequireData.value.logo = _item.logo;
  paymentRequireData.value.paymentMethodName = _item.paymentMethodName;
}

onMounted(async () => {
  isLoading.value = true;
  paymentRequireData.value.request = {};

  try {
    services.value = await clientGlobalService.getDepositGroups(
      paymentRequireData.value.account.uid
    );
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
});
</script>

<style lang="scss" scoped></style>
