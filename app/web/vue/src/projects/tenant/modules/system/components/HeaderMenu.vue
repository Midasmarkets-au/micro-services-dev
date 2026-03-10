<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li class="nav-item" v-for="tab in props.tabs" :key="tab.id">
      <a
        class="nav-link text-active-primary pb-4"
        :class="{ active: isTabActive(tab.id) }"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tab.id)"
        >{{ tab.name }}</a
      >
    </li>
  </ul>
</template>
<script setup lang="ts">
const props = defineProps({
  tabs: {
    type: Array,
    required: true,
  },
});
const emit = defineEmits(["changeTab"]);

const isTabActive = (tab: number) => {
  return tab === props.tabs[0].id;
};

const changeTab = (id: number) => {
  emit("changeTab", id);
};
</script>
