<template>
  <div>
    <el-tag effect="dark" class="tag">GMT: {{ gmtTime }}</el-tag>
    <el-tag effect="dark" class="tag" type="success"
      >HK: {{ hongKongTime }}</el-tag
    >
    <el-tag effect="dark" class="tag" type="info">SYD: {{ sydneyTime }}</el-tag>
    <el-tag effect="dark" class="tag" type="warning"
      >LA: {{ usEasternTime }}</el-tag
    >
    <el-tag effect="light" class="tag">JP: {{ jpTime }}</el-tag>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
const gmtTime = ref("");
const hongKongTime = ref("");
const sydneyTime = ref("");
const usEasternTime = ref("");
const jpTime = ref("");
const updateTimeZones = () => {
  const now = new Date();
  gmtTime.value = new Intl.DateTimeFormat("en-GB", {
    timeStyle: "medium",
    timeZone: "GMT",
  }).format(now);

  hongKongTime.value = new Intl.DateTimeFormat("en-GB", {
    timeStyle: "medium",
    timeZone: "Asia/Hong_Kong",
  }).format(now);

  sydneyTime.value = new Intl.DateTimeFormat("en-GB", {
    timeStyle: "medium",
    timeZone: "Australia/Sydney",
  }).format(now);

  usEasternTime.value = new Intl.DateTimeFormat("en-GB", {
    timeStyle: "medium",
    timeZone: "America/Los_Angeles",
  }).format(now);
  jpTime.value = new Intl.DateTimeFormat("en-GB", {
    timeStyle: "medium",
    timeZone: "Asia/Tokyo",
  }).format(now);
};

let intervalId;

onMounted(() => {
  updateTimeZones();
  intervalId = setInterval(updateTimeZones, 1000); // Update every second
});
</script>
<style scoped>
.tag {
  font-size: 15px;
  margin-left: 10px;
}
</style>
