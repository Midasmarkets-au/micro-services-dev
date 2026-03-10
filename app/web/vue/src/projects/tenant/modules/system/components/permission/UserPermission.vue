<template>
  <div class="card mb-2">
    <div class="card-header">
      <div class="card-title">
        <el-input
          clearable
          v-model="userId"
          placeholder="user ID"
          :isLoading="isLoading"
        ></el-input>
        <el-button type="success" class="ms-2" @click="addUser">Add</el-button>
      </div>
    </div>
    <div class="card-body">
      <el-collapse v-model="collapse">
        <el-collapse-item title="User Permission" name="1">
          <table class="table align-middle table-row-dashed fs-6 gy-1">
            <thead>
              <tr class="text-muted fw-bold fs-7 text-uppercase gs-0">
                <td>User Id</td>
                <td>{{ $t("fields.name") }}</td>
                <td>{{ $t("fields.email") }}</td>
                <td>{{ $t("fields.role") }}</td>
                <td>{{ $t("action.action") }}</td>
              </tr>
            </thead>
            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoading && data.length == 0">
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr
                v-for="(item, index) in data"
                :key="index"
                :class="{
                  'user-select': item.id === userSelected,
                }"
                @click="tagUser(item)"
              >
                <td>{{ item.id }}</td>
                <td>{{ item.firstName + " " + item.lastName }}</td>
                <td>{{ item.email }}</td>
                <td>{{ item.roles }}</td>
                <td>
                  <el-button
                    type="primary"
                    size="small"
                    @click="handleDetail(item)"
                    >{{ $t("action.detail") }}</el-button
                  >
                </td>
              </tr>
            </tbody>
          </table>
        </el-collapse-item>
      </el-collapse>
    </div>
  </div>
  <EditUserPermission v-if="showDetail" />
</template>

<script setup lang="ts">
import { ref, onMounted, provide, nextTick } from "vue";
import EditUserPermission from "./userPermissions/EditUserPermission.vue";
import SystemService from "../../services/SystemService";
import notification from "@/core/plugins/notification";

const isLoading = ref(false);
const data = ref<any>([]);
const showDetail = ref(false);
const userData = ref({});
const userId = ref<any>(null);
const collapse = ref("1");
const userSelected = ref(0);

provide("userData", userData);

const tagUser = (item: any) => {
  userSelected.value = item.id;
};
const handleDetail = async (item: any) => {
  showDetail.value = false;
  await nextTick();
  collapse.value = "0";
  userSelected.value = item.id;
  showDetail.value = true;
  userData.value = item;
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.getPermissionUsers();
    data.value = res.data;
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  isLoading.value = false;
};

const addUser = async () => {
  isLoading.value = true;
  try {
    await SystemService.updateUserRole(userId.value, 99);
    await fetchData();
    notification.success();
  } catch (e) {
    console.log(e);
    notification.danger();
  }
  isLoading.value = false;
};

onMounted(() => {
  fetchData();
});
</script>
<style>
.user-select {
  background-color: rgba(254, 215, 215, 0.5) !important;
}
</style>
