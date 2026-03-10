<template>
  <div class="d-flex flex-column justify-content-center" style="height: 100%">
    <div class="d-flex align-items-center mb-7">
      <div class="me-7">
        <img src="/images/walletSuccess.png" alt="" />
      </div>
      <div>
        <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
        <div>
          {{ $t("tip.withdrawSuccessTip") }}
        </div>
      </div>
    </div>

    <div class="amount-tip fs-5">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("fields.yourRequestAmount") + " " }}</span>
      <BalanceShow
        :currency-id="paymentRequireData.detail.currencyId"
        :balance="paymentRequireData.request.amount * 100"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject } from "vue";
const paymentRequireData = inject<any>("paymentRequireData");
</script>

<style lang="scss" scoped></style>
