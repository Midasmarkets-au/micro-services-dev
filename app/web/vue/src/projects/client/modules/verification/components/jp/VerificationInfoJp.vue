<template>
  <div class="card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.personalInfo") }} ({{ user.email }})
        </h2>
      </div>
      <hr />
      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.lastName')" prop="lastName">
            <el-input
              v-model="formData.lastName"
              :disabled="isSubmitting"
            ></el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.firstName')" prop="firstName">
            <el-input
              v-model="formData.firstName"
              :disabled="isSubmitting"
            ></el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.nativeNameLast')"
            prop="nativeNameLast"
          >
            <el-input
              v-model="formData.nativeNameLast"
              :disabled="isSubmitting"
            ></el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.nativeNameFirst')"
            prop="nativeNameFirst"
          >
            <el-input
              v-model="formData.nativeNameFirst"
              :disabled="isSubmitting"
            ></el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.gender')" prop="gender">
            <el-radio-group
              v-model="formData.gender"
              size="large"
              :disabled="isSubmitting"
            >
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
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.birthdate')" prop="birthday">
            <el-date-picker
              v-model="formData.birthday"
              type="date"
              size="large"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              :disabled="isSubmitting"
            />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.age')" prop="age">
            <el-input v-model="formData.age" disabled>
              <template #append>{{ $t("fields.yearsOld") }}</template>
            </el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.postalCode')"
            @input="handlePostalCodeChange"
            prop="postalCode"
          >
            <el-input v-model="formData.postalCode" :disabled="isSubmitting" />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.region')" prop="region">
            <el-input v-model="formData.region" :disabled="isSubmitting" />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.village')" prop="village">
            <el-input v-model="formData.village" :disabled="isSubmitting" />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.town')" prop="town">
            <el-input v-model="formData.town" :disabled="isSubmitting" />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.street')" prop="street">
            <el-input v-model="formData.street" :disabled="isSubmitting" />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="$t('fields.buildingNumber')"
            prop="buildingNumber"
          >
            <el-input
              v-model="formData.buildingNumber"
              :disabled="isSubmitting"
            />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.email')" prop="email">
            <el-input v-model="formData.email" :disabled="isSubmitting" />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.phone')" prop="phone">
            <el-input v-model="formData.phone" :disabled="isSubmitting" />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.occupation')" prop="occupation">
            <el-select
              v-model="formData.occupation"
              :disabled="isSubmitting"
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
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.companyName')" prop="companyName">
            <el-input v-model="formData.companyName" :disabled="isSubmitting" />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="
              $t('fields.affiliatedDepartment') +
              ' (' +
              $t('fields.optional') +
              ')'
            "
            prop="affiliatedDepartment"
          >
            <el-input
              v-model="formData.affiliatedDepartment"
              :disabled="isSubmitting"
            />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item
            :label="
              $t('fields.affiliatedPosition') +
              ' (' +
              $t('fields.optional') +
              ')'
            "
            prop="affiliatedPosition"
          >
            <el-input
              v-model="formData.affiliatedPosition"
              :disabled="isSubmitting"
            />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.companyPhone')" prop="companyPhone">
            <el-input
              v-model="formData.companyPhone"
              :disabled="isSubmitting"
            />
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="d-flex align-items-center">
          <el-form-item :label="$t('fields.nationalityInformation')" required>
            <el-radio-group v-model="formData.citizen" :disabled="isSubmitting">
              <el-radio
                value="jp"
                label="jp"
                size="large"
                @click="citizenToggle(false)"
                >{{ $t("fields.japanese") }}</el-radio
              >
              <el-radio
                :value="true"
                :label="true"
                size="large"
                @click="citizenToggle(true)"
                :disabled="isSubmitting"
                >{{ $t("fields.otherThanJapan") }}</el-radio
              >
            </el-radio-group>
          </el-form-item>

          <el-form-item
            label="&nbsp;"
            class="ms-2"
            v-if="citizenSelect"
            prop="otherCitizen"
          >
            <el-select
              v-model="formData.otherCitizen"
              :placeholder="$t('tip.selectCountry')"
              :disabled="isSubmitting"
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
      </div>

      <div class="row mb-7">
        <div style="color: #409eff; font-size: 0.9rem">
          {{ $t("tip.jpCitizenTip") }}
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6">
          <el-form-item
            :label="$t('fields.countryOfResidence')"
            prop="countryOfResidence"
          >
            <el-radio-group
              v-model="formData.countryOfResidence"
              :disabled="isSubmitting"
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

      <div class="row mb-7">
        <div style="color: #409eff; font-size: 0.9rem">
          {{ $t("tip.jpResidenceTip") }}
        </div>
      </div>

      <div class="row">
        <div class="col-lg-6">
          <el-form-item :label="$t('fields.usTaxLiability')" prop="usTax">
            <el-radio-group
              v-model="formData.usTax"
              :disabled="isSubmitting"
              class="usTax"
            >
              <el-radio :value="false" :label="false">{{
                $t("fields.usTaxOne")
              }}</el-radio>
              <el-radio :value="true" :label="true">{{
                $t("fields.usTaxTwo")
              }}</el-radio>
            </el-radio-group>
          </el-form-item>
        </div>
      </div>

      <div class="row mb-7">
        <div style="color: #409eff; font-size: 0.9rem">
          {{ $t("tip.jpTaxTip") }}
        </div>
      </div>

      <div class="row">
        <div class="d-flex align-items-center">
          <el-form-item
            :label="$t('fields.declarationRegardingForeignPeps')"
            prop="declarationRegardingForeignPeps"
          >
            <div>
              <div style="font-size: 0.9rem">
                {{ $t("tip.jpPrepTip") }}
              </div>
              <div class="d-flex align-items-center">
                <el-radio-group
                  v-model="formData.declarationRegardingForeignPeps"
                  :disabled="isSubmitting"
                >
                  <el-radio
                    value="no"
                    label="no"
                    size="large"
                    @click="pepsToggle(false)"
                    >{{ $t("fields.no") }}</el-radio
                  >
                  <el-radio
                    :value="true"
                    :label="true"
                    size="large"
                    @click="pepsToggle(true)"
                    :disabled="isSubmitting"
                    >{{ $t("fields.yes") }}</el-radio
                  >
                </el-radio-group>
                <el-form-item
                  label="&nbsp;"
                  class="ms-4 pb-2 lh-0 prep"
                  v-if="pepsSelect"
                  prop="otherPeps"
                >
                  <el-input
                    v-model="formData.otherPeps"
                    :disabled="isSubmitting"
                    :placeholder="$t('tip.pleaseBeSpecific')"
                  >
                  </el-input>
                </el-form-item>
              </div>
            </div>
          </el-form-item>
        </div>
      </div>

      <div class="row mb-7">
        <a
          href="https://www.google.com/"
          target="_blank"
          rel="noopener noreferrer"
          style="color: #409eff; text-decoration: underline"
          >{{ $t("fields.foreignPeps") }}</a
        >
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, watch } from "vue";
import { useStore } from "@/store";
import { getRegionCodes } from "@/core/data/phonesData";
import i18n from "@/core/plugins/i18n";
import { occupations } from "@/core/types/jp/verificationInfo";
import { Core, Address } from "@/core/data/jpAddressGetter";
const postalCode = ref<string>("");

const handlePostalCodeChange = async (event: Event) => {
  const input = event.target as HTMLInputElement;
  postalCode.value = input.value;
  if (postalCode.value.length >= 7) {
    new Core(postalCode.value, (address: Address) => {
      formData.value.region = address.region || "";
      formData.value.village = address.l || "";
      if (address.o) {
        formData.value.town = address.m + " " + address.o || "";
      } else {
        formData.value.town = address.m || "";
      }
    });
  } else {
    formData.value.region = "";
    formData.value.village = "";
    formData.value.town = "";
  }
};

const t = i18n.global.t;
const store = useStore();
const user = store.state.AuthModule.user;
const items = inject<any>("items");
const formData = inject<any>("formData");
const isSubmitting = inject<any>("isSubmitting");
const regionCodes = ref<any>(getRegionCodes());
const item = ref<any>(items?.value?.data?.info || {});
const citizenSelect = ref<any>(false);
const pepsSelect = ref<any>(false);
const ohterCitizen = ref<any>("");

const citizenToggle = (value: boolean) => {
  citizenSelect.value = value;
  if (value == true) {
    formData.value.citizen = null;
  } else {
    formData.value.citizen = "jp";
    formData.value.otherCitizen = null;
  }
};

const pepsToggle = (value: boolean) => {
  pepsSelect.value = value;
  if (value == true) {
    formData.value.declarationRegardingForeignPeps = null;
  } else {
    formData.value.declarationRegardingForeignPeps = "no";
    formData.value.otherPeps = "";
  }
};

watch(
  () => formData.value.birthday,
  (newBirthday) => {
    if (newBirthday) {
      const today = new Date();
      const birthDate = new Date(newBirthday);
      let age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();

      // Adjust age if the birthday hasn't occurred yet this year
      if (
        monthDiff < 0 ||
        (monthDiff === 0 && today.getDate() < birthDate.getDate())
      ) {
        age--;
      }
      formData.value.age = age.toString(); // Update age
    } else {
      formData.value.age = ""; // Reset age if birthday is cleared
    }
  }
);

onMounted(() => {
  formData.value = item.value;
  formData.value.email = user.email;
  formData.value.phone = user.phoneNumber;
  formData.value.firstName = user.firstName;
  formData.value.lastName = user.lastName;

  if (item.value.citizen == null) {
    item.value.citizen = "jp";
  } else if (item.value.citizen !== "jp") {
    citizenSelect.value = true;
  } else {
    citizenSelect.value = false;
  }

  if (item.value.declarationRegardingForeignPeps == null) {
    item.value.declarationRegardingForeignPeps = "no";
  } else if (item.value.declarationRegardingForeignPeps !== "no") {
    pepsSelect.value = true;
  } else {
    pepsSelect.value = false;
  }
});
</script>

<style scoped>
:deep .el-form-item__label {
  color: #181c32;
  font-weight: 600;
  font-size: 1.075rem;
}
:deep .prep .el-form-item__label {
  height: 0;
}

@media screen and (max-width: 768px) {
  :deep .usTax .el-radio {
    white-space: normal;
    height: auto;
  }
}
</style>
