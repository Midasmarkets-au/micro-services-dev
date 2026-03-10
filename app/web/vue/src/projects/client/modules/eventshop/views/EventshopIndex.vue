<template>
  <!-- not submit register, do not have border, otherwise display the border -->
  <div
    class="rounded pb-10"
    :class="{ 'event-shop_container': step === EventPartyStatusTypes.Applied }"
    style="box-sizing: border-box"
    v-if="!isLoading"
  >
    <div class="d-flex gap-sm-5 gap-3 flex-column">
      <!-- <CurrentAwardCard v-if="step != 0" /> -->
      <!-- if the user is not registered into event display -->
      <RegistrationCard v-if="step === 0" />
      <!-- display the last notification card -->
      <NotificationCard v-if="step != 0" />
      <!-- has been submit the registration form -->
      <StoreCard v-if="step != 0" :step="step" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onBeforeMount, provide } from "vue";
import RegistrationCard from "@/projects/client/modules/eventshop/components/RegistrationCard.vue";
import StoreCard from "@/projects/client/modules/eventshop/components/StoreCard.vue";
import NotificationCard from "@/projects/client/modules/eventshop/components/NotificationCard.vue";
import { useStore } from "@/store";
import ShopService from "../services/ShopService";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventPartyStatusTypes } from "@/core/types/shop/ShopCustomerTypes";
import Decimal from "decimal.js";
const store = useStore();
const isLoading = ref(true);
const roles = store.state.AuthModule.user.roles;
const eventDetail = ref(<any>[]);
const userDetail = ref(<any>[]);
provide(ClientGlobalInjectionKeys.EVENT_SHOP_DETAIL, eventDetail);
provide(ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL, userDetail);

const step = ref(0);
if (roles.includes("EventShop")) {
  step.value = EventPartyStatusTypes.Applied;
}

async function fetchData() {
  isLoading.value = true;
  try {
    const res = await ShopService.queryEventByKey("EventShop");
    eventDetail.value = res;

    if (roles.includes("EventShop")) {
      const user = await ShopService.queryEventUserDetail();
      if (user !== undefined && user.status !== undefined) {
        userDetail.value = user;
        // let originPoint = new Decimal(userDetail.value.point)
        //   .div(10000)
        //   .toFixed(4);
        const raw = new Decimal(userDetail.value.point).div(10000);
        // 2位小数
        const val2 = new Decimal(raw.toFixed(2));
        // 4位小数
        const val4 = new Decimal(raw.toFixed(4));
        // 差值 = 4位 - 2位
        const diff = val4.minus(val2);
        userDetail.value.point = val2;
        userDetail.value.notavailable = diff;
        step.value = user.status;
      }
    }
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
}
onBeforeMount(() => {
  fetchData();
});
</script>

<style scoped type="scss">
.event-shop_container {
  /* border-bottom: 1px solid #e5e5e5;
  border-left: 1px solid #e5e5e5;
  border-right: 1px solid #e5e5e5; */
}
</style>
