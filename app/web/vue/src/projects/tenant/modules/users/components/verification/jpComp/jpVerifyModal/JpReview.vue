<template>
  <div id="print">
    <div>
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
          items.info?.firstName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.lastName')">{{
          items.info?.lastName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.nativeNameLast')">{{
          items.info?.nativeNameLast
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.nativeNameFirst')">{{
          items.info?.nativeNameFirst
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.gender')">{{
          items.info?.gender == "0" ? $t("fields.male") : $t("fields.female")
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.birthdate')">{{
          items.info?.birthday
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.postalCode')">{{
          items.info?.postalCode
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.region')">{{
          items.info?.region
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.village')">{{
          items.info?.town
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.town')">{{
          items.info?.town
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.street')">{{
          items.info?.street
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.buildingNumber')">{{
          items.info?.buildingNumber
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.email')">{{
          items.info?.email
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.phone')">{{
          items.info?.phone
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.occupation')">{{
          getLabelByValue(items.info?.occupation, occupations)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.companyName')">{{
          items.info?.companyName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.affiliatedDepartment')">{{
          items.info?.affiliatedDepartment
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.affiliatedPosition')">{{
          items.info?.affiliatedPosition
        }}</el-descriptions-item>

        <el-descriptions-item
          v-if="items.info?.citizen != true"
          :label="$t('fields.nationalityInformation')"
          >{{ getDataByCode(items.info?.citizen)?.name }}</el-descriptions-item
        >
        <el-descriptions-item
          v-else
          :label="$t('fields.nationalityInformation')"
          >{{
            getDataByCode(items.info?.otherCitizen)?.name
          }}</el-descriptions-item
        >

        <el-descriptions-item :label="$t('fields.countryOfResidence')">
          <div v-if="items.info.countryOfResidence == 'jp'">
            {{ getDataByCode(items.info?.countryOfResidence)?.name }}
          </div>
          <div v-else>{{ $t("fields.residenceThanJapan") }}</div>
        </el-descriptions-item>

        <el-descriptions-item :label="$t('fields.usTaxLiability')">{{
          items.info?.usTax == true ? $t("action.yes") : $t("action.no")
        }}</el-descriptions-item>

        <el-descriptions-item
          v-if="items.info?.declarationRegardingForeignPeps != true"
          :label="$t('fields.declarationRegardingForeignPeps')"
          >{{ $t("action.no") }}</el-descriptions-item
        >
        <el-descriptions-item
          v-else
          :label="$t('fields.declarationRegardingForeignPeps')"
          >{{ items.info?.otherPeps }}</el-descriptions-item
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
        <el-descriptions-item :label="$t('fields.accountType')">{{
          getLabelByValue(items.financial?.accountRole, accountRolesSelection)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.preferredTradingAccount')">
          <div v-for="item in items.financial?.accountTypes" :key="item">
            {{ getLabelByValue(item, accountTypesSelection) }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.investorDistinction')"
          :span="2"
        >
          <div v-for="item in items.financial?.investorDistinction" :key="item">
            {{ getLabelByValue(item, investorDistinctionSelection) }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item :label="$t('fields.annualIncome')">{{
          searchByValue(items.financial?.annualIncome, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.valueOfInvestment')">{{
          searchByValue(items.financial?.investment, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.amountOfInvestmentFunds')">{{
          searchByValue(items.financial?.investmentFunds, moneyRange)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.mainSourceOfIncome')">{{
          getLabelByValue(items.financial?.mainIncomeSource, mainIncomeSources)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.transactionMotive')">
          <div v-if="!items.financial?.transactionMotiveOther">
            {{
              getLabelByValue(
                items.financial?.transactionMotive,
                transactionMotives
              )
            }}
          </div>
          <div v-else>
            {{ (items.financial?.transactionMotive, transactionMotives) }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.characteristicsOfMainFunds')"
          >{{
            getLabelByValue(items.financial?.mainFund, mainFunds)
          }}</el-descriptions-item
        >

        <el-descriptions-item :label="$t('fields.assetManagementPeriod')">{{
          getLabelByValue(
            items.financial?.assetManagementPeriod,
            assetManagementPeriodSelection
          )
        }}</el-descriptions-item>

        <el-descriptions-item
          :label="'FX' + ' (' + $t('fields.foreignExchangeMarginTrading') + ')'"
          >{{
            getLabelByValue(items.financial?.fx, experienceSelections)
          }}</el-descriptions-item
        >
        <el-descriptions-item :label="$t('fields.stockTradingSpot')">{{
          getLabelByValue(
            items.financial?.stockTradingSpot,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.stockTradingCredit')">{{
          getLabelByValue(
            items.financial?.stockTradingCredit,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.indexOption')">{{
          getLabelByValue(items.financial?.indexOption, experienceSelections)
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.commodityFuturesTrading')">{{
          getLabelByValue(
            items.financial?.commodityFuturesTrading,
            experienceSelections
          )
        }}</el-descriptions-item>
        <el-descriptions-item
          :label="$t('fields.otherDerivativesTransaction')"
          >{{
            getLabelByValue(
              items.financial?.otherDerivativesTransaction,
              experienceSelections
            )
          }}</el-descriptions-item
        >

        <el-descriptions-item :label="$t('fields.investmentPurpose')">
          <div v-for="item in items.financial?.investmentPurpose" :key="item">
            {{ getLabelByValue(item, investmentPurposes) }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item :label="$t('fields.financialInstiutionName')">{{
          items.financial?.financialInstiutionName
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.financialInstiutionType')">{{
          getLabelByValue(
            items.financial?.financialInstiutionType,
            financialInstiutionTypes
          )
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.branchName')">{{
          items.financial?.branchName
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.branchCode')">
          <div>
            {{ items.financial?.branchCode }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item :label="$t('fields.accountNumber')">
          <div>
            {{ items.financial?.accountNumber }}
          </div>
        </el-descriptions-item>

        <el-descriptions-item :label="$t('fields.depositType')">{{
          getLabelByValue(items.financial?.depositType, depositTypesSelection)
        }}</el-descriptions-item>

        <el-descriptions-item :label="$t('fields.financialInstiutionName')">{{
          items.financial?.financialInstiutionName
        }}</el-descriptions-item>

        <el-descriptions-item
          :label="$t('fields.accountHolderFullWidthKatakana')"
          >{{
            items.financial?.accountHolderFullWidthKatakana
          }}</el-descriptions-item
        >
      </el-descriptions>

      <div class="py-3">
        <h2
          class="fw-bold d-flex align-items-center text-dark ms-3 mt-3 mt-lg-0"
        >
          {{ $t("title.agreement") }}
        </h2>
      </div>
      <hr />
      <div class="box-container">
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            {{ $t("type.jpAgreement.check1") }}
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_1"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_1">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type margin -->
        <div
          class="d-flex bottom"
          v-if="items.financial.accountTypes.includes(accountTypes.margin)"
        >
          <div class="col-9 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeMarginTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeMarginTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.foreginExchangeTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreginExchangeTradingManual.title }}</a
              >
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_2"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_2">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type cfd -->
        <div
          class="d-flex bottom"
          v-if="items.financial.accountTypes.includes(accountTypes.cfdTrading)"
        >
          <div class="col-9 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeMarginTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeMarginTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.cfdTradingTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.cfdTradingTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.cfdTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.cfdTradingManual.title }}</a
              >
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_3"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_3">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type cfd_2 -->
        <div
          class="d-flex bottom"
          v-if="
            items.financial.accountTypes.includes(accountTypes.productTrading)
          "
        >
          <div class="col-9 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.commodityCfdTradingTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.commodityCfdTradingTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTradingManual.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTradingForCorporations.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTradingForCorporations.title }}</a
              >
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_4"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_4">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.agreementRegardingElectronicDelivery.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.agreementRegardingElectronicDelivery.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.personalInformationProtectionDeclaration.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.personalInformationProtectionDeclaration.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.agreementForAntiSocialForce.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.agreementForAntiSocialForce.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.notificationOfNotFallingUnderForeignPeps.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.notificationOfNotFallingUnderForeignPeps.title }}</a
              >
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_5"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_5">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div>①{{ $t("type.jpAgreement.check2Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check2") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_6"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_6">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div>②{{ $t("type.jpAgreement.check3Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check3") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_7"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_7">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div>③{{ $t("type.jpAgreement.check4Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check4") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_8"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_8">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div>④{{ $t("type.jpAgreement.check5Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check5") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_9"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_9">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex bottom">
          <div class="col-9 col-md-10 left">
            <div>⑤{{ $t("type.jpAgreement.check6Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check6") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_10"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_10">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="d-flex">
          <div class="col-9 col-md-10 left">
            <div class="text-decoration-underline">
              {{ $t("type.jpAgreement.check7") }}
            </div>
          </div>
          <div class="col-3 col-md-2 right">
            <el-form-item
              prop="check_11"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="items.agreement.check_11">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { occupations } from "@/core/types/jp/verificationInfo";
import {
  accountTypes,
  accountTypesSelection,
  accountRolesSelection,
  moneyRange,
  mainIncomeSources,
  transactionMotives,
  mainFunds,
  experienceSelections,
  investmentPurposes,
  assetManagementPeriodSelection,
  financialInstiutionTypes,
  depositTypesSelection,
  investorDistinctionSelection,
} from "@/core/types/jp/verificationFinancial";
import { getDataByCode } from "@/core/data/phonesData";
import { isMobile } from "@/core/config/WindowConfig";
import { jpDocs } from "@/core/data/bcrDocs";
const items = inject<any>("verificationDetails");
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
</script>
<style scoped>
:deep .el-descriptions__cell {
  word-break: normal !important;
  text-wrap: balance !important;
}

:deep tr {
  page-break-inside: avoid; /* Avoid breaking inside table rows */
}

@media (max-width: 768px) {
  :deep .card-body {
    padding: 0rem 0rem;
  }
}

.box-container {
  border: 1px solid #cccccc;
  border-radius: 10px;
  box-sizing: border-box;
}
.bottom {
  border-bottom: 1px solid #cccccc;
}
.left {
  border-right: 1px solid #cccccc;
  padding: 20px;
  letter-spacing: 0.1px;
}
.right {
  text-align: center;
  margin: auto;
}

.pdf {
  color: #000;
  text-decoration: underline;
  cursor: pointer;
}
.pdf:hover {
  color: #409eff;
  text-decoration: underline;
}

:deep .el-form-item__content {
  justify-content: center;
}
</style>
