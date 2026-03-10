<template>
  <div class="row">
    <div class="col-lg-7 pb-2">
      <el-form-item
        :label="$t('fields.preferredTradingAccount')"
        prop="accountTypes"
        class="mb-5"
      >
        <el-checkbox-group v-model="formData.accountTypes" :disabled="disabled">
          <el-checkbox
            v-for="item in accountTypesSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-checkbox>
        </el-checkbox-group>
      </el-form-item>
    </div>
    <div class="col-lg-5 pb-2">
      <el-form-item
        :label="$t('fields.accountType')"
        prop="accountRole"
        class="mb-5"
      >
        <el-radio-group v-model="formData.accountRole" :disabled="disabled">
          <el-radio
            v-for="item in accountRolesSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-7 pb-2">
      <el-form-item
        :label="$t('fields.investorDistinction')"
        prop="investorDistinction"
      >
        <el-radio-group
          v-model="formData.investorDistinction"
          :disabled="disabled"
        >
          <el-radio
            v-for="item in investorDistinctionSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.annualIncome')"
        prop="annualIncome"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.annualIncome"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="getItemByValue(formData.annualIncome, moneyRange).value"
          >
            {{ getItemByValue(formData.annualIncome, moneyRange).label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.valueOfInvestment')"
        prop="investment"
        class="mb-5"
      >
        <el-radio-group v-model="formData.investment" size="large" class="row">
          <el-radio-button
            :label="getItemByValue(formData.investment, moneyRange).value"
            :disabled="disabled"
          >
            {{ getItemByValue(formData.investment, moneyRange).label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.amountOfInvestmentFunds')"
        prop="investmentFunds"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.investmentFunds"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="getItemByValue(formData.investmentFunds, moneyRange).value"
          >
            {{ getItemByValue(formData.investmentFunds, moneyRange).label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>
  <div class="row">
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.mainSourceOfIncome')"
        prop="mainSourceOfIncome"
        class="mb-5"
      >
        <el-select
          v-model="formData.mainIncomeSource"
          :placeholder="$t('fields.select')"
          class="w-250px"
          clearable
          :disabled="disabled"
        >
          <el-option
            :label="
              getItemByValue(formData.mainIncomeSource, mainIncomeSources).label
            "
            :value="
              getItemByValue(formData.mainIncomeSource, mainIncomeSources).value
            "
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.transactionMotive')"
        prop="transactionMotive"
        class="mb-5"
      >
        <el-select
          v-model="formData.transactionMotive"
          :placeholder="$t('fields.select')"
          class="w-250px"
          clearable
          :disabled="disabled"
        >
          <el-option
            :label="
              getItemByValue(formData.transactionMotive, transactionMotives)
                .label
            "
            :value="
              getItemByValue(formData.transactionMotive, transactionMotives)
                .value
            "
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.characteristicsOfMainFunds')"
        prop="mainFund"
        class="mb-5"
      >
        <el-select
          v-model="formData.mainFund"
          :placeholder="$t('fields.select')"
          class="w-250px"
          clearable
          :disabled="disabled"
        >
          <el-option
            :label="getItemByValue(formData.mainFund, mainFunds).label"
            :value="getItemByValue(formData.mainFund, mainFunds).value"
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.assetManagementPeriod')"
        prop="assetManagementPeriod"
        class="mb-5"
      >
        <el-select
          v-model="formData.assetManagementPeriod"
          :placeholder="$t('fields.select')"
          class="w-250px"
          clearable
          :disabled="disabled"
        >
          <el-option
            :label="
              getItemByValue(
                formData.assetManagementPeriod,
                assetManagementPeriodSelection
              ).label
            "
            :value="
              getItemByValue(
                formData.assetManagementPeriod,
                assetManagementPeriodSelection
              ).value
            "
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="'FX' + ' (' + $t('fields.foreignExchangeMarginTrading') + ')'"
        prop="fx"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.fx"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="getItemByValue(formData.fx, experienceSelections).value"
          >
            {{ getItemByValue(formData.fx, experienceSelections).label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.stockTradingSpot')"
        prop="stockTradingSpot"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.stockTradingSpot"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="
              getItemByValue(formData.stockTradingSpot, experienceSelections)
                .value
            "
          >
            {{
              getItemByValue(formData.stockTradingSpot, experienceSelections)
                .label
            }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>

    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.stockTradingCredit')"
        prop="stockTradingCredit"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.stockTradingCredit"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="
              getItemByValue(formData.stockTradingCredit, experienceSelections)
                .value
            "
          >
            {{
              getItemByValue(formData.stockTradingCredit, experienceSelections)
                .label
            }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.indexOption')"
        prop="indexOption"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.indexOption"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="
              getItemByValue(formData.indexOption, experienceSelections).value
            "
          >
            {{
              getItemByValue(formData.indexOption, experienceSelections).label
            }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.commodityFuturesTrading')"
        prop="commodityFuturesTrading"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.commodityFuturesTrading"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="
              getItemByValue(
                formData.commodityFuturesTrading,
                experienceSelections
              ).value
            "
          >
            {{
              getItemByValue(
                formData.commodityFuturesTrading,
                experienceSelections
              ).label
            }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="col-lg-4 pb-2">
      <el-form-item
        :label="$t('fields.otherDerivativesTransaction')"
        prop="otherDerivativesTransaction"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.otherDerivativesTransaction"
          size="large"
          class="row"
          :disabled="disabled"
        >
          <el-radio-button
            :label="
              getItemByValue(
                formData.otherDerivativesTransaction,
                experienceSelections
              ).value
            "
          >
            {{
              getItemByValue(
                formData.otherDerivativesTransaction,
                experienceSelections
              ).label
            }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div>
    <el-form-item
      :label="$t('fields.investmentPurposeOfFxTrading')"
      prop="investmentPurpose"
      class="mb-5"
    >
      <el-checkbox-group
        v-model="formData.investmentPurpose"
        size="large"
        class="row"
        :disabled="disabled"
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

  <div class="row">
    <div class="col-lg-6">
      <el-form-item
        :label="$t('fields.financialInstiutionName')"
        prop="financialInstiutionName"
      >
        <el-input
          v-model="formData.financialInstiutionName"
          :disabled="disabled"
        ></el-input>
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item
        :label="$t('fields.financialInstiutionType')"
        prop="financialInstiutionType"
      >
        <el-select
          v-model="formData.financialInstiutionType"
          :placeholder="$t('fields.select')"
          class="w-250px"
          clearable
          :disabled="disabled"
        >
          <el-option
            :label="
              getItemByValue(
                formData.financialInstiutionType,
                financialInstiutionTypes
              ).label
            "
            :value="
              getItemByValue(
                formData.financialInstiutionType,
                financialInstiutionTypes
              ).value
            "
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
  </div>

  <div class="row pb-2">
    <div class="col-lg-6 pb-2">
      <el-form-item :label="$t('fields.branchName')" prop="branchName">
        <el-input v-model="formData.branchName" :disabled="disabled">
        </el-input>
      </el-form-item>
    </div>

    <div class="col-lg-6 pb-2">
      <el-form-item :label="$t('fields.branchCode')" prop="branchCode">
        <el-input v-model="formData.branchCode" :disabled="disabled">
        </el-input>
      </el-form-item>
    </div>

    <div class="col-lg-6 pb-2">
      <el-form-item :label="$t('fields.accountNumber')" prop="accountNumber">
        <el-input v-model="formData.accountNumber" :disabled="disabled">
        </el-input>
      </el-form-item>
    </div>

    <div class="col-lg-6 pb-2">
      <el-form-item
        :label="$t('fields.depositType')"
        prop="depositType"
        class="mb-5"
      >
        <el-radio-group v-model="formData.depositType" :disabled="disabled">
          <el-radio
            v-for="item in depositTypesSelection"
            :key="item.value"
            :label="item.value"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
    </div>

    <div class="row pb-2">
      <div class="col-lg-6 pb-2">
        <el-form-item
          :label="$t('fields.accountHolderFullWidth')"
          prop="accountHolderFullWidth"
        >
          <el-input
            v-model="formData.accountHolderFullWidth"
            :disabled="disabled"
          >
          </el-input>
        </el-form-item>
      </div>

      <div class="col-lg-6 pb-2">
        <el-form-item
          :label="$t('fields.accountHolderFullWidthKatakana')"
          prop="accountHolderFullWidthKatakana"
        >
          <el-input
            v-model="formData.accountHolderFullWidthKatakana"
            :disabled="disabled"
          >
          </el-input>
        </el-form-item>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import {
  accountTypes,
  accountTypesSelection,
  accountRolesSelection,
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
const formData = inject<any>("formData");
const verificationDetails = inject<any>("verificationDetails");
const disabled = inject<any>("disabled");

const getItemByValue = (value, items) => {
  if (Array.isArray(items)) {
    return (
      items.find((item) => item.value === value) || { label: "", value: "" }
    );
  } else if (typeof items === "object") {
    return (
      Object.values(items).find((item) => item.value === value) || {
        label: "",
        value: "",
      }
    );
  }
  return {
    label: "",
    value: "",
  };
};

onMounted(() => {
  formData.value = verificationDetails.value.financial;
});
</script>
