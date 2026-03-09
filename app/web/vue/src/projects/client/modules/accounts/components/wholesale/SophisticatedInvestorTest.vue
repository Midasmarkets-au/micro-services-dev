<template>
  <div class="card wholesale-process-card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("wholesale.testTitle_1") }}
        </h2>
      </div>
      <hr />
      <div class="fv-row mb-5 ms-3" style="font-size: 16px">
        <div class="row mt-5">
          <div class="mt-9 mb-9 fs-4">
            {{ $t("wholesale.testContent_1") }}
          </div>

          <!-- ================================================================== Channel 1 -->
          <!-- ================================================================== Channel 1 -->
          <div class="d-flex">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid ms-0 me-5 ms-md-5"
            >
              <Field
                v-model="proApplicationChannelData.channel"
                class="form-check-input"
                type="radio"
                name="proApplicationChannel"
                value="1"
              />
            </div>
            <!--end::Checkbox-->
            <div style="font-size: 18px; font-weight: bold">
              {{ $t("wholesale.testTitle_2") }}
            </div>
          </div>
          <div class="ps-md-17">
            <div class="mt-5 ms-2">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_2") }}
                </div>
              </div>
              <div class="mt-7 mb-7 ms-7">{{ $t("wholesale.and") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_3") }}
                </div>
              </div>
              <div class="mt-7 mb-7 ms-7">{{ $t("wholesale.and") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_4") }}
                  <a
                    target="_blank"
                    :href="
                      getWholeSaleDocs(
                        wholeSaleDocs.sophisticatedInvestorAcknowledgement.title
                      )
                    "
                    ><strong>{{ $t("wholesale.testContent_5") }}</strong></a
                  >
                  {{ $t("wholesale.testContent_6") }}.
                </div>
              </div>
            </div>

            <div class="fs-7 ms-9 mt-9">
              <div>{{ $t("wholesale.testContent_7") }}</div>
              <div>
                {{ $t("wholesale.testContent_8") }}
              </div>
            </div>
          </div>

          <!-- ================================================================== Channel 2 -->
          <!-- ================================================================== Channel 2 -->
          <div class="d-flex mt-12">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid ms-0 me-5 ms-md-5"
            >
              <Field
                v-model="proApplicationChannelData.channel"
                class="form-check-input"
                type="radio"
                name="proApplicationChannel"
                value="2"
              />
            </div>
            <!--end::Checkbox-->
            <div style="font-size: 18px; font-weight: bold">
              {{ $t("wholesale.testTitle_3") }}
            </div>
          </div>
          <div class="ps-md-17">
            <div class="mt-5 ms-2">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_9") }}
                </div>
              </div>
              <div class="mt-7 mb-7 ms-7">{{ $t("wholesale.and") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_10") }}
                  <a
                    target="_blank"
                    :href="
                      getWholeSaleDocs(
                        wholeSaleDocs.sophisticatedInvestorAcknowledgement.title
                      )
                    "
                    ><strong>{{ $t("wholesale.testContent_11") }}</strong></a
                  >

                  {{ $t("wholesale.testContent_12") }}.
                </div>
              </div>
            </div>
          </div>

          <!-- ================================================================== Channel 3 -->
          <!-- ================================================================== Channel 3 -->
          <div class="d-flex mt-12">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid ms-0 me-5 ms-md-5"
            >
              <Field
                v-model="proApplicationChannelData.channel"
                class="form-check-input"
                type="radio"
                name="proApplicationChannel"
                value="3"
              />
            </div>
            <!--end::Checkbox-->
            <div style="font-size: 18px; font-weight: bold">
              {{ $t("wholesale.testTitle_4") }}
            </div>
          </div>
          <div class="ps-md-17">
            <div class="mt-5 ms-2">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_13") }}
                </div>
              </div>
              <div class="mt-7 mb-7 ms-7">{{ $t("wholesale.and") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.testContent_14") }}
                  <a
                    target="_blank"
                    :href="
                      getWholeSaleDocs(
                        wholeSaleDocs.sophisticatedInvestorAcknowledgement.title
                      )
                    "
                    ><strong>{{ $t("wholesale.testContent_15") }}</strong></a
                  >
                  {{ $t("wholesale.testContent_16") }}.
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ================================================================== ErrorMessage -->
        <!-- ================================================================== ErrorMessage -->
        <div
          class="fv-plugins-message-container mt-11"
          style="text-align: center"
        >
          <div class="fv-help-block">
            <ErrorMessage name="proApplicationChannel" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, computed } from "vue";
import { Field, ErrorMessage, useSubmitForm } from "vee-validate";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useForm } from "vee-validate";
import { useStore } from "@/store";
import { getLanguage } from "@/core/types/LanguageTypes";
import { wholeSaleDocs, getWholeSaleDocs } from "@/core/data/bcrDocs";
const { t } = useI18n();
const props = defineProps<{
  data?: object;
}>();

const emits = defineEmits(["saved", "hasError"]);

const proApplicationChannelData = ref<any>(
  props.data ?? {
    channel: "",
  }
);
const startedSchema = Yup.object().shape({
  proApplicationChannel: Yup.string()
    .required(t("error.SELECT_REQUIRE") + t("fields.channel"))
    .label("Channel"),
});

const { resetForm, handleSubmit } = useForm({
  validationSchema: startedSchema,
});

const handleStepSubmit = handleSubmit(() => {
  emits("saved", 2, proApplicationChannelData.value);
}, onInvalidSubmit);

function onInvalidSubmit() {
  emits("hasError");
}

onMounted(async () => {
  resetForm();
});

defineExpose({
  handleStepSubmit,
});
</script>

<style>
.bullet {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: #000;
  margin-right: 10px;
  margin-top: 5px;
}
</style>
