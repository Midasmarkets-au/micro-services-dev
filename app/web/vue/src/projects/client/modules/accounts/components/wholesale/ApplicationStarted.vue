<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("wholesale.applicationMethod") }}</div>
    </div>
    <div class="card-body">
      <!-- <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("wholesale.applicationMethod") }}
        </h2>
      </div>
      <hr /> -->
      <div class="fv-row ms-3" style="font-size: 16px">
        <div class="row">
          <h4 class="mb-9">
            {{ $t("wholesale.startTitle_1") }}
          </h4>

          <!-- ================================================================== Method 1 -->
          <!-- ================================================================== Method 1 -->
          <div class="d-flex">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid ms-0 me-5 ms-md-5"
            >
              <Field
                v-model="proApplicationMethodData.method"
                class="form-check-input"
                type="radio"
                name="proApplicationMethod"
                value="1"
              />
            </div>
            <!--end::Checkbox-->
            <div class="fs-md-4 fs-5 fw-bold">
              1.&nbsp;&nbsp;&nbsp;{{ $t("wholesale.startTitle_2") }}
            </div>
          </div>
          <div class="mt-5 description">
            <div>
              {{ $t("wholesale.startContent_1") }}
            </div>
            <div class="mt-5 ms-md-9">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_2") }}
                </div>
              </div>
              <div class="mt-3 mb-3">{{ $t("wholesale.or") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_3") }}
                </div>
              </div>
            </div>

            <div class="mt-9 fs-3 fw-bold">
              {{ $t("wholesale.startTitle_3") }}
            </div>
            <div class="mt-6">
              {{ $t("wholesale.startContent_4") }}
              <a target="_blank" :href="fileLink"
                ><strong>{{ $t("wholesale.startLink_1") }}</strong></a
              >
              {{ $t("wholesale.startContent_5") }}
            </div>
            <div class="mt-6">
              {{ $t("wholesale.startContent_6") }}
              <a href=""><strong>wholesale@midasmkts.com</strong></a
              >. {{ $t("wholesale.startContent_7") }}
            </div>

            <div class="mt-6">
              MDM Co Ltd<br />Suite 18.03, Level 18, 201 Elizabeth Street<br />Sydney,
              NSW 2000
            </div>
          </div>

          <!-- ================================================================== Method 2 -->
          <!-- ================================================================== Method 2 -->
          <div class="d-flex mt-17">
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid ms-0 me-5 ms-md-5"
            >
              <Field
                v-model="proApplicationMethodData.method"
                class="form-check-input"
                type="radio"
                name="proApplicationMethod"
                value="2"
              />
            </div>
            <!--end::Checkbox-->
            <div class="fs-md-4 fs-5 fw-bold">
              2.&nbsp;&nbsp;&nbsp;{{ $t("wholesale.startTitle_4") }}
            </div>
          </div>

          <div class="mt-5 description">
            <div>
              {{ $t("wholesale.startContent_9") }}
            </div>

            <!-- ================================================================== Channel 1 -->
            <div
              class="mt-9 fs-3 fw-bold"
              style="
                /* text-decoration: underline;
                text-decoration-color: #ffce00;
                text-decoration-thickness: 3px;
                text-underline-offset: 5px; */
              "
            >
              {{ $t("wholesale.startTitle_5") }}
            </div>
            <div class="mt-5 ms-md-9">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_10") }}
                </div>
              </div>
              <div class="mt-3 mb-3">{{ $t("wholesale.and") }}</div>
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_11") }}
                </div>
              </div>
            </div>

            <!-- ================================================================== Channel 2 -->
            <div
              class="mt-9 fs-3 fw-bold"
              style="
                /* text-decoration: underline;
                text-decoration-color: #ffce00;
                text-decoration-thickness: 3px;
                text-underline-offset: 5px; */
              "
            >
              {{ $t("wholesale.startTitle_6") }}
            </div>
            <div class="mt-5 ms-md-9">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_12") }}
                </div>
              </div>
            </div>

            <!-- ================================================================== Channel 3 -->
            <div
              class="mt-9 fs-3 fw-bold"
              style="
                /* text-decoration: underline;
                text-decoration-color: #ffce00;
                text-decoration-thickness: 3px;
                text-underline-offset: 5px; */
              "
            >
              {{ $t("wholesale.startTitle_7") }}
            </div>
            <div class="mt-5 ms-md-9">
              <div class="d-flex">
                <div class="bullet"></div>
                <div>
                  {{ $t("wholesale.startContent_13") }}
                </div>
              </div>
            </div>
          </div>

          <div class="d-flex mt-19 fw-bold" style="color: #b52025">
            <div class="me-5">*</div>
            <div>
              {{ $t("wholesale.startContent_14") }}
            </div>
          </div>

          <!-- ================================================================== ErrorMessage -->
          <!-- ================================================================== ErrorMessage -->
          <div
            class="fv-plugins-message-container mt-11"
            style="text-align: center"
          >
            <div class="fv-help-block">
              <ErrorMessage name="proApplicationMethod" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as Yup from "yup";
import { ref, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../../services/AccountService";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { wholeSaleDocs, getWholeSaleDocs } from "@/core/data/bcrDocs";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
var fileLink = getWholeSaleDocs(
  wholeSaleDocs.certificateByQualifiedAccountant.title
);

const props = defineProps<{
  data?: object;
  accountUid?: number;
  account?: any;
}>();

const emits = defineEmits(["saved", "hasError"]);

const proApplicationMethodData = ref<any>(
  props.data ?? {
    method: "",
  }
);

const startedSchema = Yup.object().shape({
  proApplicationMethod: Yup.string()
    .required(t("wholesale.selectApplicationMethod"))
    .label("Method"),
});

const { resetForm, handleSubmit } = useForm({
  validationSchema: startedSchema,
});

const handleStepSubmit = handleSubmit(() => {
  if (proApplicationMethodData.value.method == 1) {
    submitForm();
  } else {
    emits("saved", 1, proApplicationMethodData.value);
  }
}, onInvalidSubmit);

function onInvalidSubmit() {
  emits("hasError");
}

const submitForm = async () => {
  try {
    await AccountService.postWholesaleApplication({
      accountUid: props.accountUid,
      AccountNumber: props.account.tradeAccount.accountNumber,
      request: { started: proApplicationMethodData.value },
    });

    emits("saved", 1, proApplicationMethodData.value);
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  resetForm();
});

defineExpose({
  handleStepSubmit,
});
</script>

<style type="scss" scoped>
.bullet {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: #000;
  margin-right: 10px;
  margin-top: 5px;
}

.description {
  padding-left: 96px;

  @media (max-width: 768px) {
    padding-left: 10px;
  }
}
</style>
