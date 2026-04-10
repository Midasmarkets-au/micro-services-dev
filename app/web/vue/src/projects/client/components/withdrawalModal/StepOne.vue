<template>
  <div>
    <div class="step-title">
      {{ $t("title.selectWithdrawChannel") }}
    </div>
    <div
      v-if="services.withdrawal && services.withdrawal.length > 0"
      class="row mb-4"
      style="max-height: 400px; overflow-y: auto"
    >
      <div
        class="col-lg-4 col-6 mt-3"
        v-for="(item, index) in services.withdrawal"
        :key="index"
        @click="
          (paymentRequireData.paymentServiceId = item.id),
            (selectedService = item)
        "
      >
        <MethodCard
          :item="item"
          :selectedId="paymentRequireData.paymentServiceId"
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
import { inject } from "vue";
import MethodCard from "@/projects/client/modules/wallet/components/MethodCard.vue";

const services = inject<any>("services");
const paymentRequireData = inject<any>("paymentRequireData");
const selectedService = inject<any>("selectedService");
</script>

<style lang="scss" scoped></style>
