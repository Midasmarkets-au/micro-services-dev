<template>
  <div class="w-100 card">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.personalInfo") }} ({{ user.email }})
      </div>
    </div>
    <div class="card-body">
      <!-- <div class="card-body">
        <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark">
            {{ $t("title.personalInfo") }} ({{ user.email }})
          </h2>
        </div>
        <hr />
      </div> -->

      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.firstName')" prop="firstName">
            <el-input
              v-model="formData.firstName"
              :disabled="isSubmitting"
              size="large"
            ></el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.lastName')" prop="lastName">
            <el-input
              v-model="formData.lastName"
              :disabled="isSubmitting"
              size="large"
            ></el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2">
        <div class="col-lg-12 pb-2">
          <el-form-item :label="$t('fields.gender')" prop="gender">
            <el-radio-group v-model="formData.gender" class="row w-100 gap-4">
              <el-radio label="0" border class="col-4 col-lg-2 ms-2">
                {{ $t("fields.male") }}
              </el-radio>

              <el-radio label="1" border class="col-4 col-lg-2">{{
                $t("fields.female")
              }}</el-radio>
            </el-radio-group>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.birthdate')" prop="birthday">
            <el-date-picker
              v-model="formData.birthday"
              :disabled="isSubmitting"
              type="date"
              size="large"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.citizen')" prop="citizen">
            <el-select
              v-model="formData.citizen"
              :placeholder="$t('tip.selectCountry')"
              :disabled="isSubmitting"
              size="large"
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

      <div class="row pb-2">
        <div class="col-lg-12 pb-2">
          <el-form-item :label="$t('fields.address')" prop="address">
            <el-input
              v-model="formData.address"
              :disabled="isSubmitting"
              size="large"
              autocomplete="off"
            ></el-input>
          </el-form-item>
        </div>
      </div>
    </div>
    <div class="separate-line"></div>
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.idDocument") }}
      </div>
    </div>

    <div class="card-body">
      <!-- <div class="card-body">
        <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark">
            {{ $t("title.idDocument") }}
          </h2>
        </div>
        <hr />
      </div> -->
      <el-form-item :label="$t('fields.formId')" prop="idType">
        <el-radio-group
          v-if="!isMobile"
          v-model="formData.idType"
          class="row gap-6 w-100"
          style="margin-left: 0.1rem"
        >
          <el-radio :label="1" border class="col">
            {{ $t("fields.govId") }}
          </el-radio>

          <el-radio :label="2" border class="col">{{
            $t("fields.driveLicense")
          }}</el-radio>

          <el-radio :label="3" border class="col">{{
            $t("fields.passport")
          }}</el-radio>
        </el-radio-group>
        <el-radio-group
          v-else
          v-model="formData.idType"
          class="row gap-3 w-100"
          style="margin-left: 0.1rem"
        >
          <el-radio :label="1" border class="col-12">
            {{ $t("fields.govId") }}
          </el-radio>

          <el-radio :label="2" border class="col-12">{{
            $t("fields.driveLicense")
          }}</el-radio>

          <el-radio :label="3" border class="col-12">{{
            $t("fields.passport")
          }}</el-radio>
        </el-radio-group>
      </el-form-item>

      <div class="row pb-2 mt-5">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.doc_number')" prop="idNumber">
            <el-input
              v-model="formData.idNumber"
              :disabled="isSubmitting"
              size="large"
            ></el-input>
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.idIssuer')" prop="idIssuer">
            <el-input
              v-model="formData.idIssuer"
              :disabled="isSubmitting"
              size="large"
            ></el-input>
          </el-form-item>
        </div>
      </div>

      <div class="row pb-2 mt-5">
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.issue_date')" prop="idIssuedOn">
            <el-date-picker
              v-model="formData.idIssuedOn"
              :disabled="isSubmitting"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
              name="idIssuedOn"
              size="large"
            />
          </el-form-item>
        </div>
        <div class="col-lg-6 pb-2">
          <el-form-item :label="$t('fields.expire_date')" prop="expire_date">
            <el-date-picker
              v-model="formData.expire_date"
              :disabled="isSubmitting"
              name="idExpireOn"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
              size="large"
            />
          </el-form-item>
        </div>
      </div>
    </div>
    <div class="separate-line"></div>
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.socialMedia") }}
      </div>
    </div>
    <div class="card-body">
      <!-- <div class="card-body">
        <div class="pb-3">
          <h2 class="fw-bold d-flex align-items-center text-dark">
            {{ $t("title.socialMedia") }}
          </h2>
        </div>
        <hr />
      </div> -->

      <div class="row">
        <div class="col-lg-12 mb-3">
          <div
            v-for="(type, index) in item.socialMedium"
            :key="index"
            class="row"
          >
            <div class="col-lg-12 mb-5 d-flex align-items-center">
              <label class="form-label fs-6" style="width: 120px">{{
                $t("fields." + type.name)
              }}</label>
              <input
                class="form-control form-control-md"
                v-model="type.account"
                :name="'socialMedia' + type.name"
              />
            </div>
          </div>
        </div>
      </div>
      <div
        class="mb-3 text-gray"
        style="border: 1px solid #f2f4f7; font-size: 14px; padding: 24px"
      >
        {{ $t("tip.socialMediaAuthorize_1") }}
        <span>MM Co Ltd</span>
        {{ $t("tip.socialMediaAuthorize_2") }}
        <span>MM Co Ltd</span>
        {{ $t("tip.socialMediaAuthorize_3") }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, inject, onMounted, nextTick } from "vue";
import { getRegionCodes } from "@/core/data/phonesData";
import { useStore } from "@/store";
import { isMobile } from "@/core/config/WindowConfig";
const store = useStore();
const user = store.state.AuthModule.user;
const regionCodes = ref<any>(getRegionCodes());

const items = inject<any>("items");
const formData = inject<any>("formData");
const isSubmitting = inject<any>("isSubmitting");

const item = ref<any>(items?.value?.data?.info || {});

onMounted(async () => {
  await nextTick();

  formData.value = item.value;
  if (Object.keys(item.value).length === 0) {
    formData.value.firstName = user.firstName;
    formData.value.lastName = user.lastName;
    formData.value.email = user.email;
    formData.value.phone = user.phoneNumber;
    formData.value.ccc = user.ccc;
    (formData.value.citizen = user.countryCode?.toLowerCase()),
      (formData.value.socialMedium = [
        { name: "whatsApp", account: "" },
        { name: "weChat", account: "" },
        { name: "instagram", account: "" },
        { name: "telegram", account: "" },
        { name: "line", account: "" },
      ]);
  }
});
</script>
<style scoped lang="scss">
::v-deep .el-form-item__label {
  color: #3a3e44;
  font-size: 14px;
  // font-weight: 600;
  // font-size: 1.075rem;
}

::v-deep .question .el-form-item__label {
  // font-weight: 500;
}

::v-deep .el-radio .el-radio__label {
  // font-weight: 600 !important;
  color: #3a3e44;
  font-size: 14px;
  padding: 0.6rem;
}

::v-deep .el-radio {
  height: auto;
  cursor: pointer;
  border-radius: 0.475rem;
  margin-right: 0;
  background-color: #fff;
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

.formInput {
  width: 70%;
  height: 30px;
  font-size: 16px;
  padding-left: 6px;
}

.formInput:not(:last-child) {
  margin-bottom: 15px;
}

input[type="submit"] {
  display: block;
  -webkit-appearance: button;
  font-size: 15px;
  width: 135px;
  height: 36px;
  margin-left: 277px;
  background-color: #e18988;
  color: #ffffff;
  border: none;
  border-radius: 3px;
}

/* Forces .form-input styles onto search type */
input[type="search"] {
  -webkit-appearance: textfield;
  box-sizing: content-box;
}
</style>
