<template v-if="!isLoading">
  <div
    class="card ratio overflow-hidden banner-card-desktop"
    style="--bs-aspect-ratio: 31.615%; border: 0 !important; scale: 1.002"
    v-if="!isMobile"
  >
    <div
      class="card-body d-flex align-items-start justify-content-center flex-column gap-3 px-16 banner-bg"
      :style="bannerBgStyle"
    >
      <div
        class="text-[#212121] fs-3 leading-[28px] fw-bold d-flex flex-column gap-1"
      >
        <!-- <h3 class="fw-bold banner-title">{{ data.name }}</h3>
        <div class="d-flex flex-row gap-5 items-content-center">
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">{{ $t("fields.startDate") }}</span>
            <span class="banner-detail_value">
              <TimeShow :date-iso-string="data.startOn" type="eventShop"
            /></span>
          </span>
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">{{ $t("fields.endDate") }}</span>
            <span class="banner-detail_value">
              <TimeShow :date-iso-string="data.endOn" type="eventShop"
            /></span>
          </span>
        </div> -->
      </div>
    </div>
  </div>
  <div
    class="card overflow-hidden border mb-5 banner-card-mobile w-100"
    v-if="isMobile"
    style="border: 0 !important"
  >
    <div
      class="card-body d-flex align-items-center justify-content-center flex-column gap-3 px-4 py-4 banner-bg banner-card-body-mobile"
      :style="bannerBgStyle"
    >
      <!-- <div class="fs-3 leading-[28px] fw-bold d-flex flex-column gap-1 mb-3">
        <h3 class="fw-bold banner-title fs-5">{{ $t("title.eventShop") }}</h3>
        <div class="d-flex flex-row gap-5 items-content-center">
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">{{ $t("fields.startDate") }}</span>
            <span class="banner-detail_value">
              <TimeShow :date-iso-string="data.startOn" type="eventShop"
            /></span>
          </span>
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">{{ $t("fields.endDate") }}</span>
            <span class="banner-detail_value">
              <TimeShow :date-iso-string="data.endOn" type="eventShop"
            /></span>
          </span>
        </div>
      </div> -->
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, computed, onMounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import ShopService from "../services/ShopService";
import i18n from "@/core/plugins/i18n";

const BANNER_IMAGE_URL =
  "https://midasmarkets.s3.amazonaws.com/t_10000/26/02/ef225bd7-cf31-486c-af59-2194cb712502_48c84ac2.jpg";

const isLoading = ref(false);
const t = i18n.global.t;
const data = ref(<any>[]);

const bannerBgStyle = computed(() => ({
  backgroundImage: `url("${BANNER_IMAGE_URL}")`,
  backgroundSize: "cover",
  backgroundPosition: "center",
  backgroundRepeat: "no-repeat",
}));

const fetchData = async () => {
  isLoading.value = true;
  const response = await ShopService.queryEventByKey("EventShop");
  data.value = response;
  isLoading.value = false;
};

onMounted(async () => {
  await fetchData();
});
</script>

<style scoped>
/* 1920 × 607 等比例 */
.banner-card-desktop {
  aspect-ratio: 1920 / 607;
}
.banner-card-mobile {
  aspect-ratio: 1920 / 607;
  min-height: 8rem;
}
.banner-card-body-mobile {
  min-height: 100%;
  width: 100%;
}
.banner-bg {
  color: white;
}
.banner-title {
  font-style: normal;
  color: #0a46aa;
  font-size: 70px;
  margin-bottom: 20px;
}
.banner-detail {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  color: #0a46aa;
}
.banner-detail_value {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  color: #fff;
}
.outline-button {
  border: 1px solid #212121 !important;
  color: #212121;
  box-sizing: border-box;
}
.outline-button:hover {
  background-color: #ffce00 !important;
  border: 1px solid #ffce00 !important;
}
.credit-color {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  color: #f7b93f;
}
.role-badge {
  padding: 2px 16px;
  font-weight: 500;
  border-radius: 8px;
}
</style>
