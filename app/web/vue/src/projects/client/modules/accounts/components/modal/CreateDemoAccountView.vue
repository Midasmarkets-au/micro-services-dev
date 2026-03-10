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
          :style="isMobile ? 'max-height: 100vh; ' : 'max-height: 88vh; '"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2
              class="fs-2"
              :class="{
                'fs-1': isMobile,
              }"
            >
              {{ $t("title.createDemoAccount") }}
            </h2>
            <div class="" data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <!------------------------------------------------------------------- Modal SideBar -->
          <table
            v-if="isLoading"
            class="table align-middle table-row-bordered"
            id="kt_permissions_table"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>

          <div v-else class="d-flex align-items-center">
            <form
              style="
                overflow-y: auto;
                border-left: 1px solid #e4e6ef;
                padding: 30px 30px;
              "
              :style="
                isMobile
                  ? 'width: 100%; height: 600px;'
                  : 'width: 750px; height: 400px;'
              "
            >
              <div class="row info-part">
                <div class="col-lg-12 d-flex flex-column px-12">
                  <!-------------------------------------------------------------- Step 2:: Account Type -->
                  <div class="fv-row">
                    <div class="mb-8">
                      <label class="required mb-5 createAccountTitle">
                        {{ $t("fields.platform") }}
                      </label>

                      <div class="row">
                        <div
                          class="col-6 mb-3"
                          v-for="(item, index) in availableServices"
                          :key="index"
                        >
                          <Field
                            v-model="formData.platform"
                            tabindex="2"
                            type="radio"
                            class="btn-check"
                            name="platform"
                            :value="item.platform"
                            :id="'CreativeDemoAccount' + item.label + item.id"
                          />
                          <label
                            class="bg-light btn btn-outline btn-outline-dashed btn-outline-default d-flex align-items-center"
                            :class="{
                              ' w-125px h-125px flex-column  justify-content-center':
                                isMobile,
                            }"
                            :for="'CreativeDemoAccount' + item.label + item.id"
                          >
                            <span
                              class="svg-icon svg-icon-2x"
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
                  </div>
                  <!-- -------------------------------------------------------------- Step 2:: currency -->
                  <div class="fv-row">
                    <label class="required fw-semobold mb-2 createAccountTitle">
                      {{ $t("fields.accountType") }}
                    </label>
                    <el-form-item>
                      <Field
                        v-model="formData.accountType"
                        name="accountType"
                        type="text"
                      >
                        <el-select
                          v-model="formData.accountType"
                          name="accountType"
                          type="text"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectAccountType") }}
                          </el-option>
                          <el-option
                            v-for="(item, index) in accountTypeSelections"
                            :label="t(`type.account.${item}`)"
                            :key="index"
                            :value="item"
                          /> </el-select
                      ></Field>
                      <ErrorMessage
                        as="div"
                        class="fv-plugins-message-container invalid-feedback"
                        name="accountType"
                      />
                    </el-form-item>
                  </div>

                  <div class="fv-row">
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
                            v-for="(item, index) in currencyTypeSelections"
                            :label="t(`type.currency.${item}`)"
                            :key="index"
                            :value="item"
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
                  <div class="fv-row">
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
                            v-for="(item, index) in leverageTypeSelections"
                            :label="item + ':1'"
                            :key="index"
                            :value="item"
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

                  <div class="fv-row">
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
                  class="btn btn-primary btn-sm btn-radius"
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
import { ref, nextTick, computed, onUnmounted, watch } from "vue";
import { showModal, hideModal, removeModalBackdrop } from "@/core/helpers/dom";
import { getDemoPlatformSelections } from "@/core/types/ServiceTypes";
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
import { useStore } from "@/store";
import { getTenancy } from "@/core/types/TenantTypes";

const store = useStore();
const { t, locale } = useI18n();
const liveAccountCreateRef = ref<HTMLElement | null>(null);
const isLoading = ref(false);
const isSubmitting = ref(false);
const formData = ref<any>({});
const accountTypeSelections = ref([] as any);
const currencyTypeSelections = ref([] as any);
const leverageTypeSelections = ref([] as any);
const availableServices = ref([] as any);
const siteId = computed(() => store.state.AuthModule.config.siteId);

const validationSchema = Yup.object().shape({
  platform: Yup.number().required(t("tip.requiredField")).label("Platform"),
  currency: Yup.number().required(t("tip.requiredField")).label("Currency"),
  accountType: Yup.number()
    .required(t("tip.requiredField"))
    .label("Account type"),
  leverage: Yup.number().required(t("tip.requiredField")).label("Leverage"),
  amount: Yup.number().required(t("tip.requiredField")).label("Amount"),
});

onUnmounted(() => {
  removeModalBackdrop();
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

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
  isLoading.value = true;
  formData.value = {};
  resetForm({ values: {} });

  await nextTick();

  try {
    await AccountService.getDemoAccountConfig().then((res) => {
      availableServices.value = getDemoPlatformSelections(
        res.tradingPlatformAvailable
      ).value;
      console.log("availableServices.value", availableServices.value);
      currencyTypeSelections.value = res.currencyAvailable;
      accountTypeSelections.value = res.accountTypeAvailable;
      leverageTypeSelections.value = res.leverageAvailable;
    });

    formData.value.accountType = accountTypeSelections.value[0];
    formData.value.currencyId = currencyTypeSelections.value[0];

    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

watch(
  () => formData.value.currencyId,
  () => {
    if (formData.value.currencyId == 36) formData.value.platform = 21;
  }
);

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
  //color: #717171;

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
