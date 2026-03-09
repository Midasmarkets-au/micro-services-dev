<template>
  <div>
    <div v-if="isLoading">
      <LoadingCentralBox />
    </div>
    <div class="d-flex flex-column gap-4" v-if="!isLoading">
      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.type") }}
        </label>
        <div class="row">
          <div
            v-for="(item, index) in ConfigAccountTypeSelections"
            :key="index"
            class="col-lg-6 mb-3"
          >
            <Field
              v-model="formData.accountType"
              type="radio"
              class="btn-check"
              name="accountType"
              :value="item.value"
              :id="'CreativeDemoAccount' + item.label + item.value"
            >
            </Field>
            <label
              class="btn btn-outline btn-outline-dashed btn-outline-default p-5 d-flex align-items-center"
              :for="'CreativeDemoAccount' + item.label + item.value"
            >
              <span class="svg-icon me-5 svg-icon-1">
                <inline-svg :src="item.iconPath" />
              </span>

              <span class="d-block fw-semobold text-start">
                <span class="text-dark fw-bold d-block fs-4">
                  {{ item.label }}
                </span>
              </span>
            </label>
          </div>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="accountType"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>

      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.currency") }}
        </label>
        <div>
          <Field v-model="formData.currencyId" name="currency">
            <el-select
              v-model="formData.currencyId"
              name="currency"
              type="text"
              :placeholder="$t('tip.selectCurrency')"
            >
              <el-option value="" disabled :label="$t('tip.selectCurrency')" />
              <el-option
                v-for="({ label, value }, index) in ConfigCurrencySelections"
                :label="label"
                :key="index"
                :value="value"
              />
            </el-select>
          </Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="currency"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>

      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.leverage") }}
        </label>
        <div>
          <Field v-model="formData.leverage" name="leverage">
            <el-select
              v-model="formData.leverage"
              name="leverage"
              type="text"
              :placeholder="$t('tip.selectLeverage')"
            >
              <el-option value="" disabled :label="$t('tip.selectLeverage')" />
              <el-option
                v-for="({ label, value }, index) in ConfigLeverageSelections"
                :label="label"
                :key="index"
                :value="value"
              />
            </el-select>
          </Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="leverage"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>

      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.platform") }}
        </label>
        <div>
          <Field v-model="formData.platform" name="platform">
            <el-select
              v-model="formData.platform"
              name="platform"
              type="text"
              :placeholder="$t('tip.selectPlatform')"
            >
              <el-option value="" disabled :label="$t('tip.selectPlatform')" />
              <el-option
                v-for="(item, index) in ConfigDemoPlatformSelections"
                :label="item.label"
                :key="index"
                :value="item.id"
              />
            </el-select>
          </Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="platform"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>

      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.password") }}
        </label>
        <div>
          <Field v-model="formData.password" name="password">
            <el-input
              v-model="formData.password"
              name="password"
              type="password"
              :placeholder="$t('tip.inputPassword')"
          /></Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="password"
          />
        </div>
      </div>

      <div class="fv-row mb-2">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.confirmedPassword") }}
        </label>
        <div>
          <Field
            name="confirmation"
            type="password"
            v-model="confirmedPassword"
          >
            <el-input
              name="confirmation"
              type="password"
              :placeholder="$t('fields.confirmedPassword')"
              v-model="confirmedPassword"
          /></Field>
          <ErrorMessage
            as="div"
            class="fv-plugins-message-container invalid-feedback"
            name="confirmation"
          >
            {{ $t("tip.pwdNotMatch") }}
          </ErrorMessage>
        </div>
      </div>
    </div>
  </div>
  <!-- <div>{{ formData }}</div> -->
  <!-- <div>{{ confirmedPassword }}</div> -->
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import AccountService from "../../services/AccountService";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigAccountTypeSelections } from "@/core/types/AccountInfos";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import { ConfigDemoPlatformSelections } from "@/core/types/ServiceTypes";

const isLoading = ref(true);
const isSubmitting = ref(false);
const formData = ref<any>({});
const confirmedPassword = ref("");
const { t } = useI18n();

const props = defineProps<{
  publicConfig: any;
}>();

const validationSchema = {
  accountType: "required",
  currency: "required",
  leverage: "required",
  platform: "required",
  password: "accountPassword",
  confirmation: "confirmed:@password",
};

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const initData = () => {
  formData.value = {};
  resetForm({ values: {} });
};

const submit = handleSubmit(async () => {
  isSubmitting.value = true;
  try {
    await AccountService.createDemoAccount(formData.value);
    MsgPrompt.success(t("tip.createDemoAccountSuccess"));
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
});

onMounted(async () => {
  isLoading.value = true;
  initData();
  isLoading.value = false;
});

defineExpose({ submit, isLoading: isSubmitting });
</script>

<style scoped></style>
