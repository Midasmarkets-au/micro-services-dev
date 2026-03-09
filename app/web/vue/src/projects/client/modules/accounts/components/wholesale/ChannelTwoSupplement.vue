<template>
  <div class="card wholesale-process-card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("wholesale.channelTwoTitle_1") }}
        </h2>
      </div>
      <hr />
      <div class="fv-row mb-10 ms-3" style="font-size: 16px">
        <div class="mt-5">
          <h4 class="mb-7">
            {{ $t("wholesale.channelTwoContent_1") }}
          </h4>

          <!-- ================================================================== Q 1 -->
          <!-- ================================================================== Q 1 -->
          <div class="d-flex flex-column mb-7 fv-row">
            <label class="fs-5 fw-semobold mb-5 required">{{
              $t("wholesale.channelTwoQ1")
            }}</label>

            <Field
              v-model="proApplicationMethodData.q1"
              class="form-control form-control-solid"
              placeholder=""
              name="q1"
            />
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="q1" />
              </div>
            </div>
          </div>

          <!-- ================================================================== Q 2 -->
          <!-- ================================================================== Q 2 -->
          <div class="d-flex flex-column mb-11 fv-row">
            <label class="fs-5 fw-semobold mb-5 required">{{
              $t("wholesale.channelTwoQ2")
            }}</label>

            <div class="d-flex">
              <!--begin::Checkbox-->
              <div class="form-check form-check-custom form-check-solid mx-5">
                <Field
                  v-model="proApplicationMethodData.q2"
                  class="form-check-input"
                  type="radio"
                  name="q2"
                  value="yes"
                />
              </div>
              <!--end::Checkbox-->
              <div style="font-size: 18px; font-weight: bold">
                {{ $t("wholesale.yes") }}
              </div>
            </div>

            <div class="d-flex mt-3">
              <!--begin::Checkbox-->
              <div class="form-check form-check-custom form-check-solid mx-5">
                <Field
                  class="form-check-input"
                  type="radio"
                  name="q2"
                  value="no"
                />
              </div>
              <!--end::Checkbox-->
              <div style="font-size: 18px; font-weight: bold">
                {{ $t("wholesale.no") }}
              </div>
            </div>

            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="q2" />
              </div>
            </div>
          </div>

          <!-- ================================================================== Q 3 -->
          <!-- ================================================================== Q 3 -->
          <div class="d-flex flex-column mb-11 fv-row">
            <h4 class="fs-5 fw-semobold mb-5 required">
              {{ $t("wholesale.channelTwoQ3") }}
            </h4>

            <Field
              v-model="proApplicationMethodData.q3"
              class="form-control form-control-solid"
              placeholder=""
              name="q3"
            />
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="q3" />
              </div>
            </div>
          </div>

          <!-- ================================================================== Q 4 -->
          <!-- ================================================================== Q 4 -->
          <div class="d-flex flex-column mb-5 fv-row">
            <h4 class="fs-5 fw-semobold mb-5 required">
              {{ $t("wholesale.channelTwoQ4") }}
            </h4>

            <div
              class="d-flex mt-3"
              v-for="(item, index) in q4Options"
              :key="index"
            >
              <!--begin::Checkbox-->
              <div class="form-check form-check-custom form-check-solid mx-5">
                <Field
                  v-model="proApplicationMethodData.q4"
                  class="form-check-input"
                  type="radio"
                  name="q4"
                  :value="item.value"
                />
              </div>
              <!--end::Checkbox-->
              <div style="font-size: 18px; font-weight: bold">
                {{ item.label }}
              </div>
            </div>

            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="q4" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as Yup from "yup";
import { useI18n } from "vue-i18n";
import { ref, onMounted } from "vue";
import { useForm } from "vee-validate";
import { Field, ErrorMessage } from "vee-validate";

const props = defineProps<{
  data?: object;
  accountUid?: number;
}>();

const proApplicationMethodData = ref<any>(
  props.data ?? {
    q1: "",
    q2: "",
    q3: "",
    q4: "",
  }
);

const { t } = useI18n();
const emits = defineEmits(["saved", "hasError"]);

const q4Options = [
  {
    label: t("wholesale.q4Option1"),
    value: "1",
  },
  {
    label: t("wholesale.q4Option2"),
    value: "2",
  },
  {
    label: t("wholesale.q4Option3"),
    value: "3",
  },
  {
    label: t("wholesale.q4Option4"),
    value: "4",
  },
  {
    label: t("wholesale.q4Option5"),
    value: "5",
  },
  {
    label: t("wholesale.q4Option6"),
    value: "6",
  },
];

const startedSchema = Yup.object().shape({
  q1: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q1")),
  q2: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q2")),
  q3: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q3")),
  q4: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q4")),
});

const { resetForm, handleSubmit } = useForm({
  validationSchema: startedSchema,
});

const handleStepSubmit = handleSubmit(() => {
  emits("saved", 3, proApplicationMethodData.value);
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
