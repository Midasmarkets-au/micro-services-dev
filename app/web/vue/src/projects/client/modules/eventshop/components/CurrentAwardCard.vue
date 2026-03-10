<template>
  <div>
    <div class="d-flex justify-between mt-2 ml-8">
      <div
        :class="{
          'align-items-center': progress !== EventPartyStatusTypes.Approved,
          'no-border': progress === EventPartyStatusTypes.Approved,
        }"
      >
        <!--账户状态-->
        <label
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          v-if="!isRegisterd"
        >
          {{ $t("fields.unregistered") }}
        </label>
        <label
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          v-if="progress != EventPartyStatusTypes.Approved && isRegisterd"
        >
          {{ $t("fields.accountUnderReview") }}
        </label>
        <!--账户状态-->
        <div
          v-if="progress === EventPartyStatusTypes.Approved"
          class="award-detail w-100 mt-9"
        >
          <div class="d-flex items-center">
            <span class="award-title tracking-[5px]">{{
              userDetail.point
            }}</span>
            <span class="award-title_subtitle fs-sm-5 fs-1 ml-1 text-nowrap">
              {{ $t("fields.points") }}</span
            >
          </div>
          <div class="d-flex justify-content-end mt-2">
            <span class="fs-sm-5 fs-1 ml-1 text-nowrap text-gray">
              {{ $t("fields.unavailable") }}:</span
            >
            <span class="fs-sm-5 fs-1 ml-1 text-nowrap text-gray">{{
              userDetail.notavailable
            }}</span>
          </div>
          <div class="d-flex mt-5 items-center" style="visibility: hidden">
            <span>
              <img
                width="68px"
                src="/images/eventshop/vip1.png"
                srcset="/images/eventshop/vip1@2x.png 2x"
                alt="logo"
              />
            </span>
            <div>
              <span class="fs-2">VIP</span>
              <span class="fs-2 ml-3">3</span>
            </div>
          </div>
          <!-- <div class="d-flex justify-content-between align-items-center mb-4">
            <div class="d-flex gap-sm-15 gap-2">
              <div class="d-flex flex-column">
                <span class="award-title fs-sm-1 fs-3">{{
                  userDetail.point
                }}</span>
                <span class="award-title_subtitle fs-sm-5 fs-6">
                  {{ $t("fields.currentPoints") }}</span
                >
              </div>
            </div>
            <div>
              <router-link
                :to="'/supports'"
                class="btn btn-sm btn-light text-black"
              >
                {{ $t("fields.customerService") }}
              </router-link>
            </div>
          </div> -->

          <div v-if="!isMobile">
            <div v-if="userDetail?.activeRewards.length > 0">
              <div
                v-for="(reward, index) in userDetail?.activeRewards"
                :key="index"
              >
                <div
                  class="d-flex flex-sm-row flex-column align-items-sm-center gap-sm-5 gap-2"
                  v-if="countDays(reward) > 0"
                >
                  <div class="d-flex flex-row align-items-center gap-2">
                    <span class="fs-sm-5 fs-7">{{ $t("fields.rewards") }}</span>
                    <span class="fs-sm-5 fs-7">{{ reward.shopItemName }}</span>
                  </div>
                  <div class="d-flex flex-row align-items-center gap-2">
                    <span class="fs-sm-5 fs-7">{{
                      $t("fields.validDate")
                    }}</span>
                    <span class="fs-sm-5 fs-7">
                      <TimeShow
                        :date-iso-string="reward.effectiveFrom"
                        type="eventShop"
                    /></span>
                  </div>

                  <div class="d-flex flex-row align-items-center gap-2">
                    <span class="fs-sm-5 fs-7">{{
                      $t("fields.validUntil")
                    }}</span>
                    <span class="fs-sm-5 fs-7">
                      <TimeShow
                        :date-iso-string="reward.effectiveTo"
                        type="eventShop"
                    /></span>
                  </div>
                  <div class="d-flex flex-row align-items-center gap-2">
                    <span class="fs-sm-5 fs-7">{{
                      $t("fields.remaining")
                    }}</span>
                    <span class="fs-sm-5 fs-7">
                      {{ countDays(reward) }}
                      {{ $t("fields.days") }}</span
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-else>
            <div v-if="userDetail?.activeRewards.lenght > 0">
              <div
                v-for="(reward, index) in userDetail?.activeRewards"
                :key="index"
              >
                <div
                  class="d-flex justify-content-between align-items-sm-center gap-sm-5 gap-2"
                  v-if="countDays(reward) > 0"
                >
                  <div>
                    <div class="d-flex align-items-center gap-2">
                      <span class="fs-sm-5 fs-7">{{
                        $t("fields.rewards")
                      }}</span>
                      <span class="fs-sm-5 fs-7">{{
                        reward.shopItemName
                      }}</span>
                    </div>
                  </div>
                  <div>
                    <div class="d-flex align-items-center gap-2">
                      <span class="fs-sm-5 fs-7">
                        <TimeShow
                          :date-iso-string="reward.effectiveFrom"
                          type="eventShop"
                      /></span>
                      -
                      <span class="fs-sm-5 fs-7">
                        <TimeShow
                          :date-iso-string="reward.effectiveTo"
                          type="eventShop"
                      /></span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!--vip-badge-->
      <div class="award-badge">
        <img
          src="/images/eventshop/vip1.png"
          srcset="/images/eventshop/vip1@2x.png 2x"
          alt="logo"
        />
      </div>
      <!--vip-badge-->
    </div>
    <!--vip-->
    <div
      class="d-flex justify-between pt-6 pl-8 pb-7 pr-6"
      style="visibility: hidden"
    >
      <div class="vip-level w-full pr-8 pt-2">
        <el-progress
          :stroke-width="6"
          :percentage="30"
          color="#CCD8FE"
          :show-text="false"
        />
        <div class="mt-2 justify-between flex">
          <span>距下一个还需1000分</span>
          <span>VIP4</span>
        </div>
      </div>
      <div class="vip-benefits p-1">
        <button class="btn btn-light btn-sm whitespace-nowrap">查看权益</button>
      </div>
    </div>
    <!--vip-->
  </div>
</template>
<script lang="ts" setup>
import { ref, inject, onMounted } from "vue";
import { useStore } from "@/store";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { EventPartyStatusTypes } from "@/core/types/shop/ShopCustomerTypes";
import { isMobile } from "@/core/config/WindowConfig";
import ShopService from "../services/ShopService";
const store = useStore();
const roles = store.state.AuthModule.user.roles;
const isRegisterd = ref(roles.includes("EventShop"));
const userDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL);

const countDays = (item: any) => {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const effectiveToDate = new Date(item.effectiveTo);
  effectiveToDate.setHours(0, 0, 0, 0);
  return Math.floor((effectiveToDate - today) / (1000 * 60 * 60 * 24));
};
const progress = ref(0);
if (userDetail !== undefined && userDetail.value.status !== undefined) {
  progress.value = userDetail.value.status;
}
var imgUrl = "/images/bg/shopBanner.png";

const getRewardBanner = async () => {
  if (userDetail?.value.activeRewards.length > 0) {
    var imageArr =
      userDetail?.value?.activeRewards[
        userDetail?.value.activeRewards.length - 1
      ]?.configuration.bannerImages;

    if (imageArr.length > 0) {
      const img = await ShopService.getImagesWithGuid(imageArr[0]);
      imgUrl = img;
    }
  }
  document.documentElement.style.setProperty(
    "--reward-banner-image",
    `url("${imgUrl}")`
  );
};

onMounted(() => {
  getRewardBanner();
});
</script>

<style scoped lang="scss">
.award-title {
  font-weight: 600;
  line-height: 52px;
  font-size: 52px;
  &_subtitle {
    font-weight: 400;
    line-height: 20px;
  }
  @media (max-width: 768px) {
    font-size: 22px;
  }
}
.bg-image {
  background-image: var(--reward-banner-image);
  background-size: cover;
  background-repeat: no-repeat;
}
.award-detail {
  font-weight: 400;
  //line-height: 24px;
}
.no-border {
  border: none !important;
}
</style>
