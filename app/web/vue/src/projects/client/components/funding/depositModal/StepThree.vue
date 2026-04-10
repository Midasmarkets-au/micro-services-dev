<template>
  <div class="d-flex align-items-center">
    <div v-if="!isLoading" style="width: 100%">
      <div class="step-title">
        {{ $t("title.depositInToCurrentAccount") }}
      </div>

      <div
        class="d-flex flex-column mb-5 fv-row"
        v-if="paymentRequireData.groupInfo.currencyRates.length > 0"
      >
        <label class="d-flex align-items-center fs-6 mb-2 required">
          {{ paymentRequireData.paymentMethodName }} {{ $t("fields.currency") }}
        </label>

        <Field
          name="serviceCurrencyId"
          class="form-select form-select-solid"
          as="select"
          v-model="paymentRequireData.request.currencyId"
          @change="updateExchangeRate()"
          :disabled="paymentRequireData.groupInfo.currencyRates.length == 1"
        >
          <option value="">
            {{ $t("tip.selectCurrency") }}
          </option>
          <option
            v-for="item in paymentRequireData.groupInfo.currencyRates"
            :label="$t('type.currency.' + item.currencyId)"
            :key="item.currencyId"
            :value="item.currencyId"
          ></option>
        </Field>
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="serviceCurrencyId" />
          </div>
        </div>
      </div>

      <div v-if="paymentRequireData.group == 'TRC20'" class="row mb-5">
        <!-- <div class="col-12">
          <label class="fs-6 required">{{
            $t("fields.enterYourWalletAddress")
          }}</label>

          <Field
            class="form-control form-control-solid"
            name="walletAddress"
            v-model.number="paymentRequireData.request.walletAddress"
          />
        </div> -->
      </div>

      <div class="row">
        <div
          :class="
            paymentRequireData.groupInfo.currencyRates.length > 0
              ? 'col-6'
              : 'col-12 mb-5'
          "
        >
          <label class="fs-6 required"
            >{{ $t("fields.amount") }} ({{
              $t("type.currency." + paymentRequireData.account.currencyId)
            }})</label
          >

          <Field
            class="form-control form-control-solid"
            placeholder=""
            name="amount"
            v-model.number="paymentRequireData.request.amount"
            :disabled="paymentRequireData.request.currencyId == null"
            @keyup="calculateTargetAmount()"
          />

          <div
            class="fv-plugins-message-container d-flex justify-content-between"
          >
            <div class="fv-help-block">
              <ErrorMessage name="amount"> </ErrorMessage>
            </div>
            <div v-if="amountError" style="color: #900000">
              <div v-if="paymentRequireData.account.currencyId === 841">
                <!-- {{ $t("error.amountRule") }} ${{
                  paymentRequireData.groupInfo.range[0]
                }}
                - ${{ paymentRequireData.groupInfo.range[1] }} -->
                <BalanceShow
                  :balance="paymentRequireData.groupInfo.range[0] * 100"
                  :currency-id="paymentRequireData.account.currencyId"
                />
                -
                <BalanceShow
                  :balance="paymentRequireData.groupInfo.range[1] * 100"
                  :currency-id="paymentRequireData.account.currencyId"
                />
              </div>
              <div v-else>
                <!-- {{ $t("error.amountRule") }} ${{
                  paymentRequireData.groupInfo.range[0] / 100
                }}
                - ${{ paymentRequireData.groupInfo.range[1] / 100 }} -->
                <BalanceShow
                  :balance="paymentRequireData.groupInfo.range[0]"
                  :currency-id="paymentRequireData.account.currencyId"
                />
                -
                <BalanceShow
                  :balance="paymentRequireData.groupInfo.range[1]"
                  :currency-id="paymentRequireData.account.currencyId"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          v-if="exchangeRate != 1"
          :class="
            paymentRequireData.groupInfo.currencyRates.length > 0
              ? 'col-6'
              : 'col-12'
          "
        >
          <label class="fs-6">
            {{ $t("fields.depositAmount") }}
          </label>

          <Field
            class="form-control form-control-solid"
            placeholder=""
            name="targetAmount"
            v-model="targetAmount"
            disabled
          />
          <div style="text-align: right">
            {{ $t("fields.exchangeRate") }} = 1 :
            {{ exchangeRate }}
          </div>
        </div>

        <div v-if="paymentRequireData.group == 'Credit Card'" class="row">
          <CreditCardForm ref="creditCardFormRef" />
        </div>

        <div
          v-else
          class="col-6 mb-5"
          v-for="(item, index) in paymentRequireData.groupInfo.requestKeys"
          :key="index"
        >
          <label class="fs-6">
            {{ $t("fields." + item) }}
          </label>

          <Field
            class="form-control form-control-solid"
            placeholder=""
            :name="item"
            v-model="paymentRequireData.request[item]"
          />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import * as Yup from "yup";
import { useI18n } from "vue-i18n";
import { useForm } from "vee-validate";
import { ref, onMounted, inject, watch } from "vue";
import { Field, ErrorMessage } from "vee-validate";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import CreditCardForm from "./components/CreditCardForm.vue";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const creditCardFormRef = ref<InstanceType<typeof CreditCardForm>>();
const paymentRequireData = inject<any>("paymentRequireData");
const currentStep = inject<any>("currentStep");
const isLoading = inject<any>("isLoading");

const { t } = useI18n();
const exchangeRate = ref(1);
const amountError = ref(false);
const targetAmount = ref(0);

const validationSchema = Yup.object().shape({
  amount: Yup.number()
    .required(t("error.__AMOUNT_IS_REQUIRED__"))
    .typeError(t("error.__PLEASE_INPUT_AN_AMOUNT__"))
    .integer(t("error.__MUST_BE_AN_INTEGER__")),

  serviceCurrencyId: Yup.string().when([], {
    is: () => paymentRequireData.value.groupInfo.currencyRates.length > 0,
    then: Yup.string().required(t("error.__CURRENCY_IS_REQUIRED__")),
  }),
});

const { handleSubmit } = useForm({
  validationSchema,
});

const nextStep = handleSubmit(async () => {
  if (!checkAmount()) {
    return;
  }

  if (paymentRequireData.value.group == "Credit Card") {
    try {
      if (await creditCardFormRef.value?.onSubmit()) {
        currentStep.value += 1;
      }
    } catch (error) {
      console.log(error);
    }
    return;
  }

  for (
    let i = 0;
    i < paymentRequireData.value.groupInfo.requestKeys.length;
    i++
  ) {
    const item = paymentRequireData.value.groupInfo.requestKeys[i];
    if (
      paymentRequireData.value.request[item] == undefined ||
      paymentRequireData.value.request[item] == ""
    ) {
      MsgPrompt.warning(t("title.fillRequiredInfo"));
      return;
    }
  }

  // if (
  //   paymentRequireData.value.group == "TRC20" &&
  //   (paymentRequireData.value.request.walletAddress == null ||
  //     paymentRequireData.value.request.walletAddress.length < 5)
  // ) {
  //   MsgPrompt.warning(t("title.fillRequiredInfo"));
  //   return;
  // }

  currentStep.value += 1;
});

const checkAmount = () => {
  const range1 =
    paymentRequireData.value.account.currencyId === 841
      ? paymentRequireData.value.groupInfo.range[1]
      : paymentRequireData.value.groupInfo.range[1] / 100;
  const range0 =
    paymentRequireData.value.account.currencyId === 841
      ? paymentRequireData.value.groupInfo.range[0]
      : paymentRequireData.value.groupInfo.range[0] / 100;
  if (
    paymentRequireData.value.request.amount > range1 ||
    paymentRequireData.value.request.amount < range0
  ) {
    amountError.value = true;
    return false;
  } else {
    amountError.value = false;
    paymentRequireData.value.targetAmount = targetAmount.value;
    return true;
  }
};

const updateExchangeRate = () => {
  const findCurrency = paymentRequireData.value.groupInfo.currencyRates.find(
    (item) => item.currencyId === paymentRequireData.value.request.currencyId
  );

  if (findCurrency == null) {
    exchangeRate.value = 1;
    targetAmount.value = 0;
    return;
  }

  exchangeRate.value = findCurrency.rate;
  calculateTargetAmount();
};

const calculateTargetAmount = () => {
  console.log("xx", paymentRequireData);
  if (paymentRequireData.value.request.amount) {
    targetAmount.value = Math.ceil(
      paymentRequireData.value.request.amount * exchangeRate.value
    );
  } else {
    targetAmount.value = 0;
  }
};

const isExLinkGlobal = () => {
  const group = paymentRequireData.value.group || "";
  const name = paymentRequireData.value.paymentMethodName || "";
  console.log("group", group.toLowerCase().includes("exlink global"));
  return group.toLowerCase().includes("exlink global");
};

const fetchExLinkCurrencyRates = async () => {
  const [currenciesRes, ratesRes] = await Promise.all([
    clientGlobalService.getExLinkCurrencies(),
    clientGlobalService.getExLinkExchangeRates(),
  ]);

  const currencies = currenciesRes.data || [];
  const rateList = ratesRes.data?.marketPriceList || [];

  const rateMap = new Map(
    rateList.map((r: any) => [r.sourceCoinId, r.marketInPrice])
  );

  paymentRequireData.value.groupInfo.currencyRates =
    paymentRequireData.value.groupInfo.currencyRates
      .filter((cr: any) => rateMap.has(cr.currencyId))
      .map((cr: any) => ({
        ...cr,
        rate: rateMap.get(cr.currencyId),
      }));
};

onMounted(async () => {
  isLoading.value = true;

  paymentRequireData.value.groupInfo.requestKeys =
    paymentRequireData.value.groupInfo.requestKeys.filter(
      (item) => item !== "returnUrl" && item !== "currencyId"
    );
  if (isExLinkGlobal()) {
    try {
      await fetchExLinkCurrencyRates();
    } catch (error) {
      console.error("Failed to fetch ExLink currency rates:", error);
    }
  }

  if (paymentRequireData.value.groupInfo.currencyRates.length == 1) {
    paymentRequireData.value.request.currencyId =
      paymentRequireData.value.groupInfo.currencyRates[0].currencyId;
    updateExchangeRate();
  } else if (paymentRequireData.value.groupInfo.currencyRates.length == 0) {
    paymentRequireData.value.request.currencyId =
      paymentRequireData.value.account.currencyId;
  }

  if (paymentRequireData.value.request.amount) updateExchangeRate();

  isLoading.value = false;
});

defineExpose({
  nextStep,
});
</script>
<style lang="scss" scoped>
.border-top {
  border-top: 1px solid #e4e6ef;
  border-bottom: 1px solid #e4e6ef;
  color: #000;
}
.content {
  width: 100%;
  padding: 20px 35px;
  height: 500px;
  overflow-y: auto;
}
.secondary-btn:hover {
  color: #000;
}
</style>
