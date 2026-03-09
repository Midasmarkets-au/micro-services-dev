<template>
  <div>
    <div class="amount-tip mb-11">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("tip.amountAvailable") + " " }}</span>
      <BalanceShow
        :currency-id="paymentRequireData.detail.currencyId"
        :balance="paymentRequireData.availableAmount ?? 0"
      />
    </div>
    <div class="step-title">
      <span
        >{{
          paymentRequireData.isAccount
            ? $t("tip.withdrawFromCurrentAccount")
            : $t("tip.withdrawFromCurrentWallet")
        }}: {{ $t("type.currency." + paymentRequireData.detail.currencyId) }}
      </span>
    </div>

    <div
      v-if="isLoading"
      class="d-flex align-items-center justify-content-center h-400px"
    >
      <LoadingRing />
    </div>

    <div v-else>
      <!-- Amount -->
      <div class="d-flex flex-column mb-5 fv-row">
        <label class="fs-6 mb-2 required"
          >{{ $t("fields.amount") }} ({{
            $t("type.currency." + paymentRequireData.detail.currencyId)
          }})</label
        >

        <Field
          class="form-control form-control-solid"
          name="amount"
          type="number"
          v-model.number="paymentRequireData.request.amount"
        />
        <div v-if="amountError" style="color: #900000">
          {{ $t("error.amountRule") }} ${{
            paymentRequireData.groupInfo.range[0]
          }}
          - ${{ paymentRequireData.groupInfo.range[1] }}
        </div>
        <div v-if="amountRequiredError" style="color: #900000">
          {{ $t("error.__AMOUNT_IS_REQUIRED__") }}
        </div>
      </div>

      <!-- Target -->
      <div class="d-flex flex-column mb-5 fv-row">
        <label class="d-flex align-items-center fs-6 mb-2">
          <span class="required">{{ $t("title.targetAccount") }}</span>
        </label>

        <div v-if="paymentRequireData.request.isUSDT">
          <div class="d-flex flex-lg-row flex-column gap-4 gap-lg-0">
            <Field
              name="account"
              class="form-select form-select-solid"
              as="select"
              v-model="paymentRequireData.request.targetIndex"
              v-on:change="newAccount = false"
            >
              <option value="">
                {{
                  paymentMethods.length > 0
                    ? $t("title.usdtWalletAddress")
                    : $t("tip.pleaseAddAUSDTWalletAddress")
                }}
              </option>
              <option
                v-for="(item, index) in paymentMethods"
                :key="index"
                :value="index"
              >
                {{ item.info.walletAddress }}
              </option>
            </Field>
            <button
              v-if="!hasUSDTWallet"
              class="btn btn-primary ms-lg-9 w-200px"
              @click="newAccount = true"
            >
              {{ $t("action.addNewAccount") }}
            </button>
          </div>
        </div>

        <div v-else class="d-flex flex-lg-row flex-column gap-4 gap-lg-0">
          <Field
            name="account"
            class="form-select form-select-solid"
            as="select"
            v-model="paymentRequireData.request.targetIndex"
            v-on:change="newAccount = false"
          >
            <option value="">
              {{
                paymentMethods.length > 0
                  ? $t("tip.selectBankAccount")
                  : $t("tip.pleaseAddABankAccount")
              }}
            </option>
            <option
              v-for="(item, index) in paymentMethods"
              :key="index"
              :value="index"
            >
              {{ item.info.bankName }}
              <span v-if="item.info.branchName"
                >({{ item.info.branchName }})</span
              >
              - {{ item.info.accountNo }}
            </option>
          </Field>
          <button
            class="btn btn-primary ms-lg-9 w-200px"
            @click="newAccount = true"
          >
            {{ $t("action.addNewAccount") }}
          </button>
        </div>
        <div v-if="accountRequireError" style="color: #900000">
          {{ $t("error.__ACCOUNT_IS_REQUIRED__") }}
        </div>
      </div>

      <div v-if="newAccount" style="border: 1px solid #dcdcdc; padding: 20px">
        <component
          :is="formCollection[selectedForm]"
          :ref="setComponentRef"
          @submit="submitNewBankInfo"
        ></component>
        <div
          class="mb-7 text-center"
          style="
            box-sizing: border-box;
            padding: 5px 15px;
            border-radius: 100px;
            background-color: #ffecec;
            color: #9f005b;
          "
        >
          {{ $t("tip.addBankAccountNotice") }}
        </div>
        <div style="text-align: right">
          <el-button
            class="btn btn-light btn-sm btn-bordered"
            type="danger"
            @click="newAccount = false"
            size="large"
            :disabled="isLoading"
          >
            {{ $t("action.cancel") }}
          </el-button>
          <el-button
            class="btn btn-primary btn-sm btn-bordered"
            @click="submitTargetInfo()"
            size="large"
            :loading="isLoading"
          >
            {{ $t("action.submit") }}
          </el-button>
        </div>
      </div>

      <div class="d-flex flex-column mb-5 mt-15 fv-row">
        <label class="fs-6 mb-2 required">
          {{ $t("fields.notesOnWithdrawal") }}
        </label>
        <label
          class="checkboxContainer"
          style="color: #000000; font-size: 14px"
        >
          {{ $t("tip.understandBcrDeduct") }}
          <Field
            class="form-control form-control-solid"
            type="checkbox"
            v-model="checked"
            name="checkbox"
            :value="true"
          />
          <span class="checkmark"></span>
        </label>
        <div v-if="checkBoxError" style="color: #900000">
          {{ $t("error.__CLICK_CHECK_BOX_TO_CONFIRM__") }}
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import { Field } from "vee-validate";
import { ref, inject, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import WireForm from "@/projects/client/views/profile/components/form/WireForm.vue";
import usdtForm from "@/projects/client/views/profile/components/form/UsdtForm.vue";
import LocalBankForm from "@/projects/client/views/profile/components/form/LocalBankForm.vue";
import WireDefaultForm from "@/projects/client/views/profile/components/form/WireDefaultForm.vue";

const paymentPlateformList = ref({
  Bank: 100,
  PayPal: 230,
  USDT: 240,
});

const formCollection = {
  WireForm,
  LocalBankForm,
  usdtForm,
  WireDefaultForm,
};

const { t } = useI18n();
const checked = ref(false);
const selectedForm = ref("");
const newAccount = ref(false);
const hasUSDTWallet = ref(false);
const dynamicRef = ref<any>(null);
const paymentMethods = ref([] as any);

//Error
const amountError = ref(false);
const checkBoxError = ref(false);
const amountRequiredError = ref(false);
const accountRequireError = ref(false);

const isLoading = inject<any>("isLoading");
const paymentRequireData = inject<any>("paymentRequireData");

const formErrorCheck = () => {
  amountRequiredError.value = false;
  accountRequireError.value = false;
  checkBoxError.value = false;
  amountError.value = false;

  if (!paymentRequireData.value.request.amount) {
    amountRequiredError.value = true;
    return false;
  }

  if (
    paymentRequireData.value.request.amount >
    paymentRequireData.value.availableAmount / 100
  ) {
    MsgPrompt.warning(t("error.amountAvailableNotEnough"));
    return false;
  }

  if (
    paymentRequireData.value.request.amount <
      paymentRequireData.value.groupInfo.range[0] ||
    paymentRequireData.value.request.amount >
      paymentRequireData.value.groupInfo.range[1]
  ) {
    amountError.value = true;
    return false;
  }

  if (paymentRequireData.value.request.targetIndex === undefined) {
    accountRequireError.value = true;
    return false;
  }

  paymentRequireData.value.request.targetAccount =
    paymentMethods.value[paymentRequireData.value.request.targetIndex];

  if (!checked.value) {
    checkBoxError.value = true;
    return false;
  }

  if (paymentRequireData.value.detail.currencyId === CurrencyTypes.CNY) {
    const { state, city } =
      paymentMethods.value[paymentRequireData.value.request.targetIndex].info;
    if (state === undefined || city === undefined) {
      MsgPrompt.warning(t("tip.pleaseUpdateYourCnBankInfo"));
      return false;
    }
  }

  return true;
};

const setComponentRef = (instance) => {
  dynamicRef.value = instance;
};

const submitTargetInfo = () => {
  dynamicRef.value?.returnFormData();
};

const submitNewBankInfo = async (_targetInfo) => {
  isLoading.value = true;

  try {
    await GlobalService.createUserPaymentInfo({
      paymentPlatform: paymentRequireData.value.request.isUSDT
        ? paymentPlateformList.value.USDT
        : 100,
      name: _targetInfo.name,
      info: _targetInfo,
    }).then(() => {
      getPaymentMethods();
    });

    newAccount.value = false;
    paymentRequireData.value.request.targetIndex = 0;
    MsgPrompt.success(t("tip.formSuccessSubmit"));
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const getPaymentMethods = async () => {
  paymentRequireData.value.request.isUSDT = false;
  const currencyId = paymentRequireData.value.detail.currencyId;
  const serviceName = paymentRequireData.value.selectedServiceName;

  try {
    const temp = await GlobalService.getUserPaymentInfo();
    paymentMethods.value = temp.data;

    switch (true) {
      case serviceName.includes("Wire"):
        selectedForm.value = "WireForm";
        paymentMethods.value = paymentMethods.value.filter(
          (item) => item.paymentPlatform != paymentPlateformList.value.USDT
        );
        break;
      case serviceName.includes("Local") || currencyId === CurrencyTypes.CNY:
        selectedForm.value = "LocalBankForm";
        paymentMethods.value = paymentMethods.value.filter(
          (item) => item.paymentPlatform != paymentPlateformList.value.USDT
        );
        break;
      case serviceName.includes("USDT"):
        paymentRequireData.value.request.isUSDT = true;
        selectedForm.value = "usdtForm";
        paymentMethods.value = paymentMethods.value.filter(
          (item) => item.paymentPlatform == paymentPlateformList.value.USDT
        );
        hasUSDTWallet.value = paymentMethods.value.length > 0;
        break;
      default:
        selectedForm.value = "WireDefaultForm";
        paymentMethods.value = paymentMethods.value.filter(
          (item) => item.paymentPlatform != paymentPlateformList.value.USDT
        );
        break;
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  isLoading.value = true;

  await getPaymentMethods();

  isLoading.value = false;
});

defineExpose({
  formErrorCheck,
});
</script>
<style lang="scss" scoped></style>
