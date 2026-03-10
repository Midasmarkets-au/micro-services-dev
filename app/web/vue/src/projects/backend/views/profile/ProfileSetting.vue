<template>
  <!--begin::Basic info-->
  <div class="card mb-5 mb-xl-10">
    <!--begin::Card header-->
    <div class="card-header border-0 cursor-pointer">
      <div class="card-title m-0">
        <h3 class="fw-bold m-0">Profile Details</h3>
      </div>
    </div>

    <!--begin::Content-->
    <div id="kt_account_settings_profile_details">
      <form id="kt_account_profile_details_form" class="form">
        <div class="card-body border-top p-9">
          <!-- Avatar -->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label fw-semibold fs-6"
              >Avatar</label
            >
            <div class="col-lg-8">
              <div
                class="image-input image-input-outline"
                data-kt-image-input="true"
                style="
                  background-image: url('assets/media/svg/avatars/blank.svg');
                "
              >
                <AuthImage
                  :imageGuid="user.avatar"
                  :alt="user.name"
                  class="image-input-wrapper w-125px h-125px"
                />

                <label
                  class="btn btn-icon btn-circle btn-active-color-primary w-25px h-25px bg-body shadow"
                  data-kt-image-input-action="change"
                  data-bs-toggle="tooltip"
                  title="Change avatar"
                >
                  <i class="bi bi-pencil-fill fs-7"></i>
                  <input
                    id="avatarInput"
                    type="file"
                    name="avatar"
                    accept=".png, .jpg, .jpeg, .heic"
                    @change="avatarUpload"
                  />
                  <input type="hidden" name="avatar_remove" />
                </label>
              </div>
              <div class="form-text">
                {{ $t("tip.allowedFileTypes") }}: png, jpg, jpeg.
              </div>
            </div>
          </div>

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6"
              >Full Name</label
            >
            <div class="col-lg-8 fv-row">
              <input
                v-model="collectData.full_name"
                type="text"
                name="full_name"
                class="form-control form-control-lg form-control-solid"
                placeholder="Full name"
              />
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6"
              >Contact Phone</label
            >
            <div class="col-lg-8">
              <div class="row">
                <div class="col-lg-3 fv-row">
                  <select
                    v-model="collectData.ccc"
                    name="country"
                    aria-label="Select a Country"
                    data-control="select2"
                    data-placeholder="Select a country..."
                    class="form-select form-select-solid form-select-lg fw-semibold mb-3 mb-lg-0"
                  >
                    <option value="">Code</option>
                    <option
                      v-for="(item, index) in phoneData"
                      :key="index"
                      :value="item.dialCode"
                    >
                      + {{ item.dialCode }} {{ item.name }}
                    </option>
                  </select>
                </div>
                <div class="col-lg-9 fv-row">
                  <input
                    v-model="collectData.phone"
                    type="text"
                    name="lname"
                    class="form-control form-control-lg form-control-solid"
                    placeholder="Contact phone"
                  />
                </div>
              </div>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label fw-semibold fs-6">
              <span class="required">Country</span>
              <i
                class="fas fa-exclamation-circle ms-1 fs-7"
                data-bs-toggle="tooltip"
                title="Country of origination"
              ></i>
            </label>
            <div class="col-lg-8 fv-row">
              <select
                v-model="collectData.country"
                name="country"
                aria-label="Select a Country"
                data-control="select2"
                data-placeholder="Select a country..."
                class="form-select form-select-solid form-select-lg fw-semibold"
              >
                <option value="">Select a Country...</option>
                <option
                  v-for="(item, index) in phoneData"
                  :key="index"
                  :value="item.code"
                >
                  {{ item.name }}
                </option>
              </select>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label fw-semibold fs-6">
              <span class="required">Time Zone</span>
              <i
                class="fas fa-exclamation-circle ms-1 fs-7"
                data-bs-toggle="tooltip"
                title="Country of origination"
              ></i>
            </label>
            <div class="col-lg-8 fv-row">
              <select
                v-model="collectData.timezone"
                name="timezone"
                aria-label="Select a Timezone"
                data-control="select2"
                data-placeholder="Select a Timezone..."
                class="form-select form-select-solid form-select-lg fw-semibold"
              >
                <option value="">Select a Timezone...</option>
              </select>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6"
              >Language</label
            >
            <div class="col-lg-8 fv-row">
              <select
                v-model="collectData.language"
                name="language"
                aria-label="Select a Language"
                data-control="select2"
                data-placeholder="Select a language..."
                class="form-select form-select-solid form-select-lg"
              >
                <option value="">Select a Language...</option>
                <option
                  data-kt-flag="flags/english.svg"
                  :value="LanguageCodes.enUS"
                >
                  English - United States
                </option>
                <option
                  data-kt-flag="flags/thailand.svg"
                  :value="LanguageCodes.thTh"
                >
                  ภาษาไทย - Thai
                </option>
                <!--                <option data-kt-flag="flags/south-korea.svg" value="ko">-->
                <!--                  한국어 - Korean-->
                <!--                </option>-->
                <!--                <option data-kt-flag="flags/japan.svg" value="ja">-->
                <!--                  日本語 - Japanese-->
                <!--                </option>-->
                <option
                  data-kt-flag="flags/china.svg"
                  :value="LanguageCodes.zhCN"
                >
                  简体中文 - Simplified Chinese
                </option>
                <option
                  data-kt-flag="flags/hong-kong.svg"
                  :value="LanguageCodes.zhHK"
                >
                  繁體中文 - Traditional Chinese
                </option>
              </select>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label fw-semibold fs-6"
              >Currency</label
            >
            <div class="col-lg-8 fv-row">
              <select
                v-model="collectData.currency"
                name="currency"
                aria-label="Select a Currency"
                data-control="select2"
                data-placeholder="Select a currency.."
                class="form-select form-select-solid form-select-lg"
              >
                <option value="">Select a currency..</option>
                <option data-kt-flag="flags/united-states.svg" value="USD">
                  <b>USD</b>&nbsp;-&nbsp;USA dollar
                </option>
                <option data-kt-flag="flags/united-kingdom.svg" value="GBP">
                  <b>GBP</b>&nbsp;-&nbsp;British pound
                </option>
                <option data-kt-flag="flags/australia.svg" value="AUD">
                  <b>AUD</b>&nbsp;-&nbsp;Australian dollar
                </option>
                <option data-kt-flag="flags/japan.svg" value="JPY">
                  <b>JPY</b>&nbsp;-&nbsp;Japanese yen
                </option>
                <option data-kt-flag="flags/sweden.svg" value="SEK">
                  <b>SEK</b>&nbsp;-&nbsp;Swedish krona
                </option>
                <option data-kt-flag="flags/canada.svg" value="CAD">
                  <b>CAD</b>&nbsp;-&nbsp;Canadian dollar
                </option>
                <option data-kt-flag="flags/switzerland.svg" value="CHF">
                  <b>CHF</b>&nbsp;-&nbsp;Swiss franc
                </option>
              </select>
            </div>
          </div>
          <!--end::Input group-->
        </div>

        <!--begin::Actions-->
        <div class="card-footer d-flex justify-content-end py-6 px-9">
          <button
            type="reset"
            class="btn btn-light btn-active-light-primary me-2"
          >
            Discard
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            id="kt_account_profile_details_submit"
          >
            Save Changes
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import AuthImage from "@/components/AuthImage.vue";
import { ref, inject } from "vue";
import { useStore } from "@/store";
import { Actions } from "@/store/enums/StoreEnums";
import ErrorMsg from "@/components/ErrorMsg";
import phoneData from "@/core/data/phonesData";
import { LanguageCodes } from "@/core/types/LanguageTypes";

interface UserData {
  full_name: string;
  ccc: string;
  phone: string;
  country: string;
  language: string;
  currency: string;
  timezone: string;
}
const store = useStore();
const user = store.state.AuthModule.user;
const api = inject("api");
const isLoading = ref(true);

const collectData = ref({
  full_name: user.name,
  ccc: user.ccc,
  phone: user.phone,
  country: user.country,
  timezone: user.timezone,
  language: user.language,
  currency: user.currency,
} as UserData);

const form = new FormData();

function avatarUpload(event) {
  console.log("upload");

  if (event.target.files.item(0) == null) return;

  const file = event.target.files.item(0);
  const reader = new FileReader();
  reader.readAsDataURL(file);

  form.append("avatar", file);

  api["profile.avatar"]({ data: form })
    .then(({ data }) => {
      console.log("avatar return data:", data);
      store.dispatch(Actions.SET_AVATAR, data);
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = false;
    });
}
</script>
