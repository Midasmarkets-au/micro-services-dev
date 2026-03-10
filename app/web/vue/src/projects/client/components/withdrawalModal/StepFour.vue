<template>
  <div>
    <div class="amount-tip mb-11">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("tip.amountAvailable") + " " }}</span>
      <BalanceShow
        :currency-id="selectedWallet.currencyId"
        :balance="availableAmount ?? 0"
      />
    </div>
    <div class="step-title">
      {{ $t("title.reviewWithdrawDetail") }}
    </div>
    <div class="review-wrapper d-flex">
      <div class="outline outline-color col-4">
        <div class="title-item">{{ $t("fields.action") }}</div>
        <div class="title-item">
          {{ $t("fields.amount") }} ({{ $t("type.currency." + currencyId) }})
        </div>
        <div class="title-item" v-if="isUSDT">
          {{ $t("title.usdtWalletAddress") }}
        </div>
        <div v-else>
          <div class="title-item">
            {{ $t("title.accountName") }}
          </div>
          <div class="title-item">
            {{ $t("title.accountNumber") }}
          </div>
        </div>
      </div>

      <div class="outline col-8">
        <div class="content-item">
          {{ $t("tip.withdrawFromWallet") }} -
          {{ $t("type.currency." + currencyId) }}
        </div>
        <div class="content-item">
          {{ paymentRequireData.amount }}
        </div>
        <div class="content-item" v-if="isUSDT">
          {{ paymentMethods[paymentRequireData.request].info.walletAddress }}
        </div>

        <div v-else>
          <div class="content-item">
            {{ paymentMethods[paymentRequireData.request].info.name }}
            -
            {{ paymentMethods[paymentRequireData.request].info.bankName
            }}<span
              v-if="paymentMethods[paymentRequireData.request].info.branchName"
            >
              (
              {{ paymentMethods[paymentRequireData.request].info.branchName }}
              )</span
            >
          </div>
          <div class="content-item">
            {{ paymentMethods[paymentRequireData.request].info.accountNo }}
          </div>
        </div>
      </div>
    </div>
    <div class="d-flex mt-9" style="color: #e02b1d">
      <div class="me-3">*</div>
      <div>
        <strong>{{ $t("tip.pleaseNote") }}: </strong>
        {{ $t("tip.walletWidthdrawalNote") }}
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { inject } from "vue";

const selectedWallet = inject<any>("selectedWallet");
const currencyId = inject<any>("currencyId");
const paymentRequireData = inject<any>("paymentRequireData");
const paymentMethods = inject<any>("paymentMethods");
const isUSDT = inject<any>("isUSDT");
const availableAmount = inject<any>("availableAmount");
</script>
<style lang="scss" scoped>
.content {
  width: 100%;
  padding: 20px 35px;
  height: 500px;
  overflow-y: auto;
}

.review-wrapper .outline {
  border: 1px solid #e4e6ef;
}
.review-wrapper .outline-color {
  border: 1px solid #e4e6ef;
  background-color: #f5f7fa;
}
.review-wrapper .title-item {
  color: #0053ad;
  font-size: 14px;
  font-family: Lato;
  min-height: 75px;
  padding: 16px 24px;
}

.review-wrapper .content-item {
  color: black;
  font-size: 14px;
  font-family: Lato;
  min-height: 75px;
  padding: 16px 24px;
}
</style>
