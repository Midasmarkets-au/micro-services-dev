<template>
  <div class="verify-card">
    <div class="mb-10">
      <label class="form-label fs-5 fw-bold text-dark required">{{
        $t("fields.currency")
      }}</label>

      <el-select
        v-model="verificationStartData.currency"
        placeholder="Select"
        name="currency"
        size="large"
      >
        <el-option
          v-for="(item, index) in ConfigCurrencySelections"
          :key="index"
          :value="item.value"
          :label="item.label"
        />
      </el-select>
    </div>

    <div class="mb-10">
      <label class="form-label fw-bold text-dark fs-6 required">{{
        $t("fields.accountType")
      }}</label>
      <div class="row">
        <div
          class="col-lg-3 mb-2"
          v-for="typeItem in ConfigAllAccountTypeSelections"
          :key="'type_' + typeItem.value"
        >
          <Field
            v-model="verificationStartData.accountType"
            type="radio"
            class="btn-check"
            name="accountType"
            :value="typeItem.value"
            :id="'kt_create_account_form_account_type_tt' + typeItem.label"
          />

          <label
            class="btn btn-outline btn-outline-default d-flex align-items-center text-dark"
            :for="'kt_create_account_form_account_type_tt' + typeItem.label"
          >
            <span class="d-block fw-semobold text-start">
              <span class="fw-bold d-block fs-4">
                {{ typeItem.label }}
              </span>
            </span>
          </label>
        </div>
      </div>
    </div>

    <div class="mb-10">
      <label class="form-label fw-bold text-dark fs-6 required">{{
        $t("fields.platform")
      }}</label>
      <div class="row">
        <div
          class="col-lg-3 mb-2"
          v-for="serviceItem in ConfigAllServiceSelections"
          :key="'serviceId_' + serviceItem.value"
        >
          <Field
            v-model="verificationStartData.serviceId"
            type="radio"
            class="btn-check"
            name="serviceId"
            :value="serviceItem.id"
            :id="'kt_create_account_form_service_type_tt' + serviceItem.label"
          />

          <label
            class="btn btn-outline btn-outline-default d-flex align-items-center text-dark"
            :for="'kt_create_account_form_service_type_tt' + serviceItem.label"
          >
            <span class="d-block fw-semobold text-start">
              <span class="fw-bold d-block fs-4">
                {{ serviceItem.label }}
              </span>
            </span>
          </label>
        </div>
      </div>
    </div>

    <div class="mb-10">
      <label class="form-label fs-6 fw-bold text-dark required">{{
        $t("fields.leverage")
      }}</label>

      <el-select
        v-model="verificationStartData.leverage"
        name="leverage"
        size="large"
      >
        <el-option
          v-for="(item, index) in ConfigLeverageSelections"
          :key="index"
          :value="item.value"
          :label="item.label"
        />
      </el-select>
    </div>

    <div class="mb-10">
      <label class="form-label fs-6 fw-bold text-dark">{{
        $t("fields.referralCode")
      }}</label>

      <el-input
        v-model="verificationStartData.referral"
        name="referral"
        size="large"
        :disabled="verificationStartData.referral !== ''"
      />
      <div class="m-3" v-if="verificationStartData.referral">
        Open For: <b>{{ $t(`type.accountRole.${referInfo.serviceFor}`) }}</b>
        <br />
        Sales Code: <b>{{ referInfo.salesAccountCode }}</b>
        <br />
        Sales Name: <b>{{ referInfo.salesName }}</b>
        <br />
        IB Group: <b>{{ referInfo.accountGroupName }}</b>
        <br />
      </div>
    </div>

    <div class="pb-3">
      <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
        {{ $t("tip.ansBelowQuestions") }}
      </h2>
    </div>
    <hr />

    <div>
      <el-form-item
        class="row"
        v-for="(value, index) in verificationStartData.questions"
        :label="
          {
            q1: $t('tip.verificationQ1'),
            q2: $t('tip.verificationQ2'),
            q3: $t('tip.verificationQ3'),
            q4: $t('tip.verificationQ4'),
          }[index]
        "
        :key="index"
        :prop="'questions.' + index"
      >
        <el-radio-group
          v-model="verificationStartData.questions[index]"
          class="row mt-11 w-100"
        >
          <el-radio
            :label="true"
            :value="true"
            border
            class="col-4 col-lg-2 ms-2"
          >
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio
            :label="false"
            :value="true"
            border
            class="col-4 col-lg-2"
            >{{ $t("action.no") }}</el-radio
          >
        </el-radio-group>
      </el-form-item>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ErrorMessage, Field } from "vee-validate";
import {
  ConfigAllAccountTypeSelections,
  AccountRoleTypes,
} from "@/core/types/AccountInfos";
import GlobalService from "@/projects/tenant/services/TenantGlobalService";
import { ReferralServiceTypes } from "@/core/types/ReferralServiceTypes";
import { ConfigAllServiceSelections } from "@/core/types/ServiceTypes";

const props = defineProps<{
  verificationDetails: any;
}>();
const referInfo = ref<any>({
  referCode: "",
});
const verificationStartData = ref<any>({
  ...props.verificationDetails?.started,
});

watch(
  () => props.verificationDetails,
  () => {
    verificationStartData.value = {
      ...props.verificationDetails?.started,
    };
  }
);

onMounted(() => {
  getReferCodeDetail();
});

const getReferCodeDetail = async () => {
  if (verificationStartData.value.referral !== "") {
    referInfo.value = await GlobalService.getReferralCodeAccountInfo(
      verificationStartData.value.referral
    );
    referInfo.value.serviceFor = getAccountRoleDesignatedByReferCode(
      referInfo.value.serviceType
    );
  }
};

const getAccountRoleDesignatedByReferCode = (serviceType) =>
  ({
    [ReferralServiceTypes.Broker]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Agent]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Client]: AccountRoleTypes.Client,
    // for legacy data
    [200]: AccountRoleTypes.IB,
    [300]: AccountRoleTypes.IB,
    [400]: AccountRoleTypes.Client,
  }[serviceType] ?? AccountRoleTypes.Client);
</script>

<style scoped lang="scss">
::v-deep .el-form-item__label {
  color: #181c32;
  font-weight: 600;
  font-size: 1.075rem;
  justify-content: flex-start;
}

::v-deep .question .el-form-item__label {
  font-weight: 500;
}

::v-deep .el-radio .el-radio__label {
  font-weight: 600 !important;
  font-size: 1.25rem !important;
  padding: 0.6rem;
}

::v-deep .el-radio {
  height: auto;
  cursor: pointer;
  border-radius: 0.475rem;
}

::v-deep .el-radio.is-bordered.is-checked {
  background-color: #0053ad;
  color: #fff !important;
}
::v-deep .el-radio__input.is-checked + .el-radio__label {
  color: #fff !important;
}
::v-deep .el-radio__input {
  display: none;
}
</style>
