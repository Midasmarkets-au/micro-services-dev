<template>
  <div class="card">
    <div class="card-body">
      <el-checkbox-group v-model="currentRoles" class="row row-cols-5">
        <el-checkbox
          v-for="item in data"
          :key="item.id"
          :label="item.id"
          size="large"
          class="m-3"
          border
          :disabled="isLoading"
          @change="toggleRoles(item)"
        >
          <span> {{ item.name }}</span>

          <span v-if="$can('SuperAdmin')">- {{ item.id }}</span>
        </el-checkbox>
      </el-checkbox-group>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import UserService from "../../services/UserService";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import { ElNotification } from "element-plus";
import { getCurrentInstance } from "vue";

const { proxy } = getCurrentInstance();
const isLoading = ref(false);
const data = ref<any>([]);
const userInfos = inject<any>("userInfos");
const partyId = ref(userInfos.value.partyId);
const currentRoles = ref<any>();

const getPermission = async () => {
  try {
    const result = await SystemService.getPermissionUsersById(partyId.value);
    currentRoles.value = result.user.roles;
  } catch (e) {
    console.error(e);
  }
};
const fetchData = async () => {
  isLoading.value = true;
  try {
    data.value = await UserService.queryUserRoles();
    await getPermission();
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const processOptions = async () => {
  isLoading.value = true;
  if (!proxy.$can("SuperAdmin")) {
    data.value = data.value.filter((item: any) => item.id != 10);
  }
  if (!proxy.$can("TenantAdmin")) {
    data.value = [];
  }
  isLoading.value = false;
};

const toggleRoles = async (item: any) => {
  isLoading.value = true;
  try {
    if (currentRoles.value.includes(item.id)) {
      await UserService.addUserRole(partyId.value, item.id);
    } else {
      await UserService.removeUserRole(partyId.value, item.id);
    }
    ElNotification({
      title: "Success",
      message: item.name + " updated successfully",
      type: "success",
    });
  } catch (error) {
    console.error(error);
    if (currentRoles.value.includes(item.id)) {
      currentRoles.value = currentRoles.value.filter(
        (role) => role !== item.id
      );
    } else {
      currentRoles.value.push(item.id);
    }
    ElNotification({
      title: "Error",
      message: "Failed to update " + item.name,
      type: "error",
    });
  }
  isLoading.value = false;
};

onMounted(async () => {
  await fetchData();
  await processOptions();
});
</script>
