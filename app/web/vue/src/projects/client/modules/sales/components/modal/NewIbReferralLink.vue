<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkDetailModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ $t("action.addNewLink") }}</h2>
          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->

        <div v-if="isLoading" class="d-flex justify-content-center">
          <LoadingRing />
        </div>
        <div v-else class="form-outer">
          <div
            v-if="rebateRuleDetail.calculatedLevelSetting.distributionType == 3"
            class="d-flex flex-column align-items-center"
          >
            <div class="fs-3 mt-11">{{ $t("tip.isMLM") }}</div>
            <div class="fs-3 mt-3 mb-11">
              {{ $t("tip.referralCodeNotAvailable") }}
            </div>
          </div>
          <div v-else class="card-body py-0 mt-13">
            <!--------------------------------------------- Step 1 -->

            <div class="row">
              <div class="col-12 col-lg-6">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step1") }}</span>
                  <span class="stepContent">{{ $t("tip.nameYourLink") }}</span>
                </div>

                <Field
                  class="form-control form-control-solid w-300px h-48px mt-5"
                  :placeholder="$t('tip.pleaseInput')"
                  name="ibLinkName"
                  v-model="requestData.name"
                />
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="ibLinkName" />
                  </div>
                </div>
              </div>
              <div class="col-12 col-lg-6">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step2") }}</span>
                  <span class="stepContent">{{
                    $t("action.chooseLanguage")
                  }}</span>
                </div>

                <Field
                  name="ibLinkLanguage"
                  class="form-select form-select-solid mt-5 IB-rebate-link-select"
                  as="select"
                  v-model="requestData.language"
                >
                  <option value="en-us">English</option>
                  <option value="zh-cn">Simplify Chinese (简体中文)</option>
                  <option value="zh-hk">Traditional Chinese (繁体中文)</option>
                  <option value="vi-vn">Vietnamese (Tiếng Việt Nam)</option>
                  <option value="th-th">Thai (ภาษาไทย)</option>
                  <option value="jp-jp">Japanese (日本語)</option>
                  <option value="mn-mn">Mongolian (Монгол хэл)</option>
                  <option value="id-id">Indonesian (Bahasa Indonesia)</option>
                  <option value="ms-my">Malay (Bahasa Melayu)</option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="ibLinkLanguage" />
                  </div>
                </div>
              </div>
            </div>

            <!--------------------------------------------- Step 3 -->
            <div class="d-flex align-items-center mt-15">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step3") }}</span>
              <span class="stepContent">{{
                $t("tip.selectAccountTypeUnderLink")
              }}</span>
            </div>
            <div class="mt-5">
              <Field
                v-model="requestData.ServiceType"
                class="form-check-input widget-9-check me-3"
                type="radio"
                name="ibLinkServiceType"
                value="300"
              />
              <label class="me-9" for="accountTypeIB">{{
                $t("title.ib")
              }}</label>
              <Field
                v-model="requestData.ServiceType"
                class="form-check-input widget-9-check me-3"
                type="radio"
                name="ibLinkServiceType"
                value="400"
              />
              <label class="me-9" for="accountTypeClient">{{
                $t("fields.client")
              }}</label>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="ibLinkServiceType" />
                </div>
              </div>
            </div>

            <!--------------------------------------------- Step 4 -->
            <div class="d-flex align-items-center mt-15">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step4") }}</span>
              <span class="stepContent d-flex"
                >{{ $t("tip.selectAccountTypeAndSetRebate") }}
                <div
                  v-if="selectAccountError"
                  class="fv-plugins-message-container ms-3"
                >
                  <div class="fv-help-block">
                    {{ $t("error.__MUST_SELECT_ACCOUNT__") }}
                  </div>
                </div></span
              >
            </div>

            <div class="mt-5">
              <div class="d-flex">
                <div
                  v-for="(account, index) in currentAccountLevelSetting"
                  :key="index"
                >
                  <input
                    class="form-check-input widget-9-check me-3"
                    type="checkbox"
                    :name="'rebateAccount' + account.accountType"
                    v-model="account.selected"
                  />
                  <label class="me-9" for="rebateStdAccount">{{
                    $t("type.account." + account.accountType)
                  }}</label>
                </div>
              </div>
            </div>

            <!--------------------------------------------- Step 5 -->
            <div class="d-flex align-items-center mt-15">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step5") }}</span>
              <span class="stepContent">{{
                $t("title.enableAutoCreateAccount")
              }}</span>
            </div>
            <div class="mt-5 ms-2 d-flex align-items-center mb-5">
              <el-switch
                v-model="requestData.isAutoCreatePaymentMethod"
                size="large"
                class="order-switch"
                inline-prompt
                style="
                  --el-switch-on-color: #000f32;
                  --el-switch-off-color: #fafbfd;
                "
                :active-value="1"
                :inactive-value="0"
              />
              <span class="ms-3 fs-6">{{
                requestData.isAutoCreatePaymentMethod === 1
                  ? $t("action.yes")
                  : $t("action.no")
              }}</span>
            </div>

            <!--------------------------------------------- Rebate table -->
            <div v-if="requestData.ServiceType == ReferralServiceType.Agent">
              <div
                v-for="(account, index) in currentAccountLevelSetting"
                :key="index"
              >
                <div v-if="account.selected">
                  <div
                    class="d-flex align-items-center mt-9 mb-3"
                    style="cursor: pointer"
                  >
                    <div
                      class="vertical-line"
                      style="
                        border-left: 3px solid #800020;
                        height: 16px;
                        margin-right: 15px;
                      "
                    ></div>
                    <div class="fw-500 fs-4">
                      {{ $t("type.account." + account.accountType) }}
                    </div>
                  </div>
                  <BaseRebateForm
                    ref="BaseRebateFormRef"
                    :defaultLevelSetting="defaultLevelSetting"
                    :productCategory="props.productCategory"
                    :isRoot="rebateRuleDetail.isRoot"
                    :currentAccountLevelSetting="account"
                  />
                </div>
              </div>
            </div>

            <!-- Top IB create link for client need to decide pip and commission. -->
            <div
              v-if="
                requestData.ServiceType == ReferralServiceType.Client &&
                rebateRuleDetail.isRoot
              "
              class="mt-15 ms-5"
            >
              <div
                v-for="(account, index) in currentAccountLevelSetting"
                :key="index"
              >
                <div v-if="account.selected">
                  <div
                    class="d-flex align-items-center mt-9 mb-3"
                    style="cursor: pointer"
                  >
                    <div
                      class="vertical-line"
                      style="
                        border-left: 3px solid #800020;
                        height: 16px;
                        margin-right: 15px;
                      "
                    ></div>
                    <div class="fw-500 fs-4">
                      {{ $t("type.account." + account.accountType) }}
                    </div>
                  </div>

                  <BaseRebatePCForm
                    ref="BaseRebatePCFormRef"
                    :currentAccountRebateRule="account"
                  />
                </div>
              </div>
            </div>

            <button
              class="btn btn-primary btn-md mb-15 mt-15"
              @click="generateLink"
            >
              {{ $t("action.generateLink") }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ref, computed } from "vue";

import { showModal, hideModal } from "@/core/helpers/dom";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { processKeysToCamelCase } from "@/core/services/api.client";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";

import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BaseRebateForm from "../../../ib/components/form/BaseRebateForm.vue";
import BaseRebatePCForm from "../../../ib/components/form/BaseRebatePCForm.vue";
import SalesService from "@/projects/client/modules/sales/services/SalesService";

const { t } = useI18n();
const store = useStore();
const isLoading = ref(true);
const rebateRuleDetail = ref();
const selectAccountError = ref(false);
const configLevelSetting = ref({} as any);
const defaultLevelSetting = ref({} as any);
const currentAccountLevelSetting = ref({} as any);
const IBLinkDetailModalRef = ref<null | HTMLElement>(null);
const BaseRebateFormRef = ref<InstanceType<typeof BaseRebateForm>>();
const BaseRebatePCFormRef = ref<InstanceType<typeof BaseRebatePCForm>>();

// Get Sales siteID for below IBs (should be same siteID)
const siteId = computed(() => store.state.SalesModule.salesAccount?.siteId);

const props = defineProps<{
  productCategory: any;
}>();

const requestData = ref({
  name: "",
  language: "en-us",
  ServiceType: undefined,
  isAutoCreatePaymentMethod: 0,
});

const validationSchema = Yup.object().shape({
  ibLinkName: Yup.string().required(t("error.NAME_IS_REQUIRED")),
  ibLinkLanguage: Yup.string().required(t("error.LANGUAGE_IS_REQUIRED")),
  ibLinkServiceType: Yup.string().required(t("error.SERVICE_TYPE_IS_REQUIRED")),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const resetAddLinkForm = () => {
  requestData.value = {
    name: "",
    language: "en-us",
    ServiceType: undefined,
    isAutoCreatePaymentMethod: 0,
  };

  Object.keys(currentAccountLevelSetting.value).forEach((key) => {
    currentAccountLevelSetting.value[key].selected = false;
  });

  resetForm();
};

const generateLink = handleSubmit(async () => {
  const allowAccountRequest = ref([] as any);

  if (requestData.value.ServiceType == ReferralServiceType.Agent) {
    BaseRebateFormRef.value?.forEach(function (formRef) {
      if (formRef.formCheck()) {
        allowAccountRequest.value.push(formRef.collectData());
      }
    });
  } else if (requestData.value.ServiceType == ReferralServiceType.Client) {
    if (rebateRuleDetail.value.isRoot) {
      BaseRebatePCFormRef.value?.forEach(function (formRef) {
        if (formRef.formCheck()) {
          allowAccountRequest.value.push(formRef.collectData());
        }
      });
    } else {
      Object.keys(currentAccountLevelSetting.value).forEach((key) => {
        if (currentAccountLevelSetting.value[key].selected) {
          allowAccountRequest.value.push({
            optionName: currentAccountLevelSetting.value[key].optionName,
            accountType: currentAccountLevelSetting.value[key].accountType,
            pips: currentAccountLevelSetting.value[key].pips,
            commission: currentAccountLevelSetting.value[key].commission,
          });
        }
      });
    }
  }

  try {
    if (requestData.value.ServiceType == AccountRoleTypes.IB) {
      await SalesService.postIbReferralAgentLinkBySales(
        rebateRuleDetail.value.agentAccountUid,
        {
          name: requestData.value.name,
          language: requestData.value.language,
          schema: allowAccountRequest.value,
          isAutoCreatePaymentMethod:
            requestData.value.isAutoCreatePaymentMethod,
        }
      );
    } else if (requestData.value.ServiceType == AccountRoleTypes.Client) {
      await SalesService.postIbReferralClientLinkBySales(
        rebateRuleDetail.value.agentAccountUid,
        {
          name: requestData.value.name,
          language: requestData.value.language,
          allowAccountTypes: allowAccountRequest.value,
          isAutoCreatePaymentMethod:
            requestData.value.isAutoCreatePaymentMethod,
        }
      );
    }
    MsgPrompt.success(t("tip.formSuccessSubmit")).then(() => {
      resetAddLinkForm();
      hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
});

const show = async (_item: any) => {
  isLoading.value = true;
  configLevelSetting.value = {};
  currentAccountLevelSetting.value = {};

  showModal(IBLinkDetailModalRef.value);

  try {
    rebateRuleDetail.value = await SalesService.getIbRebateRuleDetailFromSales(
      _item.uid
    );

    defaultLevelSetting.value =
      await SalesService.getAccountDefaultLevelSetting(
        rebateRuleDetail.value.agentAccountUid
      );
    defaultLevelSetting.value = processKeysToCamelCase(
      defaultLevelSetting.value
    );

    // Get account configuration LevelSetting - IB rate has OPTIONS!
    // If IB account has options, put in account configuration DefaultRebateLevelSetting
    const ibConfig = await SalesService.getIBAccountConfiguration(_item.uid);
    if (
      ibConfig.length != 0 &&
      ibConfig[0]["key"] != "AutoCreateTradeAccountEnabled"
    ) {
      configLevelSetting.value = JSON.parse(ibConfig[0].value);
    }
    const _levelSetting =
      rebateRuleDetail.value.calculatedLevelSetting.allowedAccounts;
    _levelSetting.forEach((account: any) => {
      let currentAccount = (currentAccountLevelSetting.value[
        account.accountType
      ] = {} as any);

      currentAccount.selected = false;
      currentAccount.optionName = account.optionName;
      currentAccount.accountType = account.accountType;
      currentAccount.percentage = account.percentage;
      currentAccount.allowPips = account.allowPips;
      currentAccount.allowCommissions = account.allowCommissions;
      currentAccount.pips = account.pips;
      currentAccount.commission = account.commission;

      currentAccount.items = {};
      account.items.forEach((item: any) => {
        if (Object.keys(configLevelSetting.value).length == 0) {
          currentAccount.items[item.cid] = item.r;
        } else {
          currentAccount.items[item.cid] =
            configLevelSetting.value[account.accountType][0].Category[item.cid];
        }
      });
    });
    resetAddLinkForm();
  } catch (error) {
    // console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const hide = () => {
  hideModal(IBLinkDetailModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style>
.dot {
  width: 10px;
  height: 10px;

  border-radius: 100px;
  background: #0a46aa;
}

.step {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 18px;
  color: #212121;
}

.stepContent {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 16px;
  color: #4d4d4d;
}
</style>
