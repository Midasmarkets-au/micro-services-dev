<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.transferOut')"
    :is-loading="isLoading"
    :submit="submit"
    :reset="hide"
    :disable-submit="checked"
  >
    <table
      v-if="isLoading"
      class="table align-middle table-row-bordered gy-5 mb-0"
      id="kt_permissions_table"
    >
      <tbody>
        <LoadingRing />
      </tbody>
    </table>

    <el-form v-else :rules="rules" :model="formData" ref="formRef">
      <h2 class="text-gray-600 fw-semobold fs-2 p-0 mb-6">
        {{ $t("tip.maxTransferOut") + ": " }}
        <BalanceShow
          :balance="currentBalance * 100"
          :currency-id="currencyId"
        />
      </h2>

      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("title.targetAccount") }}
      </label>
      <el-form-item prop="targetTradeAccountUid">
        <el-select v-model="formData.targetTradeAccountUid">
          <el-option
            v-for="item in accountList"
            :key="item.tradeAccount.accountNumber"
            :label="item.tradeAccount.accountNumber"
            :value="item.tradeAccount.uid"
          >
            <div style="display: flex; align-items: center; gap: 8px">
              {{ item.tradeAccount.accountNumber }}&nbsp;({{
                $t("type.currency." + item.tradeAccount.currencyId)
              }})&nbsp;&nbsp;&nbsp;
              <BalanceShow
                :currency-id="item.tradeAccount.currencyId"
                :balance="item.tradeAccount.balanceInCents"
              />
            </div>
          </el-option>
        </el-select>
      </el-form-item>

      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("title.transferOutAmount") }}
      </label>
      <el-form-item prop="amount">
        <el-input
          v-model="formData.amount"
          :placeholder="$t('tip.pleaseInput')"
        />
        <div class="fv-plugins-message-container">
          <p v-if="showTips && tips" class="fv-help-block ml-2">
            <BalanceShow
              :currency-id="tips.currencyId"
              :balance="tips.amount"
            />
          </p>
        </div>
      </el-form-item>

      <!-- <label class="fs-6 fw-semobold mb-2">
        {{ $t("fields.remark") }}
      </label>
      <el-form-item>
        <el-input
          v-model="formData.comment"
          :placeholder="$t('tip.pleaseInput')"
        />
      </el-form-item> -->

      <!-- Verification Code - Only show when config enabled and amount is not empty -->
      <div v-if="showVerificationCode" class="mb-5">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.oneTimeCode") }}
        </label>
        <div class="d-flex align-items-center gap-3 w-100">
          <el-input
            v-model="verificationCode"
            :placeholder="$t('fields.enterYourCode')"
            class="flex-grow-1"
          />

          <button
            type="button"
            class="btn btn-primary"
            :disabled="isSendingCode || countdown > 0"
            @click="sendVerificationCode"
            style="min-width: 120px"
          >
            <span v-if="countdown > 0">
              {{ Math.floor(countdown / 60) }}:{{
                String(countdown % 60).padStart(2, "0")
              }}
            </span>
            <span v-else>
              {{ $t("action.sendCode") }}
            </span>
          </button>
        </div>

        <p v-if="codeSent" class="text-success mt-2 mb-0">
          {{ $t("tip.codeSentToEmail") }}
        </p>
      </div>

      <el-form-item>
        <el-checkbox
          v-model="reverseCheck"
          :label="$t('tip.transferAgreement')"
          size="large"
        />
      </el-form-item>
    </el-form>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, watch, computed, onUnmounted } from "vue";
import AccountService from "../../services/AccountService";
import { FormInstance, FormRules } from "element-plus";
import BalanceShow from "@/components/BalanceShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import i18n from "@/core/plugins/i18n";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { number } from "yup";
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import store from "@/store";
const emits = defineEmits<{
  (e: "onCreated"): void;
}>();
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const formRef = ref<FormInstance>();
const { t } = i18n.global;

const reverseCheck = ref(false);
const checked = ref(true);
const isLoading = ref(true);
const isSubmitting = ref(false);
const accountNumber = ref(0);
const currentBalance = ref(0);
const currencyId = ref(840);
const fundType = ref(-1);
const accountList = ref([] as any);
const sourceAccount = ref({} as any);
const formData = ref({
  amount: null,
  sourceTradeAccountUid: 0,
  targetTradeAccountUid: t("tip.selectAccount"),
  comment: null,
});

// 验证码相关
const isSendingCode = ref(false);
const codeSent = ref(false);
const countdown = ref(0);
const verificationCode = ref("");
let countdownTimer: number | null = null;

// 是否显示验证码输入框 - 根据配置决定
const showVerificationCode = computed(() => {
  // 只有金额不为空且配置开启时才显示
  return (
    formData.value.amount &&
    store.state.AuthModule.config.twoFactorAuthForTransactions
      ?.tradeAccountToTradeAccount === true
  );
});
// 重置验证码状态
const resetVerificationCode = () => {
  codeSent.value = false;
  countdown.value = 0;
  verificationCode.value = "";
  if (countdownTimer) {
    clearInterval(countdownTimer);
    countdownTimer = null;
  }
};

// 开始倒计时
const startCountdown = (expires: number) => {
  countdown.value = expires;
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
  countdownTimer = setInterval(() => {
    countdown.value--;
    if (countdown.value <= 0) {
      if (countdownTimer) {
        clearInterval(countdownTimer);
        countdownTimer = null;
      }
    }
  }, 1000) as unknown as number;
};

// 发送验证码
const sendVerificationCode = async () => {
  if (countdownTimer) {
    return;
  }
  isSendingCode.value = true;
  try {
    const params = {
      authType: "TradeAccountToTradeAccount",
    };
    const res = await WalletService.sendTransferVerificationCode(params);
    codeSent.value = true;
    startCountdown(res.expiresIn);
    MsgPrompt.success(t("tip.codeSentToEmail"));
  } catch (error) {
    console.error("Send verification code error:", error);
    MsgPrompt.error(error);
  } finally {
    isSendingCode.value = false;
  }
};

const getCurentInfo = (
  currentAccountId?: number | null | undefined | string
) => {
  const currentUid = currentAccountId
    ? currentAccountId
    : formData.value.targetTradeAccountUid;
  const currentAccount = accountList.value.find(
    (item) => item.accountNumber == currentUid
  );
  return currentAccount;
};
const showTips = computed(() => {
  if (
    formData.value.targetTradeAccountUid &&
    formData.value?.amount &&
    !isNaN(formData.value?.amount)
  ) {
    return true;
  }
  return false;
});
const tips = computed(() => {
  const targetAccount = getCurentInfo(formData.value?.targetTradeAccountUid);
  if (currencyId.value !== targetAccount.currencyId) {
    const percent = targetAccount.currencyId == CurrencyTypes.USC ? 10000 : 1;
    let res = {
      currencyId: targetAccount?.currencyId,
      amount: (formData.value?.amount ?? 0) * percent,
    };
    return res;
  }
  return null;
});
const show = async (
  _accountList: any[],
  _accountNumber: number,
  _uid: number,
  _currencyId: number,
  _fundType: number,
  _currentBalance: number,
  _sourceAccount?: any
) => {
  accountList.value = _accountList;
  sourceAccount.value = _sourceAccount;
  // showModal(newTargetModalRef.value);
  modalRef.value?.show();
  isLoading.value = true;
  try {
    formData.value.sourceTradeAccountUid = _uid;
    if (_sourceAccount) {
      currentBalance.value =
        sourceAccount.value.tradeAccount.equity -
        sourceAccount.value.tradeAccount.credit -
        sourceAccount.value.tradeAccount.margin;
      currentBalance.value =
        currentBalance.value <= 0 ? 0 : currentBalance.value;
      console.log("currentBalance", currentBalance.value);
    } else {
      currentBalance.value = _currentBalance;
    }

    currencyId.value = _currencyId;
    fundType.value = _fundType;
  } catch (error) {
    MsgPrompt.error(t("tip.fail"));
  } finally {
    isLoading.value = false;
  }
  accountNumber.value = _accountNumber;
};

const hide = () => {
  modalRef.value?.hide();
  formRef.value?.resetFields();
  resetVerificationCode();
};

const submit = async () => {
  if (!formRef.value) {
    return;
  }

  // 只有显示验证码时才进行验证码验证
  if (showVerificationCode.value) {
    if (!verificationCode.value || verificationCode.value.trim() === "") {
      MsgPrompt.error(t("tip.verificationCodeRequired"));
      return;
    }
  }

  isSubmitting.value = true;
  let isValid = false;

  await formRef.value.validate((valid) => (isValid = valid));

  if (isValid && formData.value.amount) {
    try {
      await AccountService.submitTransferOutToAccountRequest({
        ...formData.value,
        amount: formData.value.amount * 100,
        verificationCode: verificationCode.value,
      });
      MsgPrompt.success(t("tip.transferOutSubmit")).then(() => {
        hide();
        emits("onCreated");
      });
    } catch (error) {
      MsgPrompt.error(error);
    } finally {
      isSubmitting.value = false;
    }
  } else {
    MsgPrompt.warning(t("tip.validationError"));
  }
};

watch(reverseCheck, (value) => {
  checked.value = value === true ? false : true;
});

// 清理定时器
onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
});

const validateAmount = (rule: any, value: any, callback: any) => {
  if (value === "") {
    callback(new Error(t("tip.pleaseInputTheAmount")));
    return;
  }
  if (value <= 0) {
    callback(new Error(t("tip.amountGreaterThanZero")));
    return;
  }
  if (value > currentBalance.value) {
    callback(new Error(t("tip.amountMustBeLessThanBalance")));
    return;
  }
  if (isNaN(value)) {
    callback(new Error(t("tip.pleaseEnterAValidNumber")));
    return;
  }
  if (!/^\d+(\.\d{1,2})?$/.test(value)) {
    callback(new Error(t("tip.upToTwoDecimalPlaces")));
    return;
  }
  callback();
};

const rules = ref<FormRules>({
  amount: [{ validator: validateAmount, trigger: "blur" }],
});

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
.el-checkbox__label {
  text-wrap: wrap;
}
</style>
