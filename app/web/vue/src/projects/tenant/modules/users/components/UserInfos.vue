<template>
  <div class="w-100 verify-card">
    <div class="row">
      <div class="col-lg-6 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.firstName")
        }}</label>

        <el-input v-model="item.firstName" name="firstName" size="large" />
      </div>

      <!-- Last Name -->
      <div class="col-lg-6 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.lastName")
        }}</label>

        <el-input v-model="item.lastName" name="lastName" size="large" />
      </div>

      <div class="col-lg-12 mb-5">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.priorName")
        }}</label>

        <el-input v-model="item.priorName" name="priorName" size="large" />
      </div>

      <div class="col-lg-12 mb-5">
        <label class="form-label fw-bold text-dark fs-6 required">{{
          $t("fields.gender")
        }}</label>
        <div class="row">
          <div class="col-6 col-lg-2">
            <input
              type="radio"
              value="0"
              class="btn-check"
              name="gender"
              v-model="item.gender"
              id="verification_gender_male_tenant"
            />
            <label
              class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
              for="verification_gender_male_tenant"
            >
              <span class="d-block fw-semobold text-start">
                <span class="fw-bold d-block fs-4 mb-2">
                  {{ $t("fields.male") }}
                </span>
              </span>
            </label>
          </div>

          <!-- --------------------------------------------- -->

          <div class="col-6 col-lg-2">
            <input
              type="radio"
              value="1"
              class="btn-check"
              name="gender"
              v-model="item.gender"
              id="verification_gender_female_tenant"
            />
            <label
              class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
              for="verification_gender_female_tenant"
            >
              <span class="d-block fw-semobold text-start">
                <span class="fw-bold d-block fs-4 mb-2">{{
                  $t("fields.female")
                }}</span>
              </span>
            </label>
          </div>
        </div>
      </div>

      <div class="col-lg-6 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.birthdate")
        }}</label>

        <el-date-picker v-model="item.birthdate" type="date" size="large" />
      </div>

      <div class="col-lg-6 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.citizen")
        }}</label>

        <el-input v-model="item.citizen" name="citizen" size="large" />
      </div>

      <div class="col-lg-12 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.phone")
        }}</label>
        <div class="row">
          <div class="col-lg-4 mb-3">
            <el-input size="large" v-model="item.ccc" name="ccc"> </el-input>
          </div>
          <div class="col-lg-8">
            <el-input v-model="item.phone" name="phone" size="large" />
          </div>
        </div>
      </div>

      <div class="col-lg-12 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.email")
        }}</label>

        <el-input v-model="item.email" name="email" size="large" />
      </div>

      <div class="col-lg-12 mb-5">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.address")
        }}</label>

        <el-input v-model="item.address" name="address" size="large" />
      </div>
    </div>

    <label class="form-label fs-6 fw-bold text-dark required mt-3">{{
      $t("title.socialMedia")
    }}</label>
    <div class="d-flex">
      <div v-for="(type, index) in item.socialMedium" :key="index">
        <div class="socialMediaCard" v-if="type.account != ''">
          <h2 class="text-center">{{ $t("fields." + type.name) }}</h2>
          <div class="mt-5">Account ID:</div>
          <div class="text-center mt-3">
            {{ type.account }}
          </div>
          <div class="text-center mt-7">
            <CopyButton :content="type.account" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { Field } from "vee-validate";
import CopyButton from "@/components/CopyButton.vue";

const props = defineProps<{
  verificationDetails: any;
}>();

const item = ref<any>({
  ...props.verificationDetails?.info,
});

watch(
  () => props.verificationDetails,
  () => {
    item.value = {
      ...props.verificationDetails?.info,
    };
  }
);
</script>

<style scoped>
.socialMediaCard {
  width: 220px;
  border: 1px solid #e0e0e0;
  border-radius: 5px;
  padding: 10px;
  margin-right: 20px;
  box-shadow: rgba(0, 0, 0, 0.24) 0px 3px 8px;
}
</style>
