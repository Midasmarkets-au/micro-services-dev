<template>
  <div>
    <div class="amount-tip mb-11">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("tip.amountAvailable") + " " }}</span>
      <BalanceShow
        :currency-id="selectedWallet.currencyId"
        :balance="availableAmount ?? 0"
      />
    </div>
    <div class="step-title">
      <span>{{ $t("tip.withdrawFromCurrentWallet") }}: </span>
      <span>{{ $t("type.currency." + currencyId) }}</span>
    </div>

    <!-- Amount -->
    <div class="d-flex flex-column mb-5 fv-row">
      <label class="fs-5 fw-semobold mb-2 required"
        >{{ $t("fields.amount") }} ({{
          $t("type.currency." + currencyId)
        }})</label
      >

      <Field
        class="form-control form-control-solid"
        name="amount"
        type="number"
        v-model.number="paymentRequireData.amount"
      />
      <div v-if="amountError" style="color: #900000">
        {{ $t("error.amountRule") }} ${{ selectedService.minValue }} - ${{
          selectedService.maxValue
        }}
      </div>
      <div v-if="amountRequiredError" style="color: #900000">
        {{ $t("error.__AMOUNT_IS_REQUIRED__") }}
      </div>
    </div>

    <!-- Target -->
    <div class="d-flex flex-column mb-5 fv-row">
      <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
        <span class="required">{{ $t("title.targetAccount") }}</span>
      </label>

      <div v-if="isUSDT" class="d-flex flex-lg-row flex-column gap-4 gap-lg-0">
        <Field
          name="account"
          class="form-select form-select-solid"
          as="select"
          v-model="paymentRequireData.request"
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
          class="btn btn-secondary ms-lg-9 w-200px"
          @click="newAccount = true"
        >
          {{ $t("action.addNewAccount") }}
        </button>
      </div>

      <div v-else class="d-flex flex-lg-row flex-column gap-4 gap-lg-0">
        <Field
          name="account"
          class="form-select form-select-solid"
          as="select"
          v-model="paymentRequireData.request"
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
          class="btn btn-secondary ms-lg-9 w-200px"
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
          type="danger"
          @click="newAccount = false"
          size="large"
          :disabled="isLoading"
        >
          {{ $t("action.cancel") }}
        </el-button>
        <el-button
          color="#ffce00"
          @click="returnFormData()"
          size="large"
          :loading="isLoading"
        >
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </div>

    <div class="d-flex flex-column mb-5 mt-15 fv-row">
      <label class="fs-5 fw-semobold mb-2 required">
        {{ $t("fields.notesOnWithdrawal") }}
      </label>
      <label class="checkboxContainer" style="color: #000000">
        {{ $t("tip.understandBcrDeduct") }}
        >{{ $t("tip.understandBcrDeduct") }}
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
</template>
<script lang="ts" setup>
import { ref, inject } from "vue";
import WireForm from "@/projects/client/views/profile/components/form/WireForm.vue";
import LocalBankForm from "@/projects/client/views/profile/components/form/LocalBankForm.vue";
import WireDefaultForm from "@/projects/client/views/profile/components/form/WireDefaultForm.vue";
import usdtForm from "@/projects/client/views/profile/components/form/UsdtForm.vue";
import { Field } from "vee-validate";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const isLoading = inject<boolean>("isLoading");
const selectedWallet = inject<any>("selectedWallet");
const currencyId = inject<any>("currencyId");
const paymentRequireData = inject<any>("paymentRequireData");
const selectedService = inject<any>("selectedService");
const paymentMethods = inject<any>("paymentMethods");
const isUSDT = inject<boolean>("isUSDT");
const hasUSDTWallet = inject<boolean>("hasUSDTWallet");
const selectedForm = inject<any>("selectedForm");
const newAccount = inject<boolean>("newAccount");
const submitNewBankInfo = inject<any>("submitNewBankInfo");
const availableAmount = inject<any>("availableAmount");

const checked = ref(false);
const amountRequiredError = ref(false);
const amountError = ref(false);
const accountRequireError = ref(false);
const checkBoxError = ref(false);

const formCollection = {
  WireForm,
  LocalBankForm,
  usdtForm,
  WireDefaultForm,
};

const dynamicRef = ref<any>(null);
const returnFormData = () => {
  dynamicRef.value?.returnFormData();
};

const formErrorCheck = () => {
  var finalResult = true;

  if (!paymentRequireData.value.amount) {
    amountRequiredError.value = true;
    finalResult = false;
  } else {
    amountRequiredError.value = false;
  }
  if (
    (paymentRequireData.value.amount < selectedService.value.minValue ||
      paymentRequireData.value.amount > selectedService.value.maxValue) &&
    finalResult === true
  ) {
    amountError.value = true;
    finalResult = false;
  } else {
    amountError.value = false;
  }
  if (paymentRequireData.value.amount > availableAmount.value / 100) {
    MsgPrompt.error(t("error.amountAvailableNotEnough"));
    finalResult = false;
  }
  if (
    paymentRequireData.value.request === "" ||
    paymentRequireData.value.request === null ||
    paymentRequireData.value.request === undefined
  ) {
    accountRequireError.value = true;
    finalResult = false;
  } else {
    accountRequireError.value = false;
  }

  if (!checked.value) {
    checkBoxError.value = true;
    finalResult = false;
  } else {
    checkBoxError.value = false;
  }

  return finalResult;
};

const setComponentRef = (instance) => {
  dynamicRef.value = instance;
};

defineExpose({
  formErrorCheck,
});
</script>
<style lang="scss" scoped></style>
