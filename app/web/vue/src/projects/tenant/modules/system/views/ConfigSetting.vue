<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="d-flex justify-content-between">
          <ul
            class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
          >
            <li
              class="nav-item"
              v-for="(item, index) in ConfigSiteTypesSelections"
              :key="index"
            >
              <a
                class="nav-link text-active-primary pb-4"
                :class="{
                  active: item.value === currentTab,
                }"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(item.value)"
                >{{ item.label }}</a
              >
            </li>
          </ul>
        </div>
      </div>
      <div class="card-toolbar">
        <button
          class="btn btn-danger btn-sm"
          :disabled="isReloading"
          @click="reload"
        >
          Reload
        </button>
      </div>
    </div>
    <div class="card-body">
      <ConfigList
        v-if="!isLoading"
        :rowId="currentTab"
        :category="ConfigCategory.public"
        :defaultData="defaultData"
      />
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import { SiteTypes, ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import { ConfigCategory } from "@/core/types/ConfigCategory";
import SystemService from "../services/SystemService";
import ConfigList from "../components/config/ConfigList.vue";
const isLoading = ref(false);
const isReloading = ref(false);
const currentTab = ref(SiteTypes.Default);
const data = ref([]);
const defaultData = ref([]);

const changeTab = (tab) => {
  currentTab.value = tab;
  fetchData();
};

const fetchDefaultData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryConfig(
      SiteTypes.Default,
      ConfigCategory.public
    );
    defaultData.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryConfig(
      currentTab.value,
      ConfigCategory.public
    );
    data.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const reload = () => {
  isReloading.value = true;
  SystemService.reloadConfiguration().then(() => {
    isReloading.value = false;
  });
};

onMounted(async () => {
  await fetchDefaultData();
});
</script>
