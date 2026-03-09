<template>
  <!-- <el-tag v-for="item in userRolesData" :key="item.id" class="me-4">
    {{ item.name }}
  </el-tag> -->
  <p>hi</p>
</template>

<script lang="ts" setup>
import { defineProps, ref, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";
import UserService from "../../modules/users/services/UserService";
const props = defineProps<{
  userDetails?: any;
}>();
const isLoading = ref(false);
const userRolesData = ref<any>([]);

const fecthData = async () => {
  isLoading.value = true;
  try {
    const roles = await SystemService.getPermissionRoles();
    userRolesData.value = roles.data.find((item: any) => {
      return props.userDetails.roles.includes(item.id);
    });
    console.log("userRolesData", userRolesData.value);
  } catch (e) {
    MsgPrompt.error(e);
  }
  isLoading.value = false;
};
onMounted(() => {
  if (props.userDetails.roles) {
    if (props.userDetails.roles.length > 0) {
      fecthData();
    }
  }
});
</script>
