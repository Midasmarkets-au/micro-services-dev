<template>
  <div class="card mb-2">
    <div class="card-body" v-loading="isLoading" style="min-height: 250px">
      <div class="row row-cols-5">
        <div v-for="(item, index) in data" :key="index" class="col py-4">
          <el-button
            size="large"
            type="warning"
            class="min-w-200px"
            plain
            round
            @click="handleDetail(item)"
            >{{ item.name }} - {{ item.id }}</el-button
          >
        </div>
      </div>
    </div>
  </div>
  <EditGroupPermissions v-if="showDetail" />
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, provide } from "vue";
import EditGroupPermissions from "./groupPermissions/EditGroupPermissions.vue";
import SystemService from "../../services/SystemService";
const isLoading = ref(false);
const showDetail = ref(false);
const data = ref<any>([]);
const groupData = ref<any>([]);
const permissions = ref<any>([]);
provide("groupData", groupData);
provide("permissions", permissions);
const handleDetail = async (item: any) => {
  showDetail.value = false;
  groupData.value = item;
  await nextTick();
  showDetail.value = true;
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.getPermissionRoles();
    const res2 = await SystemService.getPermissions();
    data.value = res.data;
    permissions.value = res2.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

onMounted(() => {
  fetchData();
});
</script>
