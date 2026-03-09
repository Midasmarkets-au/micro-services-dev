<template>
  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.firstName')" prop="firstName">
        <el-input v-model="formData.firstName" :disabled="disabled"></el-input>
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.lastName')" prop="lastName">
        <el-input v-model="formData.lastName" :disabled="disabled"></el-input>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.nativeNameLast')" prop="nativeNameLast">
        <el-input
          v-model="formData.nativeNameLast"
          :disabled="disabled"
        ></el-input>
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item
        :label="$t('fields.nativeNameFirst')"
        prop="nativeNameFirst"
      >
        <el-input
          v-model="formData.nativeNameFirst"
          :disabled="disabled"
        ></el-input>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.gender')" prop="gender">
        <el-radio-group v-model="formData.gender" :disabled="disabled">
          <el-radio-button label="0" value="0">
            {{ $t("fields.male") }}
          </el-radio-button>
          <el-radio-button label="1" value="1">
            {{ $t("fields.female") }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.birthdate')" prop="birthday">
        <el-date-picker
          v-model="formData.birthday"
          type="date"
          format="YYYY-MM-DD"
          value-format="YYYY-MM-DD"
          :disabled="disabled"
        />
      </el-form-item>
    </div>

    <div class="col-lg-6">
      <el-form-item :label="$t('fields.age')" prop="age">
        <el-input v-model="formData.age">
          <template #append>{{ $t("fields.yearsOld") }}</template>
        </el-input>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.postalCode')" prop="postalCode">
        <el-input v-model="formData.postalCode" :disabled="disabled" />
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.region')" prop="region">
        <el-input v-model="formData.region" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>
  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.village')" prop="village">
        <el-input v-model="formData.village" :disabled="disabled" />
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.town')" prop="town">
        <el-input v-model="formData.town" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>
  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.street')" prop="street">
        <el-input v-model="formData.street" :disabled="disabled" />
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.buildingNumber')" prop="buildingNumber">
        <el-input v-model="formData.buildingNumber" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.email')" prop="email">
        <el-input v-model="formData.email" :disabled="disabled" />
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.phone')" prop="phone">
        <el-input v-model="formData.phone" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>
  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.occupation')" prop="occupation">
        <el-select
          v-model="formData.occupation"
          :disabled="disabled"
          :placeholder="$t('fields.select')"
        >
          <el-option
            v-for="(item, index) in occupations"
            :key="index"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.companyName')" prop="companyName">
        <el-input v-model="formData.companyName" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item
        :label="
          $t('fields.affiliatedDepartment') + ' (' + $t('fields.optional') + ')'
        "
        prop="affiliatedDepartment"
      >
        <el-input
          v-model="formData.affiliatedDepartment"
          :disabled="disabled"
        />
      </el-form-item>
    </div>
    <div class="col-lg-6">
      <el-form-item
        :label="
          $t('fields.affiliatedPosition') + ' (' + $t('fields.optional') + ')'
        "
        prop="affiliatedPosition"
      >
        <el-input v-model="formData.affiliatedPosition" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.companyPhone')" prop="companyPhone">
        <el-input v-model="formData.companyPhone" :disabled="disabled" />
      </el-form-item>
    </div>
  </div>
  <div class="row">
    <div class="col-lg-6 d-flex align-items-center">
      <el-form-item :label="$t('fields.nationalityInformation')" required>
        <el-radio-group v-model="formData.citizen" :disabled="disabled">
          <el-radio value="jp" label="jp" size="large">{{
            $t("fields.japanese")
          }}</el-radio>
          <el-radio
            :value="true"
            :label="true"
            size="large"
            :disabled="disabled"
            >{{ $t("fields.otherThanJapan") }}</el-radio
          >
        </el-radio-group>
      </el-form-item>

      <el-form-item label="&nbsp;" class="ms-2" prop="otherCitizen">
        <el-select
          v-model="formData.otherCitizen"
          v-if="formData.citizen == true"
          :placeholder="$t('tip.selectCountry')"
          :disabled="disabled"
        >
          <el-option
            v-for="(item, index) in regionCodes"
            :key="index"
            :label="item.name"
            :value="item.code"
          >
          </el-option>
        </el-select>
      </el-form-item>
    </div>

    <div class="col-lg-6">
      <el-form-item
        :label="$t('fields.countryOfResidence')"
        prop="countryOfResidence"
      >
        <el-radio-group
          v-model="formData.countryOfResidence"
          :disabled="disabled"
        >
          <el-radio value="jp" label="jp" size="large">{{
            $t("fields.japan")
          }}</el-radio>
          <el-radio :value="true" :label="true" size="large">{{
            $t("fields.residenceThanJapan")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>

  <div class="row">
    <div class="col-lg-6">
      <el-form-item :label="$t('fields.usTaxLiability')" prop="usTax">
        <el-radio-group v-model="formData.usTax" :disabled="disabled">
          <el-radio :value="false" :label="false" size="large">{{
            $t("fields.usTaxOne")
          }}</el-radio>
          <el-radio :value="true" :label="true" size="large">{{
            $t("fields.usTaxTwo")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>
    </div>

    <div class="col-lg-6 d-flex align-items-center">
      <el-form-item
        :label="$t('fields.declarationRegardingForeignPeps')"
        prop="declarationRegardingForeignPeps"
      >
        <el-radio-group
          v-model="formData.declarationRegardingForeignPeps"
          :disabled="disabled"
        >
          <el-radio value="no" label="no" size="large">{{
            $t("fields.no")
          }}</el-radio>
          <el-radio
            :value="true"
            :label="true"
            size="large"
            :disabled="disabled"
            >{{ $t("fields.yes") }}</el-radio
          >
        </el-radio-group>
      </el-form-item>

      <el-form-item
        label="&nbsp;"
        class="ms-4"
        v-if="formData.declarationRegardingForeignPeps == true"
        prop="otherPeps"
      >
        <el-input
          v-model="formData.otherPeps"
          :disabled="disabled"
          :placeholder="$t('tip.pleaseBeSpecific')"
        >
        </el-input>
      </el-form-item>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import { getRegionCodes } from "@/core/data/phonesData";
import { occupations } from "@/core/types/jp/verificationInfo";
const formData = inject<any>("formData");
const verificationDetails = inject<any>("verificationDetails");
const disabled = inject<any>("disabled");
const regionCodes = ref<any>(getRegionCodes());
onMounted(() => {
  formData.value = verificationDetails.value.info;
});
</script>
