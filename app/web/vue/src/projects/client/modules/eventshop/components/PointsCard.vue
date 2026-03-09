<template>
  <div
    class="points-card border d-flex flex-column justify-content-between px-8 pt-8 pb-8 cursor-default"
    :style="{ backgroundImage: `url(${image})` }"
    style="background-size: cover; background-repeat: no-repeat"
  >
    <div class="d-flex flex-column gap-2">
      <div
        class="d-inline-block text-truncate d-flex flex-column"
        style="max-width: 100%; margin-bottom: 20px"
      >
        <span class="points-card-title uppercase">{{ props.item?.name }}</span>
      </div>
      <div class="row align-items-center point-card-subtitle">
        <span class="col-4">{{ $t("fields.validDate") }}</span>
        <span class="col-8" v-if="props.item?.configuration?.validPeriodInDays">
          {{ props.item?.configuration?.validPeriodInDays }}
          {{ $t("fields.days") }}
        </span>
      </div>
      <div class="row align-items-center point-card-subtitle">
        <span class="col-4">{{ $t("fields.rewards") }}</span>
        <span class="col-8">
          <div class="dropdown">
            <!-- <button
              class="btn btn-sm btn-link dropdown-toggle p-0 m-0"
              style="color: #ffffff"
              type="button"
              data-bs-toggle="dropdown"
              aria-expanded="false"
            ></button> -->

            <el-popover
              :placement="isMobile ? 'bottom' : 'right-end'"
              :width="250"
              trigger="click"
            >
              <template #reference>
                <button
                  class="btn btn-sm btn-link dropdown-toggle p-0 m-0"
                  style="color: #ffffff"
                  type="button"
                  @click="showRewardDetail(props.item)"
                >
                  {{ $t("action.view") }}
                </button>
              </template>
              <div v-html="rewardDetail?.description"></div>
            </el-popover>
            <ul class="dropdown-menu px-3">
              <li v-for="(item, index) in props.item?.detail" :key="index">
                <span
                  class="dropdown-item d-flex flex-row justify-content-between align-items-center gap-4"
                >
                  <span class="fs-7">{{ item?.label }}</span>
                  <span class="fs-7">{{ item?.award }}</span>
                </span>
                <el-divider class="my-1" :class="{ 'd-none': index > 2 }" />
              </li>
            </ul>
          </div>
        </span>
      </div>
      <div class="row align-items-center point-card-subtitle">
        <span class="col-4">{{ $t("action.redeem") }}</span>
        <span class="col-8">
          {{ props.item?.point }}
          {{ $t("fields.points") }}</span
        >
      </div>
    </div>
    <div class="d-flex">
      <template
        v-if="
          props.item?.purchaseInfo === undefined ||
          props.item?.purchaseInfo === null
        "
      >
        <button
          :disabled="userDetail.point < props.item?.point || isLoading"
          class="btn btn-sm btn-light d-flex align-items-center gap-2 px-10"
          @click="showConfirmBox('redeem')"
        >
          {{ $t("action.redeem") }}
        </button>
      </template>

      <!-- 有奖励 -->
      <button
        class="btn btn-sm btn-success d-flex align-items-center gap-2 px-10"
        @click="showConfirmBox('activate')"
        v-else-if="
          props.item.purchaseInfo?.status == EventShopRewardStatusTypes.Approved
        "
      >
        {{ $t("fields.activate") }}
      </button>
      <button
        v-else-if="
          props.item?.purchaseInfo.status === EventShopRewardStatusTypes.Active
        "
        class="btn btn-sm d-flex align-items-center gap-2 px-10 btn-primary"
      >
        {{ $t("status.active") }}
      </button>
      <button
        v-else-if="
          props.item?.purchaseInfo.status ===
          EventShopRewardStatusTypes.Inactive
        "
        class="btn btn-sm d-flex align-items-center gap-2 px-10 btn-danger"
      >
        {{ $t("status.inactive") }}
      </button>
      <button
        v-else-if="
          props.item?.purchaseInfo.status ===
            EventShopRewardStatusTypes.Pending ||
          props.item?.purchaseInfo.status ===
            EventShopRewardStatusTypes.Processing
        "
        class="btn btn-sm d-flex align-items-center gap-2 px-10 btn-info"
      >
        {{ $t("status.pending") }}
      </button>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import ShopService from "../services/ShopService";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventShopRewardStatusTypes } from "@/core/types/shop/ShopPointsTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
const props = defineProps({
  item: Object,
  activeReward: Object,
});
const emits = defineEmits<{
  (e: "fetchData"): void;
}>();
const userDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL);
const openConfirmBoxModel = inject(
  ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const status = ref(userDetail?.value?.status ? userDetail.value.status : 0);
const isLoading = ref(false);
const rewardDetail = ref({});
const handleBuy = async () => {
  isLoading.value = true;
  try {
    await ShopService.purchaseReward({
      shopItemHashId: props.item?.hashId,
    });
    MsgPrompt.success(t("status.success")).then(() => {
      emits("fetchData");
    });
    userDetail.value.point = userDetail.value.point - props.item?.point;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const handleActivate = async () => {
  isLoading.value = true;
  try {
    await ShopService.activateReward(props.item?.purchaseInfo?.hashId);
    MsgPrompt.success(t("status.success")).then(() => {
      window.location.reload();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const showConfirmBox = async (action: string) => {
  if (action === "redeem") {
    openConfirmBoxModel?.(handleBuy);
  } else {
    openConfirmBoxModel?.(handleActivate);
  }
};
const showRewardDetail = async (item: any) => {
  const res = await ShopService.getItemDetail(item.hashId);
  rewardDetail.value = res;
};
const image = ref("");
const fetchImage = async () => {
  if (props.item.images.length > 0) {
    const imageUrl = await ShopService.getImagesWithGuid(props.item.images[0]);
    image.value = imageUrl;
  }
};
fetchImage();
</script>
<style scoped lang="scss">
.points-card {
  color: #ffffff;
  border-radius: 8px;
  width: 308px;
  height: 292px;
  &:hover {
    box-shadow: 0px 10px 20px rgba(0, 0, 0, 0.05);
  }
  &-title {
    font-weight: 900;
    font-size: 24px;
    line-height: 36px;
    &_description {
      font-weight: 400;
      font-size: 14px;
      line-height: 20px;
    }
  }
  &-subtitle {
    font-weight: 400;
    font-size: 16px;
    line-height: 24px;
  }
  &-img {
    width: 160px;
    height: 160px;
    flex: none;
  }
}
</style>
