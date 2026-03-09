<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.financial") }}
      </div>
    </div>

    <div class="card-body">
      <!-- <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark">
            {{ $t("title.financial") }}
          </h2>
        </div>
        <hr /> -->
      <el-form-item :label="$t('fields.industry')" prop="industry" class="mb-5">
        <el-select v-model="formData.industry" name="industry" size="large">
          <el-option :value="$t('fields.government')">{{
            $t("fields.government")
          }}</el-option>
          <el-option :value="$t('fields.accounting')">{{
            $t("fields.accounting")
          }}</el-option>
          <el-option :value="$t('fields.banking')">{{
            $t("fields.banking")
          }}</el-option>
          <el-option :value="$t('fields.finance')">{{
            $t("fields.finance")
          }}</el-option>
          <el-option :value="$t('fields.insurance')">{{
            $t("fields.insurance")
          }}</el-option>
          <el-option :value="$t('fields.other')">{{
            $t("fields.other")
          }}</el-option>
        </el-select>
      </el-form-item>

      <el-form-item :label="$t('fields.position')" prop="position" class="mb-5">
        <el-radio-group
          v-model="formData.position"
          name="position"
          class="row w-100"
          :class="isMobile ? 'gap-3' : 'gap-6'"
          style="margin-left: 0.1rem"
        >
          <el-radio
            v-for="pos in position"
            :key="pos.id"
            :label="pos.value"
            :class="isMobile ? 'col-12' : 'col'"
            border
          >
            {{ pos.text }}
          </el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.annualIncome')"
        prop="income"
        class="mb-5"
      >
        <el-radio-group
          v-if="!isMobile"
          v-model="formData.income"
          name="income"
          class="d-flex flex-wrap"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio
            v-for="pos in range_section"
            :key="pos.id"
            :label="pos.value"
            style="flex: 1 1 calc(33.333% - 1rem)"
            border
          >
            {{ pos.text }}
          </el-radio>
        </el-radio-group>
        <el-radio-group
          v-else
          v-model="formData.income"
          name="income"
          class="row gap-3 w-100"
        >
          <el-radio
            v-for="pos in range_section"
            :key="pos.id"
            :label="pos.value"
            class="col-12"
            border
          >
            {{ pos.text }}
          </el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.valueOfInvestment')"
        prop="investment"
        class="mb-5"
      >
        <el-radio-group
          v-if="!isMobile"
          v-model="formData.investment"
          name="investment"
          class="d-flex flex-wrap"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio
            v-for="pos in range_section"
            :key="pos.id"
            :label="pos.value"
            style="flex: 1 1 calc(33.333% - 1rem)"
            border
          >
            {{ pos.text }}
          </el-radio>
        </el-radio-group>

        <el-radio-group
          v-else
          v-model="formData.investment"
          name="investment"
          class="row gap-3 w-100"
        >
          <el-radio
            v-for="pos in range_section"
            :key="pos.id"
            :label="pos.value"
            class="col-12"
            border
          >
            {{ pos.text }}
          </el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.howToFundTrading')"
        prop="fund"
        class="mb-5"
      >
        <el-checkbox-group
          v-model="formData.fund"
          class="d-flex flex-wrap w-100"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
          @change="initHasOtherFund"
        >
          <el-checkbox
            v-for="pos in funds"
            :key="pos.id"
            :label="pos.value"
            style="flex: 1 1 calc(33.333% - 1rem); height: 47.6px"
            border
          >
            {{ pos.text }}
          </el-checkbox>
        </el-checkbox-group>
      </el-form-item>

      <el-form-item
        :label="$t('fields.other')"
        class="mb-5"
        prop="otherFunds"
        v-if="hasOtherFund"
      >
        <el-input
          v-model="formData.otherFunds"
          tabindex="2"
          type="text"
          name="other-funds"
          :placeholder="$t('tip.useCommaSeparate')"
          autocomplete="off"
          size="large"
        />
      </el-form-item>
    </div>
    <div class="separate-line"></div>
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.relevantTradingExperience") }}
      </div>
    </div>
    <div class="card-body">
      <!-- <div class="card-body">
        <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
            {{ $t("title.relevantTradingExperience") }}
          </h2>
        </div>
        <hr />
      </div> -->

      <el-form-item
        :label="$t('tip.haveYouEverAttendedAnEducationSeminar')"
        prop="bg1"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.bg1"
          name="bg1"
          class="row w-100"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio label="0" border class="col-4 col-lg-2">
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio label="1" border class="col-4 col-lg-2">{{
            $t("action.no")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item
        :label="$t('tip.previousExperienceTradingStockBondsCFD')"
        prop="bg2"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.bg2"
          name="bg2"
          class="row w-100"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio label="0" border class="col-4 col-lg-2">
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio label="1" border class="col-4 col-lg-2">{{
            $t("action.no")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>
    </div>
    <div class="separate-line"></div>
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.backgroundInformation") }}
      </div>
    </div>
    <div class="card-body">
      <!-- <div class="card-body">
        <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
            {{ $t("title.backgroundInformation") }}
          </h2>
        </div>
        <hr />
      </div> -->

      <el-form-item
        :label="$t('tip.verificationFinancialAreYouBcrEmployee')"
        prop="exp1"
        class="mb-5"
      >
        <el-radio-group
          v-model="formData.exp1"
          name="exp1"
          class="row w-100"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio label="0" border class="col-4 col-lg-2">
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio label="1" border class="col-4 col-lg-2">{{
            $t("action.no")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>
      <div v-if="formData.exp1 == '0'">
        <el-form-item :label="$t('fields.employer')" prop="exp1_employer">
          <el-input
            v-model="formData.exp1_employer"
            tabindex="2"
            type="text"
            name="exp1_employer"
            size="large"
            autocomplete="off"
          />
        </el-form-item>

        <el-form-item :label="$t('fields.position')" prop="exp1_position">
          <el-input
            v-model="formData.exp1_position"
            tabindex="2"
            type="text"
            name="exp1_position"
            size="large"
            autocomplete="off"
          />
        </el-form-item>
        <el-form-item
          :label="$t('fields.remuneration')"
          prop="exp1_remuneration"
        >
          <el-input
            v-model="formData.exp1_remuneration"
            tabindex="2"
            type="text"
            name="exp1_remuneration"
            size="large"
            autocomplete="off"
          />
        </el-form-item>
      </div>

      <el-form-item
        :label="v_item.text"
        :prop="v_item.id"
        class="mb-5"
        v-for="(v_item, index) in trading_exp"
        :key="index"
      >
        <el-radio-group
          v-model="formData[v_item.id]"
          :name="v_item.id"
          class="row w-100"
          style="margin-left: 0.1rem; column-gap: 1.5rem; row-gap: 1rem"
        >
          <el-radio label="0" border class="col-4 col-lg-2">
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio label="1" border class="col-4 col-lg-2">{{
            $t("action.no")
          }}</el-radio>
        </el-radio-group>

        <el-form-item
          :label="v_item.more_question"
          :prop="v_item.id + '_more'"
          class="mb-5 w-100 mt-5"
          v-if="formData[v_item.id] == '0'"
        >
          <el-input
            v-model="formData[v_item.id + '_more']"
            type="text"
            size="large"
            autocomplete="off"
          />
        </el-form-item>
      </el-form-item>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useVerificationForm } from "../../composables/useVerificationForm";
import { useFinancialOptions } from "../../composables/useFinancialOptions";

const { items, formData } = useVerificationForm();
const { range_section, position, funds, trading_exp } = useFinancialOptions();

const item = ref<any>(items?.value?.data?.financial || {});
const hasOtherFund = ref(false);

const initHasOtherFund = () => {
  if (formData.value.fund) {
    hasOtherFund.value = formData.value.fund.includes(funds.value.fund_4.value);
  }
};

onMounted(() => {
  formData.value = item.value;
  initHasOtherFund();
});
</script>
<style scoped lang="scss">
// ::v-deep .el-input__inner {
//   font-size: 1.25rem;
//   font-weight: 600;
// }

// ::v-deep .el-form-item__label {
//   color: #181c32;
//   font-weight: 600;
//   font-size: 1.075rem;
// }

// ::v-deep .question .el-form-item__label {
//   font-weight: 500;
// }

::v-deep .el-radio .el-radio__label {
  // font-weight: 600 !important;
  // font-size: 1.25rem !important;
  padding: 0.5rem;

  @media (max-width: 768px) {
    font-size: 1rem !important;
  }
}

::v-deep .el-radio {
  height: auto;
  cursor: pointer;
  border-radius: 0.475rem;
  margin-right: 0;
}

::v-deep .el-radio.is-bordered.is-checked {
  background-color: #0a46aa;
  color: #fff !important;
}
::v-deep .el-radio__input.is-checked + .el-radio__label {
  color: #fff !important;
}
::v-deep .el-radio__input {
  display: none;
}

::v-deep .el-checkbox__input {
  display: none;
}
::v-deep .el-checkbox .el-checkbox__label {
  // font-weight: 600 !important;
  // font-size: 1.25rem !important;
  padding: 0.5rem;

  @media (max-width: 768px) {
    font-size: 1rem !important;
  }
}

::v-deep .el-checkbox {
  height: auto;
  cursor: pointer;
  border-radius: 0.475rem;
  margin-right: 0;
}

::v-deep .el-checkbox.is-bordered.is-checked {
  background-color: #0053ad;
  color: #fff !important;
}
::v-deep .el-checkbox__input.is-checked + .el-checkbox__label {
  color: #fff !important;
}
</style>
