<template>
  <div class="card">
    <div class="card-body">
      <div class="pb-3">
        <h2
          class="fw-bold d-flex align-items-center text-dark ms-3 mt-3 mt-lg-0"
        >
          {{ $t("title.personalInfo") }}
        </h2>
      </div>
      <hr />
      <el-descriptions :column="colSize" size="large" border class="pb-3">
        <el-descriptions-item :label="$t('fields.firstName')">{{
          items.data?.info?.firstName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.lastName')">{{
          items.data?.info?.lastName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.nativeNameLast')">{{
          items.data?.info?.nativeNameLast
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.nativeNameFirst')">{{
          items.data?.info?.nativeNameFirst
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.gender')" span="2">{{
          items.data?.info?.gender == "0"
            ? $t("fields.male")
            : $t("fields.female")
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.birthdate')">{{
          items.data?.info?.birthday
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.age')">{{
          items.data?.info?.age
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.postalCode')">{{
          items.data?.info?.postalCode
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.region')">{{
          items.data?.info?.region
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.village')">{{
          items.data?.info?.village
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.town')">{{
          items.data?.info?.town
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.street')">{{
          items.data?.info?.street
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.buildingNumber')">{{
          items.data?.info?.buildingNumber
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.email')">{{
          items.data?.info?.email
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.phone')">{{
          items.data?.info?.phone
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.occupation')">{{
          getLabelByValue(items.data?.info?.occupation, occupations)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.companyName')">{{
          items.data?.info?.companyName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.affiliatedDepartment')">{{
          items.data?.info?.affiliatedDepartment
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.affiliatedPosition')">{{
          items.data?.info?.affiliatedPosition
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.companyPhone')">{{
          items.data?.info?.companyPhone
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.nationalityInformation')">{{
          getDataByCodeUpdate(
            items.data?.info?.citizen,
            items.data?.info?.otherCitizen
          )
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.countryOfResidence')">{{
          getDataByCodeUpdate(
            items.data?.info?.countryOfResidence,
            $t("fields.residenceThanJapan")
          )
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.usTaxLiability')">{{
          items.data?.info?.usTax == true ? $t("action.yes") : $t("action.no")
        }}</el-descriptions-item>

        <el-descriptions-item
          :label="$t('fields.declarationRegardingForeignPeps')"
          span="2"
          >{{
            items.data?.info?.declarationRegardingForeignPeps == "no"
              ? $t("action.no")
              : items.data?.info?.otherPeps
          }}</el-descriptions-item
        >
      </el-descriptions>

      <div class="py-3">
        <h2
          class="fw-bold d-flex align-items-center text-dark ms-3 mt-3 mt-lg-0"
        >
          {{ $t("title.financial") }}
        </h2>
      </div>
      <hr />
      <el-descriptions :column="colSize" border class="pb-lg-3" size="large">
        <el-descriptions-item :label="$t('fields.accountType')" :span="2">{{
          getLabelByValue(
            items.data?.financial?.accountRole,
            accountRolesSelection
          )
        }}</el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.preferredTradingAccount')"
          :span="2"
        >
          <div v-for="item in items.data?.financial?.accountTypes" :key="item">
            {{ getLabelByValue(item, accountTypesSelection) }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item
          :label="$t('fields.investorDistinction')"
          :span="2"
        >
          <div
            v-for="item in items.data?.financial?.investorDistinction"
            :key="item"
          >
            {{ getLabelByValue(item, investorDistinctionSelection) }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item :label="$t('fields.mainSourceOfIncome')">{{
          getLabelByValue(
            items.data?.financial?.mainIncomeSource,
            mainIncomeSources
          )
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.annualIncome')">{{
          searchByValue(items.data?.financial?.annualIncome, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.valueOfInvestment')">{{
          searchByValue(items.data?.financial?.investment, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.amountOfInvestmentFunds')">{{
          searchByValue(items.data?.financial?.investmentFunds, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.mainSourceOfIncome')">{{
          getLabelByValue(
            items.data?.financial?.mainIncomeSource,
            mainIncomeSources
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.transactionMotive')">
          <div v-if="!items.data?.financial?.transactionMotiveOther">
            {{
              getLabelByValue(
                items.data?.financial?.transactionMotive,
                transactionMotives
              )
            }}
          </div>
          <div v-else>
            {{ (items.data?.financial?.transactionMotive, transactionMotives) }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.characteristicsOfMainFunds')"
          >{{
            getLabelByValue(items.data?.financial?.mainFund, mainFunds)
          }}</el-descriptions-item
        >
        <el-descriptions-item :label="$t('fields.assetManagementPeriod')">{{
          getLabelByValue(
            items.data?.financial?.assetManagementPeriod,
            assetManagementPeriodSelection
          )
        }}</el-descriptions-item>

        <el-descriptions-item
          :label="'FX' + ' (' + $t('fields.foreignExchangeMarginTrading') + ')'"
          >{{
            getLabelByValue(items.data?.financial?.fx, experienceSelections)
          }}</el-descriptions-item
        >
        <el-descriptions-item :label="$t('fields.stockTradingSpot')">{{
          getLabelByValue(
            items.data?.financial?.stockTradingSpot,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.stockTradingCredit')">{{
          getLabelByValue(
            items.data?.financial?.stockTradingCredit,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.indexOption')">{{
          getLabelByValue(
            items.data?.financial?.indexOption,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.commodityFuturesTrading')">{{
          getLabelByValue(
            items.data?.financial?.commodityFuturesTrading,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.otherDerivativesTransaction')"
          >{{
            getLabelByValue(
              items.data?.financial?.otherDerivativesTransaction,
              experienceSelections
            )
          }}</el-descriptions-item
        >

        <el-descriptions-item
          :label="$t('fields.investmentPurposeOfFxTrading')"
        >
          <div
            v-for="item in items.data?.financial?.investmentPurpose"
            :key="item"
          >
            {{ getLabelByValue(item, investmentPurposes) }}
          </div>
        </el-descriptions-item>
      </el-descriptions>

      <div class="py-3">
        <h2
          class="fw-bold d-flex align-items-center text-dark ms-3 mt-3 mt-lg-0"
        >
          {{ $t("title.marginReturnAccount") }}
        </h2>
      </div>
      <hr />
      <el-descriptions :column="colSize" border class="pb-lg-3" size="large">
        <el-descriptions-item :label="$t('fields.financialInstiutionName')">
          <div>
            {{ items.data?.financial?.financialInstiutionName }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.financialInstiutionType')">
          <div>
            {{
              getLabelByValue(
                items.data?.financial?.financialInstiutionType,
                financialInstiutionTypes
              )
            }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.financialInstitutionCode')">
          <div>
            {{ items.data?.financial?.financialInstitutionCode }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.branchName')">
          <div>
            {{ items.data?.financial?.branchName }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.branchCode')">
          <div>
            {{ items.data?.financial?.branchCode }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.accountNumber')">
          <div>
            {{ items.data?.financial?.accountNumber }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.depositType')">
          <div>
            {{
              getLabelByValue(
                items.data?.financial?.depositType,
                depositTypesSelection
              )
            }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.accountHolderFullWidth')">
          <div>
            {{ items.data?.financial?.accountHolderFullWidth }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.accountHolderFullWidthKatakana')"
        >
          <div>
            {{ items.data?.financial?.accountHolderFullWidthKatakana }}
          </div>
        </el-descriptions-item>
      </el-descriptions>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { occupations } from "@/core/types/jp/verificationInfo";
import {
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
import { getDataByCode } from "@/core/data/phonesData";
import { isMobile } from "@/core/config/WindowConfig";
const items = inject<any>("items");
const colSize = isMobile.value ? 1 : 2;
function getLabelByValue(value, array) {
  const data = array.find((data) => data.value === value);
  return data ? data.label : null;
}

function searchByValue(value, range) {
  for (const key in range) {
    if (range[key].value === value) {
      return range[key].label;
    }
  }
  return null; // If no match found
}

function getDataByCodeUpdate(code, answer) {
  var res = getDataByCode(code);
  res = res.name ?? null;
  if (!res) {
    return answer;
  }

  return res;
}
</script>
<style scoped>
:deep .el-descriptions__cell {
  word-break: normal !important;
  text-wrap: balance !important;
}

@media (max-width: 768px) {
  :deep .card-body {
    padding: 0rem 0rem;
  }
}
</style>
