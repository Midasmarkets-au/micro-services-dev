<template v-if="!isLoading">
  <div
    class="card ratio overflow-hidden banner-card-desktop"
    style="--bs-aspect-ratio: 31.615%; border: 0 !important; scale: 1.002"
    v-if="!isMobile"
  >
    <div
      class="card-body d-flex flex-column justify-content-end align-items-end banner-bg gap-3 px-16"
      :style="bannerBgStyle"
    >
      <div class="d-flex flex-row items-content-center gap-5 banner-actions">
        <!-- click open modal -->
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
    class="card overflow-hidden border mb-5 banner-card-mobile"
    v-if="isMobile"
    style="border: 0 !important"
  >
    <div
      class="card-body d-flex flex-column justify-content-end align-items-end gap-3 px-4 pb-4 pt-3 banner-bg banner-card-body-mobile"
      :style="bannerBgStyle"
    >
      <div
        class="d-flex flex-row items-content-center gap-3 banner-actions banner-actions-mobile"
      >
        <!-- click open modal -->
        <button
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          @click="openEventDescription()"
        >
          {{ $t("action.viewDetails") }}
        </button>
        <router-link
          :to="'/eventshop'"
          class="btn btn-sm d-flex align-items-center gap-2 outline-button"
        >
          {{ $t("title.eventShop") }}
        </router-link>
      </div>
    </div>
  </div>
  <EventDetailsCard ref="eventDescriptionRef" />
</template>
<script lang="ts" setup>
import { ref, computed, onMounted } from "vue";
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

const BANNER_IMAGE_URL =
  "https://midasmarkets.s3.amazonaws.com/t_10000/26/02/ef225bd7-cf31-486c-af59-2194cb712502_48c84ac2.jpg";
let imgUrl = BANNER_IMAGE_URL;
const eventDescriptionRef = ref<any>(null);
const openEventDescription = () => {
  eventDescriptionRef.value?.show(data.value, imgUrl);
};

const fetchData = async () => {
  isLoading.value = true;
  const response = await ShopService.queryEventByKey("EventShop");
  data.value = response;
  isLoading.value = false;
};

const bannerBgStyle = computed(() => ({
  backgroundImage: `url("${BANNER_IMAGE_URL}")`,
  backgroundSize: "cover",
  backgroundPosition: "center",
  backgroundRepeat: "no-repeat",
}));

onMounted(async () => {
  await fetchData();
  // document.documentElement.style.setProperty(
  //   "--banner-bg-image",
  //   `url("${imgUrl}")`
  // );
});
</script>

<style scoped>
/* 1920 × 607 等比例：高度 / 宽度 ≈ 31.615% */
.banner-card-desktop {
  aspect-ratio: 1920 / 607;
}
.banner-card-mobile {
  aspect-ratio: 1920 / 607;
}
.banner-card-body-mobile {
  min-height: 100%;
}
.banner-bg {
  color: white;
}
.banner-actions {
  margin-top: auto;
}
.banner-actions-mobile .btn,
.banner-actions-mobile a.btn {
  padding: 0.4rem 0.85rem !important;
  font-size: 0.875rem !important;
  min-height: unset;
  line-height: 1.35;
}
.banner-actions-mobile {
  gap: 0.75rem !important;
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
  /* background-color: #ffce00 !important;
  border: 1px solid #ffce00 !important; */
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
