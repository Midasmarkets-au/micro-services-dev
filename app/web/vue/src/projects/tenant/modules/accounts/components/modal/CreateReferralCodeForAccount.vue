<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('fields.referralCode')"
    :is-loading="isLoading"
    :submit="submit"
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="160px"
      class="demo-ruleForm"
      status-icon
    >
      <el-form-item label="Service Type" prop="serviceType">
        <el-select v-model="formData.serviceType">
          <el-option key="IB" label="IB" :value="300" />
          <el-option key="Client" label="Client" :value="400" />
        </el-select>
      </el-form-item>

      <el-form-item label="Name" prop="name">
        <el-input v-model="formData.name" />
      </el-form-item>

      <el-form-item label="Summary" prop="summary">
        <el-input type="textarea" v-model="formData.summary" />
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import { inject, ref } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const modalRef = ref();
const ruleFormRef = ref();
const isLoading = ref(true);
const formData = ref({} as any);
const accountDetails = inject<any>(AccountInjectionKeys.ACCOUNT_DETAILS, {});

const emits = defineEmits(["fetchData"]);

const resetForm = async () => {
  formData.value = {
    partyId: accountDetails.value.partyId,
    accountId: accountDetails.value.id,
    name: "",
    serviceType: "",
  };

  ruleFormRef.value?.resetFields();
};

const rules = ref({
  name: [
    {
      required: true,
      message: "Please input code name",
      trigger: "blur",
    },
  ],
  serviceType: [
    {
      required: true,
      message: "Please select service type",
      trigger: "change",
    },
  ],
});

const submit = async () => {
  if (!formData.value.name || !formData.value.serviceType) {
    MsgPrompt.warning("Please fill the required fields");
    return;
  }

  if (formData.value.summary && formData.value.summary != "") {
    try {
      JSON.parse(formData.value.summary);
      formData.value.summary = JSON.stringify(
        JSON.parse(formData.value.summary)
      );
    } catch (error) {
      MsgPrompt.warning("Invalid JSON format");
      return;
    }
  } else {
    delete formData.value.summary;
  }

  try {
    isLoading.value = true;
    await AccountService.addReferralCode({
      ...formData.value,
    });

    MsgPrompt.success("ReferralCode created successfully").then(() => {
      hide();
      emits("fetchData");
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
const show = async () => {
  await resetForm();
  modalRef.value?.show();
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped></style>
