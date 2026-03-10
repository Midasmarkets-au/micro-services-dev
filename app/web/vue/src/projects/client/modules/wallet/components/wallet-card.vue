<template>
  <div
    class="wallet-card"
    id="walletCard"
    :class="isSelected ? 'wallet-card-active' : ''"
  >
    <div
      class="position-relative d-flex flex-column justify-content-between pt-0 px-6 py-2 w-100 h-100"
    >
      <div>
        <div class="d-flex align-items-center justify-content-between">
          <div class="d-flex align-items-center">
            <div class="symbol symbol-circle symbol-25px">
              <inline-svg
                :src="'/images/currency/' + props.item.currencyId + '.svg'"
                class="me-2 w-25px"
                style=""
              />
            </div>
            <span href="#" class="fw-bold text-black fs-3">{{
              $t("type.currency." + props.item.currencyId)
            }}</span>
            <span class="ms-2 text-black" v-if="props.item.fundType == 1">{{
              $t("type.fundType." + props.item.fundType)
            }}</span>
          </div>

          <!-- 交易按钮区域 -->
          <div class="d-flex gap-2" v-if="isSelected">
            <button
              class="btn btn-primary px-3 py-2"
              style="font-size: 14px"
              @click.stop="$emit('onWithdraw')"
            >
              {{ $t("action.withdraw") }}
            </button>
            <button
              class="btn btn-sm btn-danger px-3 py-2"
              style="font-size: 14px"
              @click.stop="$emit('onTransfer')"
            >
              {{ $t("action.transferOut") }}
            </button>
          </div>
        </div>
        <!-- <span class="wallet-icon">
          <img
            :src="getTenantWalletLogo['src']"
            :style="getTenantWalletLogo['style']"
          />
        </span> -->
      </div>
      <div class="d-block">
        <span class="text-black fw-light d-block" style="color: #070b0f">
          {{ $t("fields.balance") }}
        </span>
        <span class="text-dark d-block fs-1">
          <BalanceShow
            :currency-id="props.item.currencyId"
            :balance="props.item.balance"
          />
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps } from "vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { getTenantWalletLogo } from "@/core/types/TenantTypes";

const props = defineProps<{
  item: any;
  isSelected: boolean;
}>();

const emit = defineEmits<{
  onWithdraw: [];
  onTransfer: [];
}>();
</script>

<style scoped lang="scss">
.wallet-icon {
  position: absolute;
  top: 5px;
  right: 5px;
  img {
    width: 50px;
    height: 50px;
    opacity: 0.6;
  }
}
</style>

<style scoped type="scss">
/* .wallet-bg {
  background-image: url("/images/bg/wallet_bg.png");
  background-size: cover;
  background-repeat: no-repeat;
} */
</style>
