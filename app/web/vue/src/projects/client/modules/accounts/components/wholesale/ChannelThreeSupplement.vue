<template>
  <div class="card wholesale-process-card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("wholesale.channelThreeTitle_1") }}
        </h2>
      </div>
      <hr />
      <div class="fv-row mb-10 ms-3" style="font-size: 16px">
        <div class="mt-5">
          <h4 class="mb-7">
            {{ $t("wholesale.channelThreeTitle_2") }}
          </h4>

          <!-- ================================================================== Q 1 -->
          <!-- ================================================================== Q 1 -->
          <div class="d-flex flex-column mb-7 fv-row">
            <label class="fs-5 fw-semobold mb-5 required">{{
              $t("wholesale.channelThreeQ1")
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
          <div class="d-flex flex-column mb-7 fv-row">
            <label class="fs-5 fw-semobold mb-5 required">{{
              $t("wholesale.channelThreeQ2")
            }}</label>

            <Field
              v-model="proApplicationMethodData.q2"
              class="form-control form-control-solid"
              placeholder=""
              name="q2"
            />
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="q2" />
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

const emits = defineEmits(["saved", "hasError"]);

const { t } = useI18n();

const proApplicationMethodData = ref<any>(
  props.data ?? {
    q1: "",
    q2: "",
  }
);

const startedSchema = Yup.object().shape({
  q1: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q1")),
  q2: Yup.string()
    .required(t("error.fieldIsRequired"))
    .label(t("wholesale.q2")),
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
