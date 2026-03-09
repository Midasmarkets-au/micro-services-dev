<template>
  <el-checkbox-group v-model="accountTags">
    <el-checkbox
      :disable="isLoading"
      v-for="(item, index) in tagOptions"
      :key="index"
      :label="item.label"
      @change="updateData"
      border
    >
    </el-checkbox>
  </el-checkbox-group>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import { getAllAccountTagTypes } from "@/core/types/AccountTagTypes";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { ElNotification } from "element-plus";

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const isLoading = ref(false);
let tagOptions = ref(getAllAccountTagTypes());
const accountTags = ref([]);

const updateData = async () => {
  isLoading.value = true;

  const finalData = {
    id: accountDetails.value.id,
    tagNames: accountTags.value,
  };
  try {
    await AccountService.updateAccountTagsById(
      accountDetails.value.id,
      finalData
    ).then((res) => {
      ElNotification({
        title: "Success",
        message: "Update Success",
        type: "success",
      });
    });
  } catch (err) {
    ElNotification({
      title: "Error",
      message: "Update Failed",
      type: "error",
    });
  }
  isLoading.value = false;
};
onMounted(() => {
  accountTags.value = accountDetails.value.tags.map((item: any) => item.name);
});
</script>
