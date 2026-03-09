<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-3 fw-semobold mb-8"
  >
    <li class="nav-item" v-for="(tab, index) in tabs" :key="index">
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
</template>
<script setup lang="ts">
import { inject, ref } from "vue";
import { getOptions } from "@/core/helpers/convertToOptions";
const props = defineProps<{
  tabs: any;
}>();
const emit = defineEmits(["changeTab"]);
const tabs = getOptions(props.tabs, "status");
const currTab = ref(tabs[0].value);
const changeTab = (_tab: any) => {
  currTab.value = _tab;
  emit("changeTab", _tab);
};
const isLoading = inject<boolean>("isLoading");

defineExpose({
  changeTab,
});
</script>
