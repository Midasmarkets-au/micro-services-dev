<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_lists"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkListsModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ title }}</h2>

          <div
            class="btn btn-sm btn-icon btn-active-color-primary"
            data-bs-dismiss="modal"
          >
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div style="max-height: 80vh; overflow: auto">
          <table
            v-if="isLoading"
            class="table table-row-brodered table-row-gray-200 align-middle"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>

          <template v-else>
            <!-- Available Account Types ==================================================== -->
            <!-- ============================================================================ -->
            <div class="d-flex mt-5 mb-5 ml-5">
              <div class="d-flex">
                <div
                  v-for="(accountType, index) in allAccountType"
                  :key="index"
                >
                  <div
                    v-if="availableAccountType.includes(accountType)"
                    class="accountTypeBtn"
                    :class="{
                      'active-accountTypeBtn':
                        selectedAccountType == accountType,
                    }"
                    @click="selectedAccountType = accountType"
                  >
                    {{ $t("type.account." + accountType) }}
                  </div>
                </div>
              </div>
            </div>

            <!-- ============================================================================ -->
            <!-- ============================================================================ -->
            <table
              class="table table-row-dashed table-row-gray-200 align-middle"
            >
              <tbody class="fw-semibold text-gray-600">
                <!-- Account Owner Info ============================================== -->
                <!-- ================================================================= -->
                <tr
                  class="text-center"
                  style="border-bottom: 1px solid #ecedf4"
                >
                  <td></td>
                  <td
                    v-for="({ uid }, index) in relationList"
                    :key="index"
                    style="position: relative"
                  >
                    <div class="mb-2">
                      <strong>{{
                        $t("type.roleType." + accountInfo[uid].role)
                      }}</strong
                      >:
                      {{ accountInfo[uid].nativeName }}
                    </div>

                    <span class="typeBadge">{{ uid }}</span>

                    <div
                      v-if="
                        rebateInfo[uid] &&
                        rebateInfo[uid].calculatedLevelSetting.allowedAccounts
                      "
                      class="mt-2"
                    >
                      <span
                        v-for="(acc, index) in rebateInfo[uid]
                          .calculatedLevelSetting.allowedAccounts"
                        class="accountBadge me-1"
                        :class="['type1', 'type2', 'type3'][index % 3]"
                        :key="'type_' + index"
                      >
                        {{ $t("type.shortAccount." + acc.accountType) }}
                      </span>
                    </div>
                  </td>
                </tr>

                <!-- Pips and Commission ============================================= -->
                <!-- ================================================================= -->
                <tr
                  class="text-center"
                  style="border-bottom: 1px solid #ecedf4"
                >
                  <td></td>
                  <td v-for="({ uid }, index) in relationList" :key="index">
                    <div
                      v-if="
                        rebateInfo[uid]?.calculatedLevelSetting.allowedAccounts[
                          selectedAccountType
                        ]
                      "
                    >
                      <div v-if="rebateInfo[uid].isRoot">
                        <div>
                          <div>{{ $t("fields.whichPips") }}</div>
                          <div class="d-flex justify-content-center mt-2">
                            <div
                              v-for="(item, index) in rebateInfo[uid]
                                .levelSetting.allowedAccounts[
                                selectedAccountType
                              ].allowPips"
                              :key="index"
                              class="me-2 pcItem"
                            >
                              {{ item }}
                            </div>
                          </div>
                        </div>
                        <div class="mt-3">
                          <div>
                            <div>{{ $t("fields.whichCommission") }}</div>
                            <div class="d-flex justify-content-center mt-2">
                              <div
                                v-for="(item, index) in rebateInfo[uid]
                                  .levelSetting.allowedAccounts[
                                  selectedAccountType
                                ].allowCommissions"
                                :key="index"
                                class="me-2 pcItem"
                              >
                                {{ item }}
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>

                      <div
                        class="d-flex flex-column justify-content-center align-items-center"
                        v-else-if="accountInfo[uid].role == UserRoleTypes.IB"
                      >
                        <div class="d-flex align-items-center">
                          <div class="me-3">
                            Pips:
                            {{
                              rebateInfo[uid].schema[selectedAccountType]
                                .pips ?? "--"
                            }}
                          </div>
                        </div>

                        <div
                          class="d-flex justify-content-center align-items-center mt-3"
                        >
                          <div class="me-3">
                            Com:
                            {{
                              rebateInfo[uid].schema[selectedAccountType]
                                .commission ?? "--"
                            }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>

                <!-- ================================================================= -->
                <!-- ================================================================= -->

                <tr
                  class="text-center"
                  style="border-bottom: 1px solid #ecedf4"
                >
                  <td></td>
                  <td
                    v-for="({ uid, belowAcc }, index) in relationList"
                    :key="index"
                  >
                    <div
                      v-if="
                        accountInfo[uid].role == UserRoleTypes.IB &&
                        rebateInfo[belowAcc] &&
                        rebateInfo[belowAcc].schema[selectedAccountType]
                      "
                      class="d-flex flex-column justify-content-center align-items-center"
                    >
                      <div>{{ $t("fields.howMuchTake") }}</div>
                      <div class="percentageField">
                        {{
                          rebateInfo[belowAcc].schema[selectedAccountType]
                            .percentage
                        }}%
                      </div>
                    </div>
                  </td>
                </tr>

                <!-- ================================================================= -->
                <!-- ================================================================= -->

                <tr
                  class="text-center"
                  v-for="(category, p_index) in productCategory"
                  :key="p_index"
                >
                  <td>
                    {{ category.value }}
                  </td>
                  <td
                    v-for="({ uid, belowAcc }, a_index) in relationList"
                    :key="a_index"
                  >
                    <el-input
                      v-if="
                        belowAcc &&
                        accountInfo[uid].role == UserRoleTypes.Sales &&
                        rebateInfo[belowAcc]?.levelSetting.allowedAccounts[
                          selectedAccountType
                        ]
                      "
                      class="w-100px"
                      v-model="
                        rebateInfo[belowAcc].levelSetting.allowedAccounts[
                          selectedAccountType
                        ].items.find((acc) => acc.cid == category.key).r
                      "
                      disabled
                    />

                    <el-input
                      v-else-if="
                        belowAcc &&
                        accountInfo[uid].role == UserRoleTypes.IB &&
                        accountInfo[belowAcc].role == UserRoleTypes.IB &&
                        rebateInfo[belowAcc]?.schema[selectedAccountType]
                      "
                      class="w-100px"
                      v-model="
                        rebateInfo[belowAcc].schema[
                          selectedAccountType
                        ].items.find((acc) => acc.cid == category.key).r
                      "
                      disabled
                    />

                    <span
                      v-else-if="
                        accountInfo[uid].role == UserRoleTypes.IB &&
                        (!belowAcc ||
                          (belowAcc &&
                            accountInfo[belowAcc].role == UserRoleTypes.Client))
                      "
                      >{{
                        rebateInfo[uid].calculatedLevelSetting.allowedAccounts[
                          selectedAccountType
                        ]?.items.find((acc) => acc.cid == category.key).r
                      }}</span
                    >
                  </td>
                </tr>
              </tbody>
            </table>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { UserRoleTypes } from "@/core/types/RoleTypes";

import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../../services/SalesService";
import { processKeysToCamelCase } from "@/core/services/api.client";
import { showModal, hideModal } from "@/core/helpers/dom";

const title = ref("");
const isLoading = ref(true);
const rebateInfo = ref({} as any);
const selectedAccountType = ref(0);
const accountInfo = ref({} as any);
const relationList = ref([] as any);
const allAccountType = ref([] as any);
const defaultRebateRate = ref({} as any);
const defaultRebateRateForNewAccountType = ref({} as any);
const productCategory = ref(Array<any>());
const availableAccountType = ref([] as any);
const topIbRebateInfo = ref();
const IBLinkListsModalRef = ref<null | HTMLElement>(null);

// =========================================================================================================
// =========================================================================================================

const buildRelationForm = async (_referPath) => {
  isLoading.value = true;
  relationList.value = [];
  selectedAccountType.value = 0;

  try {
    const [getCategory, getDefaultLevelSetting, queryAccounts, getAgentRules] =
      await Promise.all([
        SalesService.getCategory(),
        SalesService.getDefaultLevelSetting().then((result) =>
          processKeysToCamelCase(result)
        ),
        SalesService.getLevelAccounts(_referPath[_referPath.length - 1]),
        SalesService.getAgentRules({
          agentUids: _referPath,
        }),
      ]);

    accountInfo.value = queryAccounts.reduce((acc, cur) => {
      acc[cur["uid"]] = cur;
      return acc;
    }, {});
    productCategory.value = getCategory;
    rebateInfo.value = adjustRebateInfo(getAgentRules.data);
    defaultRebateRateForNewAccountType.value = JSON.parse(
      JSON.stringify(getDefaultLevelSetting)
    );
    defaultRebateRate.value = adjustDefaultRebateRate(getDefaultLevelSetting);
    allAccountType.value = Object.keys(defaultRebateRate.value);
  } catch (error) {
    // MsgPrompt.error(t("error.oopsErrorAccured"), "5263");
  }

  // console.log("accountInfo", accountInfo.value);

  _referPath.forEach((uid) => {
    relationList.value.push({
      uid: uid,
      role: accountInfo.value[uid].role,
      upperAcc: _referPath[_referPath.indexOf(uid) - 1] ?? null,
      belowAcc: _referPath[_referPath.indexOf(uid) + 1] ?? null,
    });
  });

  const _lastAccount = relationList.value[relationList.value.length - 1];

  availableAccountType.value = {
    [UserRoleTypes.Sales]: Object.keys(defaultRebateRate.value),
    [UserRoleTypes.IB]: Object.keys(
      rebateInfo.value[_lastAccount.uid]?.calculatedLevelSetting
        .allowedAccounts ?? {}
    ),
    [UserRoleTypes.Client]: [
      accountInfo.value[_lastAccount.uid].type.toString(),
    ],
  }[_lastAccount.role];

  if (selectedAccountType.value == 0) {
    selectedAccountType.value = availableAccountType.value[0];
  }

  // console.log("_referPath", _referPath);
  // console.log("allAccountType", allAccountType.value);
  // console.log(
  //   "defaultRebateRateForNewAccountType",
  //   defaultRebateRateForNewAccountType.value
  // );
  // console.log("defaultRebateRate", defaultRebateRate.value);
  // console.log("selectedAccountType.value", selectedAccountType.value);
  // console.log("rebateInfo", rebateInfo.value);

  isLoading.value = false;
};

// =========================================================================================================
// =========================================================================================================

const adjustDefaultRebateRate = (_data) => {
  topIbRebateInfo.value = Object.values(rebateInfo.value).find(
    (acc: any) => acc.isRoot
  );

  Object.keys(_data).forEach((acc) => {
    _data[acc] =
      _data[acc].find(
        (option) =>
          option.optionName ==
          topIbRebateInfo.value?.levelSetting.allowedAccounts[acc]?.optionName
      ) ?? _data[acc][0];

    _data[acc].allowPipOptions = _data[acc].allowPipOptions.map((option) => ({
      label: option,
      value: option,
    }));
    _data[acc].allowCommissionOptions = _data[acc].allowCommissionOptions.map(
      (option) => ({
        label: option,
        value: option,
      })
    );
  });

  return _data;
};

const adjustRebateInfo = (_data) => {
  const convertArrayToObject = (rules) => {
    return rules.reduce((acc, cur) => {
      acc[cur.accountType] = cur;
      return acc;
    }, {});
  };

  _data.forEach((acc) => {
    acc.schema = convertArrayToObject(acc.schema);
    acc.levelSetting.allowedAccounts = convertArrayToObject(
      acc.levelSetting.allowedAccounts
    );
    acc.calculatedLevelSetting.allowedAccounts = convertArrayToObject(
      acc.calculatedLevelSetting.allowedAccounts
    );
  });

  _data = _data.reduce((acc, cur) => {
    acc[cur["agentAccountUid"]] = cur;
    return acc;
  }, {});

  return _data;
};

const show = async (_item: any) => {
  isLoading.value = true;
  title.value = _item.user.displayName;

  showModal(IBLinkListsModalRef.value);

  try {
    const _referPath = await SalesService.getReferalPath(_item.uid);
    buildRelationForm(_referPath);
  } catch (error) {
    // MsgPrompt.error(t("error.oopsErrorAccured"), "5263");
  }
};

defineExpose({
  show,
});
</script>

<style scoped>
.seperateLine {
  width: 2px;
  height: 42px;
  margin-right: 15px;
  background-color: lightgray;
}

.unAvailableAccountTypeBtn {
  width: 100px;
  text-align: center;
  padding: 10px 15px;
  border: 1px solid gray;
  border-radius: 5px;
  margin-right: 15px;
  background-color: #f0f0f0;
  cursor: pointer;
  opacity: 0.5;
}

.accountTypeBtn {
  width: 100px;
  text-align: center;
  padding: 10px 15px;
  border: 1px solid #ecedf4;
  border-radius: 5px;
  margin-right: 15px;
  cursor: pointer;
}

.accountTypeBtn:hover {
  border: 1px solid #000f32;
  background-color: #000f32;
  color: white;
}

.active-accountTypeBtn {
  border: 1px solid #000f32;
  background-color: #000f32;
  color: white;
}

.active-unAvailableAccountTypeBtn {
  border: 1px solid #000f32;
  background-color: #000f32;
  color: white;
}

.accountBadge {
  width: 43px;
  height: 20px;
  border-radius: 8px;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 700;
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

.pcItem {
  display: flex;
  justify-content: center;
  align-items: center;

  width: 22px;
  height: 22px;

  border-radius: 5px;
  box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
  background-color: white;
}

.percentageField {
  background-color: white;
  width: 80px;
  border-radius: 10px;
  margin-top: 5px;
  box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
}
</style>
