<template>
  <div class="w-100 verify-card">
    <div class="">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.agreement") }}
        </h2>
      </div>
      <hr />

      <div class="fv-row mb-10 ms-3">
        <div class="row">
          <div class="d-flex align-items-center">
            <!--begin::Checkbox-->
            <div class="form-check form-check-custom form-check-solid mx-5">
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
          <div style="margin-left: 50px; margin-top: 20px">
            <ul class="fw-bold">
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
          </div>
        </div>

        <div class="row mt-5">
          <h4>{{ $t("title.electronicIdentityVerification") }}*</h4>
          <p>
            {{ $t("tip.verificationAgreementElectronicIdentityVerification") }}
          </p>

          <div
            class="d-flex align-items-center"
            style="
              border: 1px solid gray;
              border-radius: 10px;
              box-sizing: border-box;
              padding: 20px;
            "
          >
            <!--begin::Checkbox-->
            <div class="form-check form-check-custom form-check-solid mx-5">
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
            class="d-flex align-items-center mt-5"
            style="
              border: 1px solid gray;
              border-radius: 10px;
              box-sizing: border-box;
              padding: 20px;
            "
          >
            <!--begin::Checkbox-->
            <div class="form-check form-check-custom form-check-solid mx-5">
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { Field } from "vee-validate";
import questionDB from "@/core/data/wholesaleTest";
import { getBaDocs, baVerficationDocs } from "@/core/data/bcrDocs";
const questions = ref<any>(questionDB["en-us"]);

const props = defineProps<{
  verificationDetails: any;
}>();

const item = ref<any>({
  ...props.verificationDetails?.agreement,
});

watch(
  () => props.verificationDetails,
  () => {
    item.value = {
      ...props.verificationDetails?.agreement,
    };
  }
);
</script>

<style scoped></style>
