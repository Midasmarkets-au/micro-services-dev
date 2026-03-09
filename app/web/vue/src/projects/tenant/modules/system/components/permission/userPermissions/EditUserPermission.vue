<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        {{ userData.firstName + " " + userData.lastName }}
      </div>
    </div>
    <div class="card-body">
      <div>
        <p class="fs-4">Change User Role</p>
        <div class="row row-cols-5">
          <el-checkbox
            v-for="(item, index) in userRoles"
            :key="index"
            border
            class="my-4 col"
            @change="toggleRole(item)"
            :checked="item.own == true"
          >
            {{ item.Name }} - {{ item.Id }}
          </el-checkbox>
        </div>
        <el-divider></el-divider>
        <p class="fs-4">Change User Permission</p>
        <div v-for="(apiType, i) in userPermissions" :key="i">
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
                {{ item.Id }} | {{ item.Method }} |
                {{ item.Action }}
              </el-checkbox>
            </el-col>
          </el-row>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import SystemService from "../../../services/SystemService";
import notification from "@/core/plugins/notification";
import { ElLoading } from "element-plus";
const userData = inject<any>("userData");
const userRoles = ref<any>([]);
const userPermissions = ref<any>([]);

const fetchData = async () => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });
  try {
    const res = await SystemService.getPermissionUsersByIdV2(userData.value.id);
    await processUserRoles(res);
    await processUserPermissions(res);
  } catch (error) {
    console.log(error);
    notification.danger();
  }
  loading.close();
};

const processUserPermissions = async (res: any) => {
  userPermissions.value = res.permissions;

  userPermissions.value = userPermissions.value.map((item: any) => {
    return {
      ...item,
      Type: getCategoryName(item.Action, item.Category),
      Action: reNameAction(item.Action),
    };
  });

  userPermissions.value = userPermissions.value.reduce((acc, item) => {
    if (!acc[item.Type]) {
      acc[item.Type] = [];
    }
    acc[item.Type].push(item);
    return acc;
  }, {});

  const sortedKeys = Object.keys(userPermissions.value).sort((a, b) =>
    a.localeCompare(b)
  );
  const sortedPermissions = {};

  for (const key of sortedKeys) {
    if (key == "WEB") continue;
    sortedPermissions[key] = userPermissions.value[key];
  }
  sortedPermissions["WEB"] = userPermissions.value["WEB"];

  userPermissions.value = sortedPermissions;
};

const processUserRoles = async (res: any) => {
  userRoles.value = res.roles;
};

const toggleRole = async (item: any) => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });
  try {
    await SystemService.updateUserRole(userData.value.id, item.Id);
    notification.success();
  } catch (error) {
    notification.danger();
  }

  item.own = !item.own;
  loading.close();
};

const togglePermission = async (permission: any) => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });
  try {
    const res = await SystemService.updateUserPermission(
      userData.value.id,
      permission.Id
    );
    notification.success();
  } catch (error) {
    notification.danger();
  }
  loading.close();
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

onMounted(() => {
  fetchData();
});
</script>
