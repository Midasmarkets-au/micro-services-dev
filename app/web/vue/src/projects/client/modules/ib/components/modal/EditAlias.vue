<template>
  <el-dialog
    v-model="centerDialogVisible"
    :title="title"
    width="650"
    align-center
    :before-close="hide"
  >
    <div v-if="!isMobile">
      <div v-for="(item, index) in accounts" :key="index">
        <div class="d-flex gap-6 align-items-center mb-4">
          <div style="width: 120px">
            <span> {{ item.uid }}</span>
            <span v-if="item?.siteId"
              >({{ $t("type.siteType." + item.siteId) }})</span
            >
          </div>
          <el-input
            v-model="item.alias"
            placeholder="Please enter the alias"
            style="width: 180px; margin-right: 10px"
            clearable
            :disabled="isLoading"
          ></el-input>
          <div class="d-flex gap-6">
            <button
              class="btn btn-primary btn-sm"
              @click="updateAlias(item)"
              :disabled="isLoading"
            >
              {{ $t("action.updateAlias") }}
            </button>
            <button
              class="btn btn-danger btn-sm"
              v-if="item.isDefault"
              disabled
            >
              {{ $t("status.default") }}
            </button>
            <button
              v-else
              class="btn btn-success btn-sm"
              :disabled="isLoading"
              @click="updateDefaultAccount(item)"
            >
              {{ $t("action.setDefault") }}
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-else>
      <div v-for="(item, index) in accounts" :key="index" class="border_bottom">
        <div class="gap-6 align-items-center">
          <span> {{ item.uid }}</span>
          <span v-if="item?.siteId"
            >({{ $t("type.siteType." + item.siteId) }})</span
          >
          <button
            class="ms-8 btn btn-danger btn-sm"
            v-if="item.isDefault"
            disabled
          >
            {{ $t("status.default") }}
          </button>
          <button
            v-else
            class="ms-8 btn btn-success btn-sm"
            @click="updateDefaultAccount(item)"
          >
            {{ $t("action.setDefault") }}
          </button>
        </div>
        <div class="mt-2 mb-6">
          <el-input
            v-model="item.alias"
            placeholder="Please enter the alias"
            style="width: 110px"
            clearable
            :disabled="isLoading"
          ></el-input>
          <button
            class="ms-4 btn btn-primary btn-sm"
            @click="updateAlias(item)"
            style="white-space: nowrap"
            :disabled="isLoading"
          >
            {{ $t("action.updateAlias") }}
          </button>
        </div>
      </div>
    </div>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import i18n from "@/core/plugins/i18n";
import AccountService from "../../../accounts/services/AccountService";
import { ElNotification } from "element-plus";
import { useStore } from "@/store";
import { isMobile } from "@/core/config/WindowConfig";

const t = i18n.global.t;
const title = ref(t("title.updateAccountSettings"));
const store = useStore();
const defaultAgentAccount = ref(
  store.state.AuthModule.user.defaultAgentAccount
);
const isLoading = ref(false);
const centerDialogVisible = ref(false);
const accounts = ref(<any>[]);
const emits = defineEmits<{
  (e: "update"): void;
}>();

const updateDefaultAccount = async (item: any) => {
  isLoading.value = true;
  try {
    await AccountService.updateDefaultSalesAccount(item.uid).then(() => {
      ElNotification({
        title: t("status.success"),
        type: "success",
      });
      accounts.value.forEach((account: any) => {
        account.isDefault = account.uid == item.uid;
      });
      defaultAgentAccount.value = item.uid;
      store.state.AuthModule.user.defaultAgentAccount = item.uid;
      emits("update");
    });
  } catch (error) {
    ElNotification({
      title: t("error.__UPDATE_FAILED__"),
      type: "error",
    });
    console.error(error);
  }

  isLoading.value = false;
};

const updateAlias = async (item: any) => {
  isLoading.value = true;
  try {
    await AccountService.updateAlias({
      uid: item.uid,
      alias: item.alias,
    }).then(() => {
      ElNotification({
        title: t("status.success"),
        type: "success",
      });
      emits("update");
    });
  } catch (error) {
    ElNotification({
      title: t("error.__UPDATE_FAILED__"),
      type: "error",
    });
    console.error(error);
  }

  isLoading.value = false;
};
const show = (_accounts: any) => {
  centerDialogVisible.value = true;
  if (_accounts.length > 1) {
    accounts.value = _accounts.map((account: any) => ({
      ...account,
      isDefault: account.uid == defaultAgentAccount.value,
    }));
  } else {
    accounts.value = [{ ..._accounts }];
  }
};
const hide = () => {
  centerDialogVisible.value = false;
  accounts.value = [];
};

defineExpose({
  show,
  hide,
});
</script>
<style scoped>
.border_bottom {
  border-bottom: 1px solid #ebeef5;
  margin-bottom: 20px;
}
</style>
