<template>
  <div
    class="shop-card d-flex flex-row justify-content-between align-items-center px-4 pt-8 pb-5 cursor-default"
  >
    <div class="image-style">
      <el-image :src="image" class="object-cover shop-card-img">
        <template #error>
          <div class="image-slot">
            <el-icon><icon-picture /></el-icon>
          </div> </template
      ></el-image>
    </div>
    <div class="shop-content">
      <div class="d-flex flex-column gap-3">
        <div class="">
          <el-tooltip
            :content="item?.name"
            placement="top"
            v-if="item?.name && item.name.length > 14"
          >
            <span
              class="shop-card-title fs-sm-3 d-inline-block text-truncate"
              >{{ item?.name }}</span
            >
          </el-tooltip>
          <div v-else>
            <span
              class="shop-card-title fs-sm-3 d-inline-block text-truncate"
              >{{ item?.name }}</span
            >
          </div>
        </div>
        <div class="shop-card-price mb-5">
          <ShopPoints :points="item?.point" /> {{ $t("fields.points") }}
        </div>
      </div>
      <button
        class="btn btn-sm btn-light btn-radius d-flex align-items-center btn-bordered gap-2 px-sm-10 px-8"
        :disabled="status != EventPartyStatusTypes.Approved"
        @click.prevent="openEventRegistration"
      >
        {{ $t("action.redeem") }}
      </button>
    </div>
  </div>
  <ItemDetailsCard ref="eventRegistrationRef" />
</template>
<script setup lang="ts">
import { ref, defineProps, inject } from "vue";
import ItemDetailsCard from "@/projects/client/modules/eventshop/components/modal/ItemDetailsCard.vue";
import ShopService from "../services/ShopService";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventPartyStatusTypes } from "@/core/types/shop/ShopCustomerTypes";
import { Picture as IconPicture } from "@element-plus/icons-vue";
const props = defineProps({
  item: Object,
});
const item = ref<any>(props.item);
const userDetail = inject<any>(
  ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL
);
const status = ref<any>(
  userDetail?.value?.status ? userDetail.value.status : 0
);
const eventRegistrationRef = ref<any>(null);
const openEventRegistration = () => {
  eventRegistrationRef.value?.show(props.item);
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
.shop-card {
  color: #000f32;
  border-radius: 20px;
  border: 1px solid #fff;
  background-color: #fff;

  @media screen and (max-width: 768px) {
    height: 268px;
  }
  &:hover {
    box-shadow: 0px 10px 20px rgba(0, 0, 0, 0.05);
  }
  &-title {
    font-weight: 400;
    max-width: 150px;
    margin-bottom: 4px;
    @media screen and (max-width: 768px) {
      max-width: 100px;
    }
    @media screen and (max-width: 1512px) {
      max-width: 120px;
    }
  }
  &-price {
    font-weight: 400;
    color: #f7b93f;
  }
  &-img {
    width: 266px;
    flex: none;
    @media screen and (max-width: 768px) {
      width: 110px;
      height: 110px;
    }
    @media screen and (min-width: 769px) and (max-width: 1600px) {
      width: 200px;
      height: 150px;
    }
  }
  .shop-card-img {
    border-radius: 20px;
    border: 1px solid #f2f4f7;
  }
}
.image-style .image-slot {
  border-radius: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
  background: var(--el-fill-color-light);
  color: var(--el-text-color-secondary);
  font-size: 30px;
}
.image-style .image-slot .el-icon {
  font-size: 30px;
}
</style>
