<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeSalesOrIb')"
    :is-loading="isLoading"
    :width="450"
    disable-footer
  >
    <div>
      <div class="w-100 row overflow-hidden">
        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.salesCode") }}
          </label>

          <div class="d-flex">
            <el-form-item>
              <el-input disabled v-model="formData.code" />
            </el-form-item>
            <div class="ms-5">
              <el-button type="primary" @click="newSalesInfo">{{
                $t("action.change")
              }}</el-button>
            </div>
          </div>
        </div>

        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.salesInfo") }}
          </label>
          <el-form-item>
            <el-input disabled v-model="formData.salesName" />
          </el-form-item>
        </div>
        <el-divider></el-divider>
        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.ibGroup") }}
          </label>

          <div class="d-flex">
            <el-form-item>
              <el-input disabled v-model="formData.group" />
            </el-form-item>
            <div class="ms-5">
              <el-button type="primary" @click="newAgentInfo">{{
                $t("action.change")
              }}</el-button>
              <el-button @click="openConfirmBoxModal">{{
                $t("action.remove")
              }}</el-button>
            </div>
          </div>
        </div>

        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.ibInfo") }}
          </label>
          <el-form-item>
            <el-input disabled v-model="formData.agentName" />
          </el-form-item>
        </div>
      </div>

      <SimpleForm
        ref="newAgentInfoRef"
        :title="$t('fields.newIbGroup')"
        :disableSubmit="!ibGroupExist"
        :is-loading="false"
        :submit="submit"
        width="300"
      >
        <div class="mb-5">
          <label class="required fs-6 mb-4"
            >{{ $t("fields.newIbGroup") }}
          </label>

          <div class="d-flex align-items-center">
            <div>
              <el-form-item style="margin: 0">
                <el-input
                  v-model="newAgentAccount.group"
                  @keyup="getIbAccountInfo"
                />
              </el-form-item>
            </div>
            <i
              v-if="ibGroupExist"
              class="fa-regular fa-xl fa-circle-check ms-3"
              style="color: #14a44d"
            ></i>
            <i
              v-else
              class="fa-regular fa-xl fa-circle-xmark ms-3"
              style="color: #900000"
            ></i>
          </div>
        </div>
        <div>
          <label class="required fs-6 mb-4">{{ $t("fields.name") }} </label>

          <el-form-item>
            <el-input disabled v-model="newAgentAccount.name" />
          </el-form-item>
        </div>
      </SimpleForm>

      <SimpleForm
        ref="newSalesInfoRef"
        :title="$t('fields.newSalesCode')"
        :disableSubmit="!salesCodeExist"
        :is-loading="false"
        :submit="submitSalesChange"
        width="300"
      >
        <div class="mb-5">
          <label class="required fs-6 mb-4"
            >{{ $t("fields.newSalesCode") }}
          </label>
          <div class="d-flex align-items-center">
            <div>
              <el-form-item style="margin: 0">
                <el-input
                  v-model="newSalesAccount.code"
                  @keyup="getSalesAccountInfo"
                />
              </el-form-item>
            </div>
            <i
              v-if="salesCodeExist"
              class="fa-regular fa-xl fa-circle-check ms-3"
              style="color: #14a44d"
            ></i>
            <i
              v-else
              class="fa-regular fa-xl fa-circle-xmark ms-3"
              style="color: #900000"
            ></i>
          </div>
        </div>
        <div>
          <label class="required fs-6 mb-4">{{ $t("fields.name") }} </label>

          <el-form-item>
            <el-input disabled v-model="newSalesAccount.name" />
          </el-form-item>
        </div>
      </SimpleForm>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { inject, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const emits = defineEmits<{
  (e: "update"): void;
}>();
const isLoading = ref(true);

const ibGroupExist = ref(false);
const salesCodeExist = ref(false);

const modalRef = ref<InstanceType<typeof SimpleForm>>();
const formData = ref({} as any);
const accountDetails = ref({} as any);

const newAgentAccount = ref({} as any);
const newSalesAccount = ref({} as any);
const show = async (details: any) => {
  const res = await AccountService.queryAccounts({ id: details.id });
  accountDetails.value = res.data[0];
  console.log({ ...accountDetails.value });
  formData.value.group = accountDetails.value.group;
  formData.value.code = accountDetails.value.code;
  formData.value.salesName = accountDetails.value.salesAccount.name || "***";
  formData.value.agentName = accountDetails.value.agentAccount.name || "***";
  modalRef.value?.show();
  isLoading.value = false;
  // formData.value.group = accountDetails.modalRef.value?.show();
};

const newAgentInfoRef = ref<InstanceType<typeof SimpleForm>>();
const newSalesInfoRef = ref<InstanceType<typeof SimpleForm>>();

const newAgentInfo = async () => {
  newAgentInfoRef.value?.show();
};

const newSalesInfo = async () => {
  newSalesInfoRef.value?.show();
};

const getIbAccountInfo = async () => {
  if (!newAgentAccount.value.group || newAgentAccount.value.group === "")
    return;
  try {
    const res = await AccountService.queryAccounts({
      group: newAgentAccount.value.group,
      role: AccountRoleTypes.IB,
    });

    if (res.data.length > 1 || res.data.length == 0) {
      newAgentAccount.value.name = "";
      ibGroupExist.value = false;
      return;
    }

    const account = res.data[0];
    newAgentAccount.value.name =
      account.name || "IB account valid but has no name";
    newAgentAccount.value.code = account.code;
    newAgentAccount.value.agentId = account.id;
    ibGroupExist.value = true;
  } catch (e) {
    console.log(e);
    newAgentAccount.value.name = "";
    newAgentAccount.value.code = "";
    ibGroupExist.value = false;
  }
};

const getSalesAccountInfo = async () => {
  if (!newSalesAccount.value.code || newSalesAccount.value.code === "") return;
  try {
    const res = await AccountService.queryAccounts({
      code: newSalesAccount.value.code,
      role: AccountRoleTypes.Sales,
    });

    if (res.data.length > 1 || res.data.length == 0) {
      newSalesAccount.value.name = "";
      salesCodeExist.value = false;
      return;
    }

    const account = res.data[0];
    newSalesAccount.value.name =
      account.name || "Sales account valid but has no name";
    newSalesAccount.value.code = account.code;
    newSalesAccount.value.salesId = account.id;
    salesCodeExist.value = true;
  } catch (e) {
    console.log(e);
    newSalesAccount.value.name = "";
    newSalesAccount.value.code = "";
    salesCodeExist.value = false;
  }
};

const submit = async () => {
  if (newAgentAccount.value.group === accountDetails.value.group) {
    MsgPrompt.info("New IB group is the same as the old one");
    return;
  }
  try {
    await AccountService.changeAgentGroup(
      accountDetails.value.id,
      newAgentAccount.value.agentId
    );
    emits("update");
    newAgentInfoRef.value?.hide();
    modalRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  }
};

const submitSalesChange = async () => {
  if (newSalesAccount.value.code === accountDetails.value.code) {
    MsgPrompt.info("New Sales group is the same as the old one");
    return;
  }
  try {
    await AccountService.changeSalesGroup(
      accountDetails.value.id,
      newSalesAccount.value.salesId
    );
    emits("update");
    newSalesInfoRef.value?.hide();
    modalRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  }
};

const openConfirmBoxHandler = inject(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const openConfirmBoxModal = async () => {
  try {
    const res = await AccountService.queryAccounts({
      group: accountDetails.value.group,
      role: AccountRoleTypes.IB,
    });
    const currentAgentAccount = res.data[0];

    openConfirmBoxHandler?.(() =>
      AccountService.removeAgentGroup(
        accountDetails.value.id,
        currentAgentAccount.id
      )
        .then(() => {
          emits("update");
          modalRef.value?.hide();
        })
        .catch((e) => MsgPrompt.error(e))
    );
  } catch (e) {
    MsgPrompt.error("Current agent account not found");
  }
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>

<style scoped></style>
