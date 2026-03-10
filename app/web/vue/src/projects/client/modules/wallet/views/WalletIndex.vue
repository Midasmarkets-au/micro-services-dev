<template>
  <div v-if="isLoading" style="margin-top: 200px">
    <table
      class="table align-middle table-row-bordered gy-5"
      id="kt_ecommerce_sales_table"
    >
      <LoadingRing />
    </table>
  </div>
  <div v-else>
    <div v-if="!isMobile" class="row g-2">
      <div class="col-12 col-xl-3">
        <!-- xl以上：垂直排列 -->
        <div class="d-none d-xl-block">
          <div
            v-for="(item, index) in wallets"
            :key="index"
            class="mb-2 h-[182px]"
            :class="{
              'p-0': item.hashId === selectedWallet.hashId,
              'p-2': item.hashId !== selectedWallet.hashId,
            }"
          >
            <WalletCard
              @click="getCurrentWallet(index)"
              @on-withdraw="showWithdrawModal"
              @on-transfer="showTransferModal"
              :item="item"
              :isSelected="item.hashId === selectedWallet.hashId"
              class="w-100 h-100"
            />
          </div>
        </div>
        <!-- xl以下：水平排列，每行3个 -->
        <div class="d-xl-none">
          <div class="row g-2 justify-center">
            <div
              v-for="(item, index) in wallets"
              :key="index"
              class="col-4 mb-2"
            >
              <div
                class="h-[182px]"
                :class="{
                  'p-0': item.hashId === selectedWallet.hashId,
                  'p-2': item.hashId !== selectedWallet.hashId,
                }"
              >
                <WalletCard
                  @click="getCurrentWallet(index)"
                  @on-withdraw="showWithdrawModal"
                  @on-transfer="showTransferModal"
                  :item="item"
                  :isSelected="item.hashId === selectedWallet.hashId"
                  class="w-100 h-100"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-12 col-xl-9">
        <div class="mb-5">
          <WalletTransactionTable
            v-if="!isLoading"
            :selectedWallet="selectedWallet"
            :showActionButtons="false"
          />
        </div>
      </div>
    </div>

    <div v-if="isMobile" style="margin: 0 -15px">
      <div
        style="
          width: 100vw;
          display: flex;
          align-items: center;
          justify-content: space-between;
          box-sizing: border-box;
          padding: 0 10px;
        "
      >
        <span
          class="cursor-pointer fs-1 fw-bold"
          @click="getCurrentWallet(mobileSelectedWalletIdx - 1)"
          :class="{
            'text-gray': mobileSelectedWalletIdx === 0,
            'text-secondary': mobileSelectedWalletIdx > 0,
          }"
        >
          {{ "<" }}
        </span>

        <div style="display: flex; gap: 15px; overflow: hidden; width: 328px">
          <div
            v-for="(item, idx) in wallets"
            :key="idx"
            style="min-width: 328px; transition: transform 0.3s ease-out"
            :style="`transform: translateX(calc(${
              -mobileSelectedWalletIdx * 100
            }% - ${15 * mobileSelectedWalletIdx}px));`"
          >
            <MobileWalletCard
              :wallet="item"
              @show-action-modal="mobileTableRef?.showModal"
            />
          </div>
        </div>
        <span
          class="cursor-pointer fs-1 fw-bold"
          @click="getCurrentWallet(mobileSelectedWalletIdx + 1)"
          :class="{
            'text-gray': mobileSelectedWalletIdx === wallets.length - 1,
            'text-secondary': mobileSelectedWalletIdx < wallets.length - 1,
          }"
        >
          {{ ">" }}
        </span>
      </div>

      <div>
        <WalletMobileTransactionTable
          ref="mobileTableRef"
          :selected-wallet="selectedWallet"
          v-if="!isLoading"
        />
      </div>
    </div>
  </div>

  <!-- Modal组件 -->
  <CreateWithdrawModal
    ref="createWithdrawRef"
    @on-created="refreshWalletData"
  />
  <CreateTransferModal
    ref="createTransferRef"
    @on-created="refreshWalletData"
  />
</template>

<script lang="ts" setup>
import "../assets/css/style.css";
import { ref, onMounted, nextTick } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { WalletCriteria } from "../models/Wallet";
import WalletService from "../services/WalletService";
import { isMobile } from "@/core/config/WindowConfig";
import WalletCard from "../components/wallet-card.vue";
import WalletTransactionTable from "../components/WalletTransactionTable.vue";
import MobileWalletCard from "@/projects/client/modules/wallet/components/MobileWalletCard.vue";
import WalletMobileTransactionTable from "@/projects/client/modules/wallet/components/WalletMobileTransactionTable.vue";
import CreateWithdrawModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";
import CreateTransferModal from "../components/modal/CreateTransferModal.vue";

const isLoading = ref(true);
const wallets = ref<any>([]);
const selectedWallet = ref<any>();
const mobileSelectedWalletIdx = ref(0);
const walletCriteria = ref<WalletCriteria>({} as WalletCriteria);
const mobileTableRef = ref<InstanceType<typeof WalletMobileTransactionTable>>();
const createWithdrawRef = ref<InstanceType<typeof CreateWithdrawModal>>();
const createTransferRef = ref<InstanceType<typeof CreateTransferModal>>();

const getCurrentWallet = async (idx: number) => {
  isLoading.value = false;

  if (idx < 0 || idx >= wallets.value.length) {
    return;
  }
  mobileSelectedWalletIdx.value = idx;

  const wallet = wallets.value[idx];
  if (selectedWallet.value && wallet.hashId === selectedWallet.value.hashId) {
    return;
  }
  selectedWallet.value = wallet;

  await nextTick();
  isLoading.value = false;
};

const showWithdrawModal = () => {
  createWithdrawRef.value?.show(false, selectedWallet.value);
};

const showTransferModal = () => {
  createTransferRef.value?.show(selectedWallet.value.id);
};

const refreshWalletData = async () => {
  try {
    const res = await WalletService.getWalletsV2();
    wallets.value = res.data.filter((item) => {
      return item.isPrimary === true; // ✅ 添加过滤逻辑
    });
    // 保持当前选中的钱包
    const currentSelectedIndex = wallets.value.findIndex(
      (w) => w.hashId === selectedWallet.value?.hashId
    );
    if (currentSelectedIndex >= 0) {
      await getCurrentWallet(currentSelectedIndex);
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  isLoading.value = true;

  try {
    const res = await WalletService.getWalletsV2();

    wallets.value = res.data.filter((item) => {
      return item.isPrimary === true;
    }); //res.data;
    walletCriteria.value = res.criteria;

    if (wallets.value.length > 0) {
      await getCurrentWallet(0);
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
});
</script>
