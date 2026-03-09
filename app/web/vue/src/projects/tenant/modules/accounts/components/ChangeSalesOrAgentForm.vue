<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeSalesOrIb')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 1000px"
    disable-footer
  >
    <div>
      <LoadingCentralBox v-if="isLoading" />
      <div v-else class="w-100 row overflow-hidden">
        <div class="col-lg-5">
          <div
            class="w-100 d-flex justify-content-center"
            style="border-bottom: 1px dashed #ccc"
          >
            <h1 class="mb-5 text-gray-600">
              {{ $t("title.currentSalesAndIb") }}
            </h1>
          </div>

          <div class="row">
            <div class="col-lg-6" style="border-right: 1px dashed #ccc">
              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.salesCode") }}
              </label>
              <el-form-item>
                <el-input
                  disabled
                  v-model="accountDetails.salesAccount.group"
                />
              </el-form-item>

              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.salesInfo") }}
              </label>
              <el-form-item>
                <el-input disabled v-model="accountDetails.salesAccount.name" />
              </el-form-item>
            </div>
            <div class="col-lg-6">
              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.ibGroup") }}
              </label>
              <el-form-item>
                <el-input
                  disabled
                  v-model="accountDetails.agentAccount.group"
                />
              </el-form-item>

              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.ibInfo") }}
              </label>
              <el-form-item>
                <el-input disabled v-model="accountDetails.agentAccount.name" />
              </el-form-item>
            </div>
          </div>
        </div>

        <div class="col-lg-1 d-flex justify-content-center align-items-center">
          <div class="d-flex flex-column gap-5">
            <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>
            <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>
            <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>
          </div>
        </div>

        <div class="col-lg-6">
          <div class="" style="border-bottom: 1px dashed #ccc">
            <div class="w-100 d-flex justify-content-center">
              <h1 class="mb-5 text-gray-800">
                {{ $t("title.newSalesAndIb") }}
              </h1>
            </div>
          </div>

          <div class="row">
            <div class="col-lg-6" style="border-right: 1px dashed #ccc">
              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.salesCode") }}
              </label>
              <el-form-item>
                <el-autocomplete
                  clearable
                  v-model="newSalesGroupName"
                  :fetch-suggestions="querySalesGroupAsync"
                  @blur="getNewSalesAccount"
                  :disabled="
                    newAgentGroupName !== null && newAgentGroupName !== ''
                  "
                >
                  <template #append>
                    <el-button :icon="Search as any" />
                  </template>
                </el-autocomplete>
              </el-form-item>

              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.salesInfo") }}
              </label>
              <el-form-item>
                <el-input disabled v-model="newSalesAccount.name" />
              </el-form-item>
            </div>
            <div class="col-lg-6">
              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.ibGroup") }}
              </label>
              <el-form-item>
                <el-autocomplete
                  clearable
                  v-model="newAgentGroupName"
                  :fetch-suggestions="queryAgentGroupAsync"
                  @blur="getNewAgentAccount"
                >
                  <template #append>
                    <el-button :icon="Search as any" />
                  </template>
                </el-autocomplete>
              </el-form-item>

              <label class="required fs-6 fw-semobold mb-2 mt-4">
                {{ $t("fields.ibInfo") }}
              </label>
              <el-form-item>
                <el-input disabled v-model="newAgentAccount.name" />
              </el-form-item>

              <div
                v-if="accountDetails.role === AccountRoleTypes.IB"
                style="border-top: 1px dashed #ccc"
              >
                <label class="required fs-6 fw-semobold mb-2 mt-4">
                  {{ $t("fields.ibSelfGroup") + " (For IB's own group)" }}
                </label>
                <el-form-item props="ibSelfGroupName">
                  <el-input v-model="formData.name" />
                </el-form-item>
              </div>
            </div>
          </div>
          <div class="w-100 mt-5 d-flex justify-content-center">
            <el-button
              :loading="isSubmitting"
              type="primary"
              @click="submitSalesAndAgentReAssign"
              >{{ $t("action.submit") }}
            </el-button>
            <el-button @click="hide" plain>{{ $t("action.cancel") }}</el-button>
          </div>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { computed, inject, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";
import { Search } from "@element-plus/icons-vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { useI18n } from "vue-i18n";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const newAgentGroupName = ref(null);
const newSalesGroupName = ref(null);
const { t } = useI18n();
const account = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const accountDetails = ref(account);
const formData = ref<any>({ name: "" });

const validInput = (value: string) =>
  value !== "" && value !== undefined && value !== null;
const validateIbSelfGroup = async (value: any) => {
  isSubmitting.value = true;
  if (!validInput(value)) {
    isSubmitting.value = false;
    return false;
  }

  if (value?.length < 3) {
    MsgPrompt.warning(t("tip.ibGroupLengthShouldBeAtLeast3"));
    isSubmitting.value = false;
    return false;
  }

  const { data } = await AccountService.queryAccounts({
    group: value,
    roles: [AccountRoleTypes.IB, AccountRoleTypes.Broker],
  });

  const exists = data?.length > 0;

  if (exists) {
    MsgPrompt.warning(t("tip.ibGroupExists"));
    isSubmitting.value = false;
    return false;
  }

  isSubmitting.value = false;
  return true;
};

const emits = defineEmits<{
  (event: "re-assign-finished"): void;
}>();

const show = async () => {
  isLoading.value = true;
  modalRef.value?.show();

  if (!account) {
    MsgPrompt.error("Account not injected");
    return;
  }

  const { data } = await AccountService.queryAccounts({
    id: account.value.id,
  });
  if (data?.length === 0) {
    MsgPrompt.error("Account not found");
    return;
  }
  accountDetails.value = data[0];
  if (
    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
      accountDetails.value.role
    )
  ) {
    formData.value.name = accountDetails.value.group;
    formData.value.groupId = accountDetails.value.agentGroupId;
    formData.value.ownerAccountId = accountDetails.value.id;
  }
  clearNewSalesAccount();
  clearNewAgentAccount();

  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = async () => {
  try {
    //
  } catch (error) {
    MsgPrompt.error(error);
  }
  hide();
};

let prevAgentGroupStr = "";
let prevAgentGroupList = [];
const queryAgentGroupAsync = async (
  queryString: string,
  cb: (arg: any) => void
) => {
  if (prevAgentGroupStr === queryString) {
    cb(prevAgentGroupList);
    return;
  }

  const res = await AccountService.getFullAccountGroupNames(
    AccountGroupTypes.Agent,
    queryString == "null" ? "" : queryString
  );
  prevAgentGroupStr = queryString;
  prevAgentGroupList = res.map((item) => ({ value: item, label: item }));
  cb(prevAgentGroupList);
};

let prevSalesGroupStr = "";
let prevSalesGroupList = [];
const querySalesGroupAsync = async (
  queryString: string,
  cb: (arg: any) => void
) => {
  if (prevSalesGroupStr === queryString) {
    cb(prevSalesGroupList);
    return;
  }
  const res = await AccountService.getFullAccountGroupNames(
    AccountGroupTypes.Sales,
    queryString == "null" ? "" : queryString
  );
  prevSalesGroupStr = queryString;
  prevSalesGroupList = res.map((item) => ({ value: item, label: item }));
  cb(prevSalesGroupList);
};

const newSalesAccount = ref({} as any);
const getNewSalesAccount = async () => {
  if (newSalesGroupName.value === null || newSalesGroupName.value === "") {
    clearNewSalesAccount();
    clearNewAgentAccount();
    return;
  }
  const res = await AccountService.queryAccounts({
    role: AccountRoleTypes.Sales,
    code: newSalesGroupName.value,
  });
  if (res.data.length === 0) {
    MsgPrompt.error("Sales account not found");
    return;
  }
  newSalesAccount.value = res.data[0];
  clearNewAgentAccount();
};

const newAgentAccount = ref({} as any);
const getNewAgentAccount = async () => {
  if (newAgentGroupName.value === null || newAgentGroupName.value === "") {
    clearNewSalesAccount();
    clearNewAgentAccount();
    return;
  }
  const res = await AccountService.queryAccounts({
    role: AccountRoleTypes.IB,
    group: newAgentGroupName.value,
  });
  if (res.data.length === 0) {
    MsgPrompt.error("Agent account not found");
    return;
  }
  newAgentAccount.value = res.data[0];
  newSalesGroupName.value = newAgentAccount.value.code;
  newSalesAccount.value = {
    id: newAgentAccount.value.salesAccount.id,
    name: newAgentAccount.value.salesAccount.name,
  };
};

const clearNewSalesAccount = () => {
  newSalesAccount.value = {} as any;
  newSalesGroupName.value = null;
};

const clearNewAgentAccount = () => {
  newAgentAccount.value = {} as any;
  newAgentGroupName.value = null;
};

const isSubmitting = ref(false);
const submitSalesAndAgentReAssign = async () => {
  try {
    isSubmitting.value = true;
    let actions = "";
    if (shouldRemoveAgent.value) {
      await AccountService.removeAccountFromAgent(
        accountDetails.value.id,
        accountDetails.value.agentAccount.id
      );
      actions += "Remove agent => ";
    }

    if (shouldReAssignSales.value) {
      await AccountService.assignAccountToSales(
        accountDetails.value.id,
        newSalesAccount.value.id
      );
      actions += "Re-assign sales => ";
    }

    if (shouldReAssignAgent.value) {
      await AccountService.assignAccountToAgent(
        accountDetails.value.id,
        newAgentAccount.value.id
      );
      actions += "Re-assign agent & sales => ";
    }

    if (
      shouldRenameAgentSelfGroupName.value &&
      (await validateIbSelfGroup(formData.value.name))
    ) {
      await AccountService.renameSelfGroupName(
        formData.value.groupId,
        formData.value
      );
      actions += "Rename agent self group => ";
    }
    MsgPrompt.success(actions)
      .then(() => hide())
      .then(() => {
        emits("re-assign-finished");
      });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};

const shouldRemoveAgent = computed(
  () =>
    (newAgentGroupName.value === null || newAgentGroupName.value === "") &&
    accountDetails.value.agentAccount.id !== 0
);

const shouldReAssignSales = computed(
  () =>
    newSalesGroupName.value !== null &&
    newSalesGroupName.value !== "" &&
    newSalesAccount.value.id !== accountDetails.value.salesAccount.id
);

const shouldReAssignAgent = computed(
  () =>
    newAgentGroupName.value !== null &&
    newAgentGroupName.value !== "" &&
    newAgentAccount.value.id !== accountDetails.value.agentAccount.id
);

const shouldRenameAgentSelfGroupName = computed(
  () =>
    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
      accountDetails.value.role
    ) && accountDetails.value.group !== formData.value.name
);

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}
</style>
