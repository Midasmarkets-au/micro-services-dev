<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-md-3">
          {{ $t("title.agreement") }}
        </h2>
      </div>
      <hr />

      <div class="fv-row mb-10 ms-3">
        <div class="row">
          <div class="d-flex align-items-center">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid mx-md-5 me-5"
            >
              <input
                v-model="item.consent_1"
                class="form-check-input"
                type="checkbox"
                value=""
              />
            </div>
            <!--end::Checkbox-->
            <div>
              {{ $t("tip.verificationAgreementAcknowledgeTerms") }}
            </div>
          </div>
          <div class="agreementList">
            <ul v-if="isAuTenant" class="fw-bold">
              <li v-for="(item, index) in baVerficationDocs" :key="index">
                <a
                  :href="getBaDocs(item.title)"
                  target="_blank"
                  class="cursor-pointer text-dark text-hover-primary"
                >
                  {{ $t(`title.${index}`) }}
                </a>
              </li>
            </ul>
            <ul v-else class="fw-bold">
              <li v-for="(item, index) in bviVerficationDocs" :key="index">
                <a
                  :href="getBviDocs(item.title)"
                  target="_blank"
                  class="cursor-pointer text-dark text-hover-primary"
                >
                  {{ $t(`title.${index}`) }}
                </a>
              </li>
            </ul>
          </div>
        </div>

        <div class="row mt-5">
          <h4>{{ $t("title.electronicIdentityVerification") }}*</h4>
          <p>
            {{ $t("tip.verificationAgreementElectronicIdentityVerification1") }}
            <span v-if="isAuTenant">MM Co Ltd</span>
            <span v-else>MM Co Ltd</span>
            {{ $t("tip.verificationAgreementElectronicIdentityVerification2") }}
          </p>

          <div
            class="d-flex align-items-center identifyOption"
            style="
              border: 1px solid gray;
              border-radius: 10px;
              box-sizing: border-box;
            "
          >
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid mx-md-5 me-5"
            >
              <input
                v-model="item.consent_2"
                class="form-check-input"
                type="radio"
                name="consent"
                value="true"
              />
            </div>
            <!--end::Checkbox-->
            <div>
              {{ $t("tip.verificationAgreementConsentPassingInfo") }}
            </div>
          </div>
          <div
            class="d-flex align-items-center mt-5 identifyOption"
            style="
              border: 1px solid gray;
              border-radius: 10px;
              box-sizing: border-box;
            "
            v-if="!isSeaTenant"
          >
            <!--begin::Checkbox-->

            <div
              class="form-check form-check-custom form-check-solid mx-md-5 me-5"
            >
              <input
                v-model="item.consent_2"
                class="form-check-input"
                type="radio"
                name="consent"
                value="false"
              />
            </div>
            <!--end::Checkbox-->
            <div>
              <p>
                {{ $t("tip.verificationAgreementNotConsentPassingInfo") }}
              </p>
              <p style="color: #900000">
                {{ $t("tip.verificationAgreementPrepareId") }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div style="color: #900000; text-align: right">
        <p v-if="showError">
          {{ $t("tip.checkAllTheCheckboxes") }}
        </p>
        <p v-if="showError2">{{ $t("tip.selectAnIdentificationMethod") }}</p>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";
// import { Field, ErrorMessage } from "vee-validate";
import { useStore } from "@/store";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { SiteTypes } from "@/core/types/SiteTypes";
import {
  getBaDocs,
  getBviDocs,
  baVerficationDocs,
  bviVerficationDocs,
} from "@/core/data/bcrDocs";
import { getUserTenancy, tenancies } from "@/core/types/TenantTypes";
const store = useStore();

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);

const siteId = computed(() => projectConfig.value.siteId);

// const isAuTenant = computed(
//   () => projectConfig.value.siteId == SiteTypes.Australia
// );
const isAuTenant = computed(() => getUserTenancy() == tenancies.au);
const isSeaTenant = computed(() => getUserTenancy() == tenancies.sea);
// var language = store.state.AuthModule.user.language;

const emits = defineEmits(["saved", "hasError"]);

const props = defineProps<{
  data?: any;
  step: number;
}>();

const isSubmit = ref(false);
const showError = ref(false);
const showError2 = ref(false);
const item = ref(
  props.data || {
    consent_1: "",
    consent_2: "",
    consent_3: "",
  }
);

const handleStepSubmit = async () => {
  if (item.value.consent_1 == "") {
    showError.value = true;
  } else {
    showError.value = false;
  }
  if (item.value.consent_2 === "") {
    showError2.value = true;
  } else {
    showError2.value = false;
  }
  if (showError.value || showError2.value) {
    emits("hasError");
    return;
  }
  try {
    isSubmit.value = true;
    const res = await VerificationService.postVerificationAgreement({
      consent_1: item.value.consent_1,
      consent_2: item.value.consent_2,
    });
    item.value = res.data;
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
</script>
<style scoped>
.agreementList {
  margin-left: 50px;
  margin-top: 20px;
}
.identifyOption {
  padding: 20px;
}
@media (max-width: 768px) {
  .verify-card {
    padding-left: 20px;
    padding-right: 20px;
  }
  .agreementList {
    margin-left: 0px;
  }
  .identifyOption {
    padding: 20px 10px;
  }
}
</style>
