<template>
  <div :class="isMobile ? '' : 'h-full d-flex flex-1 flex-column'">
    <!-- 有 level rule(level setting) 的 IB 才能創建新鏈結, 不然沒有規則 -->
    <div class="card mb-2 round-bl-br">
      <div class="card-header px-4">
        <div class="card-title-noicon d-flex gap-2">
          <span
            class="basic-tab btn btn-light btn-bordered"
            :class="{ 'active-tab btn-primary': activeTab === tab.manageLink }"
            @click="activeTab = tab.manageLink"
          >
            {{ t("title.manageLink") }}
          </span>
          <span
            v-if="newLinkAvailable"
            class="basic-tab btn btn-light btn-bordered"
            :class="{ 'active-tab btn-primary': activeTab === tab.addLink }"
            @click="activeTab = tab.addLink"
          >
            {{ t("title.addLink") }}
          </span>
        </div>
      </div>
    </div>
    <div class="card card-body round-tl-tr flex-1">
      <!------------------------------------------------ Header - Manage Link -->
      <div v-if="activeTab == tab.manageLink">
        <div v-if="isMobile">
          <ManageLinkMobile />
          <TableFooter @page-change="fetchData" :criteria="criteria" />
        </div>
        <div v-else>
          <div class="card-title card-title-noicon font-medium fs-2">
            {{ $t("action.manageLink") }}
          </div>
          <!------------------------------------------------ table - Manage Link -->
          <div class="overflow-auto px-3" style="white-space: nowrap">
            <table
              class="table align-middle table-row-bordered fs-5 gy-5 font-medium"
              style="color: #000"
            >
              <thead>
                <tr class="fw-normal gs-0 fs-4">
                  <th>{{ $t("fields.linkName") }}</th>
                  <th>{{ $t("fields.referCode") }}</th>
                  <th>{{ $t("fields.accountType") }}</th>
                  <th>{{ $t("fields.linkType") }}</th>
                  <th>{{ $t("fields.lang") }}</th>
                  <!-- <th>{{ $t("tip.numberOfClients") }}</th> -->
                  <th>
                    <div class="text-center">
                      {{ $t("title.rebateSettings") }}
                    </div>
                  </th>
                  <th>
                    <div class="text-center">
                      {{ $t("title.autoCreateAccount") }}
                    </div>
                  </th>
                  <th>{{ $t("fields.clickCopy") }}</th>
                </tr>
              </thead>

              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!isLoading && ibLinks.length === 0">
                <NoDataBox />
              </tbody>

              <tbody v-else>
                <tr v-for="(item, index) in ibLinks" :key="index">
                  <td>
                    {{ item.name
                    }}<i
                      class="fa-solid fa-pen-to-square ms-2"
                      style="cursor: pointer"
                      @click="editCode(item)"
                    ></i>
                  </td>
                  <td>{{ item.code }}</td>
                  <td>
                    <template
                      v-if="item.serviceType != ReferralServiceType.Client"
                    >
                      <div class="d-flex justify-content-start gap-1">
                        <span
                          v-for="(acc, index) in item.summary.schema"
                          class="accountBadge"
                          :class="['type1', 'type2', 'type3'][index % 3]"
                          :key="'type_' + index"
                          >{{
                            $t("type.shortAccount." + acc.accountType)
                          }}</span
                        >
                      </div>
                    </template>

                    <template
                      v-if="item.serviceType == ReferralServiceType.Client"
                    >
                      <div class="d-flex justify-content-start gap-1">
                        <span
                          v-for="(acc, index) in item.summary.allowAccountTypes"
                          class="accountBadge"
                          :class="['type1', 'type2', 'type3'][index % 3]"
                          :key="'type_' + index"
                          >{{
                            $t("type.shortAccount." + acc.accountType)
                          }}</span
                        >
                      </div>
                    </template>
                  </td>
                  <td>
                    <div
                      class="d-flex align-items-center justify-content-start"
                    >
                      {{ $t("type.accountRole." + item.serviceType) }}
                      <div
                        v-if="
                          item.serviceType == AccountRoleTypes.Client &&
                          item.isDefault == 1
                        "
                        class="ms-1"
                      >
                        <el-tag type="success" size="small">
                          {{ $t("status.default") }}</el-tag
                        >
                      </div>
                    </div>
                  </td>
                  <td>
                    {{
                      item.summary.language == undefined
                        ? $t("type.language." + getLanguage)
                        : $t("type.language." + item.summary.language)
                    }}
                  </td>
                  <!-- <td class="text-center">{{ item.referredCount }}</td> -->
                  <td class="text-center">
                    <i
                      class="fa-regular fa-eye"
                      style="cursor: pointer"
                      @click="showDetail(item.code)"
                    ></i>
                  </td>
                  <td class="text-center">
                    {{
                      item.summary?.isAutoCreatePaymentMethod === 1
                        ? $t("action.yes")
                        : $t("action.no")
                    }}
                  </td>
                  <td>
                    <CopyReferralLink
                      :code="item.code"
                      :siteId="store.state.AuthModule.config.siteId"
                      :language="
                        item.summary.language == undefined
                          ? getLanguage
                          : item.summary.language
                      "
                      @click="confirmCopy(item)"
                    />
                  </td>
                </tr>
              </tbody>
            </table>
            <TableFooter @page-change="fetchData" :criteria="criteria" />
          </div>
        </div>
      </div>
      <!--------------------------------------------------------------------->
      <!------------------------------------------------ header - Add Link -->
      <!--------------------------------------------------------------------->
      <div v-if="activeTab == tab.addLink && newLinkAvailable">
        <div class="card-title card-title-noicon font-medium fs-2">
          {{ $t("action.addNewLink") }}
        </div>
        <div :class="isMobile ? 'py-0 mt-2' : 'py-0 mt-10'">
          <!--------------------------------------------- Step 1 -->
          <div class="d-flex align-items-center">
            <div class="dot me-3"></div>
            <span class="step me-3">{{ $t("fields.step1") }}</span>
            <span class="stepContent">{{ $t("tip.nameYourLink") }}</span>
          </div>

          <Field
            class="form-control form-control-solid w-300px mt-5"
            :class="isMobile ? 'h-35px' : 'w-300px h-55px'"
            :placeholder="$t('tip.pleaseInput')"
            name="ibLinkName"
            v-model="requestData.name"
          />
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="ibLinkName" />
            </div>
          </div>

          <!--------------------------------------------- Step 2 -->
          <div
            class="d-flex align-items-center"
            :class="isMobile ? 'mt-5' : 'mt-15'"
          >
            <div class="dot me-3"></div>
            <span class="step me-3">{{ $t("fields.step2") }}</span>
            <span class="stepContent">{{ $t("action.chooseLanguage") }}</span>
          </div>

          <Field
            name="ibLinkLanguage"
            class="form-select form-select-solid mt-5 IB-rebate-link-select"
            as="select"
            v-model="requestData.language"
          >
            <option value="en-us">English</option>
            <option value="zh-cn">Simplify Chinese (简体中文)</option>
            <option value="zh-tw">Traditional Chinese (繁体中文)</option>
            <option value="vi-vn">Vietnamese (Tiếng Việt Nam)</option>
            <option value="th-th">Thai (ภาษาไทย)</option>
            <option value="jp-jp">Japanese (日本語)</option>
            <!-- <option value="mn-mn">Mongolian (Монгол хэл)</option> -->
            <option value="id-id">Indonesian (Bahasa Indonesia)</option>
            <option value="ms-my">Malay (Bahasa Melayu)</option>
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="ibLinkLanguage" />
            </div>
          </div>

          <!--------------------------------------------- Step 3 -->
          <div
            class="d-flex align-items-center"
            :class="isMobile ? 'mt-5' : 'mt-15'"
          >
            <div class="dot me-3"></div>
            <span class="step me-3" style="white-space: nowrap">{{
              $t("fields.step3")
            }}</span>
            <span class="stepContent">{{
              $t("tip.selectAccountTypeUnderLink")
            }}</span>
          </div>
          <div
            class="mt-5 ms-2 d-flex align-items-center"
            :class="isMobile ? 'mb-5 fs-7' : 'mb-15 fs-5'"
          >
            <Field
              v-model="requestData.ServiceType"
              class="form-check-input widget-9-check me-3"
              type="radio"
              name="ibLinkServiceType"
              value="300"
              style="border: 1px solid #ccd3e0; width: 15px; height: 15px"
            />
            <label class="me-9" for="accountTypeIB">{{ $t("title.ib") }}</label>
            <Field
              v-model="requestData.ServiceType"
              class="form-check-input widget-9-check me-3"
              type="radio"
              name="ibLinkServiceType"
              value="400"
              style="border: 1px solid #ccd3e0; width: 15px; height: 15px"
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
          <div class="d-flex align-items-center">
            <div class="dot me-3"></div>
            <span class="step me-3" style="white-space: nowrap">{{
              $t("fields.step4")
            }}</span>
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

          <!-- Account Select Options -->
          <div class="mt-5">
            <div class="mt-5 d-flex">
              <div
                v-for="(account, index) in currentAccountLevelSetting"
                :key="index"
              >
                <div
                  v-if="account.accountType != 11"
                  class="d-flex align-items-center fs-6"
                >
                  <input
                    class="form-check-input widget-9-check me-3"
                    type="checkbox"
                    :name="'rebateAccount' + account.accountType"
                    v-model="account.selected"
                    style="
                      border: 1px solid #ccd3e0;
                      width: 15px;
                      height: 15px;
                      border-radius: 4px;
                    "
                  />
                  <label class="me-5 text-nowrap" for="rebateStdAccount">{{
                    $t("type.account." + account.accountType)
                  }}</label>
                </div>
              </div>
            </div>
          </div>

          <!--------------------------------------------- Rebate table -->
          <div
            v-if="requestData.ServiceType == ReferralServiceType.Agent"
            :class="isMobile ? 'mt-10' : 'mt-15 ms-5'"
          >
            <div
              v-for="(account, index) in currentAccountLevelSetting"
              :key="index"
            >
              <div v-if="account.selected">
                <div
                  class="d-flex align-items-center justify-content-between mt-8 mb-3"
                  style="cursor: pointer"
                >
                  <div class="d-flex align-items-center">
                    <div
                      class="vertical-line"
                      style="
                        border-left: 3px solid #800020;
                        height: 16px;
                        margin-right: 11px;
                      "
                    ></div>
                    <div class="fw-400 fs-4">
                      {{ $t("type.account." + account.accountType) }}
                    </div>
                  </div>
                  <div v-if="Object.keys(configLevelSetting).length != 0">
                    <div
                      v-if="configLevelSetting[account.accountType].length > 1"
                      class="d-flex justify-content-end"
                    >
                      <button
                        v-for="(option, index) in configLevelSetting[
                          account.accountType
                        ]"
                        :key="index"
                        class="btn btn-sm btn-light-primary border-0 me-3"
                        @click="
                          changeAccountLevelSetting(account.accountType, index)
                        "
                      >
                        {{ option.OptionName }}
                      </button>
                    </div>
                  </div>
                </div>

                <BaseRebateForm
                  ref="BaseRebateFormRef"
                  :productCategory="productCategory"
                  :isRoot="rebateRuleDetail.isRoot"
                  :currentAccountLevelSetting="account"
                  :defaultLevelSetting="defaultLevelSetting"
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
                  class="d-flex align-items-center mt-8 mb-3"
                  style="cursor: pointer"
                >
                  <div
                    class="vertical-line"
                    style="
                      border-left: 3px solid #800020;
                      height: 16px;
                      margin-right: 11px;
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
          <div
            class="d-flex align-items-center"
            :class="isMobile ? 'mt-5' : 'mt-15'"
          >
            <div class="dot me-3"></div>
            <span class="step me-3" style="white-space: nowrap">{{
              $t("fields.step5")
            }}</span>
            <span class="stepContent">{{
              $t("title.enableAutoCreateAccount")
            }}</span>
          </div>
          <div
            class="mt-5 ms-2 d-flex align-items-center"
            :class="isMobile ? 'mb-5 fs-7' : 'mb-15 fs-5'"
          >
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
  <IBLinkDetailModal
    ref="IBLInkDetailRef"
    :productCategory="productCategory"
    :defaultLevelSetting="defaultLevelSetting"
    :currentAccountRebateRule="currentAccountLevelSetting"
  />

  <el-dialog
    v-model="showConfirm"
    title="Link Copied"
    align-center
    center
    class="rounded-3"
    style="width: max-content; max-width: 100%"
  >
    <div class="copy_content">
      <div class="mb-2 d-flex">
        <span>{{ $t("fields.linkName") }}: </span
        ><span class="fw-light">{{ confirmData.name }}</span>
      </div>
      <div class="mb-2">
        <span>{{ $t("fields.referCode") }}: </span
        ><span class="fw-light">{{ confirmData.code }}</span>
      </div>
      <div class="mb-2 d-flex">
        <span>{{ $t("fields.accountType") }}: </span>
        <template v-if="confirmData.serviceType != ReferralServiceType.Client">
          <div class="d-flex gap-1 ml-1">
            <span
              v-for="(acc, index) in confirmData.summary.schema"
              class="accountBadge fs-7"
              :class="['type1', 'type2', 'type3'][index % 3]"
              :key="'type_' + index"
              >{{ $t("type.shortAccount." + acc.accountType) }}</span
            >
          </div>
        </template>
        <template v-if="confirmData.serviceType == ReferralServiceType.Client">
          <div class="d-flex gap-1">
            <span
              v-for="(acc, index) in confirmData.summary.allowAccountTypes"
              class="accountBadge fs-7"
              :class="['type1', 'type2', 'type3'][index % 3]"
              :key="'type_' + index"
              >{{ $t("type.shortAccount." + acc.accountType) }}</span
            >
          </div>
        </template>
      </div>
      <div class="mb-2">
        <span>{{ $t("fields.linkType") }}: </span
        ><span class="fw-light">{{
          $t("type.accountRole." + confirmData.serviceType)
        }}</span>
      </div>
      <div class="mb-2">
        <span>{{ $t("fields.lang") }} :</span
        ><span class="fw-light">
          {{
            confirmData.summary.language == undefined
              ? $t("type.language." + getLanguage)
              : $t("type.language." + confirmData.summary.language)
          }}</span
        >
      </div>
      <div class="mb-2">
        <span>{{ $t("title.ReferralLink") }}: </span
        ><span class="fw-light">{{ copiedLink }}</span>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer d-flex justify-content-end">
        <button
          class="btn btn-primary btn-sm btn-radius"
          @click="showConfirm = false"
        >
          OK
        </button>
      </div>
    </template>
  </el-dialog>
  <EditReferralLink ref="EditReferralLinkRef" @fetchData="fetchData(1)" />
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ref, onMounted, computed, provide } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { getLanguage } from "@/core/types/LanguageTypes";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import { isMobile } from "@/core/config/WindowConfig";
import * as Yup from "yup";
import IbService from "../services/IbService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import IBLinkDetailModal from "../components/IBLinkDetail.vue";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import BaseRebateForm from "../components/form/BaseRebateForm.vue";
import { processKeysToCamelCase } from "@/core/services/api.client";
import BaseRebatePCForm from "../components/form/BaseRebatePCForm.vue";
import EditReferralLink from "@/projects/client/modules/ib/components/modal/EditReferralLink.vue";
import ManageLinkMobile from "../components/link/ManageLinkMobile.vue";

const ibLinks = ref();
const { t } = useI18n();
const store = useStore();
const copiedLink = ref("");
const isLoading = ref(true);
const showConfirm = ref(false);
const confirmData = ref(<any>[]);
const selectAccountError = ref(false);
const rebateRuleDetail = ref({} as any);
const productCategory = ref(Array<any>());
const configLevelSetting = ref({} as any);
const defaultLevelSetting = ref({} as any);
const currentAccountLevelSetting = ref({} as any);

const projectConfig: PublicSetting = store.state.AuthModule.config;
const IBLInkDetailRef = ref<InstanceType<typeof IBLinkDetailModal>>();
const BaseRebateFormRef = ref<InstanceType<typeof BaseRebateForm>>();
const BaseRebatePCFormRef = ref<InstanceType<typeof BaseRebatePCForm>>();
const newLinkAvailable = ref(projectConfig?.rebateEnabled);
const EditReferralLinkRef = ref<InstanceType<typeof EditReferralLink>>();

const siteId = computed(() => store.state.AuthModule.config.siteId);

var baseUrl = process.env.VUE_APP_BASE_URL;

if (siteId.value == 3 || siteId.value == 1) {
  baseUrl = process.env.VUE_APP_BASE_CDN_URL;
}
const criteria = ref({
  page: 1,
  size: 20,
  status: 0,
});
const activeTab = ref("manageLink");
const tab = ref({
  manageLink: "manageLink",
  addLink: "addLink",
});

const requestData = ref({
  name: "",
  language: getLanguage.value,
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

const changeAccountLevelSetting = (accountType: any, index: number) => {
  currentAccountLevelSetting.value[accountType].optionName =
    configLevelSetting.value[accountType][index].OptionName;

  Object.keys(currentAccountLevelSetting.value[accountType].items).forEach(
    (key) => {
      currentAccountLevelSetting.value[accountType].items[key] =
        configLevelSetting.value[accountType][index].Category[key];
    }
  );

  BaseRebateFormRef.value?.forEach(function (formRef) {
    formRef.updateTotalRemain();
  });
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const result = await IbService.getIbLinks(criteria.value);
    processKeysToCamelCase(result);
    ibLinks.value = result.data;
    criteria.value = result.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
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

  if (allowAccountRequest.value.length == 0) {
    selectAccountError.value = true;
    return;
  }

  try {
    if (requestData.value.ServiceType == AccountRoleTypes.IB) {
      await IbService.postIBLinkForIB({
        name: requestData.value.name,
        language: requestData.value.language,
        schema: allowAccountRequest.value,
        isAutoCreatePaymentMethod: requestData.value.isAutoCreatePaymentMethod,
      });
    } else if (requestData.value.ServiceType == AccountRoleTypes.Client) {
      await IbService.postIBLinkForClient({
        name: requestData.value.name,
        language: requestData.value.language,
        allowAccountTypes: allowAccountRequest.value,
        isAutoCreatePaymentMethod: requestData.value.isAutoCreatePaymentMethod,
      });
    }
    MsgPrompt.success(t("tip.formSuccessSubmit")).then(() => {
      fetchData(1);
      activeTab.value = "manageLink";
      resetAddLinkForm();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
});

const confirmCopy = (item: any) => {
  showConfirm.value = true;
  confirmData.value = item;
  const siteId = store.state.AuthModule.config.siteId;
  const lang =
    item.summary.language == undefined ? getLanguage : item.summary.language;
  copiedLink.value =
    baseUrl +
    "/sign-up?code=" +
    item.code +
    "&siteId=" +
    siteId +
    "&lang=" +
    lang;
};

onMounted(async () => {
  try {
    if (newLinkAvailable.value) {
      productCategory.value = await IbService.getCategory();
      rebateRuleDetail.value = await IbService.getRebateRuleDetail();

      defaultLevelSetting.value = await IbService.getDefaultLevelSetting();
      defaultLevelSetting.value = processKeysToCamelCase(
        defaultLevelSetting.value
      );
      console.log("rebateRuleDetail", rebateRuleDetail);
      // Get account configuration LevelSetting - IB rate has OPTIONS!
      // If IB account has options, put in account configuration DefaultRebateLevelSetting
      const ibAccounts = await IbService.getIBAccountDetail();
      let hasConfigLevelSetting = ibAccounts.data
        .find((acc) => acc.uid == store.state.AgentModule.agentAccount?.uid)
        .configurations.find(
          (item) => item.key === "DefaultRebateLevelSetting"
        );
      if (hasConfigLevelSetting)
        configLevelSetting.value = JSON.parse(hasConfigLevelSetting.value);

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
              configLevelSetting.value[account.accountType][0].category?.[
                item.cid
              ] ??
              configLevelSetting.value[account.accountType][0].Category?.[
                item.cid
              ];
          }
        });
      });

      resetForm();
    }

    fetchData(1);
  } catch (error) {
    MsgPrompt.error(error);
  }
});

const editCode = (item: any) => {
  EditReferralLinkRef.value?.show(item);
};
const showDetail = (_code: string) => {
  IBLInkDetailRef.value?.show(_code, rebateRuleDetail.value.isRoot);
};
provide("isLoading", isLoading);
provide("ibLinks", ibLinks);
provide("editCode", editCode);
provide("showDetail", showDetail);
provide("confirmCopy", confirmCopy);
provide("siteId", siteId);
</script>

<style scoped lang="scss">
.svg-container {
  transition: transform 0.3s ease-in-out;
}

.arrow.rotate-up .svg-container {
  transform: rotate(-180deg);
}

.showForm {
  max-height: 1000px;
  transition: max-height 0.3s ease-in-out;
}

.hideForm {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease-in-out;
}
.copy_content {
  font-size: 14px;
  > div {
    > span:first-child {
      color: #9ba2ab;
    }
    > span:nth-child(2) {
      padding-left: 10px;
      color: #000;
    }
  }
}
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

.IB-rebate-link-select {
  padding: 16px 20px;
  width: 300px;
  height: 56px;
}

.accountBadge {
  border-radius: 8px;
  padding: 2px 8px;
  font-size: 12px;
  height: 28px;
  line-height: 28px;
}

.type1 {
  background: rgba(88, 168, 255, 0.1);
  color: #4196f0;
}

.type2 {
  background: rgba(255, 164, 0, 0.15);
  color: #ffa400;
}

.type3 {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
}
.basic-tab {
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 15px;
  // border-radius: 5px 5px 0 0;
  // width: 150px;
  // height: 40px;
  // border: 2px solid #ffd400;
  cursor: pointer;
  //border-bottom: 0;
  transition: background-color 0.3s;

  @media (max-width: 768px) {
    flex: 1;
  }
}

.active-tab {
  background-color: #000f32;
  color: #fff;
}

@media (max-width: 768px) {
  .step {
    font-size: 14px !important;
  }
  .stepContent {
    font-size: 12px !important;
  }
  .form-select {
    font-size: 0.85rem !important;
    padding: 0px 16px;
    height: 35px;
  }

  .form-select option {
    font-size: 1.1rem !important;
  }

  .form-control {
    font-size: 0.85rem !important;
  }
}
</style>
