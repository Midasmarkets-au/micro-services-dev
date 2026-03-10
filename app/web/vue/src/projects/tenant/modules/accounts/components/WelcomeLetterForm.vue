<template>
  <div>
    <div class="card">
      <!--      <div class="d-flex justify-content-end align-items-center mb-6">-->
      <div class="card-header">
        <div class="card-title">Welcome Letter Infos</div>
        <div class="card-toolbar">
          <span class="me-3 fw-semibold">{{ $t("action.attempted") }}</span>
          <span
            class="bg-primary text-white w-25px h-25px rounded d-flex justify-content-center align-items-center fw-semibold"
            >{{ 0 }}</span
          >
        </div>
      </div>

      <div class="card-body">
        <el-form
          ref="ruleFormRef"
          :model="ruleForm"
          :rules="rules"
          label-width="150px"
          class="demo-ruleForm"
          size="default"
          status-icon
        >
          <el-form-item label="Account" prop="account">
            <el-input v-model="ruleForm.account" />
          </el-form-item>

          <el-form-item label="Name" prop="name">
            <el-input v-model="ruleForm.name" />
          </el-form-item>

          <el-form-item label="Email" prop="email">
            <el-input v-model="ruleForm.email" />
          </el-form-item>

          <el-form-item label="Language" prop="language">
            <el-radio-group v-model="ruleForm.language">
              <el-radio
                v-for="item in languageSelections"
                :key="item.code"
                :label="item.code"
                >{{ item.name }}</el-radio
              >
            </el-radio-group>
          </el-form-item>

          <el-form-item label="Date" prop="date">
            <el-date-picker
              v-model="ruleForm.date"
              type="date"
              label="Pick a date"
              :placeholder="$t('action.select')"
              style="width: 100%"
            />
          </el-form-item>

          <el-form-item label="Read Only" prop="readOnly">
            <el-input v-model="ruleForm.readOnly">
              <template #append>
                <el-button @click="ruleForm.readOnly = ''">{{
                  $t("action.reset")
                }}</el-button>
              </template></el-input
            >
          </el-form-item>

          <el-form-item label="Phone Password" prop="phonePassword">
            <el-input v-model="ruleForm.phonePassword">
              <template #append>
                <el-button @click="ruleForm.phonePassword = ''">{{
                  $t("action.reset")
                }}</el-button>
              </template></el-input
            >
          </el-form-item>

          <el-form-item label="Other Emails" prop="otherEmails">
            <el-input v-model="ruleForm.otherEmails" />
          </el-form-item>

          <el-form-item>
            <el-button type="primary" @click="submitForm(ruleFormRef)"
              >Submit</el-button
            >
            <el-button>Preview</el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, inject, Ref } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { LanguageTypes } from "@/core/types/LanguageTypes";

const emits = defineEmits<{
  (e: "submit"): void;
}>();

const languageSelections = ref(LanguageTypes.all);

const getUserInfos = inject<() => any>(
  AccountInjectionKeys.GET_USER_INFOS,
  () => ({})
);
const userInfos = ref<any>({});

const accountDetails = inject<Ref>(
  AccountInjectionKeys.ACCOUNT_DETAILS,
  ref<any>({})
);

const ruleFormRef = ref<FormInstance>();
const ruleForm = ref({});

const rules = reactive<FormRules>({
  account: [
    {
      required: true,
      message: "Please input  account",
      trigger: "blur",
    },
  ],

  name: [
    {
      required: true,
      message: "Please input name",
      trigger: "blur",
    },
  ],

  email: [
    {
      required: true,
      message: "Please input email",
      trigger: "blur",
    },
  ],

  language: [
    {
      required: true,
      message: "Please select language",
      trigger: "change",
    },
  ],

  date: [
    {
      type: "date",
      required: true,
      message: "Please pick a date",
      trigger: "change",
    },
  ],

  readOnly: [
    {
      required: true,
      message: "Please input read only",
      trigger: "blur",
    },
  ],

  phonePassword: [
    {
      required: true,
      message: "Please input read only",
      trigger: "blur",
    },
  ],
});

const submitForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate((valid, fields) => {
    if (valid) {
      console.log("submit!");
      emits("submit");
    } else {
      console.log("error submit!", fields);
    }
  });
};

const resetForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  userInfos.value = await getUserInfos();
  ruleForm.value = {
    account: accountDetails.value.tradeAccount.accountNumber,
    name: `${userInfos.value.firstName} ${userInfos.value.lastName}`,
    email: userInfos.value.email,
    language: userInfos.value.language,
    date: "",
    password: "SDFdfaD11取上一步",
    readOnly: "DFdfa",
    phonePassword: "1231",
    deposit: "",
    otherEmails: "",
    isSend2These: false,
  };
  formEl.resetFields();
};

onMounted(() => {
  console.log(LanguageTypes.all);
  resetForm(ruleFormRef.value);
});
</script>

<style scoped></style>
