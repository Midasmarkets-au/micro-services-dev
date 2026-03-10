<template>
  <headerMenu :tabs="tabs" @change-Tab="handleTabs" />
  <template v-for="tab in tabs" :key="tab.id">
    <template v-if="currentTab === tab.id">
      <component :is="getComponent(tab.component)"></component>
    </template>
  </template>
</template>
<script setup lang="ts">
import { ref } from "vue";
import headerMenu from "@/projects/tenant/modules/system/components/HeaderMenu.vue";
import groupPermission from "@/projects/tenant/modules/system/components/permission/GroupPermission.vue";
import userPermssion from "@/projects/tenant/modules/system/components/permission/UserPermission.vue";
import permissionList from "@/projects/tenant/modules/system/components/permission/PermissionList.vue";
const tabs = ref([
  {
    name: "Role Permission",
    component: "groupPermission",
    id: 1,
  },
  {
    name: "User Permission",
    component: "userPermssion",
    id: 2,
  },
  {
    name: "Permission List",
    component: "permissionList",
    id: 3,
  },
]);
const componentMap = {
  groupPermission,
  userPermssion,
  permissionList,
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};

const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
</script>
