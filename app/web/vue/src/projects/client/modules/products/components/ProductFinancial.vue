<template>
  <div class="card my-5">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.financial") }}
        </h2>
      </div>
      <hr />

      <el-form-item
        :label="$t('fields.investorDistinction')"
        prop="investorDistinction"
      >
        <el-checkbox-group
          v-model="formData.investorDistinction"
          :disabled="isSubmitting"
        >
          <el-checkbox
            v-for="item in investorDistinctionSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-checkbox>
        </el-checkbox-group>
      </el-form-item>

      <div class="row mb-5">
        <a
          href="https://www.google.com/"
          target="_blank"
          rel="noopener noreferrer"
          style="color: #409eff; text-decoration: underline"
          >{{ $t("fields.aboutInvestorClassification") }}</a
        >
      </div>

      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.annualIncome')" prop="annualIncome">
            <el-select
              v-model="formData.annualIncome"
              :disabled="isSubmitting"
              :placeholder="$t('fields.select')"
              style="width: 100%"
            >
              <el-option
                v-for="(item, index) in moneyRange"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.valueOfInvestment')"
            prop="investment"
          >
            <el-select
              v-model="formData.investment"
              :disabled="isSubmitting"
              :placeholder="$t('fields.select')"
              style="width: 100%"
            >
              <el-option
                v-for="(item, index) in moneyRange"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.amountOfInvestmentFunds')"
            prop="investmentFunds"
          >
            <el-select
              v-model="formData.investmentFunds"
              :disabled="isSubmitting"
              :placeholder="$t('fields.select')"
              style="width: 100%"
            >
              <el-option
                v-for="(item, index) in moneyRange"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-4 col-lg-6 mb-5">
          <el-form-item
            :label="$t('fields.mainSourceOfIncome')"
            prop="mainIncomeSource"
          >
            <el-select
              v-model="formData.mainIncomeSource"
              :placeholder="$t('fields.select')"
              style="width: 100%"
              clearable
              :disabled="isSubmitting"
            >
              <el-option
                v-for="(item, index) in mainIncomeSources"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
        <div
          class="col-lg-4 col-md-6 mb-5"
          v-if="formData.mainIncomeSource == 7"
        >
          <el-form-item label="&nbsp;">
            <el-input
              class="w-250px"
              v-model="formData.mainIncomeSourceOther"
              :disabled="isSubmitting"
            >
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-4 col-lg-6 mb-5">
          <el-form-item
            :label="$t('fields.transactionMotive')"
            prop="transactionMotive"
          >
            <el-select
              v-model="formData.transactionMotive"
              :placeholder="$t('fields.select')"
              style="width: 100%"
              clearable
              :disabled="isSubmitting"
            >
              <el-option
                v-for="(item, index) in transactionMotives"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
        <div
          class="col-lg-4 col-md-6 mb-5"
          v-if="
            formData.transactionMotive == 6 || formData.transactionMotive == 5
          "
        >
          <el-form-item label="&nbsp;">
            <el-input
              class="w-250px"
              v-model="formData.transactionMotiveOther"
              :disabled="isSubmitting"
            >
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-4 col-lg-6 mb-5">
          <el-form-item
            :label="$t('fields.characteristicsOfMainFunds')"
            prop="mainFund"
          >
            <el-select
              v-model="formData.mainFund"
              :placeholder="$t('fields.select')"
              style="width: 100%"
              clearable
              :disabled="isSubmitting"
            >
              <el-option
                v-for="(item, index) in mainFunds"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
        <div
          class="col-lg-4 col-md-6 mb-5"
          v-if="formData.mainFund == 6 || formData.mainFund == 5"
        >
          <el-form-item label="&nbsp;">
            <el-input
              class="w-250px"
              v-model="formData.mainFundOther"
              :disabled="isSubmitting"
            >
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.assetManagementPeriod')"
            prop="assetManagementPeriod"
          >
            <el-select
              v-model="formData.assetManagementPeriod"
              :disabled="isSubmitting"
              :placeholder="$t('fields.select')"
              style="width: 100%"
            >
              <el-option
                v-for="(item, index) in assetManagementPeriodSelection"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
      </div>
    </div>
  </div>

  <div class="card my-5">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.relevantTradingExperience") }}
        </h2>
      </div>
      <hr />

      <el-form-item
        :label="'FX' + ' (' + $t('fields.foreignExchangeMarginTrading') + ')'"
        prop="fx"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.fx"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.stockTradingSpot')"
        prop="stockTradingSpot"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.stockTradingSpot"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.stockTradingCredit')"
        prop="stockTradingCredit"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.stockTradingCredit"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.indexOption')"
        prop="indexOption"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.indexOption"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.commodityFuturesTrading')"
        prop="commodityFuturesTrading"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.commodityFuturesTrading"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.otherDerivativesTransaction')"
        prop="otherDerivativesTransaction"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.otherDerivativesTransaction"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-radio-button
            v-for="(v_item, index) in experienceSelections"
            :key="index"
            :label="v_item.value"
            class="col-lg-4 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.investmentPurpose')"
        prop="investmentPurpose"
        class="mb-5"
      >
        <el-checkbox-group
          v-model="formData.investmentPurpose"
          size="large"
          class="row"
          :disabled="isSubmitting"
        >
          <el-checkbox
            v-for="(v_item, index) in investmentPurposes"
            :key="index"
            :label="v_item.value"
            class="col-lg-3 col-md-6 mb-3"
          >
            {{ v_item.label }}
          </el-checkbox>
        </el-checkbox-group>
      </el-form-item>
    </div>
  </div>
  <div class="card my-5">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.marginReturnAccount") }}
        </h2>
      </div>
      <hr />

      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.financialInstiutionName')"
            prop="financialInstiutionName"
          >
            <el-input
              v-model="formData.financialInstiutionName"
              :disabled="isSubmitting"
            >
            </el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.financialInstiutionType')"
            prop="financialInstiutionType"
          >
            <el-select
              v-model="formData.financialInstiutionType"
              :placeholder="$t('fields.select')"
            >
              <el-option
                v-for="(item, index) in financialInstiutionTypes"
                :key="index"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.financialInstitutionCode')"
            prop="financialInstitutionCode"
          >
            <el-input
              v-model="formData.financialInstitutionCode"
              :disabled="isSubmitting"
            >
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.branchName')" prop="branchName">
            <el-input v-model="formData.branchName" :disabled="isSubmitting">
            </el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.branchCode')" prop="branchCode">
            <el-input v-model="formData.branchCode" :disabled="isSubmitting">
            </el-input>
          </el-form-item>
        </div>
      </div>

      <el-form-item
        :label="$t('fields.depositType')"
        prop="depositType"
        class="mb-5"
      >
        <el-radio-group v-model="formData.depositType" :disabled="isSubmitting">
          <el-radio
            v-for="item in depositTypesSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>

      <div class="row pb-2">
        <div class="col-lg-8 pb-2">
          <el-form-item
            :label="$t('fields.accountHolderFullWidth')"
            prop="accountHolderFullWidth"
          >
            <el-input
              v-model="formData.accountHolderFullWidth"
              :disabled="isSubmitting"
            >
              <template #append>
                <div class="w-150px">
                  {{ $t("fields.accountHolderFullWidthSample") }}
                </div></template
              >
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2">
        <div class="col-lg-8 pb-2">
          <el-form-item
            :label="$t('fields.accountHolderFullWidthKatakana')"
            prop="accountHolderFullWidthKatakana"
          >
            <el-input
              v-model="formData.accountHolderFullWidthKatakana"
              :disabled="isSubmitting"
            >
              <template #append>
                <div class="w-150px">
                  {{ $t("fields.accountHolderFullWidthKatakanaSample") }}
                </div>
              </template>
            </el-input>
          </el-form-item>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, onMounted } from "vue";
import {
  investorDistinctionSelection,
  moneyRange,
  mainIncomeSources,
  transactionMotives,
  mainFunds,
  experienceSelections,
  investmentPurposes,
  assetManagementPeriodSelection,
  financialInstiutionTypes,
  depositTypesSelection,
} from "@/core/types/jp/verificationFinancial";
const items = inject<any>("items");
const formData = inject<any>("formData");
const isSubmitting = inject<any>("isSubmitting");

const item = ref<any>(items?.value?.data?.financial || {});

onMounted(() => {
  formData.value = item.value;
});
</script>

<style scoped>
:deep .el-form-item__label {
  color: #181c32;
  font-weight: 600;
  font-size: 1.075rem;
}
:deep .el-radio-button__inner {
  width: 100%;
  text-align: left;
  border: var(--el-border) !important;
  border-radius: var(--el-border-radius-base) !important;
  box-shadow: 0 0 0 0
    var(--el-radio-button-checked-border-color, var(--el-color-primary)) !important;
}
</style>
