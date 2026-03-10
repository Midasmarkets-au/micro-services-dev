<template>
  <div class="card p-4 p-lg-11">
    <div v-if="!isLoading" class="row row-cols-lg-3 align-items-end">
      <div
        class="col text-center"
        v-for="(item, index) in ibLinks"
        :key="index"
      >
        <h2 class="mb-7 fs-2 text-black">{{ item.name }}</h2>
        <div class="sale-link-card">
          <div class="d-flex justify-content-center">
            <div
              class="d-flex justify-content-center align-items-center sale-link-code px-2 px-lg-6"
            >
              {{ item.code }}
            </div>
          </div>

          <div class="position-relative d-flex justify-content-center mt-13">
            <CopyReferralLink
              :code="item.code"
              :language="getLanguage"
              :siteId="projectConfig.siteId"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import IbService from "../services/IbService";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import { getLanguage } from "@/core/types/LanguageTypes";

const { locale } = useI18n();
const store = useStore();
const ibLinks = ref([] as any);
const isLoading = ref(true);
const projectConfig: PublicSetting = store.state.AuthModule.config;
const salesAccount = computed(() => store.state.SalesModule.salesAccount);

// const language = ref(locale.value);

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await IbService.getIbLinks({ status: 0 });
    ibLinks.value = res.data;
  } catch (error) {
    // console.log(error);
  } finally {
    isLoading.value = false;
  }
};

watch(salesAccount, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    fetchData();
  }
});

onMounted(async () => {
  moibleNavScroller(".ib-menu", ".scroll-to");
  isLoading.value = true;
  await fetchData();
});
</script>

<style scoped lang="scss">
.sale-link-card {
  border: 1px solid #f2f4f7;
  padding: 40px;
  border-radius: 20px;
  width: 100%;
  margin: 0 auto;
  font-size: 18px;
  margin-bottom: 50px;
  background-color: #fff;
  &:hover {
    box-shadow: 0px 2px 4px 0px rgba(21, 63, 133, 0.32);
  }
}

@media (max-width: 768px) {
  .sale-link-code {
    font-size: 26px !important;
  }
}
.sale-link-code {
  //border: 1px solid gray;
  border-radius: 10px;
  font-size: 30px;
  font-weight: 600;
  color: #0a46aa;
}
</style>
