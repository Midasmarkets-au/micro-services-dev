<template>
  <div class="card">
    <div class="card-header border-0">
      <div class="w-100">
        <!-- 有 level rule(level setting) 的 IB 才能創建新鏈結, 不然沒有規則 -->
        <!-- <div
          class="my-3 px-0 d-flex gap-1"
          style="border-bottom: 3px #ffd400 solid"
        >
          <span
            class="basic-tab"
            :class="{ 'active-tab': activeTab === tab.manageLink }"
            @click="activeTab = tab.manageLink"
          >
            {{ t("title.manageLink") }}
          </span>
        </div> -->
        <!------------------------------------------------ Header - Manage Link -->
        <div v-if="activeTab == tab.manageLink">
          <div class="mt-7 card-title-icon font-medium fs-2">
            {{ $t("action.manageLink") }}
          </div>
          <!------------------------------------------------ table - Manage Link -->
          <div class="overflow-auto">
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
                  <!-- <th>{{ $t("title.rebateSettings") }}</th> -->
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
                    {{ item.name }}
                    <i
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
                      <div
                        class="d-flex justify-content-start gap-1 align-items-center"
                      >
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

        <!--------------------------------------------------------------------->
        <!------------------------------------------------ header - Add Link -->
        <!--------------------------------------------------------------------->
        <!---------这段代码逻辑上用不上---------------->
        <div v-if="activeTab == tab.addLink && newLinkAvailable">
          <div class="mt-7 card-title">{{ $t("action.addNewLink") }}</div>
          <hr />

          <div class="py-0 mt-13">
            <!--------------------------------------------- Step 1 -->
            <div class="d-flex">
              <div>
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step1") }}</span>
                  <span class="stepContent">{{ $t("tip.nameYourLink") }}</span>
                </div>

                <Field
                  class="form-control form-control-solid w-300px h-55px mt-5"
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
              <div class="ms-17">
                <!--------------------------------------------- Step 2 -->
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
              </div>
            </div>

            <div class="d-flex">
              <!--------------------------------------------- Step 3 -->
              <div>
                <div class="d-flex align-items-center mt-15">
                  <div class="dot me-3"></div>
                  <span class="step me-3" style="white-space: nowrap">{{
                    $t("fields.step3")
                  }}</span>
                  <span class="stepContent">{{
                    $t("tip.selectAccountTypeUnderLink")
                  }}</span>
                </div>
                <div class="mt-5 mb-15">
                  <Field
                    v-model="requestData.serviceType"
                    class="form-check-input widget-9-check me-3"
                    type="radio"
                    name="ibLinkServiceType"
                    value="300"
                  />
                  <label class="me-9" for="accountTypeIB">{{
                    $t("title.ib")
                  }}</label>
                  <Field
                    v-model="requestData.serviceType"
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
              </div>

              <!--------------------------------------------- Step 4 -->

              <div class="ms-17">
                <div>
                  <div class="d-flex align-items-center mt-15">
                    <div class="dot me-3"></div>
                    <span class="step me-3" style="white-space: nowrap">{{
                      $t("fields.step4")
                    }}</span>
                    <span class="stepContent">{{
                      $t("tip.selectAccountTypeAndSetRebate")
                    }}</span>
                  </div>
                  <div class="mt-5 mb-15">
                    <span
                      v-for="(item, index) in defaultDirectLevelSetting"
                      :key="index"
                    >
                      <Field
                        v-model="requestData.optionName"
                        class="form-check-input widget-9-check me-3"
                        type="radio"
                        name="ibLinkDirectLevelType"
                        :value="item.optionName"
                      />
                      <label class="me-9" for="ibLinkDirectLevelType">{{
                        item.optionName
                      }}</label>
                    </span>

                    <div class="fv-plugins-message-container">
                      <div class="fv-help-block">
                        <ErrorMessage name="ibLinkDirectLevelType" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div
              class="d-flex"
              v-if="requestData.serviceType == ReferralServiceType.Agent"
            >
              <div v-if="requestData.optionName">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step5") }}</span>
                  <span class="stepContent">{{
                    $t("fields.selectLevel")
                  }}</span>
                </div>

                <Field
                  name="ibLinkLevel"
                  class="form-select form-select-solid mt-5 IB-rebate-link-select"
                  as="select"
                  v-model="requestData.level"
                >
                  <option
                    v-for="(item, index) in 10"
                    :key="index"
                    :value="item"
                  >
                    {{ item }}
                  </option>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="ibLinkLevel" />
                  </div>
                </div>
              </div>
              <div class="ms-17" v-if="requestData.level">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step6") }}</span>
                  <span class="stepContent">{{
                    $t("fields.levelSetting")
                  }}</span>
                </div>

                <div>
                  <div
                    v-for="(item, index) in requestData.directLevelSetting"
                    :key="index"
                    class="d-flex align-items-center justify-content-between mt-5"
                  >
                    <label class="me-5 fs-5">Level {{ index + 1 }}</label>
                    <Field
                      class="form-control form-control-solid w-300px h-55px"
                      :name="'ibLinkSetting' + index"
                      v-model="requestData.directLevelSetting[index]"
                      @change="updateRemainTotal"
                      :disabled="index == 0"
                    />
                    <span class="ms-5">%</span>
                  </div>
                </div>
              </div>
            </div>

            <button
              class="btn btn-primary btn-md mb-15 mt-15"
              @click="generateLink"
              :disabled="submitting"
            >
              {{ $t("action.generateLink") }}
              <span
                v-if="submitting"
                class="spinner-border spinner-border-sm align-middle ms-2"
              ></span>
            </button>
          </div>
        </div>
      </div>
    </div>
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
          <template
            v-if="confirmData.serviceType != ReferralServiceType.Client"
          >
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
          <template
            v-if="confirmData.serviceType == ReferralServiceType.Client"
          >
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

    <!-- <IBLinkDetailModal ref="IBLInkDetailRef" /> -->
    <EditReferralLink ref="EditReferralLinkRef" @fetchData="fetchData(1)" />
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ref, onMounted, watch, computed } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { getLanguage } from "@/core/types/LanguageTypes";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";

import * as Yup from "yup";
import IbService from "../services/IbService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import IBLinkDetailModal from "../components/IBLinkDetail.vue";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import BaseRebateForm from "../components/form/BaseRebateForm.vue";
import { processKeysToCamelCase } from "@/core/services/api.client";
import BaseRebatePCForm from "../components/form/BaseRebatePCForm.vue";
import EditReferralLink from "@/projects/client/modules/ib/components/modal/EditReferralLink.vue";

const ibLinks = ref();
const { t } = useI18n();
const store = useStore();
const copiedLink = ref("");
const isLoading = ref(true);
const showConfirm = ref(false);
const confirmData = ref(<any>[]);
const selectAccountError = ref(false);
const defaultDirectLevelSetting = ref([] as any);
const submitting = ref(false);

const projectConfig: PublicSetting = store.state.AuthModule.config;
const IBLInkDetailRef = ref<InstanceType<typeof IBLinkDetailModal>>();

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
  serviceType: undefined,
  optionName: undefined,
  accountType: undefined,
  level: undefined,
  directLevelSetting: [] as any,
} as any);

const validationSchema = Yup.object().shape({
  ibLinkName: Yup.string().required(t("error.NAME_IS_REQUIRED")),
  ibLinkLanguage: Yup.string().required(t("error.LANGUAGE_IS_REQUIRED")),
  ibLinkServiceType: Yup.string().required(t("error.SERVICE_TYPE_IS_REQUIRED")),
  ibLinkDirectLevelType: Yup.string().required(
    t("error.SERVICE_TYPE_IS_REQUIRED")
  ),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const resetAddLinkForm = () => {
  submitting.value = false;

  requestData.value = {
    name: "",
    language: getLanguage.value,
    serviceType: undefined,
    optionName: undefined,
    level: undefined,
    directLevelSetting: [],
  };

  resetForm();
};

// const changeAccountLevelSetting = (accountType: any, index: number) => {
//   currentAccountLevelSetting.value[accountType].optionName =
//     configLevelSetting.value[accountType][index].OptionName;

//   Object.keys(currentAccountLevelSetting.value[accountType].items).forEach(
//     (key) => {
//       currentAccountLevelSetting.value[accountType].items[key] =
//         configLevelSetting.value[accountType][index].Category[key];
//     }
//   );

//   BaseRebateFormRef.value?.forEach(function (formRef) {
//     formRef.updateTotalRemain();
//   });
// };

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

const updateRemainTotal = () => {
  var total = 0;
  for (var i = 1; i < requestData.value.directLevelSetting.length; i++) {
    total += parseFloat(requestData.value.directLevelSetting[i]);
  }

  requestData.value.directLevelSetting[0] = 100 - total;
};

const generateLink = handleSubmit(async () => {
  submitting.value = true;
  if (
    requestData.value.level == undefined &&
    requestData.value.serviceType == ReferralServiceType.Agent
  ) {
    MsgPrompt.warning(t("error.levelIsRequired"));
    submitting.value = false;
    return;
  }

  if (requestData.value.directLevelSetting[0] < 0) {
    MsgPrompt.warning(t("error.totalPercentageExceed"));
    submitting.value = false;
    return;
  }

  try {
    if (requestData.value.serviceType == AccountRoleTypes.IB) {
      await IbService.postIBLinkForIB({
        name: requestData.value.name,
        language: requestData.value.language,
        distributionType: 3,
        schema: [],
        percentageSchema: {
          optionName: requestData.value.optionName,
          accountType: requestData.value.accountType,
          percentageSetting: requestData.value.directLevelSetting,
        },
      });
    } else if (requestData.value.serviceType == AccountRoleTypes.Client) {
      await IbService.postIBLinkForClient({
        name: requestData.value.name,
        language: requestData.value.language,
        distributionType: 3,
        allowAccountTypes: [],
        percentageSchema: {
          optionName: requestData.value.optionName,
          accountType: requestData.value.accountType,
        },
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

  submitting.value = false;
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

const editCode = (item: any) => {
  EditReferralLinkRef.value?.show(item);
};

const showDetail = (_code: string) => {
  IBLInkDetailRef.value?.show(_code, true);
};

watch(
  () => requestData.value.level,
  (newVal) => {
    requestData.value.directLevelSetting = new Array(newVal).fill(0);
    requestData.value.directLevelSetting[0] = 100;
  }
);

watch(
  () => activeTab.value,
  () => {
    resetForm();
  }
);

watch(
  () => requestData.value.optionName,
  (newVal) => {
    if (newVal == undefined) return;
    requestData.value.accountType = defaultDirectLevelSetting.value.find(
      (item) => item.optionName == newVal
    ).accountType;
  }
);

onMounted(async () => {
  isLoading.value = true;

  try {
    if (newLinkAvailable.value) {
      defaultDirectLevelSetting.value = [
        {
          optionName: "SEA Standard 5",
          accountType: 13,
        },
        {
          optionName: "SEA Standard 10",
          accountType: 13,
        },
        {
          optionName: "SEA Standard 15",
          accountType: 13,
        },
      ];
    }
    await fetchData(1);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    resetAddLinkForm();
  }
});
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
</style>
