<template>
  <!--begin::Row-->
  <div class="row mb-5 mb-xl-5 mx-0" :class="!isMobile ? 'g-2' : ''">
    <div class="col-xl-3 col-md-5 mb-xl-5 gap-2">
      <TwoFaModal ref="twoFaRef" @close="eventNoticeShow" />
      <EventNotice ref="eventNoticeRef" />
      <EventShopBanner v-if="isMobile && showEventShop" :with-detail="true" />
      <user-overview />
      <VerificationTip v-if="isMobile" />
      <!-- <UserGuide /> -->
      <AppDownload v-if="tenancy === 'bvi' || tenancy === 'sea'" />
      <wallet-overview class="mb-5" v-if="$cans(['Ib'])" />
      <accounts-overview class="mb-5" v-if="isMobile" />
      <!-- <calander-overview /> -->
    </div>
    <div class="col-xl-6 col-md-5 mb-xl-5 d-flex flex-column gap-2">
      <!-- if the user is not register as eventShop, display this banner -->
      <EventShopBanner v-if="!isMobile && showEventShop" :with-detail="true" />
      <VerificationTip v-if="!isMobile" />
      <accounts-overview v-if="!isMobile" :showEventShop="showEventShop" />
    </div>
    <div class="col-xl-3 col-md-5 mb-xl-5">
      <calander-overview />
    </div>
  </div>
  <!--  <DashboardNotice />-->
  <!--end::Row-->
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import EventShopBanner from "@/projects/client/modules/eventshop/components/EventShopBanner.vue";
import UserOverview from "../components/UserOverview.vue";
import CalanderOverview from "../components/CalendarOverview.vue";
import WalletOverview from "../modules/wallet/components/WalletOverview.vue";
import AccountsOverview from "../modules/accounts/components/AccountsOverview.vue";
import VerificationTip from "@/projects/client/components/VerificationTip.vue";
import UserGuide from "../components/UserGuide.vue";
import { isMobile } from "@/core/config/WindowConfig";
import EventNotice from "../components/eventNotice/EventNotice.vue";
import { useStore } from "@/store";
import TwoFaModal from "../components/twoFactor/TwoFaModal.vue";
import AppDownload from "../components/AppDownload.vue";
const store = useStore();
const tenancy = store.state.AuthModule.user.tenancy;
const showEventShop = ref(false);
const twoFaRef = ref<any>(null);
const eventNoticeRef = ref<any>(null);

const checkEventShopPermission = () => {
  if (tenancy === "bvi" || tenancy === "sea") {
    showEventShop.value = store.state.AuthModule.user.roles.includes("Client");
  }
};

const popUpCheck = () => {
  var twoFaCheck = twoFaRef.value.noShowCheck();
  if (twoFaCheck == false) {
    eventNoticeShow();
  }
};

const eventNoticeShow = () => {
  eventNoticeRef.value.showData();
};

onMounted(() => {
  popUpCheck();
  checkEventShopPermission();
});
</script>
