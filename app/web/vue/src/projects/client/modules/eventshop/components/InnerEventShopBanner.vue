<template v-if="!isLoading">
  <div
    class="banner-wrapper overflow-hidden"
    style="border: 0 !important; scale: 1.002"
    v-if="!isMobile"
  >
    <img :src="imgUrl" alt="banner" class="banner-img" />
    <div
      class="banner-overlay d-flex align-items-start justify-content-center flex-column gap-3 px-16"
    >
      <div class="fs-3 leading-[28px] fw-bold d-flex flex-column gap-1">
        <h3 class="fw-bold banner-title">{{ data.name }}</h3>
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
      </div>
    </div>
  </div>
  <div
    class="banner-wrapper overflow-hidden mb-5"
    v-if="isMobile"
    style="border: 0 !important"
  >
    <img :src="imgUrl" alt="banner" class="banner-img" />
    <div
      class="banner-overlay d-flex align-items-start justify-content-center flex-column gap-3 px-6 pb-10"
      style="padding-top: 25px !important"
    >
      <div class="fs-3 leading-[28px] fw-bold d-flex flex-column gap-1 mb-3">
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
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import ShopService from "../services/ShopService";
import i18n from "@/core/plugins/i18n";

const isLoading = ref(false);
const t = i18n.global.t;
const data = ref(<any>[]);

var imgUrl =
  "https://midasmarkets.s3.amazonaws.com/t_10000/26/03/523d599f-69de-421c-9629-295a6acdf502_20d43d49.jpg";

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
.banner-wrapper {
  position: relative;
  border-radius: 1.25rem;
}

.banner-img {
  display: block;
  width: 100%;
  height: auto;
  object-fit: contain;
}

.banner-overlay {
  position: absolute;
  inset: 0;
  color: white;
}

.banner-title {
  font-style: normal;
  color: #fff;
  font-size: 70px;
  margin-bottom: 20px;
}
.banner-detail {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  color: #fff;
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
