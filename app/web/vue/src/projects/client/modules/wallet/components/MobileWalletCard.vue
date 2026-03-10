<template>
  <div class="mobile-wallet-card card">
    <div class="currency-label">
      <span class="svg-icon svg-icon-2x">
        <inline-svg
          :src="'/images/currency/' + wallet.currencyId + '.svg'"
          style=""
        />
      </span>

      <a href="#" class="fw-bold text-black" style="font-size: 20px">{{
        $t("type.currency." + wallet.currencyId)
      }}</a>
      <span class="ms-2 text-black fw-bold fs-5">{{
        $t("type.fundType." + wallet.fundType)
      }}</span>
    </div>

    <div class="balance-info">
      <span class="balance-label">
        {{ $t("fields.balance") }}
      </span>

      <div class="balance-number">
        <BalanceShow
          :currency-id="wallet.currencyId"
          :balance="wallet.balance"
        />
      </div>
    </div>

    <div class="wallet-actions">
      <div class="action-box" v-if="false">
        <div class="img-circle" @click="emits('showActionModal', 'deposit')">
          <img src="/images/icons/finance/wallet-deposit.svg" alt="" />
        </div>
        <label>{{ $t("action.deposit") }}</label>
      </div>

      <div class="action-box">
        <div class="img-circle" @click="emits('showActionModal', 'withdraw')">
          <img src="/images/icons/finance/wallet-withdraw.svg" alt="" />
        </div>
        <label>{{ $t("action.withdraw") }}</label>
      </div>

      <div class="action-box">
        <div class="img-circle" @click="emits('showActionModal', 'transfer')">
          <img src="/images/icons/finance/AccountTransfer.png" alt="" />
        </div>
        <label>{{ $t("action.transfer") }}</label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import BalanceShow from "@/components/BalanceShow.vue";
import { getTenancy, tenancies } from "@/core/types/TenantTypes";
const props = defineProps<{
  wallet: any;
}>();

const emits = defineEmits<{
  (event: "showActionModal", actionName: string): void;
}>();
</script>

<style scoped lang="scss">
.mobile-wallet-card {
  position: relative;
  width: 100%;
  height: 100%;
  border-radius: 10px;
  padding: 24px 24px 20px 24px;
  //background: #ffd400;
  background-image: url("/images/bg/wallet_bg.png");
  background-size: cover;
  background-repeat: no-repeat;
  box-shadow: 0px 4px 4px 0px rgba(242, 201, 0, 0.25);
  display: flex;
  flex-direction: column;
  justify-content: space-between;

  .svg-logo {
    position: absolute;
    top: 12px;
    right: 12px;
  }

  .currency-label {
    display: flex;
    align-items: center;
    position: absolute;
    top: 23px;
    right: 20px;
    z-index: 1;
  }

  .balance-info {
    .balance-label {
      color: #070b0f;
      font-size: 12px;
      font-style: normal;
      font-weight: 300;
    }

    .balance-number {
      color: #070b0f;
      font-size: 28px;
      font-style: normal;
      font-weight: 600;
    }
  }

  .wallet-actions {
    padding: 10px 0;
    z-index: 2;
    position: relative;
    border-radius: 16px;
    overflow: hidden;
    display: flex;

    img {
      width: 24px;
      height: 24px;
    }

    &::after {
      content: "";
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(255, 255, 255, 0.2);
      backdrop-filter: blur(14.5px);
      z-index: -1;
    }

    .action-box {
      display: flex;
      flex: 1;
      flex-direction: column;
      align-items: center;
      gap: 5px;
      .img-circle {
        width: 40px;
        height: 40px;
        display: flex;
        justify-content: center;
        align-items: center;
        background: rgba(255, 255, 255, 0.3);
        border-radius: 50%;
        cursor: pointer;

        &:hover {
          background: rgba(255, 255, 255);
          transition: 0.3s;
        }
      }

      label {
        color: #002957;
        /* link/medium */
        font-size: 14px;
        font-style: normal;
        font-weight: 400;
      }
    }
  }
}
</style>
