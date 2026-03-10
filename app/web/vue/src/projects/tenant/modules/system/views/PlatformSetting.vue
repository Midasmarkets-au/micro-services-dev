<template>
  <div class="card">
    <div class="card-header"></div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>{{ $t("fields.name") }}</td>
            <td>{{ $t("fields.platform") }}</td>
            <td>Description</td>
            <td>{{ $t("action.action") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.name }}</td>
            <td>{{ ServiceTypes[item.platform] }} ({{ item.platform }})</td>
            <td>{{ item.description }}</td>
            <td>
              <el-popover trigger="click" :width="500">
                <template #reference
                  ><el-button type="primary">View Group</el-button></template
                >
                <div class="overflow-scroll h-500px">
                  <div class="row">
                    <div
                      v-for="(i, index) in item.groups"
                      :key="index"
                      class="col-6"
                    >
                      {{ i }}
                    </div>
                  </div>
                </div>
              </el-popover>

              <el-button type="success" @click="addGroup(item)"
                >Add Group</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <AddGroup ref="modalRef" @submit="fetchData" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import SystemService from "../services/SystemService";
import { ServiceTypes } from "@/core/types/ServiceTypes";
import AddGroup from "../components/platform/AddGroup.vue";
const isLoading = ref(false);
const data = ref(<any>[]);
const modalRef = ref<InstanceType<typeof AddGroup>>();
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.getServices();
    data.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const addGroup = (item: any) => {
  modalRef.value?.show(item);
};

onMounted(() => {
  fetchData();
});
</script>
