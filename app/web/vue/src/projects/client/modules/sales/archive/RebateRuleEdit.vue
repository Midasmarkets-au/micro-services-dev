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
          <h2 class="fs-2">{{ $t("title.ibLinkDetail") }}</h2>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->

        <div class="form-outer">
          <table
            v-if="isLoading"
            class="table table-row-dashed table-row-gray-200 align-middle"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>

          <template v-else>
            <div
              class="mt-5"
              v-if="parentUid != targetRebateRuleDetail.agentAccountUid"
            >
              <div class="mt-5 d-flex">
                <div
                  v-for="(account, index) in props.currentRebateRuleDetail
                    .schema"
                  :key="index"
                >
                  <input
                    class="form-check-input widget-9-check me-3"
                    type="checkbox"
                    :name="'rebateAccount' + account.accountType"
                    v-model="account.selected"
                    :disabled="account.defaultSelected"
                  />
                  <label class="me-9" for="rebateStdAccount">{{
                    $t("type.account." + account.accountType)
                  }}</label>
                </div>
              </div>
            </div>

            <!--------------------------------------------- Rebate table -->
            <div v-if="parentUid != targetRebateRuleDetail.agentAccountUid">
              <div
                v-for="(account, index) in props.currentRebateRuleDetail.schema"
                :key="index"
              >
                <div v-if="account['selected']">
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

                  <!-- parentUid 是上級,  targetRebateRuleDetail.agentAccountUid 是自己-->

                  <BaseRebateEditForm
                    ref="BaseRebateFormRef"
                    :siteId="salesAccount?.siteId"
                    :productCategory="productCategory"
                    :isRoot="props.currentRebateRuleDetail.isRoot"
                    :rebateAccountRule="
                      targetRebateRuleDetail.schema[account.accountType]
                    "
                    :totalRemainSchema="props.totalRemainSchema"
                    :currentAccountRebateRuleDetail="account"
                  />
                </div>
              </div>
              <div class="d-flex justify-content-center">
                <button
                  class="btn btn-primary btn-md btn-radius mb-10 mt-5 w-250px"
                  @click="updateRebateRule"
                >
                  {{ $t("action.updateRebateRule") }}
                </button>
              </div>
            </div>

            <div v-else>
              <TopAgentRebateEditForm
                ref="TopAgentRebateEditFormRef"
                :productCategory="productCategory"
                :rebateRule="targetRebateRuleDetail"
                @hide="hide"
              />

              <div class="d-flex justify-content-center">
                <button
                  class="btn btn-primary btn-md btn-radius mb-10 mt-5 w-250px"
                  @click="updateRebateRuleForTopAgent"
                >
                  {{ $t("action.updateRebateRule") }}
                </button>
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useStore } from "@/store";

import SalesService from "../services/SalesService";
import { showModal, hideModal } from "@/core/helpers/dom";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BaseRebateEditForm from "./form/BaseRebateEditForm.vue";
import TopAgentRebateEditForm from "./form/TopAgentRebateEditForm.vue";
import { is } from "@vee-validate/rules";
import { processKeysToCamelCase } from "@/core/services/api.client";

const parentUid = ref(0);
const store = useStore();
const isSales = ref(false);
const salesDefaultLevelSetting = ref({});
const isLoading = ref(true);
const filledAccountSchema = ref({});
const IBLinkDetailModalRef = ref<null | HTMLElement>(null);
const availableAccounts = ref({} as any);
const targetRebateRuleDetail = ref();
const BaseRebateFormRef = ref<InstanceType<typeof BaseRebateEditForm>>();
const TopAgentRebateEditFormRef =
  ref<InstanceType<typeof TopAgentRebateEditForm>>();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);

const props = defineProps<{
  productCategory: any;
}>();

const updateRebateRuleForTopAgent = async () => {
  TopAgentRebateEditFormRef.value?.update();
};

const updateRebateRule = async () => {
  const allowAccountRequest = ref([] as any);
  BaseRebateFormRef.value?.forEach(function (formRef) {
    if (formRef.formCheck()) {
      allowAccountRequest.value.push(formRef.collectData());
    }
  });

  try {
    await SalesService.putIBRebateRule(
      parentUid.value,
      targetRebateRuleDetail.value.id,
      allowAccountRequest.value
    );
    MsgPrompt.success("Update Success").then(() => {
      hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const setAccountRule = async (_accountType: string, _index: number) => {
  var _initAccount = filledAccountSchema.value[_accountType];
  var _defaultAccount = salesDefaultLevelSetting.value[_accountType][_index];

  _initAccount["selectedDefaultRebateOptions"] = _index;
  _initAccount["allowPipOptions"] = _defaultAccount.allowPipOptions;
  _initAccount["allowCommissionOptions"] =
    _defaultAccount.allowCommissionOptions;

  _initAccount["items"] = [];
  props.productCategory.forEach((category) => {
    _initAccount["items"].push({
      cid: category.key,
      r: _defaultAccount.category[category.key] ?? 0,
    });
  });
};

const setupSalesForm = async (_parentUid: number, _editUid: number) => {
  availableAccounts.value = store.state.AuthModule.config.accountTypeAvailable;
  targetRebateRuleDetail.value = await SalesService.getRebateRuleDetail(
    _editUid
  );
  targetRebateRuleDetail.value = targetRebateRuleDetail.value.schema;
  console.log("targetRebateRuleDetail", targetRebateRuleDetail.value);

  try {
    salesDefaultLevelSetting.value =
      await SalesService.getDefaultLevelSetting();
    salesDefaultLevelSetting.value = processKeysToCamelCase(
      salesDefaultLevelSetting.value
    );
  } catch (error) {
    MsgPrompt.error(error);
  }

  availableAccounts.value.forEach((accountType) => {
    if (salesDefaultLevelSetting.value[accountType] === undefined) {
      return;
    }

    const targetAccountRebateRule = targetRebateRuleDetail.value.schema.find(
      (acc: any) =>
        acc.optionName == salesDefaultLevelSetting.value[accountType].optionName
    );

    if (targetAccountRebateRule) {
      filledAccountSchema.value[accountType] = targetAccountRebateRule;
      filledAccountSchema.value[accountType]["selected"] = true;
      filledAccountSchema.value[accountType]["defaultSelected"] = true;
    }
    filledAccountSchema.value[accountType] = {};
    filledAccountSchema.value[accountType]["accountType"] = accountType;
    filledAccountSchema.value[accountType]["selected"] = false;
    filledAccountSchema.value[accountType]["allowPips"] = [];
    filledAccountSchema.value[accountType]["allowCommissions"] = [];

    setAccountRule(accountType, 0);
  });
};

const show = async (_isSales: any, _parentUid: number, _editUid: number) => {
  isLoading.value = true;
  isSales.value = _isSales;
  parentUid.value = _parentUid;
  showModal(IBLinkDetailModalRef.value);

  if (_isSales) {
    setupSalesForm(_parentUid, _editUid);
  }

  try {
    // const temp = ref({} as any);
    // targetRebateRuleDetail.value = await SalesService.getRebateRuleDetail(
    //   _editUid
    // );
    // targetRebateRuleDetail.value.schema.forEach((account: any) => {
    //   temp.value[account.accountType] = account;
    // });
    // targetRebateRuleDetail.value.schema = temp.value;
    // console.log("targetRebateRuleDetail", targetRebateRuleDetail.value);
    // props.currentRebateRuleDetail.schema.forEach((currentAcc) => {
    //   if (targetRebateRuleDetail.value.schema[currentAcc.accountType]) {
    //     currentAcc["selected"] = true;
    //     currentAcc["defaultSelected"] = true;
    //   } else {
    //     currentAcc["selected"] = false;
    //     currentAcc["defaultSelected"] = false;
    //   }
    // });
  } catch (error) {
    // console.log(error);
  } finally {
    // isLoading.value = false;
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
