<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">Zoom (TBD)</div>
      <div class="card-toolbar">
        <el-button type="primary" @click="openCreateMeeting" disabled>
          Create Meeting
        </el-button>
        <el-button type="danger" disabled>End Current Meeting</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <td>Topic</td>
            <td>Host</td>
            <td>Start Time</td>
            <td>End Time</td>
            <td>duration</td>
            <td>Link</td>
            <td>Record</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else></tbody>
      </table>
    </div>
    <CreateMeeting ref="createMeetingRef" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ToolServices from "../services/ToolServices";
import CreateMeeting from "../modals/ZoomIndex/CreateMeeting.vue";

const isLoading = ref(true);
const data = ref([]);
const createMeetingRef = ref<InstanceType<typeof CreateMeeting>>();
const fetchData = async () => {
  isLoading.value = true;

  isLoading.value = false;
};

const openCreateMeeting = () => {
  createMeetingRef.value?.show();
};

onMounted(() => {
  //   fetchData();
});
</script>
