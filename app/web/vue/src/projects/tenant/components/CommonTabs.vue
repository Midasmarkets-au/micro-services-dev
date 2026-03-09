<template>
  <el-tabs @tab-click="handleClick" v-model="activeTab">
    <el-tab-pane
      v-for="(tab, index) in tabsData"
      :key="index"
      :label="tab.label"
      :name="tab.index"
      :disabled="isLoading"
    />
  </el-tabs>
</template>

<script setup lang="ts">
import { ref, inject, nextTick, watch } from "vue";

const isLoading = inject<any>("isLoading");
const criteria = inject<any>("criteria");
const fetchData = inject<any>("fetchData");
const tabsData = inject<any>("tabsData");

const activeTab = ref(0);
const isHandleClickTriggered = ref(false);

const handleClick = async () => {
  isHandleClickTriggered.value = true;
  await updateTab();
  isHandleClickTriggered.value = false;
};

const updateTab = async () => {
  await nextTick();
  criteria.value.status = tabsData[activeTab.value].status;
  await fetchData(1);
};

watch(
  () => criteria.value.status,
  (newVal) => {
    if (isHandleClickTriggered.value == false) {
      activeTab.value = tabsData.findIndex((tab) => tab.status === newVal);
      console.log(activeTab.value);
    }
  }
);
</script>
<style scoped>
::v-deep .el-tabs__item {
  font-size: 16px;
  color: #a1a5b7;
}
</style>
