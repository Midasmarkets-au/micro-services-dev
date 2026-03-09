<template>
  <el-checkbox
    v-model="transferPermission"
    :label="$t('title.transfer')"
    true-label="1"
    false-label="0"
    size="large"
    :disable="isLoading"
    @change="updateData"
    border
  />
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import notification from "@/core/plugins/notification";

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const transferPermission = ref("");
const isLoading = ref(false);

const updateData = async () => {
  isLoading.value = true;

  try {
    if (transferPermission.value === "0") {
      await AccountService.disableAccountTransferPermissionById(
        accountDetails.value.id
      );
    } else if (transferPermission.value === "1") {
      await AccountService.enableAccountTransferPermissionById(
        accountDetails.value.id
      );
    }
    notification.success();
  } catch (err) {
    notification.danger();
    console.error(err);
  }
  isLoading.value = false;
};

onMounted(() => {
  transferPermission.value = accountDetails.value.permission.charAt(0);
});
</script>
