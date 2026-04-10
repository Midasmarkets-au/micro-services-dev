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
      <div class="fs-3 fw-bold d-flex flex-column gap-1">
        <h3 class="fw-bold banner-title leading-[4.28rem] text-5xl">
          {{ data.name }}
        </h3>
        <div
          class="d-flex flex-row gap-5 text-2xl leading-[4.27rem] items-content-center"
        >
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
      <div class="d-flex flex-row items-content-center gap-5 mt-10">
        <button
          class="btn btn-sm btn-secondary btn-radius"
          style="
            padding-left: 4.2rem;
            padding-right: 4.2rem;
            font-size: 1.28rem;
          "
          @click="openEventDescription()"
        >
          {{ $t("action.viewDetails") }}
        </button>
        <router-link
          :to="'/eventshop'"
          class="btn-sm btn btn-radius outline-button"
          style="
            padding-left: 3.42rem;
            padding-right: 3.42rem;
            font-size: 1.28rem;
          "
        >
          {{ $t("title.eventShop") }}
        </router-link>
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
        <div class="d-flex flex-row gap-5">
          <span class="d-flex flex-row gap-2">
            <span class="banner-detail">{{ $t("fields.startDate") }}</span>
            <span class="banner-detail_value"
              ><TimeShow :date-iso-string="data.startOn" type="eventShop"
            /></span>
          </span>
          <span class="d-flex flex-row gap-2">
            <span class="banner-detail">{{ $t("fields.endDate") }}</span>
            <span class="banner-detail_value">
              <TimeShow :date-iso-string="data.endOn" type="eventShop"
            /></span>
          </span>
        </div>
      </div>
      <div class="d-flex flex-row items-content-center gap-5">
        <button
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          @click="openEventDescription()"
        >
          {{ $t("action.viewDetails") }}
        </button>
        <router-link
          :to="'/eventshop'"
          class="btn-sm btn d-flex align-items-center gap-2 outline-button"
        >
          {{ $t("title.eventShop") }}
        </router-link>
      </div>
    </div>
  </div>
  <EventDetailsCard ref="eventDescriptionRef" />
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import EventDetailsCard from "@/projects/client/modules/eventshop/components/modal/EventDetailsCard.vue";
import ShopService from "../services/ShopService";
import i18n from "@/core/plugins/i18n";
const props = defineProps<{
  withDetail?: boolean;
}>();
const isLoading = ref(false);
const t = i18n.global.t;
const eventNote = ref<string>(t("tip.eventShopDetail"));
const data = ref(<any>[]);

var imgUrl =
  "https://midasmarkets.s3.amazonaws.com/t_10000/26/03/523d599f-69de-421c-9629-295a6acdf502_20d43d49.jpg";
const eventDescriptionRef = ref<any>(null);
const openEventDescription = () => {
  eventDescriptionRef.value?.show(data.value, imgUrl);
};

const fetchData = async () => {
  isLoading.value = true;
  const response = await ShopService.queryEventByKey("EventShop");
  data.value = response;
  console.log("data.value", data.value.images);
  //const img = await ShopService.getImagesWithGuid(data.value.images.desktop);
  //console.log("img", img);
  // if (img) imgUrl = img;
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
  font-weight: 400;
  color: #fff;
}
.banner-detail {
  font-style: normal;
  font-weight: 400;
  font-size: 1.1rem;
  color: #fff;
}
.banner-detail_value {
  font-style: normal;
  font-weight: 400;
  font-size: 1.11rem;
  color: #fff;
}
.outline-button {
  box-sizing: border-box;
  background-color: #fff;
}
.outline-button:hover {
  color: #000;
}
.credit-color {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
}
.role-badge {
  padding: 2px 16px;
  font-weight: 500;
  border-radius: 8px;
}
</style>
