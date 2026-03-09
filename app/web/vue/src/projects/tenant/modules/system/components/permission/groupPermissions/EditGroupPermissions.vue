<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ groupData.name }}</div>
    </div>
    <div class="card-body">
      <div v-for="(apiType, i) in finalPermissions" :key="i">
        <h3 class="mt-3">{{ i }}</h3>
        <el-row>
          <el-col :span="8" v-for="(item, index) in apiType" :key="index">
            <el-checkbox
              border
              class="my-4"
              style="width: 90%"
              @change="togglePermission(item)"
              :checked="item.own == true"
            >
              {{ item.id }} | {{ item.method }} |
              {{ item.action }}
            </el-checkbox>
          </el-col>
        </el-row>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import SystemService from "../../../services/SystemService";
import notification from "@/core/plugins/notification";
import { ElLoading } from "element-plus";

const groupData = inject<any>("groupData");
const permissions = inject<any>("permissions");
const rolePermissions = ref<any>([]);
const finalPermissions = ref<any>([]);

const fecthData = async () => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    spinner: "el-icon-loading",
    background: "rgba(0, 0, 0, 0.7)",
  });
  try {
    const res = await SystemService.getPermissionRolesById(groupData.value.id);
    rolePermissions.value = res;
    await processPermissions();
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  loading.close();
};

const togglePermission = async (permission: any) => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });
  try {
    await SystemService.updateRolePermission(groupData.value.id, permission.id);
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  loading.close();
};

const processPermissions = async () => {
  finalPermissions.value = permissions.value.map((item: any) => {
    return {
      ...item,
      type: getCategoryName(item.action, item.category),
      action: reNameAction(item.action),
      own: rolePermissions.value.includes(item.id),
    };
  });
  finalPermissions.value = finalPermissions.value.reduce((acc, item) => {
    if (!acc[item.type]) {
      acc[item.type] = [];
    }
    acc[item.type].push(item);
    return acc;
  }, {});

  const sortedKeys = Object.keys(finalPermissions.value).sort((a, b) =>
    a.localeCompare(b)
  );
  const sortedPermissions = {};

  for (const key of sortedKeys) {
    sortedPermissions[key] = finalPermissions.value[key];
  }

  finalPermissions.value = sortedPermissions;
};

const getCategoryName = (name: string, type: string) => {
  if (type === "API") {
    const subString = name.split("/tenant/")[1];
    return subString === undefined ? name : subString.split("/")[0];
  } else {
    return type;
  }
};

const reNameAction = (name: string) => {
  const subString = name.split("/tenant/")[1];
  return subString === undefined ? name : subString;
};

onMounted(async () => {
  await fecthData();
});
</script>
