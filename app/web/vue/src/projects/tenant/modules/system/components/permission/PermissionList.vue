<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-button @click="resetCategory()">All</el-button>
        <el-button @click="showByCategory('API')">API</el-button>
        <el-button @click="showByCategory('WEB')">Web</el-button>
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="createOrEdit()">
          Create Permission
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>ID</td>
            <td>Name</td>
            <td>Auth</td>
            <td>Action</td>
            <td>Method</td>
            <td>Category</td>
            <td>CreatedOn</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.id }}</td>
            <td>{{ item.key }}</td>
            <td>{{ item.auth }}</td>
            <td>{{ item.action }}</td>
            <td>{{ item.method }}</td>
            <td>{{ item.category }}</td>
            <td>{{ item.createdOn }}</td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>
  <CreatePermission ref="createPermissionRef" @submit="fetchData(1)" />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import SystemService from "../../services/SystemService";
import CreatePermission from "./CreatePermission.vue";
const isLoading = ref(false);
const data = ref<any>([]);
const tempData = ref<any>([]);
const criteria = ref({ page: 1, size: 20 });
const createPermissionRef = ref<InstanceType<typeof CreatePermission>>();

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await SystemService.getPermissions(criteria.value);
    data.value = res.data;
    tempData.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const createOrEdit = () => {
  createPermissionRef.value?.show();
};

const showByCategory = (category: string) => {
  data.value = tempData.value;
  data.value = data.value.filter((item: any) => {
    return item.category === category;
  });
};

const resetCategory = () => {
  data.value = tempData.value;
};

const pageChange = (page: number) => {
  fetchData(page);
};
onMounted(() => {
  fetchData(1);
});
</script>
