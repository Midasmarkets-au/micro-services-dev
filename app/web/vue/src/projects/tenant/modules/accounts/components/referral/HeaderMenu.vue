<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li class="nav-item" v-for="tab in props.tabs" :key="tab.id">
      <a
        class="nav-link text-active-primary pb-4"
        :class="[
          { active: currTab === tab.id },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tab.id)"
        >{{ tab.name }}</a
      >
    </li>
  </ul>
</template>
<script setup lang="ts">
import { inject, ref } from "vue";

const isLoading = inject<boolean>("isLoading");
const props = defineProps({
  tabs: {
    type: Array,
    required: true,
  },
});
const emit = defineEmits(["changeTab"]);

const currTab = ref(props.tabs[0].id);
const changeTab = (_tab: any) => {
  currTab.value = _tab;
  emit("changeTab", _tab);
};
</script>
