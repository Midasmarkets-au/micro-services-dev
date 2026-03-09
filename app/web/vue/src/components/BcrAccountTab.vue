<template>
  <div class="my-3 px-0 d-flex gap-3" style="overflow: scroll">
    <span
      v-for="(item, index) in tabList"
      :key="index"
      @click="changeTabKey(item.key)"
      class="basic-tab btn btn-light btn-bordered"
      :class="{ 'active-tab btn-primary': activeTabKey === item.key }"
    >
      {{ item.label }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

const props = defineProps<{
  tabList: Array<any>;
  selectedTab: string;
}>();

const activeTabKey = ref(props.selectedTab);

const emits = defineEmits<{
  (e: "changeTab", tab: string): void;
}>();

const changeTabKey = (_tab: string) => {
  activeTabKey.value = _tab;
  emits("changeTab", _tab);
};

defineExpose({
  changeTabKey,
});
</script>

<style scoped lang="scss">
.basic-tab {
  display: flex;
  justify-content: center;
  align-items: center;
  //border: 2px solid #ffd400;
  cursor: pointer;
  white-space: nowrap;
  border-bottom: 0;
  transition: background-color 0.3s;
  background-color: #fff;
  font-size: 15px;
  @media (max-width: 768px) {
    flex: 1;
  }
}

.active-tab {
  background-color: #000f32;
  color: #fff;
}
</style>
