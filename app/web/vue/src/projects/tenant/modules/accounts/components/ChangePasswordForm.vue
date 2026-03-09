<template>
  <SimpleForm
    ref="modalRef"
    :title="
      $t(
        formData.mode === 'reset'
          ? 'action.resetToInitialPassword'
          : formData.type === 'trade'
          ? 'action.changeTradePassword'
          : formData.type === 'readonly'
          ? 'action.changeInvestorPassword'
          : formData.type === 'login'
          ? 'action.changeLoginUserPassword'
          : 'action.changePassword'
      ) + (userEmail ? '(' + userEmail + ')' : '')
    "
    :is-loading="isLoading"
    :disable-submit="!isEmailConfirmed"
    :submit="submitForm"
  >
    <el-form
      v-if="formData.mode !== 'reset'"
      ref="ruleFormRef"
      :model="formData"
      status-icon
      :rules="rules"
      label-width="120px"
      class="demo-ruleForm"
    >
      <el-form-item :label="$t('fields.password')" prop="pass">
        <el-input
          v-model="formData.pass"
          :type="passwordVisible ? 'text' : 'password'"
          name="new-password"
          autocomplete="new-password"
          required
        />
        <!-- 密码要求提示 -->
        <div class="password-requirements mt-2">
          <div
            v-for="(requirement, index) in passwordRequirements"
            :key="index"
            class="requirement-item"
            :class="{ valid: requirement.test(formData.pass) }"
          >
            <span class="requirement-icon">
              {{ requirement.test(formData.pass) ? "✓" : "○" }}
            </span>
            <span class="requirement-text">{{ requirement.label }}</span>
          </div>
        </div>
      </el-form-item>
      <el-form-item :label="$t('fields.confirmPassword')" prop="checkPass">
        <el-input
          v-model="formData.checkPass"
          :type="passwordVisible ? 'text' : 'password'"
          name="confirm-password"
          autocomplete="new-password"
          required
        />
      </el-form-item>
      <el-form-item :label="$t('fields.reason')">
        <el-input
          v-model="formData.reason"
          type="textarea"
          :rows="3"
          :placeholder="$t('tip.reasonOptional')"
        />
      </el-form-item>
    </el-form>
    <!-- Reset 模式：只显示原因输入框 -->
    <el-form v-else label-width="120px" class="demo-ruleForm">
      <el-alert
        :title="$t('tip.resetPasswordWarning')"
        type="warning"
        show-icon
        :closable="false"
        class="mb-4"
      />
      <el-form-item :label="$t('fields.reason')" required>
        <el-input
          v-model="formData.reason"
          type="textarea"
          :rows="3"
          :placeholder="$t('tip.reasonRequired')"
        />
      </el-form-item>
    </el-form>

    <!-- 确认邮箱区域 - 独立样式 -->
    <div class="email-confirmation-section">
      <!-- <div class="confirmation-header">
        <i class="el-icon-warning-outline"></i>
        <span class="confirmation-title">{{ $t("fields.confirmEmail") }}</span>
      </div> -->
      <div class="confirmation-description">
        {{ $t("tip.typeEmailToConfirm", { email: userEmail }) }}
      </div>
      <el-input
        v-model="formData.email"
        :placeholder="userEmail"
        autocomplete="off"
        size="large"
        class="confirmation-input"
      >
        <template #suffix>
          <i
            v-if="isEmailConfirmed"
            class="el-icon-circle-check"
            style="color: #67c23a"
          ></i>
        </template>
      </el-input>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive, inject, computed } from "vue";
import i18n from "@/core/plugins/i18n";
import type { FormInstance, FormRules } from "element-plus";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ElNotification } from "element-plus";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
// import SvgIcon from "@/projects/tenant/components/SvgIcon.vue";
// const passwordShow_1 = ref(false);
const ruleFormRef = ref<FormInstance>();
const t = i18n.global.t;
const userEmail = inject(AccountInjectionKeys.USER_EMAIL, ref(""));

const formData = ref({
  pass: "",
  checkPass: "",
  type: "",
  email: "",
  id: 0,
  mode: "change", // 'change' or 'reset'
  reason: "",
});

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const tradeAccount = ref<any>();
const passwordVisible = ref(true); // 默认显示密码

// 检查邮箱是否匹配
const isEmailConfirmed = computed(() => {
  return (
    formData.value.email.trim() !== "" &&
    formData.value.email.trim() === userEmail.value.trim()
  );
});

// 密码要求配置
const passwordRequirements = [
  {
    label: t("error.signup_ps_8"), // 至少8个字符
    test: (val: string) => val.length >= 8,
  },
  {
    label: t("error.signup_ps_upper"), // 必须包含大写字母
    test: (val: string) => /[A-Z]/.test(val),
  },
  {
    label: t("error.signup_ps_lower"), // 必须包含小写字母
    test: (val: string) => /[a-z]/.test(val),
  },
  {
    label: t("error.signup_ps_symbol"), // 必须包含特殊符号
    test: (val: string) => /[@#$%^&*!]/.test(val),
  },
];

const validatePass = (rule: any, value: any, callback: any) => {
  if (value === "") {
    callback(new Error(t("error.signup_ps_require")));
    return;
  }

  // 检查所有密码要求
  const failedRequirements = passwordRequirements.filter(
    (req) => !req.test(value)
  );

  if (failedRequirements.length > 0) {
    // 返回第一个未满足的要求
    callback(new Error(failedRequirements[0].label));
    return;
  }

  // 如果确认密码已输入，触发确认密码验证
  if (formData.value.checkPass !== "") {
    if (!ruleFormRef.value) return;
    ruleFormRef.value.validateField("checkPass", () => null);
  }

  callback();
};
const validatePass2 = (rule: any, value: any, callback: any) => {
  if (value === "") {
    callback(new Error(t("error.signup_ps_require")));
  } else if (value !== formData.value.pass) {
    callback(new Error(t("error.signup_ps_match")));
  } else {
    callback();
  }
};

const rules = reactive<FormRules>({
  pass: [{ required: true, validator: validatePass, trigger: "blur" }],
  checkPass: [{ required: true, validator: validatePass2, trigger: "blur" }],
});

const show = (
  _tradeAccount: any,
  _id: number,
  type: string,
  mode = "change"
) => {
  isLoading.value = true;
  resetForm();
  modalRef.value?.show();
  tradeAccount.value = _tradeAccount;
  formData.value.type = type;
  formData.value.mode = mode;
  formData.value.email = "";
  formData.value.reason = "";
  // 显式清空密码字段，防止浏览器自动填充
  formData.value.pass = "";
  formData.value.checkPass = "";
  formData.value.id = _id;
  isLoading.value = false;
  passwordVisible.value = true;
  console.log("formData", formData.value);
};

const hide = () => {
  resetForm();
  modalRef.value?.hide();
};

const resetForm = () => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
};

const submitForm = async () => {
  try {
    // Reset 模式：只需验证 reason 和 email
    if (formData.value.mode === "reset") {
      if (!formData.value.reason || formData.value.reason.trim() === "") {
        ElNotification({
          message: t("tip.reasonRequired"),
          type: "warning",
        });
        return false;
      }

      // 调用重置到初始密码的 API
      await AccountService.resetToInitial(formData.value.id, {
        passwordType: formData.value.type,
        reason: formData.value.reason,
      });

      ElNotification({
        message: t("tip.passwordResetSuccess"),
        type: "success",
      });

      hide();
      return true;
    }

    // Change 模式：需要验证密码表单
    if (!ruleFormRef.value) return;

    const valid = await ruleFormRef.value.validate();
    if (!valid) {
      console.log("error submit!");
      return false;
    }

    console.log("submit!");
    console.log("tradeAccount", tradeAccount);

    // 准备提交数据，不包含 email（仅用于前端确认）
    const submitData = {
      newPassword: formData.value.pass,
      reason: formData.value.reason || undefined,
    };

    if (formData.value.type === "main" || formData.value.type === "investor") {
      await AccountService.changeTrade(
        formData.value.id,
        formData.value.type,
        submitData
      );
    } else {
      await AccountService.changeCrm(formData.value.id, submitData);
    }

    ElNotification({
      message: t("tip.passwordResetSuccess"),
      type: "success",
    });

    hide();
  } catch (error) {
    // 验证失败或 API 调用失败都会进入这里
    if (error && typeof error === "object" && "message" in error) {
      // API 错误
      MsgPrompt.error(error);
    } else {
      // 表单验证失败
      console.log("Form validation failed", error);
    }
  }
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

// 密码要求提示样式
.password-requirements {
  padding: 8px 12px;
  background-color: #f5f7fa;
  border-radius: 4px;
  font-size: 12px;

  .requirement-item {
    display: flex;
    align-items: center;
    color: #909399;
    transition: all 0.3s ease;

    &.valid {
      color: #67c23a;

      .requirement-icon {
        color: #67c23a;
        font-weight: bold;
      }
    }

    .requirement-icon {
      margin-right: 8px;
      font-size: 14px;
      font-weight: bold;
      width: 16px;
      display: inline-block;
      text-align: center;
    }

    .requirement-text {
      flex: 1;
    }
  }
}

// 确认邮箱区域样式
.email-confirmation-section {
  margin-top: 24px;
  padding: 20px;
  background: linear-gradient(135deg, #fff5e6 0%, #ffe8cc 100%);
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(255, 169, 64, 0.15);

  .confirmation-header {
    display: flex;
    align-items: center;
    margin-bottom: 8px;

    i {
      font-size: 20px;
      color: #fa8c16;
      margin-right: 8px;
    }

    .confirmation-title {
      font-size: 16px;
      font-weight: 600;
      color: #d46b08;
    }
  }

  .confirmation-description {
    font-size: 13px;
    color: #8c5100;
    margin-bottom: 12px;
    line-height: 1.5;
  }

  .confirmation-input {
    :deep(.el-input__inner) {
      border: 2px solid #ffa940;
      background-color: #fff;

      &:focus {
        border-color: #fa8c16;
        box-shadow: 0 0 0 2px rgba(255, 169, 64, 0.2);
      }
    }
  }
}
</style>
