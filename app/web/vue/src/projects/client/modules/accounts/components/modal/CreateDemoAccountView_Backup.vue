<template>
  <div
    id="kt_modal_create_app"
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    ref="liveAccountCreateRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          style="max-height: 80vh"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2
              class="fw-bold"
              :class="{
                'fs-1': isMobile,
              }"
            >
              {{ $t("title.createDemoAccount") }}
            </h2>
            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <!------------------------------------------------------------------- Modal SideBar -->
          <div class="d-flex align-items-center">
            <form
              style="
                width: 750px;
                height: 400px;
                overflow-y: auto;
                border-left: 1px solid #e4e6ef;
                padding: 30px 30px;
              "
            >
              <div class="row info-part">
                <div class="col-lg-6">
                  <div class="mb-8">
                    <label class="required mb-5 createAccountTitle">
                      {{ $t("fields.platform") }}
                    </label>

                    <div class="row">
                      <div
                        class="col-6 mb-3"
                        v-for="(item, index) in ConfigDemoPlatformSelections"
                        :key="index"
                      >
                        <Field
                          v-model="formData.platform"
                          tabindex="2"
                          type="radio"
                          class="btn-check"
                          name="platform"
                          :value="item.id"
                          :id="'CreativeDemoAccount' + item.label + item.id"
                        />
                        <label
                          class="btn btn-outline btn-outline-dashed btn-outline-default d-flex align-items-center"
                          :class="{
                            ' w-125px h-125px flex-column  justify-content-center':
                              isMobile,
                          }"
                          :for="'CreativeDemoAccount' + item.label + item.id"
                        >
                          <span
                            class="svg-icon svg-icon-3x"
                            :class="{
                              'mx-0 mb-3 svg-icon-4x': isMobile,
                            }"
                          >
                            <inline-svg :src="item.iconPath" />
                          </span>

                          <span class="d-block fw-semobold text-start">
                            <span class="text-dark fw-bold d-block">{{
                              item.label
                            }}</span>
                          </span>
                        </label>
                      </div>
                    </div>
                    <ErrorMessage
                      class="fv-plugins-message-container invalid-feedback"
                      name="platform"
                    />
                  </div>

                  <!-------------------------------------------------------------- Step 2:: Account Type -->
                  <div class="mb-8">
                    <div class="fv-row mb-4">
                      <label
                        class="required fw-semobold mb-5 createAccountTitle"
                      >
                        {{ $t("fields.accountType") }}
                      </label>
                      <div class="d-flex flex-column gap-3">
                        <div
                          class="d-flex align-items-center gap-3"
                          v-for="(
                            { label, value }, index
                          ) in ConfigDemoAccountTypeSelections"
                          :key="index"
                        >
                          <!--begin::Checkbox-->
                          <div
                            class="form-check form-check-custom form-check-solid"
                          >
                            <Field
                              :id="'accountTypeDemo' + value"
                              v-model="formData.accountType"
                              class="form-check-input"
                              type="radio"
                              name="accountType"
                              :value="value"
                            />
                          </div>
                          <!--end::Checkbox-->
                          <label
                            class="cursor-pointer"
                            :class="{
                              'fs-1': isMobile,
                            }"
                            :for="'accountTypeDemo' + value"
                          >
                            {{ label }}
                          </label>
                        </div>

                        <ErrorMessage
                          class="fv-plugins-message-container invalid-feedback"
                          name="accountType"
                        />
                      </div>
                    </div>
                  </div>
                </div>

                <div class="col-lg-6 d-flex flex-column">
                  <!-- -------------------------------------------------------------- Step 2:: currency -->
                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-3 createAccountTitle">
                      {{ $t("fields.currency") }}
                    </label>
                    <el-form-item>
                      <Field
                        name="currency"
                        type="text"
                        v-model="formData.currencyId"
                      >
                        <el-select
                          as="el-select"
                          v-model="formData.currencyId"
                          name="currency"
                          type="text"
                          :placeholder="$t('tip.selectCurrency')"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectCurrency") }}
                          </el-option>

                          <el-option
                            v-for="(
                              { label, value }, index
                            ) in ConfigCurrencySelections"
                            :label="label"
                            :key="index"
                            :value="value"
                          />
                        </el-select>
                      </Field>
                      <ErrorMessage
                        class="fv-plugins-message-container invalid-feedback"
                        name="currency"
                      />
                    </el-form-item>
                  </div>
                  <!-- -------------------------------------------------------------- Step 2:: Leverage -->
                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-2 createAccountTitle">
                      {{ $t("fields.leverage") }}
                    </label>
                    <el-form-item>
                      <Field
                        v-model="formData.leverage"
                        name="leverage"
                        type="text"
                      >
                        <el-select
                          v-model="formData.leverage"
                          name="leverage"
                          type="text"
                          :placeholder="$t('tip.selectLeverage')"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectLeverage") }}
                          </el-option>
                          <el-option
                            v-for="(
                              { label, value }, index
                            ) in ConfigLeverageSelections"
                            :label="label"
                            :key="index"
                            :value="value"
                          /> </el-select
                      ></Field>
                      <ErrorMessage
                        as="div"
                        class="fv-plugins-message-container invalid-feedback"
                        name="leverage"
                      />
                    </el-form-item>
                  </div>

                  <!-- -------------------------------------------------------------- Step 2:: Referral -->

                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-2 createAccountTitle">
                      {{ $t("fields.amount") }}
                    </label>
                    <el-form-item>
                      <Field
                        v-model.number="formData.amount"
                        name="amount"
                        type="number"
                      >
                        <el-input
                          v-model.number="formData.amount"
                          name="amount"
                          type="number"
                          :placeholder="$t('tip.pleaseInput')" /></Field
                      ><ErrorMessage
                        as="div"
                        class="fv-plugins-message-container invalid-feedback"
                        name="amount"
                      />
                    </el-form-item>
                  </div>
                </div>
              </div>
            </form>
          </div>
          <!--end::Content-->

          <div class="modal-footer">
            <!--begin::Actions-->
            <div class="d-flex flex-stack">
              <!--begin::Wrapper-->
              <div>
                <button
                  class="btn btn-primary"
                  @click="handleDemoSubmitted"
                  :disabled="isSubmitting || isLoading"
                >
                  <span v-if="isLoading">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>
                  <span v-else
                    >{{ $t("action.submit") }}
                    <!--                    <span class="svg-icon svg-icon-3 me-1">-->
                    <!--                      <inline-svg src="/images/icons/arrows/arr064.svg" />-->
                    <!--                    </span>-->
                  </span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, watch, onUnmounted } from "vue";
import { showModal, hideModal, removeModalBackdrop } from "@/core/helpers/dom";
import { ConfigDemoPlatformSelections } from "@/core/types/ServiceTypes";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import {
  ConfigAccountTypeSelections,
  ConfigDemoAccountTypeSelections,
} from "@/core/types/AccountInfos";
import AccountService from "../../services/AccountService";
import { Field, ErrorMessage, useForm } from "vee-validate";
import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { isMobile } from "@/core/config/WindowConfig";

const { t, locale } = useI18n();
const liveAccountCreateRef = ref<HTMLElement | null>(null);
const isLoading = ref(false);
const isSubmitting = ref(false);
const formData = ref<any>({});

const validationSchema = Yup.object().shape({
  platform: Yup.number().required().label("Platform"),
  currency: Yup.number().required().label("Currency"),
  accountType: Yup.number().required().label("Account type"),
  leverage: Yup.number().required().label("Leverage"),
  amount: Yup.number().required().label("Amount"),
});

onUnmounted(() => {
  removeModalBackdrop();
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

// const isValid = ref(false);

// watch(
//   formData,
//   () => {
//     isValid.value = !!(
//       formData.value.platform &&
//       formData.value.currencyId &&
//       formData.value.leverage &&
//       formData.value.accountType &&
//       formData.value.amount
//     );
//   },
//   {
//     deep: true,
//   }
// );

const emits = defineEmits<{
  (e: "demoAccountCreated"): void;
}>();

const close = () => {
  hideModal(liveAccountCreateRef.value);
};

// check here, if the form is valid, then submit, else, continue

const handleDemoSubmitted = handleSubmit(async () => {
  try {
    isLoading.value = true;
    // formData.value.platform = 21;
    await AccountService.createDemoAccount({
      ...formData.value,
    });
    MsgPrompt.success(t("tip.createDemoAccountSuccess")).then(() => {
      emits("demoAccountCreated");
      close();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = true;
    isLoading.value = false;
  }
});

const show = async () => {
  showModal(liveAccountCreateRef.value);
  isSubmitting.value = false;
  isLoading.value = false;
  formData.value = {};
  resetForm({ values: {} });

  await nextTick();
};

// onMounted(() => {});

defineExpose({
  show,
});
</script>

<style scoped lang="scss">
.info-part {
  @media (max-width: 768px) {
    label {
      font-size: 16px;
    }
  }
}

.fields-title {
  color: #0053ad;
  background-color: #f5f7fa;
  border-right: 1px solid #e4e6ef;
}
.stepper-icon-round {
  border-radius: 100% !important;
}

.createAccountTitle {
  color: #717171;

  @media (max-width: 768px) {
    font-size: 16px;
  }
}

.alpha-tag {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
  border-radius: 8px;
  padding: 0 8px;
}

.standard-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ff8a00;
  border-radius: 8px;
  padding: 0 8px;
}

.statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-weight: 700;
}
</style>
