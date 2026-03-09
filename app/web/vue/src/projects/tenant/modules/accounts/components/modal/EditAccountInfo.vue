<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="label"
      :is-loading="isLoading"
      disable-footer
    >
      <div class="mb-9 d-flex justify-content-center">
        <el-select v-model="vModel" class="m-2 w-250px" :disabled="isLoading">
          <el-option
            v-for="item in options"
            :key="item?.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>
      <div class="mb-9" v-if="updateType === 'status'">
        <div class="d-flex justify-content-center" :disabled="isLoading">
          <el-input
            v-model="comment"
            class="mb-2 w-250px"
            :placeholder="$t('tip.enterReason')"
            type="textarea"
          />
        </div>
        <p class="text-danger text-center" v-if="showCommentError">
          {{ $t("error.fieldIsRequired") }}
        </p>
      </div>

      <div class="d-flex justify-content-center">
        <button
          class="btn btn-light btn-success btn-md me-3"
          @click="updateData"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>{{ $t("action.update") }}</span>
        </button>
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "../../../users/services/UserService";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
const isLoading = ref(false);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const accountDetail = ref(<any>[]);
const options = ref(null);
const label = ref("");
const vModel = ref(<any>null);
const comment = ref("");
const showCommentError = ref(false);
const updateType = ref("");

const updateSiteId = async () => {
  await UserService.updateAccountSiteID(accountDetail.value?.id, {
    id: accountDetail.value.id,
    site: vModel.value,
  });
  MsgPrompt.success("Update Site ID successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updateAccountType = async () => {
  await UserService.updateAccountType(accountDetail.value?.id, {
    id: accountDetail.value.id,
    type: vModel.value,
  });
  MsgPrompt.success("Update Account Type successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updatePlatformGroup = async () => {
  await UserService.updatePlatformGroup(
    accountDetail.value?.partyId,
    vModel.value
  );
  await UserService.rebuildUserIndex(accountDetail.value.value);
  MsgPrompt.success("Update Platform Group successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updateFundType = async () => {
  await AccountService.updateFundType(accountDetail.value?.id, vModel.value);
  MsgPrompt.success("Update Fund Type successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updateStatus = async () => {
  if (comment.value.trim() === "") {
    showCommentError.value = true;
    return;
  }
  showCommentError.value = false;
  await AccountService.updateAccountStatus(
    accountDetail.value?.id,
    vModel.value,
    {
      comment: comment.value,
    }
  );
  MsgPrompt.success("Update Status successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updateLeverage = async () => {
  await AccountService.updateTradeAccountLeverage(
    accountDetail.value?.id,
    vModel.value
  );
  MsgPrompt.success("Update Leverage successfully").then(() => {
    modalForm.value?.hide();
  });
};

const updateData = async () => {
  isLoading.value = true;
  try {
    const updateFn = updateFunctions[updateType.value];
    await updateFn().then(() => {
      emit("refresh");
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const updateFunctions = {
  siteId: updateSiteId,
  accountType: updateAccountType,
  fundType: updateFundType,
  platformGroup: updatePlatformGroup,
  status: updateStatus,
  leverage: updateLeverage,
};

const show = (_accountDetail, _data) => {
  modalForm.value?.show();
  accountDetail.value = _accountDetail;
  options.value = _data?.options;
  label.value = _data?.label;
  vModel.value = _data?.vModel;
  updateType.value = _data?.type;
};

const emit = defineEmits(["refresh"]);
defineExpose({
  show,
});
</script>

<style scoped></style>
