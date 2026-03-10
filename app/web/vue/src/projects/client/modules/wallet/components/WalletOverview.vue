<template>
  <div class="card" v-if="wallets && wallets.length > 0">
    <div class="card-header">
      <h3 class="card-title align-items-start flex-column">
        <span class="card-label fw-bold text-gray-800"
          >{{ $t("title.wallet") }} &nbsp;
          <span class="text-gray-400 fs-7">
            | {{ selectedWallet + 1 }} of {{ wallets.length }}</span
          >
        </span>
      </h3>
      <div class="card-toolbar">
        <router-link to="/wallet" style="color: #595959">{{
          $t("action.viewMore")
        }}</router-link>
      </div>
    </div>
    <div class="position-relative">
      <button
        class="arrow left"
        @click="prevCard"
        :class="{
          'text-gray-300': selectedWallet == 0,
          'text-gray-700': selectedWallet > 0,
        }"
      >
        &lt;
      </button>
      <button
        class="arrow right"
        @click="nextCard"
        :class="{
          'text-gray-300': selectedWallet == wallets.length - 1,
          'text-gray-700': selectedWallet < wallets.length - 1,
        }"
      >
        &gt;
      </button>

      <div class="card-body">
        <div class="card-container">
          <div
            v-for="(wallet, index) in wallets"
            class="dashboard-wallet-card"
            :class="{}"
            :style="`transform: translateX(${-selectedWallet * 100}%);`"
            :key="index"
            @click="goToWallet(wallet)"
          >
            <walletCard
              class="custom-md-width custom-sm-width wallet-sm-width ms-6"
              :isSelected="false"
              :item="wallet"
            ></walletCard>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, inject, onMounted } from "vue";

import walletCard from "./wallet-card.vue";
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import { useRouter } from "vue-router";

const router = useRouter();

const wallets = ref(Array<any>());
const selectedWallet = ref(0);

onMounted(async () => {
  const res = await WalletService.getWallets();
  wallets.value = res.data;
});

const nextCard = () => {
  if (selectedWallet.value >= wallets.value.length - 1) {
    return false;
  }
  selectedWallet.value += 1;
};
const prevCard = () => {
  if (selectedWallet.value <= 0) {
    return false;
  }
  selectedWallet.value -= 1;
};

const goToWallet = (wallet: any) => {
  router.push(`/wallet/${wallet.currencyId}/${wallet.fundType}`);
};
</script>

<style scoped>
.card-container {
  display: flex;
  overflow-x: hidden;
}

.dashboard-wallet-card {
  transition: transform 0.3s ease-out;
}

.arrow {
  position: absolute;
  top: 40%;
  transform: translateY(-50%);
  font-size: 2rem;
  z-index: 1;
  background-color: transparent;
  border: none;
  cursor: pointer;
}

.arrow.left {
  left: 5px;
}

.arrow.right {
  right: 5px;
}

.wallet-sm-width {
  min-width: 240px;
}

@media (min-width: 768px) {
  .custom-md-width {
    min-width: 330px;
  }
}

@media (min-width: 576px) {
  .custom-sm-width {
    min-width: 310px;
  }
}
</style>
