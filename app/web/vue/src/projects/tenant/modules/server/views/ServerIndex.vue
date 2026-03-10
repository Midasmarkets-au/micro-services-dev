<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title align-items-center">
        <h3>Server Metrics</h3>
      </div>
    </div>

    <div
      v-if="isLoading"
      style="height: 300px"
      class="d-flex align-items-center justify-content-center"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
    <div v-else-if="!isLoading && data.length == 0">
      <NoDataBox />
    </div>

    <div class="card-body" v-else>
      <div class="row row-cols-5">
        <div class="col mb-4" v-for="(item, index) in data" :key="index">
          <div class="card" v-loading="item.isLoading">
            <div class="card-header">
              <h3 class="card-title">{{ item.name }}</h3>
              <div class="card-toolbar" v-if="item.stat == 'glances'">
                <div class="d-flex align-items-center ms-3">
                  AR
                  <el-switch
                    v-model="item.autoRefresh"
                    class="ms-1"
                    @change="autoRefreshSwitch(item)"
                  />
                </div>
              </div>
            </div>
            <div class="card-body">
              <div>
                <el-button size="small" @click="copy(item.ip)">
                  {{ item.ip }}</el-button
                >

                <el-button size="small" class="ms-2" @click="copy(item.region)">
                  {{ item.region }}</el-button
                >
                <el-button
                  size="small"
                  class="ms-2"
                  @click="copy(item.instance)"
                >
                  {{ item.instance }}</el-button
                >
              </div>
              <div v-if="item.isRefreshing" class="mt-5">
                <div class="mb-2">
                  {{ item.metrics.timer }} seconds left for next refresh
                </div>
                <div
                  class="d-flex gap-5 justify-content-between align-items-center"
                >
                  <div>CPU:</div>
                  <el-progress
                    class="w-75"
                    :stroke-width="16"
                    :percentage="item.metrics.cpu.percent"
                    :color="progressColor(item.metrics.cpu.percent)"
                  />
                </div>
                <div
                  class="d-flex gap-5 justify-content-between align-items-center mt-2"
                >
                  <div>Mem:</div>
                  <el-progress
                    class="w-75"
                    :stroke-width="16"
                    :percentage="item.metrics.memory.percent"
                    :color="progressColor(item.metrics.memory.percent)"
                  />
                </div>
                <div
                  class="d-flex gap-5 justify-content-between align-items-center mt-2"
                >
                  <div>Disk:</div>
                  <el-progress
                    class="w-75"
                    :stroke-width="16"
                    :percentage="item.metrics.disk.percent"
                    :color="progressColor(item.metrics.disk.percent)"
                  />
                </div>
                <div class="mt-2">
                  <div class="d-flex">
                    <div class="me-15">Load:</div>
                    <div class="d-flex justify-content-between w-50">
                      <div>{{ item.metrics.load.min1.toFixed(2) }}</div>
                      <div>{{ item.metrics.load.min5.toFixed(2) }}</div>
                      <div>{{ item.metrics.load.min15.toFixed(2) }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import ServerServices from "../services/ServerServices";
import notification from "@/core/plugins/notification";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { useStore } from "@/store";
import Clipboard from "clipboard";
import { ElMessage } from "element-plus";

const store = useStore();
const user = store.state.AuthModule.user;
const autoRefreshIds = ref<any>([]);
const localStorageKey = `server_auto_refresh_${user.tenancy}`;
const isLoading = ref(true);
const data = ref<any[]>([]);
const criteria = ref({
  status: 0,
  size: 100,
});

const copy = (value) => {
  Clipboard.copy(value as string);
  ElMessage("Copied");
};

const autoRefreshSwitch = async (item) => {
  try {
    if (item.autoRefresh == false) {
      autoRefreshIds.value = autoRefreshIds.value.filter(
        (id) => id !== item.id
      );
      clearInterval(item.intervalId);
    } else {
      autoRefreshIds.value.push(item.id);
      await refreshMetricsData(item);
    }
    localStorage.setItem(localStorageKey, JSON.stringify(autoRefreshIds.value));
  } catch (e) {
    console.log(e);
  }
};

const fetchData = async () => {
  try {
    const res = await ServerServices.getServersData(criteria.value);

    data.value = res.data;
    data.value = data.value.map((item) => {
      return {
        ...item,
        isRefreshing: false,
        autoRefresh: autoRefreshIds.value.includes(item.id),
      };
    });
  } catch (error) {
    console.log(error);
    notification.danger();
  }
};

const getMetricsData = async (item) => {
  try {
    const res = await ServerServices.getServerMetrics({ id: item.id });
    var disk = JSON.parse(res.metrics.diskJson);

    item.metrics = {
      cpu: JSON.parse(res.metrics.cpuJson),
      memory: JSON.parse(res.metrics.memoryJson),
      disk: disk.filter((item) => item.mnt_point === "/")[0],
      load: JSON.parse(res.metrics.loadJson),
      isLoading: false,
    };
    item.metrics = {
      cpu: {
        ...item.metrics.cpu,
        percent: Math.floor(item.metrics.cpu.total),
      },
      memory: {
        ...item.metrics.memory,
        percent: Math.floor(
          (item.metrics.memory.used / item.metrics.memory.total) * 100
        ),
      },
      disk: {
        ...item.metrics.disk,
      },
      load: {
        ...item.metrics.load,
      },
    };
    console.log("metrics", item.metrics);
    item.isRefreshing = true;
  } catch (error) {
    console.log(error);
    notification.danger();
  }
};

const refreshMetricsData = (item) => {
  item.isLoading = true;
  if (item.intervalId) {
    clearInterval(item.intervalId);
    item.intervalId = null;
  }

  const fetchDataAndRestartTimer = () => {
    getMetricsData(item);

    let timeLeft = 30;
    item.metrics.timer = timeLeft;

    item.intervalId = setInterval(() => {
      timeLeft--;
      item.metrics.timer = timeLeft;

      if (timeLeft <= 0) {
        clearInterval(item.intervalId);
        fetchDataAndRestartTimer(); // Refresh data and restart the timer
      }
    }, 1000);
  };
  item.isLoading = false;
  fetchDataAndRestartTimer(); // Initial call to start the process
};

const progressColor = (value) => {
  if (value < 50) {
    return "#67C23A";
  } else if (value < 80) {
    return "#E6A23C";
  } else {
    return "#F56C6C";
  }
};

onMounted(async () => {
  isLoading.value = true;
  autoRefreshIds.value = JSON.parse(
    localStorage.getItem(localStorageKey) || "[]"
  );
  await fetchData();

  for (let i = 0; i < autoRefreshIds.value.length; i++) {
    const item = data.value.find((item) => item.id == autoRefreshIds.value[i]);
    if (item) {
      await refreshMetricsData(item);
    }
  }
  isLoading.value = false;
});

onUnmounted(() => {
  autoRefreshIds.value.forEach((id) => {
    const item = data.value.find((item) => item.id == id);
    if (item) {
      clearInterval(item.intervalId);
    }
  });
});
</script>
