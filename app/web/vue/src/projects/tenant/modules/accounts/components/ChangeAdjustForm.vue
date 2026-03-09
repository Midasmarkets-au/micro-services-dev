<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeAdjust')"
    :is-loading="isLoading"
    :submit="submit"
    disable-submit
    style="width: 380px"
  >
    <h1>TBD....</h1>
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-width="40px"
          class="demo-ruleForm"
          status-icon
        >
          <el-form-item label="Login" prop="login">
            <el-input placeholder="Input Login" v-model="formData.login" />
          </el-form-item>
          <el-form-item label="Amount" prop="amount">
            <el-input v-model="formData.amount" />
          </el-form-item>
          <el-form-item label="Remark" prop="remark">
            <el-input v-model="formData.remark" />
          </el-form-item>
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { inject, reactive, ref } from "vue";
import { backendConfigLeverageSelections } from "@/core/types/LeverageTypes";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { FormInstance, FormRules } from "element-plus";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  login: [{ required: true, message: "Please input Login", trigger: "blur" }],
  amount: [{ required: true, message: "Please input Amount", trigger: "blur" }],
  remark: [{ required: true, message: "Please input Remark", trigger: "blur" }],
});

const formData = ref<any>({});

const show = async () => {
  isLoading.value = true;
  modalRef.value?.show();
  resetForm();
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  await ruleFormRef.value.validate((valid, fields) => {
    if (valid) {
      try {
        //
        MsgPrompt.success("success");
      } catch (error) {
        MsgPrompt.error(error);
      }
      hide();
    } else {
      console.log("error submit!", fields);
    }
  });
};

const resetForm = () => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
};

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
