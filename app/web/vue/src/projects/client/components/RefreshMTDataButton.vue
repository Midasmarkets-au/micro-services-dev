<template>
  <el-button
    :disabled="isRefreshing"
    class="mt-2"
    type="primary"
    :icon="Refresh"
    @click="handleRefresh"
  >
    {{ $t("action.refresh") }}
  </el-button>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import AccountService from "@/projects/client/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Refresh } from "@element-plus/icons-vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps<{
  accountId: number;
}>();

const emit = defineEmits<{
  refreshed: [data: any];
}>();

const isRefreshing = ref(false);

const handleRefresh = async () => {
  if (isRefreshing.value) return;

  try {
    isRefreshing.value = true;

    const result = await AccountService.refreshMTDataById(props.accountId);

    MsgPrompt.success(`${t("action.refresh")} ${t("status.success")}`);
    emit("refreshed", result);
  } catch (error: any) {
    console.error("Refresh MT data error:", error);
    MsgPrompt.error(`${t("action.refresh")} ${t("status.failed")}`);
  } finally {
    isRefreshing.value = false;
  }
};
</script>

<style scoped lang="scss">
.rotate-animation {
  animation: rotate 1s linear infinite;
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
