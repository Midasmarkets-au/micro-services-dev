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
      <div class="col-12 d-flex flex-column">
        <label for="" class="required mb-3">{{
          $t("title.usdtWalletAddress")
        }}</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="walletAddress"
          v-model="wireForm.walletAddress"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="walletAddress" />
          </div>
        </div>
      </div>
    </div>
  </div>

  <!--begin::Basic info-->
</template>

<script lang="ts" setup>
import * as Yup from "yup";
import { ref, onMounted } from "vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  data?: object;
}>();
const { t } = useI18n();
const validationSchema = Yup.object().shape({
  nickName: Yup.string().required(t("error.__NICKNAME_IS_REQUIRED__")),
  walletAddress: Yup.string().required(t("error.__ACCOUNT_IS_REQUIRED__")),
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
//     console.log("props.data", props.data);
//     if (newData) {
//       wireForm.value = newData.info;
//     } else {
//       wireForm.value = {};
//     }
//   }
// );
onMounted(() => {
  if (props.data && props.data.info) {
    wireForm.value = props.data.info;
  } else {
    wireForm.value = {};
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
