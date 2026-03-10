<template>
  <div
    class="modal fade"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="CreateDepositModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content">
        <form
          class="form"
          id="kt_modal_new_address_form"
          @submit.prevent="postReferralCode"
        >
          <div class="modal-header" id="kt_modal_new_address_header">
            <h2 class="fs-2">Generate Referral Link</h2>

            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <div class="modal-body py-10 px-lg-17">
            <div
              class="scroll-y me-n7 pe-7"
              id="kt_modal_new_address_scroll"
              data-kt-scroll="true"
              data-kt-scroll-activate="{default: false, lg: true}"
              data-kt-scroll-max-height="auto"
              data-kt-scroll-dependencies="#kt_modal_new_address_header"
              data-kt-scroll-wrappers="#kt_modal_new_address_scroll"
              data-kt-scroll-offset="300px"
            >
              <!-- Referral Code -->
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="fs-5 fw-semobold mb-2 required">Code Name</label>

                <Field
                  class="form-control form-control-solid"
                  :placeholder="$t('tip.pleaseInput')"
                  name="referralCodeName"
                  v-model="referralData.name"
                  :disabled="editable"
                />
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="referralCodeName" />
                  </div>
                </div>
              </div>

              <!-- Select Country -->
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
                  <span class="required">Country</span>
                </label>

                <Field
                  name="country"
                  class="form-select form-select-solid"
                  as="select"
                  v-model="referralData.supplement.country"
                  :disabled="editable"
                >
                  <option value="">
                    {{ $t("wallet.deposit.selectCurrency") }}
                  </option>
                  <option
                    v-for="(item, index) in phoneCountryData"
                    :key="index"
                    :value="item.code"
                  >
                    {{ item.name }}
                  </option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="country" />
                  </div>
                </div>
              </div>

              <!-- Currency -->
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
                  <span class="required">{{
                    $t("wallet.deposit.currency")
                  }}</span>
                </label>

                <Field
                  name="currency"
                  class="form-select form-select-solid"
                  as="select"
                  v-model="referralData.supplement.currency"
                  :disabled="editable"
                >
                  <option value="">
                    {{ $t("wallet.deposit.selectCurrency") }}
                  </option>
                  <option
                    v-for="(
                      { label, value }, index
                    ) in ConfigCurrencySelections"
                    :label="label"
                    :key="index"
                    :value="value"
                  ></option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="currency" />
                  </div>
                </div>
              </div>

              <!-- Language -->
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
                  <span class="required">Language</span>
                </label>

                <Field
                  name="language"
                  class="form-select form-select-solid"
                  as="select"
                  v-model="referralData.supplement.language"
                  :disabled="editable"
                >
                  <option value="">Select a language</option>
                  <option
                    v-for="(lang, index) in countries"
                    :label="lang.name"
                    :key="index"
                    :value="lang.code"
                  ></option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="language" />
                  </div>
                </div>
              </div>

              <div class="d-flex flex-column mb-5 fv-row">
                <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
                  <span class="required">Type</span>
                </label>

                <Field
                  name="type"
                  class="form-select form-select-solid"
                  as="select"
                  v-model="referralData.supplement.type"
                  :disabled="editable"
                >
                  <option value="">Select account type</option>
                  <option value="std">Standard</option>
                  <option value="pro">Pro</option>
                  <option value="adv">Advantage</option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="type" />
                  </div>
                </div>
              </div>
            </div>
          </div>
          <!--end::Modal body-->

          <!--begin::Modal footer-->
          <div class="modal-footer flex-center">
            <button
              type="reset"
              id="kt_modal_new_address_cancel"
              class="btn btn-light me-3"
              @click="hide"
            >
              {{ $t("action.discard") }}
            </button>

            <!-- <button
              ref="submitButtonRef"
              type="submit"
              id="kt_modal_new_address_submit"
              class="btn"
              :class="cancelDeposit ? 'btn-danger' : 'btn-primary'"
            >
              <span class="indicator-label">
                <span v-if="editable">{{ $t("action.update") }}</span>
                <span v-else>{{ $t("action.submit") }}</span>
              </span>

              <span class="indicator-progress">
                {{ $t("action.pleaseWait") }}
                <span
                  class="spinner-border spinner-border-sm align-middle ms-2"
                ></span>
              </span>
            </button> -->
          </div>
        </form>
        <!-- {{ paymentRequireData }} -->
        <!--end::form-->
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from "axios";
import * as Yup from "yup";
import { ref } from "vue";
import IBService from "../services/IbService";
import Swal from "sweetalert2/dist/sweetalert2.js";
import PhoneCountryData from "@/core/data/phonesData";
import { showModal, hideModal } from "@/core/helpers/dom";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { ILanguage, LanguageTypes } from "@/core/types/LanguageTypes";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";

const editCodeId = ref("");
const editable = ref(false);
const isLoading = ref(false);
// const cancelDeposit = ref(false);
const agentAccount = ref({} as any);

const phoneCountryData = ref(PhoneCountryData);
const submitButtonRef = ref<null | HTMLButtonElement>(null);
const CreateDepositModalRef = ref<null | HTMLElement>(null);

let countries: ILanguage[];
countries = LanguageTypes.all;

const emits = defineEmits<{
  (e: "fetchData"): void;
}>();

const referralData = ref({
  name: "",
  supplement: {
    country: "",
    currency: "",
    language: "",
    type: "",
  },
} as any);

const validationSchema = Yup.object().shape({
  referralCodeName: Yup.string().required().label("Code Name"),
  country: Yup.string().required().label("Country"),
  currency: Yup.string().required().label("Currency"),
  language: Yup.string().required().label("Language"),
  type: Yup.string().required().label("Account type"),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const show = async (_agentAccount: object, _code?: string) => {
  resetForm();
  editable.value = false;

  agentAccount.value = _agentAccount;

  if (_code != "undefine") {
    const response = await axios.get("/api/v1/referralcode/" + _code);
    referralData.value.name = response.data.name;

    editCodeId.value = response.data.id;
    editable.value = true;

    if (response.data.supplement) {
      referralData.value.supplement = {
        country: response.data.supplement.country,
        currency: response.data.supplement.currency,
        language: response.data.supplement.language,
        type: response.data.supplement.type,
      };
    }
  }

  showModal(CreateDepositModalRef.value);
  isLoading.value = false;
};

const postReferralCode = handleSubmit(async () => {
  if (!submitButtonRef.value) {
    return;
  }

  //Disable button & Activate indicator
  submitButtonRef.value.disabled = true;
  submitButtonRef.value.setAttribute("data-kt-indicator", "on");

  try {
    if (editable.value)
      await IBService.putReferralCode(editCodeId.value, referralData.value);
    else
      await IBService.postReferralCode(agentAccount.value, referralData.value);

    emits("fetchData");

    // Success and close model
    Swal.fire({
      text: "form has been successfully submitted!",
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok, got it!",
      customClass: {
        confirmButton: "btn btn-primary",
      },
    }).then(() => {
      hide();
    });
  } catch (error) {
    Swal.fire({
      text: "Sorry, looks like there are some errors detected, please try again.",
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Ok, got it!",
      customClass: {
        confirmButton: "btn btn-primary",
      },
    });
  }

  submitButtonRef.value.disabled = false;
  submitButtonRef.value.setAttribute("data-kt-indicator", "off");
});

const hide = () => {
  hideModal(CreateDepositModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style scoped></style>
