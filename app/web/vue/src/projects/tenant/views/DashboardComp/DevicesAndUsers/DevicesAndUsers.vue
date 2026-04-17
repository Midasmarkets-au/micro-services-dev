<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="fs-6 fw-bold">
          {{ $t("fields.totalUsers") }}: {{ totalUsers }}
        </div>
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="fecthData" :loading="isLoading">
          {{ $t("action.refresh") }}
        </el-button>
      </div>
    </div>
    <div class="card-body fs-5" v-loading="isLoading">
      <div class="d-flex gap-5">
        <div v-for="(item, index) in data" :key="index">
          <el-card
            :style="{ borderTop: `2px solid ${getSiteColor(item.tenantId)}` }"
            v-if="$can('SuperAdmin') || tenantId == item.tenantId"
          >
            <template #header>
              <div class="fw-bold uppercase">
                {{ item.tenantName }}
                {{ TenantTypes[item.tenantId] }} -
                {{ $t("fields.totalUsers") }}: {{ item.total }}
              </div>
            </template>
            <p
              class="fs-6 fw-bold"
              :style="{ color: `${getSiteColor(item.tenantId)}` }"
            >
              {{ $t("fields.browsers") }}
            </p>
            <div v-for="(client, i) in item.clientStat" :key="i">
              <div class="mb-1">{{ i }} : {{ client }}</div>
            </div>
            <hr />
            <p
              class="fs-6 fw-bold"
              :style="{ color: `${getSiteColor(item.tenantId)}` }"
            >
              {{ $t("fields.devices") }}
            </p>
            <div v-for="(device, j) in item.deviceStat" :key="j">
              <div>{{ j }} : {{ device }}</div>
            </div>
          </el-card>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import { ref, onMounted } from "vue";
import { TenantTypes } from "@/core/types/TenantTypes";
import { getCurrentInstance } from "vue";
const { proxy } = getCurrentInstance();
const isLoading = ref(false);
const data = ref<any>([]);
const totalUsers = ref(0);
const tenantId = ref<any>(null);

var siteColors = {
  au: "#7a9e7a",
  bvi: "#fa6b6c",
  sea: "#349beb",
  mn: "#b497b4",
  jp: "#f6c23e",
};

const getSiteColor = (site: any) => {
  return siteColors[TenantTypes[site]] || "#000000"; // default color if site not found
};

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryOnlineUsers();
    data.value = res.filter((item: any) => {
      return item.tenantId != TenantTypes.jp;
    });

    totalUsers.value = data.value.reduce((acc: number, item: any) => acc + item.total, 0);
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

onMounted(() => {
  tenantId.value = localStorage.getItem("tenant");
  fecthData();
});
</script>
<style scoped lang="scss">
.uppercase {
  text-transform: uppercase;
}
</style>
