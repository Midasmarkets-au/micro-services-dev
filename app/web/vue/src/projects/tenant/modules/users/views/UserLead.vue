<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-3 fw-semobold mb-8"
  >
    <li class="nav-item" v-for="(tab, index) in TabStatus" :key="index">
      <a
        :class="[
          'nav-link text-active-primary pb-1',
          { active: currTab === tab.value },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="!isLoading && changeTab(tab.value)"
        >{{ tab.label }}</a
      >
    </li>
  </ul>

  <MainIndex v-if="currTab === 0" />
  <UtmIndex v-else-if="currTab === 1" />
</template>
<script setup lang="ts">
import { ref, provide } from "vue";
import MainIndex from "../components/lead/MainIndex.vue";
import UtmIndex from "../components/lead/UtmIndex.vue";

const TabStatus = [
  { label: "All", value: 0 },
  { label: "UTM", value: 1 },
];

const isLoading = ref(false);
provide("isLoading", isLoading);
const currTab = ref(TabStatus[1].value);

const changeTab = (tab: any) => {
  currTab.value = tab;
};
</script>
