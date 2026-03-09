<template>
  <div class="mb-7">
    <div class="row mb-5">
      <div class="col-12 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("fields.paymentAccountNickname")
        }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="nickName"
          v-model="wireForm.name"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="nickName" />
          </div>
        </div>
      </div>
    </div>

    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("fields.accountHolder")
        }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="accountHolder"
          v-model="wireForm.holder"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="accountHolder" />
          </div>
        </div>
      </div>

      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("fields.countryOfYourBank")
        }}</label>

        <Field
          as="select"
          class="form-select form-select-solid fw-semibold"
          v-model="wireForm.bankCountry"
          name="country"
        >
          <option value="">{{ $t("tip.selectCountry") }}</option>
          <option
            v-for="(item, index) in phonesData"
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
    </div>

    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="mb-3"
          >{{ $t("fields.bsb") }} ({{ $t("tip.ifApplication") }})</label
        >
        <Field
          type="text"
          class="form-control form-control-solid"
          name="bsb"
          v-model="wireForm.bsb"
        />
      </div>

      <div class="col-6 d-flex flex-column">
        <label for="" class="mb-3"
          >{{ $t("fields.swiftCode") }} ({{ $t("tip.ifApplication") }})</label
        >
        <Field
          type="text"
          class="form-control form-control-solid"
          name="swiftCode"
          v-model="wireForm.swiftCode"
        />
      </div>
    </div>

    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{ $t("fields.bankName") }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="bankName"
          v-model="wireForm.bankName"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="bankName" />
          </div>
        </div>
      </div>
      <div class="col-6 d-flex flex-column">
        <label for="" class="mb-3 required">{{
          $t("fields.branchName")
        }}</label>
        <Field
          type="text"
          class="form-control form-control-solid required"
          name="branchName"
          v-model="wireForm.branchName"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="branchName" />
          </div>
        </div>
      </div>
    </div>
    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="mb-3 required">{{ $t("fields.state") }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="state"
          v-model="wireForm.state"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="state" />
          </div>
        </div>
      </div>

      <div class="col-6 d-flex flex-column">
        <label for="" class="mb-3 required">{{ $t("fields.city") }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="city"
          v-model="wireForm.city"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="city" />
          </div>
        </div>
      </div>
    </div>
    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{ $t("fields.accountNo") }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="accountNumber"
          v-model="wireForm.accountNo"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="accountNumber" />
          </div>
        </div>
      </div>

      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("fields.confirmAccountNo")
        }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="confirmAccountNumber"
          v-model="wireForm.confirmAccountNo"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="confirmAccountNumber" />
          </div>
        </div>
      </div>
    </div>
  </div>

  <!--begin::Basic info-->
</template>

<script lang="ts" setup>
import * as Yup from "yup";
import { watch, ref, onMounted } from "vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import phonesData from "@/core/data/phonesData";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  data?: object;
}>();
const { t } = useI18n();
const validationSchema = Yup.object().shape({
  nickName: Yup.string().required(t("error.__NICKNAME_IS_REQUIRED__")),
  country: Yup.string().required(t("error.__COUNTRY_IS_REQUIRED__")),
  accountHolder: Yup.string().required(
    t("error.__ACCOUNT_HOLDER_IS_REQUIRED__")
  ),
  accountNumber: Yup.string().required(
    t("error.__ACCOUNT_NUMBER_IS_REQUIRED__")
  ),
  confirmAccountNumber: Yup.string()
    .required(t("error.__CONFIRM_ACCOUNT_NUMBER_IS_REQUIRED__"))
    .oneOf(
      [Yup.ref("accountNumber"), null],
      t("error.__ACCOUNT_NUMBER_NOT_MATCH__")
    ),

  bankName: Yup.string().required(t("error.__BANK_NAME_IS_REQUIRED__")),
  city: Yup.string().required(t("error.cityRequired")),
  branchName: Yup.string().required(t("error.branchNameRequired")),
  state: Yup.string().required(t("error.stateOrProvinceRequired")),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const emits = defineEmits<{
  (e: "submit", _form: object): void;
}>();

const wireForm = ref<any>({});

const returnFormData = handleSubmit(() => {
  emits("submit", wireForm.value);
});

// watch(
//   () => props.data,
//   (newData) => {
//     if (newData) {
//       wireForm.value = newData.info;
//     } else {
//       wireForm.value = {};
//     }
//   }
// );
onMounted(() => {
  console.log("onMounted local", props.data);
  if (props.data) {
    wireForm.value = props.data.info;
  }
});
defineExpose({
  returnFormData,
  resetForm,
  wireForm,
});
</script>

<style lang="scss">
.back-card {
  border: 1px solid #e5e5e5;
  border-radius: 10px;
  padding: 2rem 3rem;
}
</style>
