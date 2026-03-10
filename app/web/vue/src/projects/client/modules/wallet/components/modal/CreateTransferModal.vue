<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.transferOut')"
    :is-loading="isLoading"
    :submit="postTransferRequest"
    :disable-submit="checked"
  >
    <form
      class="form"
      id="kt_modal_new_address_form"
      @submit.prevent="postTransferRequest"
    >
      <div id="kt_modal_new_address_scroll">
        <!-- Tabs for IB/Sales users -->
        <div v-if="showTransferTabs" class="mb-5">
          <el-tabs v-model="currentTab">
            <el-tab-pane
              :label="$t('fields.transactionTypeTransfer')"
              name="self"
            ></el-tab-pane>
            <el-tab-pane
              :label="$t('action.transferToOthers')"
              name="others"
            ></el-tab-pane>
          </el-tabs>
        </div>

        <!--------------------------------------------------------------  Select Wallet -->
        <div>
          <div class="d-flex flex-column">
            <div class="d-flex flex-column mb-5 fv-row">
              <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
                {{ $t("fields.wallet") }}:
              </label>

              <Field
                name="walletId"
                class="form-select form-select-solid"
                as="select"
                v-model.number="TransferRequireData.walletId"
                :disabled="isWalletIdExist"
                @change="changeWallet"
              >
                <option value="">
                  {{ $t("tip.selectAccount") }}
                </option>
                <option
                  v-for="({ id, currencyId, balance }, index) in wallets"
                  :key="index"
                  :value="id"
                >
                  {{ $t("type.currency." + currencyId) }}&nbsp;&nbsp;&nbsp;
                  <BalanceShow :currency-id="currencyId" :balance="balance" />
                </option>
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="walletId" />
                </div>
              </div>
            </div>

            <!-- Select Payment Method -->
            <div class="d-flex flex-column mb-5 fv-row">
              <label
                class="d-flex align-items-center fs-5 fw-semobold mb-2 required"
              >
                {{
                  currentTab === "self"
                    ? $t("fields.targetTradeAccount")
                    : $t("fields.targetAccount")
                }}
              </label>

              <!-- 转账给自己 - 下拉选择 -->
              <template v-if="currentTab === 'self'">
                <Field
                  name="tradeAccountUid"
                  class="form-select form-select-solid"
                  as="select"
                  v-model.number="TransferRequireData.tradeAccountUid"
                >
                  <option value="">
                    {{ $t("tip.selectAccount") }}
                  </option>
                  <option
                    v-for="item in availableTradeAccounts"
                    :key="item.uid"
                    :value="item.uid"
                  >
                    {{ item.accountNumber }}&nbsp;({{
                      $t("type.currency." + item.currencyId)
                    }})&nbsp;&nbsp;&nbsp;
                    <BalanceShow
                      :currency-id="item.currencyId"
                      :balance="item.balanceInCents"
                    />
                  </option>
                </Field>
              </template>

              <!-- 转账给他人 - 搜索框 -->
              <template v-else>
                <!-- 邮箱输入框 -->
                <div class="mb-3">
                  <input
                    type="text"
                    class="form-control form-control-solid"
                    :class="{ 'is-invalid': searchError }"
                    :placeholder="$t('action.searchAccountPlaceholder')"
                    v-model="searchEmail"
                    @blur="handleEmailBlur"
                    @keyup.enter="handleEmailBlur"
                    @input="handleEmailInput"
                  />
                  <div v-if="searchLoading" class="text-muted small mt-1">
                    {{ $t("action.searching") }}...
                  </div>
                  <div
                    v-if="searchError"
                    class="fv-help-block text-danger mt-1"
                  >
                    {{ searchError }}
                  </div>
                </div>

                <!-- 搜索结果选择 -->
                <!-- <Field name="tradeAccountUid" v-slot="{ field }"> -->
                <Field
                  name="tradeAccountUid"
                  class="form-select form-select-solid"
                  as="select"
                  v-model.number="TransferRequireData.tradeAccountUid"
                  :placeholder="
                    searchedAccounts.length > 0
                      ? $t('title.targetAccount')
                      : $t('tip.noSearchResults')
                  "
                  :disabled="searchedAccounts.length === 0"
                >
                  <option value="">
                    {{ $t("title.targetAccount") }}
                  </option>
                  <option
                    v-for="item in searchedAccounts"
                    :key="item.uid"
                    :value="item.uid"
                  >
                    {{ item.name }}&nbsp;{{ item.accountNumber }}&nbsp;({{
                      $t("type.currency." + item.currencyId)
                    }})&nbsp;&nbsp;&nbsp;
                  </option>
                </Field>
              </template>

              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="tradeAccountUid" />
                </div>
              </div>
            </div>
          </div>

          <!-- Amount -->
          <div class="d-flex flex-column mb-5 fv-row">
            <label class="fs-5 fw-semobold mb-2 required">{{
              $t("fields.amount")
            }}</label>

            <Field
              class="form-control form-control-solid"
              placeholder=""
              name="amount"
              v-model.number="TransferRequireData.amount"
            />
            <div class="fv-plugins-message-container">
              <p v-if="showTips" class="fv-help-block ml-2">
                <BalanceShow
                  :currency-id="tips.currencyId"
                  :balance="tips.amount"
                />
              </p>
              <div class="fv-help-block">
                <ErrorMessage name="amount" />
              </div>
            </div>
          </div>

          <!-- Verification Code - Only show when amount is not empty -->
          <div v-if="showVerificationCode" class="mb-5 fv-row">
            <label class="fs-5 fw-semobold mb-2 required">
              {{ $t("fields.oneTimeCode") }}
            </label>

            <div class="d-flex align-items-center gap-3 w-100">
              <Field
                class="form-control form-control-solid flex-grow-1"
                :placeholder="$t('fields.enterYourCode')"
                name="verificationCode"
                v-model="TransferRequireData.verificationCode"
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

            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="verificationCode" />
              </div>
              <p v-if="codeSent" class="text-success mt-2 mb-0">
                {{ $t("tip.codeSentToEmail") }}
              </p>
            </div>
          </div>

          <el-checkbox
            v-model="reverseCheck"
            :label="$t('tip.transferAgreement')"
            size="large"
          />
        </div>
      </div>
    </form>
  </SimpleForm>
  <!--end::Modal - New Address-->
</template>

<script setup lang="ts">
import { ref, nextTick, watch, computed, onUnmounted } from "vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import WalletService, {
  TradeAccountResType,
  TransferReqType,
  WalletsResType,
} from "../../services/WalletService";
import BalanceShow from "@/components/BalanceShow.vue";
import store from "@/store";

import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
const transferTo = ref("tradeAccount");
const isLoading = ref(false);
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
const modalRef = ref<InstanceType<typeof SimpleForm>>();
import { processErrorMessage } from "@/core/types/ErrorMessage";
const wallets = ref({} as any);
const TransferRequireData = ref<TransferReqType>({} as TransferReqType);
const tradeAccounts = ref<TradeAccountResType[]>([]);
const isWalletIdExist = ref(false);
const reverseCheck = ref(false);
const checked = ref(true);
const availableTradeAccounts = ref<TradeAccountResType[]>([]);
const emits = defineEmits<{
  (e: "onCreated"): void;
}>();

// 获取用户角色
const user = store.state.AuthModule.user;
const showTransferTabs = computed(
  () =>
    user.roles &&
    (user.roles.includes("IB") ||
      user.roles.includes("Sales") ||
      user.roles.includes("Rep"))
);

// Tab 切换
const currentTab = ref<"self" | "others">("self");
const searchedAccounts = ref<TradeAccountResType[]>([]);
const searchLoading = ref(false);
const searchEmail = ref("");
const searchError = ref(""); // 搜索错误消息

// 验证码相关
const isSendingCode = ref(false);
const codeSent = ref(false);
const countdown = ref(0);
let countdownTimer: number | null = null;

// 显示验证码输入框的条件 - 根据 tab 和配置决定
const showVerificationCode = computed(() => {
  if (!TransferRequireData.value.amount) {
    return false;
  }
  return true;
  // 根据当前 tab 检查对应的配置
  // if (currentTab.value === "self") {
  //   // 钱包转交易账户
  //   return (
  //     store.state.AuthModule.config.twoFactorAuthForTransactions
  //       ?.walletToTradeAccount === true
  //   );
  // } else {
  //   // 钱包转钱包
  //   return (
  //     store.state.AuthModule.config.twoFactorAuthForTransactions
  //       ?.walletToWalletTransfer === true
  //   );
  // }
});

// 监听 tab 切换
watch(currentTab, () => {
  TransferRequireData.value.tradeAccountUid = undefined;
  searchedAccounts.value = [];
  searchEmail.value = "";
  searchError.value = "";
  resetVerificationCode();
});

// 重置验证码状态
const resetVerificationCode = () => {
  codeSent.value = false;
  countdown.value = 0;
  TransferRequireData.value.verificationCode = undefined;
  if (countdownTimer) {
    clearInterval(countdownTimer);
    countdownTimer = null;
  }
};

// 开始倒计时 (2分钟 = 120秒)
const startCountdown = (expries: number) => {
  countdown.value = expries;
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
  console.log("currentTab", currentTab.value);
  if (countdownTimer) {
    return;
  }
  isSendingCode.value = true;
  try {
    const params = {
      authType:
        currentTab.value === "self"
          ? "WalletToTradeAccount"
          : "WalletToWalletTransfer",
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

// 邮箱输入时清除错误和选中的账户
const handleEmailInput = () => {
  searchError.value = "";
  // 清空之前选中的账户（表示需要重新搜索）
  if (TransferRequireData.value.tradeAccountUid) {
    TransferRequireData.value.tradeAccountUid = undefined;
  }
};

// 邮箱输入框失去焦点时触发搜索
const handleEmailBlur = async () => {
  const email = searchEmail.value.trim();

  // 清空之前的错误
  searchError.value = "";

  if (!email) {
    searchedAccounts.value = [];
    TransferRequireData.value.tradeAccountUid = undefined;
    return;
  }

  searchLoading.value = true;

  try {
    // 调用新的搜索 API（用于转账给他人）
    const response = await WalletService.searchTransferTargetAccount(email);
    console.log("search response", response);

    // 适配后端返回格式：{ walletId: "123", email: "xx", name: "客户姓名", currencyId: 1 }
    if (response && typeof response === "object") {
      // 如果返回的是单个对象（包含 walletId）
      if (response.walletId) {
        searchedAccounts.value = [
          {
            uid: parseInt(response.walletId) || 0,
            accountNumber: parseInt(response.walletId) || 0,
            currencyId: parseInt(response.currencyId) || 1,
            email: response.email,
            name: response.name,
            balanceInCents: 0,
            balance: 0,
            leverage: 0,
            serviceId: 0,
            lastSyncedOn: "",
          },
        ];
      } else if (response.data) {
        // 如果返回格式为 { data: {...} } 或 { data: [...] }
        const data = response.data;
        if (Array.isArray(data)) {
          searchedAccounts.value = data;
        } else if (data && typeof data === "object" && data.walletId) {
          // data 是单个对象
          searchedAccounts.value = [
            {
              uid: parseInt(data.walletId) || 0,
              accountNumber: parseInt(data.walletId) || 0,
              currencyId: parseInt(data.currencyId) || 1,
              email: data.email,
              name: data.name,
              balanceInCents: 0,
              balance: 0,
              leverage: 0,
              serviceId: 0,
              lastSyncedOn: "",
            },
          ];
        } else {
          searchedAccounts.value = data ? [data] : [];
        }
      } else {
        searchedAccounts.value = [];
      }
    } else {
      searchedAccounts.value = [];
    }

    // 如果没有搜索结果，显示页面提示（不弹框）
    if (searchedAccounts.value.length === 0) {
      searchError.value = t("tip.noSearchResults");
      TransferRequireData.value.tradeAccountUid = undefined;
    } else {
      // 有搜索结果时，自动选中第一个
      TransferRequireData.value.tradeAccountUid = searchedAccounts.value[0].uid;
    }
  } catch (error: any) {
    // 显示错误消息在页面上（不弹框）
    searchError.value = processErrorMessage(error);
    searchedAccounts.value = [];
    TransferRequireData.value.tradeAccountUid = undefined;
  } finally {
    searchLoading.value = false;
  }
};

// 清理定时器
onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
});

const validationSchema = computed(() => {
  const baseSchema = {
    walletId: Yup.number()
      .typeError(t("tip.requiredField"))
      .required(t("tip.requiredField"))
      .label("Wallet"),
    tradeAccountUid: Yup.number()
      .required(t("tip.requiredField"))
      .label("Trade Account"),
    amount: Yup.number()
      .required(t("tip.requiredField"))
      .typeError(t("tip.requiredField"))
      .label("Amount")
      .test("balance-check", function (value) {
        if (!value || !TransferRequireData.value.walletId) {
          return true; // 如果金额或钱包ID为空，让其他验证规则处理
        }
        const selectWallet = wallets.value.find(
          (item) => item.id === TransferRequireData.value.walletId
        );
        if (!selectWallet) {
          return true; // 如果找不到钱包，让其他验证规则处理
        }
        // 比较金额（注意：提交时 amount 会乘以 100，所以这里需要比较转换后的值）
        const amountInCents = value * 100;
        if (amountInCents > selectWallet.balance) {
          // 返回错误消息字符串，会在 ErrorMessage 组件中显示
          return this.createError({
            message: t("error.__BALANCE_NOT_ENOUGH__"),
          });
        }
        return true;
      }),
  };

  // 根据当前 tab 检查是否需要验证码
  const needVerificationCode =
    currentTab.value === "self"
      ? store.state.AuthModule.config.twoFactorAuthForTransactions
          ?.walletToTradeAccount === true
      : store.state.AuthModule.config.twoFactorAuthForTransactions
          ?.walletToWalletTransfer === true;

  if (needVerificationCode) {
    // 验证码为必填项
    return Yup.object().shape({
      ...baseSchema,
      verificationCode: Yup.string()
        .required(t("tip.verificationCodeRequired"))
        .label("Verification Code"),
    });
  } else {
    return Yup.object().shape(baseSchema);
  }
});

const showTips = computed(() => {
  if (
    TransferRequireData.value.tradeAccountUid &&
    TransferRequireData.value?.amount &&
    !isNaN(TransferRequireData.value?.amount)
  ) {
    const currentAccount = getCurentInfo();
    if (currentAccount && currentAccount.currencyId == CurrencyTypes.USC) {
      return true;
    }
    return false;
  }
  return false;
});
const getCurentInfo = () => {
  let currentAccount: TradeAccountResType | undefined;
  if (currentTab.value === "self") {
    currentAccount = tradeAccounts.value.find(
      (item) => item.uid == TransferRequireData.value.tradeAccountUid
    );
  } else {
    currentAccount = searchedAccounts.value.find(
      (item) => item.uid == TransferRequireData.value.tradeAccountUid
    );
  }
  return currentAccount;
};
const tips = computed(() => {
  const currentAccount = getCurentInfo();
  console.log("currentAccount", currentAccount);
  let res = {
    currencyId: currentAccount?.currencyId,
    amount: TransferRequireData.value?.amount * 10000,
  };
  console.log("res", res);
  return res;
});

const { handleSubmit, resetForm } = useForm({
  validationSchema: validationSchema,
});
const changeWallet = async () => {
  const walletId = TransferRequireData.value.walletId;
  try {
    const selectWallet = wallets.value.find((item) => item.id === walletId);
    // 取消 不同钱包无法互转的限制
    // availableTradeAccounts.value = tradeAccounts.value.filter(
    //   (item) => item.currencyId === selectWallet?.currencyId
    // );
    // delete TransferRequireData.value.tradeAccountUid;
    // TransferRequireData.value.tradeAccountUid = "";
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
  return true;
};

const reset = () => {
  resetForm({ values: {} });
  isLoading.value = false;
  transferTo.value = "";
  isWalletIdExist.value = false;
  wallets.value = {} as WalletsResType;
  tradeAccounts.value = [];
  TransferRequireData.value = {} as TransferReqType;
  currentTab.value = "self";
  searchedAccounts.value = [];
  searchEmail.value = "";
  searchError.value = "";
  resetVerificationCode();
};

const show = async (_walletId?: number) => {
  isLoading.value = true;

  reset();
  await nextTick();

  modalRef.value?.show();

  if (_walletId) {
    TransferRequireData.value.walletId = _walletId;
    isWalletIdExist.value = true;
  }

  try {
    const walletResponseData = await WalletService.getWallets();

    wallets.value = walletResponseData.data.filter((item) => {
      return item.isPrimary === true;
    });
    // walletResponseData.data; //

    const result = wallets.value.find((item) => item.id === _walletId);

    const tradeAccountResponseData = await WalletService.getTradeAccounts({
      CurrencyId: result?.currencyId,
      FundType: result?.fundType,
      includeClosed: false,
    });
    console.log("tradeAccountResponseData", tradeAccountResponseData);
    // 转账给自己：使用原有数据格式（标准的交易账户列表）
    tradeAccounts.value = tradeAccountResponseData.data || [];
    availableTradeAccounts.value = tradeAccountResponseData.data || [];
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const postTransferRequest = handleSubmit(async () => {
  isLoading.value = true;
  try {
    if (currentTab.value == "others") {
      await WalletService.postTransferToWalletAccount({
        ...TransferRequireData.value,
        amount: TransferRequireData.value.amount * 100,
      });
    } else {
      await WalletService.postTransferToTradeAccount({
        ...TransferRequireData.value,
        amount: TransferRequireData.value.amount * 100,
      });
    }
    let tips = "";
    if (currentTab.value == "others") {
      tips = t("tip.transferToWalletSuccess");
    } else {
      tips = t("tip.transferToTradeAccSuccess");
    }
    if (TransferRequireData.value.amount > 3000) {
      tips = t("tip.transferSuccessSubmitTip");
    }
    MsgPrompt.success(tips).then(() => {
      modalRef.value?.hide();
      emits("onCreated");
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
});

watch(reverseCheck, (value) => {
  checked.value = value === true ? false : true;
});

defineExpose({
  hide: () => modalRef.value?.hide(),
  show,
});
</script>

<style scoped>
.el-checkbox__label {
  text-wrap: wrap;
}
:deep(.el-checkbox__label) {
  white-space: normal;
}
</style>
