<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.personalInfo") }} ({{ user.email }})
        </h2>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.firstName")
          }}</label>
          <Field
            v-model="item.firstName"
            tabindex="1"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="firstName"
            autocomplete="off"
          >
            <el-input v-model="item.firstName" name="firstName" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="firstName" />
            </div>
          </div>
        </div>

        <!-- Last Name -->
        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.lastName")
          }}</label>
          <Field
            :value="item.lastName"
            tabindex="2"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="lastName"
            autocomplete="off"
          >
            <el-input v-model="item.lastName" name="lastName" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="lastName" />
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5" v-show="siteIdCheck">
          <label class="form-label fs-6 fw-bold text-dark">{{
            $t("fields.priorName")
          }}</label>
          <Field
            :value="item.priorName"
            tabindex="3"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="priorName"
            autocomplete="off"
          >
            <el-input v-model="item.priorName" name="priorName" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="priorName" />
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.gender")
          }}</label>
          <div class="row">
            <div class="col-6 col-lg-2">
              <Field
                v-model="item.gender"
                type="radio"
                class="btn-check"
                name="gender"
                :value="0"
                id="verification_gender_male"
              />
              <label
                class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
                for="verification_gender_male"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-md-4 my-md-1">
                    {{ $t("fields.male") }}
                  </span>
                </span>
              </label>
            </div>

            <!-- --------------------------------------------- -->

            <div class="col-6 col-lg-2">
              <Field
                v-model="item.gender"
                type="radio"
                class="btn-check"
                name="gender"
                :value="1"
                id="verification_gender_female"
              />
              <label
                class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
                for="verification_gender_female"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-md-4 my-md-1">{{
                    $t("fields.female")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="gender"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.birthdate")
          }}</label>
          <Field
            v-model="item.birthday"
            tabindex="4"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="birthday"
            autocomplete="off"
          >
            <el-date-picker
              v-model="item.birthday"
              type="date"
              size="large"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="birthday" />
            </div>
          </div>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.citizen")
          }}</label>
          <!--          <Field-->
          <!--            v-model="item.citizen"-->
          <!--            tabindex="5"-->
          <!--            class="form-control form-control-lg form-control-solid"-->
          <!--            type="text"-->
          <!--            name="citizen"-->
          <!--            autocomplete="off"-->
          <!--          >-->
          <!--            <el-input v-model="item.citizen" name="citizen" size="large" />-->
          <!--          </Field>-->

          <!--          <div>{{ item.citizen }}</div>-->
          <Field
            v-model="item.citizen"
            class="form-control form-control-lg form-control-solid"
            name="citizen"
            autoComplete="off"
          >
            <el-select
              v-model="item.citizen"
              :placeholder="$t('tip.selectCountry')"
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
          </Field>

          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="citizen" />
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5" v-show="siteIdCheck">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.phone")
          }}</label>
          <div class="row">
            <div class="col-lg-4 mb-3">
              <!--            <div class="btn btn-primary">{{ phoneRegionCode }}</div>-->
              <Field
                v-model="phoneRegionCode"
                class="form-control form-control-lg form-control-solid"
                name="ccc"
              >
                <el-select
                  @change="changePhoneRegionCode"
                  size="large"
                  placeholder="Select"
                  v-model="phoneRegionCode"
                >
                  <el-option
                    v-for="item in phoneDataList"
                    :key="item.code"
                    :label="`${item.name} +${item.dialCode}`"
                    :value="item.code"
                  />
                </el-select>
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="ccc" />
                </div>
              </div>
            </div>
            <div class="col-lg-8">
              <Field
                :value="item.phone"
                tabindex="6"
                class="form-control form-control-lg form-control-solid"
                type="text"
                name="phone"
                autocomplete="off"
              >
                <el-input v-model="item.phone" name="phone" size="large" />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="phone" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5" v-show="siteIdCheck">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.email")
          }}</label>
          <Field
            :value="item.email"
            tabindex="7"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="email"
            autocomplete="off"
          >
            <el-input v-model="item.email" name="email" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="email" />
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.address")
          }}</label>
          <Field
            v-model="item.address"
            tabindex="8"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="address"
          >
            <el-input
              v-if="siteIdCheck"
              v-model="item.address"
              name="address"
              size="large"
              id="au_address"
              autocomplete="off"
            />
            <el-input
              v-else
              v-model="item.address"
              name="address"
              size="large"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="address" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div
    class="w-100 card verify-card mt-5"
    style="max-width: 880px; margin: auto"
  >
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.idDocument") }}
        </h2>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.formId")
          }}</label>
          <div class="row">
            <div class="col-md-4 mb-3">
              <Field
                v-model="item.idType"
                type="radio"
                class="btn-check"
                name="idType"
                :value="1"
                id="verification_info_id_government"
              />
              <label
                class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
                for="verification_info_id_government"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-md-4 my-md-1">{{
                    $t("fields.govId")
                  }}</span>
                </span>
              </label>
            </div>

            <div class="col-md-4 mb-3">
              <Field
                v-model="item.idType"
                type="radio"
                class="btn-check"
                name="idType"
                :value="2"
                id="verification_info_id_dl"
              />
              <label
                class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
                for="verification_info_id_dl"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-md-4 my-md-1">{{
                    $t("fields.driveLicense")
                  }}</span>
                </span>
              </label>
            </div>

            <div class="col-md-4 mb-3">
              <Field
                v-model="item.idType"
                type="radio"
                class="btn-check"
                name="idType"
                :value="3"
                id="verification_info_id_ps"
              />
              <label
                class="btn btn-outline btn-outline-default px-3 py-2 d-flex align-items-center"
                for="verification_info_id_ps"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-md-4 my-md-1">{{
                    $t("fields.passport")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="idType"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.doc_number")
          }}</label>
          <Field
            v-model="item.idNumber"
            tabindex="9"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="idNumber"
            autocomplete="off"
          >
            <el-input v-model="item.idNumber" name="idNumber" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="idNumber" />
            </div>
          </div>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.issuer")
          }}</label>
          <Field
            v-model="item.idIssuer"
            tabindex="10"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="idIssuer"
            autocomplete="off"
          >
            <el-input v-model="item.idIssuer" name="idIssuer" size="large" />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="idIssuer" />
            </div>
          </div>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark">{{
            $t("fields.issue_date")
          }}</label>
          <Field
            :value="item.idIssuedOn"
            tabindex="11"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="idIssuedOn"
            autocomplete="off"
          >
            <el-date-picker
              v-model="item.idIssuedOn"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
              name="idIssuedOn"
              size="large"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="idIssuedOn" />
            </div>
          </div>
        </div>

        <!-- Last Name -->
        <div class="col-lg-6 mb-5">
          <label class="form-label fs-6 fw-bold text-dark">{{
            $t("fields.expire_date")
          }}</label>
          <Field
            v-model="item.idExpireOn"
            tabindex="12"
            class="form-control form-control-lg form-control-solid"
            type="text"
            name="idExpireOn"
            autocomplete="off"
          >
            <el-date-picker
              v-model="item.idExpireOn"
              name="idExpireOn"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
              size="large"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="idExpireOn" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div
    class="w-100 card verify-card mt-5"
    style="max-width: 880px; margin: auto"
  >
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("title.socialMedia") }}
        </h2>
      </div>
      <hr />
      <div class="row">
        <div class="col-lg-12 mb-3">
          <div
            v-for="(type, index) in item.socialMedium"
            :key="index"
            class="row"
          >
            <div class="col-lg-12 mb-5 d-flex align-items-center">
              <label
                class="form-label fs-6 fw-bold text-dark"
                style="width: 120px"
                >{{ $t("fields." + type.name) }}</label
              >
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
        class="form-control form-control-md mb-3"
        style="border: 2px solid #808080"
      >
        {{ $t("tip.socialMediaAuthorize_1") }}
        <span v-if="siteIdCheck">MM Co Ltd</span>
        <span v-else>MM Co Ltd</span>
        {{ $t("tip.socialMediaAuthorize_2") }}
        <span v-if="siteIdCheck">MM Co Ltd</span>
        <span v-else>MM Co Ltd</span>
        {{ $t("tip.socialMediaAuthorize_3") }}
      </div>

      <div v-if="socialMediaCheck" class="text-end" style="color: #f1416c">
        {{ $t("tip.socialMediaError") }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, watch, computed, nextTick } from "vue";
import { Field, ErrorMessage } from "vee-validate";
import { useStore } from "@/store";
import { getRegionCodes } from "@/core/data/phonesData";
import * as Yup from "yup";
import { useForm } from "vee-validate";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { SiteTypes } from "@/core/types/SiteTypes";
import { useAddressFinder } from "@/core/data/auAddressFinder";
import { getUserTenancy, tenancies } from "@/core/types/TenantTypes";
const store = useStore();
const user = store.state.AuthModule.user;

const config = computed<PublicSetting>(() => store.state.AuthModule.config);
// const siteIdCheck = computed(() => config.value.siteId == SiteTypes.Australia);
const siteIdCheck = computed(() => getUserTenancy() == tenancies.au);
const phoneDataList = ref(getRegionCodes());
const phoneRegionCode = ref(
  phoneDataList.value[user.countryCode.toLowerCase()].code
);
const regionCodes = ref(getRegionCodes());
const emits = defineEmits(["saved", "hasError"]);

const props = defineProps<{
  data?: any;
  step: number;
}>();

const socialMediaCheck = ref(false);

const isSubmit = ref(false);
const item = ref<any>(
  props.data || {
    firstName: user.firstName,
    lastName: user.lastName,
    email: user.email,
    ccc: user.ccc,
    phone: user.phoneNumber,
    citizen: user.countryCode?.toLowerCase(),
    socialMedium: [
      { name: "whatsApp", account: "" },
      { name: "weChat", account: "" },
      { name: "instagram", account: "" },
      { name: "telegram", account: "" },
      { name: "line", account: "" },
    ],
  }
);

const infoSchema = Yup.object().shape({
  firstName: Yup.string().required().label("First Name"),
  lastName: Yup.string().required().label("Last Name"),
  // priorName: Yup.string().required().label("Prior Name"),
  birthday: Yup.string().required().label("Birth Date"),
  gender: Yup.string().required().label("Gender"),
  citizen: Yup.string().required().label("Citizen"),
  ccc: Yup.string().required().label("Country Code"),
  phone: Yup.string().required().label("Phone"),
  email: Yup.string().required().label("Email"),
  address: Yup.string().required().label("Address"),
  idType: Yup.string().required().label("ID Type"),
  idNumber: Yup.string().required().label("ID Number"),
  idIssuer: Yup.string().required().label("Office of Issue"),
  // idIssuedOn: Yup.string().required().label("ID Issue Date"),
  // idExpireOn: Yup.string().required().label("ID Expiry Date"),
});

const { resetForm, handleSubmit } = useForm<any>({
  validationSchema: infoSchema,
});

function onInvalidSubmit() {
  emits("hasError");
}

const handleStepSubmit = handleSubmit((values) => {
  values = {
    ...item.value,
  };

  for (const val in values) {
    // eslint-disable-next-line no-prototype-builtins
    if (values.hasOwnProperty(val)) {
      if (values[val]) {
        item.value[val] = values[val];
      }
    }
  }

  socialMediaCheck.value = item.value.socialMedium.every(
    (item) => item.account === ""
  );

  socialMediaCheck.value = false;

  if (socialMediaCheck.value) {
    onInvalidSubmit();
    return;
  }

  submitForm();
}, onInvalidSubmit);

const changePhoneRegionCode = () => {
  item.value.ccc = phoneDataList.value[phoneRegionCode.value].dialCode;
};

const submitForm = async () => {
  isSubmit.value = true;
  try {
    const res = await VerificationService.postVerificationInfo(item.value);
    item.value = res;
    emits("saved", props.step, res);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmit.value = false;
  }
};

defineExpose({
  handleStepSubmit,
});

onMounted(async () => {
  // phoneRegionCode.value =
  //   phoneDataList.value[user.countryCode.toLowerCase()].code;
  if (siteIdCheck.value == true) {
    await nextTick();
    const auAddressElement = ref(
      document.getElementById("au_address") as HTMLInputElement
    );
    useAddressFinder(auAddressElement);
  }
});
</script>
