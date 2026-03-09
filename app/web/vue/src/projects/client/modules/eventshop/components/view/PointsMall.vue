<template>
  <div class="mt-5" v-if="!isMobile">
    <div v-if="activeType == EventShopItemTypes.Product">
      <div
        class="card mb-2 d-flex flex-row gap-2 align-items-center overflow-auto round-bl-br"
      >
        <div class="card-header">
          <ul
            class="d-flex flex-row align-items-center gap-20 m-0 p-0 point-ul"
          >
            <button
              style="word-break: keep-all"
              class="cursor-pointer lh-0 pl-5 pr-5"
              :class="{
                'active-nav': activeCategory === null,
              }"
              @click="changeCategory(null)"
              :disabled="isLoading"
            >
              {{ $t("fields.all") }}
            </button>
            <button
              v-for="(option, optionIndex) in categoryData"
              style="word-break: keep-all"
              :key="optionIndex"
              :value="optionIndex"
              class="cursor-pointer d-flex align-items-center lh-0 pl-5 pr-5"
              :class="{
                'active-nav': activeCategory === optionIndex,
              }"
              @click="changeCategory(optionIndex)"
              :disabled="isLoading"
            >
              {{ option }}
            </button>
          </ul>
        </div>
      </div>
      <div
        v-if="isLoading"
        class="d-flex justify-content-center card card-body align-items-center round-tl-tr"
      >
        <LoadingRing />
      </div>
      <div
        v-else-if="!isLoading && data.length === 0"
        class="d-flex justify-content-center card card-body align-items-center round-tl-tr"
      >
        <NoDataBox />
      </div>
      <div
        class="row mt-2 ms-0 mr-0 card card-body round-tl-tr d-flex flex-row g-6"
        v-else
      >
        <div v-for="(item, index) in data" :key="index" class="col-4 mb-5">
          <ItemCard :item="item" />
        </div>
        <div class="my-5 d-flex justify-content-center align-items-center">
          <el-pagination
            :hide-on-single-page="true"
            layout="prev, pager, next"
            :total="productCriteria.total"
            :page-size="productCriteria.size"
            :current-page="productCriteria.page"
            @update:current-page="fetchItem"
          />
        </div>
      </div>
    </div>
    <div v-else>
      <div
        v-if="isLoading"
        class="d-flex justify-content-center card card-body round-tl-tr"
      >
        <LoadingRing />
      </div>
      <div
        v-else-if="!isLoading && rewardData.length === 0"
        class="d-flex justify-content-center card card-body round-tl-tr"
      >
        <NoDataBox />
      </div>
      <div class="row mt-5 ms-1 card card-body round-tl-tr" v-else>
        <div
          v-for="(item, index) in rewardData"
          :key="index"
          class="col-4 mb-5"
        >
          <PointsCard
            :item="item"
            :active-reward="activeReward"
            @fetch-data="fetchData"
          />
        </div>
      </div>
    </div>
  </div>
  <div v-if="isMobile">
    <div v-if="activeType == EventShopItemTypes.Product">
      <div
        class="d-flex flex-row gap-2 mb-2 align-items-center overflow-scroll pt-2 pb-3 card round-bl-br"
      >
        <div class="card-header">
          <ul class="d-flex flex-row align-items-center gap-8 m-0 p-0 point-ul">
            <button
              style="word-break: keep-all; text-wrap: nowrap"
              class="btn btn-sm btn-link py-0"
              :class="{
                'active-nav': activeCategory === null,
              }"
              @click="changeCategory(null)"
              :disabled="isLoading"
            >
              {{ $t("fields.all") }}
            </button>
            <button
              v-for="(option, optionIndex) in categoryData"
              style="word-break: keep-all"
              :key="optionIndex"
              :value="optionIndex"
              class="cursor-pointer d-flex align-items-center lh-0"
              :class="{
                'active-nav': activeCategory === optionIndex,
              }"
              @click="changeCategory(optionIndex)"
              :disabled="isLoading"
            >
              {{ option }}
            </button>
          </ul>
        </div>
      </div>
      <!-- <el-divider class="my-3" /> -->
      <div
        v-if="isLoading"
        class="d-flex mt-2 justify-content-center align-items-center card card-body round-tl-tr"
      >
        <LoadingRing />
      </div>
      <div
        v-else-if="!isLoading && data.length === 0"
        class="d-flex justify-content-center align-items-center card card-body round-tl-tr"
      >
        <NoDataBox />
      </div>
      <div
        class="row mt-5 align-items-center card card-body round-tl-tr"
        v-else
      >
        <div v-for="(item, index) in data" :key="index" class="col-12 mb-5">
          <ItemCard :item="item" />
        </div>
      </div>
      <div class="my-5 d-flex justify-content-center align-items-center">
        <el-pagination
          :hide-on-single-page="true"
          layout="prev, pager, next"
          :total="productCriteria.total"
          :page-size="productCriteria.size"
          :current-page="productCriteria.page"
          @update:current-page="fetchItem"
        />
      </div>
    </div>
    <div v-else>
      <div
        v-if="isLoading"
        class="d-flex justify-content-center align-items-center card card-body round-tl-tr"
      >
        <LoadingRing />
      </div>
      <div
        v-else-if="!isLoading && rewardData.length === 0"
        class="d-flex justify-content-center align-items-center card card-body round-tl-tr"
      >
        <NoDataBox />
      </div>
      <div class="grid row gap-3 mt-5 ms-1 card card-body round-tl-tr" v-else>
        <PointsCard
          class="g-col-3"
          v-for="(item, index) in rewardData"
          :key="index"
          :item="item"
          :active-reward="activeReward"
          @fetch-data="fetchData"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import ItemCard from "../ItemCard.vue";
import PointsCard from "../PointsCard.vue";
import { isMobile } from "@/core/config/WindowConfig";
import ShopService from "../../services/ShopService";
import {
  EventShopItemTypes,
  EventShopItemOptions,
  EventShopItemCategoryOptions,
  EventItemTypes,
} from "@/core/types/shop/ShopItemTypes";
import { EventShopRewardStatusTypes } from "@/core/types/shop/ShopPointsTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import NoDataBox from "@/components/NoDataBox.vue";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventPartyStatusTypes } from "@/core/types/shop/ShopCustomerTypes";
import Decimal from "decimal.js";
import { useStore } from "@/store";
const store = useStore();
const tenancy = store.state.AuthModule.user.tenancy;

const userDetail = inject<any>(
  ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL
);
const status = ref(userDetail?.value?.status ? userDetail.value.status : 0);
const data = ref(<any>[]);
const rewardData = ref(<any>[]);
const activeReward = ref(<any>[]);
const activeType = ref(EventShopItemTypes.Product);
const activeCategory = ref(null);
const categoryData = ref(<any>[]);
const isLoading = ref(true);
const productCriteria = ref<any>({
  type: EventShopItemTypes.Product,
  page: 1,
  size: 16,
  sortField: "createdOn",
  sortFlag: false,
  eventKey: "EventShop",
});

const rewardCriteria = ref<any>({
  types: [
    EventItemTypes.AgentReward,
    EventItemTypes.ClientReward,
    EventItemTypes.SalesReward,
  ],
  page: 1,
  size: 16,
  sortField: "createdOn",
  sortFlag: false,
  eventKey: "EventShop",
});

const changeTab = async (type: number) => {
  activeType.value = type;
  if (type === EventShopItemTypes.Reward) {
    await fecthReward();
  } else {
    await fetchData();
  }
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    await fetchItem(1);
  } catch (e) {
    MsgPrompt.error(e);
    console.log(e);
  }
  isLoading.value = false;
};

const fetchItem = async (page: number) => {
  isLoading.value = true;
  productCriteria.value.page = page;
  try {
    const res = await ShopService.getItemList(productCriteria.value);
    productCriteria.value = res.criteria;
    data.value = res.data;
  } catch (e) {
    MsgPrompt.error(e);
    console.log(e);
  }
  isLoading.value = false;
};

const fetchCategoryData = async () => {
  isLoading.value = true;
  try {
    const res = await ShopService.queryShopCategoryList();
    categoryData.value = res;
  } catch (e) {
    MsgPrompt.error(e);
    console.log(e);
  }
  isLoading.value = false;
};

const fecthReward = async () => {
  isLoading.value = true;
  try {
    const rewardRes = await ShopService.getItemList(rewardCriteria.value);
    const res = await ShopService.queryRewardList();
    rewardData.value = rewardRes.data;
    rewardData.value.forEach((item: any) => {
      item.point = new Decimal(item.point).div(10000).toNumber();
      res.data.forEach((reward: any) => {
        if (item.hashId === reward.shopItemHashId) {
          item.purchaseInfo = reward;
          item.statusInfo = "purchased";
        }
      });
      activeReward.value = rewardData.value.find(
        (item: any) =>
          item.purchaseInfo?.status === EventShopRewardStatusTypes.Active
      );
    });
  } catch (e) {
    MsgPrompt.error(e);
    console.log(e);
  }
  isLoading.value = false;
};

const changeCategory = async (category) => {
  activeCategory.value = category;
  productCriteria.value.category = category;
  await fetchItem(1);
};

onMounted(() => {
  fetchData();
  fetchCategoryData();
});
</script>

<style lang="scss" scoped>
.active-nav {
  color: #000f32;
  position: relative;
  &::after {
    content: "";
    position: absolute;
    bottom: -36px; // 贴到底部
    left: 50%;
    transform: translateX(-50%);
    width: 22px;
    height: 3px;
    background-color: #000f32;
    border-radius: 2px; // 可选，圆角
  }
}
.point-ul {
  list-style: none;
  text-wrap: nowrap;
  color: #000;
  font-size: 20px;
}
</style>
