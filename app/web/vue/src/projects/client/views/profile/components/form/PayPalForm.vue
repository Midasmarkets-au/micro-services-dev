<template>
  <div class="mb-7">
    <div class="row mb-5">
      <div class="col-6 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("fields.paymentMethodNickName")
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
import { onMounted, ref } from "vue";
import { Field, ErrorMessage, useForm } from "vee-validate";

const validationSchema = Yup.object().shape({
  nickName: Yup.string().required().label("Nick Name"),
  accountHolder: Yup.string().required().label("Account Holder"),
  accountNumber: Yup.string().required().label("Account Number"),
  confirmAccountNumber: Yup.string()
    .required()
    .oneOf([Yup.ref("accountNumber"), null], "Account Number must match")
    .label("Confirm Account Number"),
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

defineExpose({
  returnFormData,
});
</script>

<style lang="scss">
.back-card {
  border: 1px solid #e5e5e5;
  border-radius: 10px;
  padding: 2rem 3rem;
}
</style>
