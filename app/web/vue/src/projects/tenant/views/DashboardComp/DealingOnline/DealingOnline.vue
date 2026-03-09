<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="fs-6 fw-bold">{{ $t("fields.adminOnline") }}</div>
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="fecthData" :loading="isLoading">
          {{ $t("action.refresh") }}
        </el-button>
      </div>
    </div>
    <div class="card-body fs-5" v-loading="isLoading">
      <div>
        <div v-for="(item, index) in data" :key="index" class="mb-3">
          <el-card
            :style="{ borderTop: `2px solid ${getSiteColor(item.tenantId)}` }"
            v-if="$can('SuperAdmin') || tenantId == item.tenantId"
          >
            <template #header>
              <div class="fw-bold uppercase">
                {{ TenantTypes[item.tenantId] }}
              </div>
            </template>
            <div v-for="(admin, i) in item.users" :key="i">
              <div class="mb-1">{{ admin.email }}</div>
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
const isLoading = ref(false);
const data = ref<any>([]);
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
    const res = await SystemService.queryOnlineAdmin();
    data.value = res;
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
