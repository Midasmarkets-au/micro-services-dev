<template>
  <div class="credit-card-form mt-7">
    <h2 class="[ t-lin t-lin--dot]">{{ t("fields.creditCard") }}</h2>
    <form @submit="onSubmit">
      <div class="form-group">
        <label for="ccNumber">{{ t("fields.ccNumber") }}</label>
        <div class="card-number-input">
          <Field
            name="ccNumber"
            type="text"
            id="ccNumber"
            placeholder="1234 5678 9012 3456"
            v-model="paymentRequireData.request.ccNumber"
          />
          <div class="card-icons">
            <img src="/images/wallet/visa.png" alt="Visa" />
            <img src="/images/wallet/mastercard.png" alt="MasterCard" />
            <img src="/images/wallet/amex.png" alt="American Express" />
            <img src="/images/wallet/discover.png" alt="Discover" />
          </div>
        </div>
        <ErrorMessage name="ccNumber" />
      </div>
      <div class="form-group name-inputs row">
        <div class="col-6">
          <label for="billingFirstName">{{ t("fields.firstName") }}</label>
          <Field
            name="billingFirstName"
            type="text"
            id="billingFirstName"
            v-model="paymentRequireData.request.billingFirstName"
          />
          <ErrorMessage name="billingFirstName" />
        </div>
        <div class="col-6">
          <label for="billingLastName">{{ t("fields.lastName") }}</label>
          <Field
            name="billingLastName"
            type="text"
            id="billingLastName"
            v-model="paymentRequireData.request.billingLastName"
          />
          <ErrorMessage name="billingLastName" />
        </div>
      </div>
      <div class="form-group expiry-cvv-inputs row mb-11">
        <div class="col-4">
          <label for="ccCvc">{{ t("fields.ccCvc") }}</label>
          <Field
            name="ccCvc"
            type="text"
            id="ccCvc"
            placeholder="CVV"
            class="h-45px"
            v-model="paymentRequireData.request.ccCvc"
          />
          <ErrorMessage name="ccCvc" />
        </div>
        <div class="col-4">
          <label for="ccMonth">{{ t("fields.ccMonth") }}</label>
          <Field
            as="select"
            name="ccMonth"
            id="ccMonth"
            class="h-45px"
            v-model="paymentRequireData.request.ccMonth"
          >
            <option value="" disabled>--</option>
            <option
              v-for="month in months"
              :key="month.value"
              :value="month.value"
            >
              {{ month.text }}
            </option>
          </Field>
          <ErrorMessage name="ccMonth" />
        </div>
        <div class="col-4">
          <label for="ccYear">{{ t("fields.ccYear") }}</label>
          <Field
            as="select"
            name="ccYear"
            id="ccYear"
            class="h-45px"
            v-model="paymentRequireData.request.ccYear"
          >
            <option value="" disabled>--</option>
            <option v-for="year in years" :key="year" :value="year">
              {{ year }}
            </option>
          </Field>
          <ErrorMessage name="ccYear" />
        </div>
      </div>
      <h2 class="[ t-lin t-lin--dot]">{{ t("fields.billingInformation") }}</h2>
      <div class="form-group">
        <label for="billingAddress">{{ t("fields.billingAddress") }}</label>
        <Field
          name="billingAddress"
          type="text"
          id="billingAddress"
          v-model="paymentRequireData.request.billingAddress"
        />
        <ErrorMessage name="billingAddress" />
      </div>
      <div class="form-group location-inputs row">
        <div class="col-3">
          <label for="billingCity">{{ t("fields.billingCity") }}</label>
          <Field
            name="billingCity"
            type="text"
            id="billingCity"
            v-model="paymentRequireData.request.billingCity"
          />
          <ErrorMessage name="billingCity" />
        </div>
        <div class="col-3">
          <label for="billingState">{{ t("fields.billingState") }}</label>
          <Field
            name="billingState"
            type="text"
            id="billingState"
            v-model="paymentRequireData.request.billingState"
          />
          <ErrorMessage name="billingState" />
        </div>
        <div class="col-3">
          <label for="billingZipcode">{{ t("fields.billingZipcode") }}</label>
          <Field
            name="billingZipcode"
            type="text"
            id="billingZipcode"
            v-model="paymentRequireData.request.billingZipcode"
          />
          <ErrorMessage name="billingZipcode" />
        </div>
        <div class="col-3">
          <label for="billingCountry">{{ t("fields.billingCountry") }}</label>
          <Field
            class="h-45px"
            as="select"
            name="billingCountry"
            id="billingCountry"
            v-model="paymentRequireData.request.billingCountry"
          >
            <option value="" disabled>--</option>
            <option
              v-for="(item, index) in regionCodes"
              :key="index"
              :label="item.name"
              :value="item.code"
            >
              {{ item.name }}
            </option>
          </Field>
          <ErrorMessage name="billingCountry" />
        </div>
      </div>
      <div class="form-group">
        <label for="billingPhone">{{ t("fields.userMobile") }}</label>
        <Field
          name="billingPhone"
          type="text"
          id="billingPhone"
          v-model="paymentRequireData.request.billingPhone"
        />
        <ErrorMessage name="billingPhone" />
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import * as Yup from "yup";
import { useI18n } from "vue-i18n";
import { useForm, Field, ErrorMessage } from "vee-validate";
import { inject, ref } from "vue";
import { getRegionCodes } from "@/core/data/phonesData";

const paymentRequireData = inject<any>("paymentRequireData");
const { t } = useI18n();
const regionCodes = ref(getRegionCodes());
const months = [
  { value: "01", text: "1" },
  { value: "02", text: "2" },
  { value: "03", text: "3" },
  { value: "04", text: "4" },
  { value: "05", text: "5" },
  { value: "06", text: "6" },
  { value: "07", text: "7" },
  { value: "08", text: "8" },
  { value: "09", text: "9" },
  { value: "10", text: "10" },
  { value: "11", text: "11" },
  { value: "12", text: "12" },
];

const currentYear = new Date().getFullYear();
const years = Array.from({ length: 20 }, (_, i) => currentYear + i);

const validationSchema = Yup.object().shape({
  ccNumber: Yup.string().required(t("tip.requiredField")),
  billingFirstName: Yup.string().required(t("tip.requiredField")),
  ccMonth: Yup.string().required(t("tip.requiredField")),
  ccYear: Yup.string().required(t("tip.requiredField")),
  ccCvc: Yup.string().required(t("tip.requiredField")).max(3, "3 Max"),
  billingAddress: Yup.string()
    .required(t("tip.requiredField"))
    .max(200, "200 Max"),
  billingCity: Yup.string()
    .required(t("tip.requiredField"))
    .max(100, "100 Max"),
  billingState: Yup.string().when("billingCountry", {
    is: "us",
    then: Yup.string().required(t("tip.requiredField")).max(3, "3 Max"),
    otherwise: Yup.string().max(3, "3 Max"),
  }),
  billingZipcode: Yup.string()
    .required(t("tip.requiredField"))
    .max(12, "12 Max"),
  billingCountry: Yup.string().required(t("tip.requiredField")),
  billingPhone: Yup.string().required(t("tip.requiredField")).max(25, "25 Max"),
});

const { handleSubmit } = useForm({
  validationSchema,
});

const onSubmit = handleSubmit((values) => {
  if (values.billingState === undefined || values.billingState === "") {
    values.billingState = "N/A";
  }
  if (values.billingLastName === undefined || values.billingLastName === "") {
    values.billingLastName = "N/A";
  }

  return true;
});

defineExpose({
  onSubmit,
});
</script>

<style scoped>
h2 {
  text-align: center;
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 10px;
  box-sizing: border-box;
  border: 1px solid #ccc;
  border-radius: 5px;
}

.form-group .name-inputs,
.form-group .expiry-cvv-inputs,
.form-group .location-inputs {
  display: flex;
  gap: 10px;
}

.form-group .name-inputs div,
.form-group .expiry-cvv-inputs div,
.form-group .location-inputs div {
  flex: 1;
}

.form-group .card-number-input {
  display: flex;
  align-items: center;
  gap: 10px;
}

.form-group .card-icons {
  display: flex;
  gap: 5px;
}

.form-group .card-icons img {
  width: 40px;
  height: auto;
}

.form-group span {
  color: red;
  font-size: 12px;
  margin-top: 5px;
  display: block;
}

button {
  width: 100%;
  padding: 15px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
}

button:hover {
  background-color: #0056b3;
}

.t-lin {
  display: table;
  white-space: nowrap;
  padding: 0 0.8em;
}
.t-lin:before,
.t-lin:after {
  content: "";
  display: table-cell;
  position: relative;
  top: 0.5em;
  width: 50%;
  border-top: 1px solid;
}
.t-lin:before {
  right: 0.8em;
}
.t-lin:after {
  left: 0.8em;
}

.t-lin--dot:before,
.t-lin--dot:after {
  top: 0.4em;
  border-top: 4px dotted;
}
</style>
