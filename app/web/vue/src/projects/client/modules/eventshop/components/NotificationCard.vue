<template>
  <div
    :class="{ 'd-none': close, '': !close }"
    class="px-sm-15 px-3 z-3"
    v-if="Object.keys(notification).length > 0 && !isMobile"
  >
    <div
      class="notification-bg w-100 px-sm-10 px-3 py-3 d-flex align-items-center justify-content-between"
    >
      <div
        class="d-flex flex-row fs-xl-7 fs-lg-8 fs-9 align-items-center gap-1 lh-0"
      >
        <inline-svg
          class="me-2"
          src="/images/icons/general/ring_black.svg"
        ></inline-svg>
        <span class="fw-semibold" v-if="!isMobile"
          >{{ $t("fields.orderNumber") }}: {{ notification.hashId }},</span
        >
        <span class="fw-semibold" v-if="!isMobile"
          >{{ $t("fields.pointSpended") }}:
          <ShopPoints :points="notification.totalPoint" />,</span
        >
        <span class="fw-semibold"
          ><span v-if="!isMobile">{{ $t("fields.itemName") }}:</span>
          {{ notification.eventShopItemName }} ,</span
        >
        <span class="fw-semibold" v-if="!isMobile"
          >{{ $t("fields.quantity") }}: {{ notification.quantity }},</span
        >
        <span class="fw-semibold" v-if="!isMobile"
          >{{ $t("fields.date") }}: </span
        ><TimeShow
          :date-iso-string="notification.updatedOn"
          type="customCSSv2"
        />
      </div>
      <div
        class="d-flex flex-row fs-sm-7 fs-9 align-items-center gap-xl-10 gap-lg-5 gap-2"
      >
        <span class="fw-semibold">{{
          EventShopOrderStatusOptions[notification.status].label
        }}</span>
        <button
          class="py-0 btn btn-sm btn-link text-blackd fs-sm-7 fs-9 lh-0"
          @click="showDetail(notification)"
        >
          {{ $t("action.view") }}
        </button>
        <button
          class="py-0 btn btn-sm btn-link text-blackd fs-7"
          @click="handleClose"
        >
          x
        </button>
      </div>
    </div>
  </div>
  <OrderDetail ref="orderDetailRef" />
</template>

<script lang="ts" setup>
import { isMobile } from "@/core/config/WindowConfig";
import { ref, onMounted } from "vue";
import ShopService from "../services/ShopService";
import { EventShopOrderStatusOptions } from "@/core/types/shop/ShopPointsTypes";
import OrderDetail from "./modal/OrderDetail.vue";
const isLoading = ref(false);
const notification = ref<any>({});
const orderDetailRef = ref<InstanceType<typeof OrderDetail>>();
const criteria = ref<any>({
  page: 1,
  size: 10,
  sortField: "createdOn",
});
const close = ref<boolean>(false);
const handleClose = () => {
  close.value = true;
};

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await ShopService.getOrderList(criteria.value);
    // console.log("res", res);
    // res.data = [
    //   {
    //     totalPoint: 1,
    //     hashId: 1,
    //     status: 2,
    //   },
    // ];
    if (res.data.length > 0) notification.value = res.data[0];
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};
const showDetail = (item: any) => {
  orderDetailRef.value.show(item);
};
onMounted(() => {
  fecthData();
});
</script>

<style scoped>
.notification-bg {
  background: #fafbfd;
  border-radius: 10px;
}
</style>
