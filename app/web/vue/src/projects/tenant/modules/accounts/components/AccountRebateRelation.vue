<template>
  <div class="form-outer">
    <table
      v-if="isLoading"
      class="table table-row-dashed table-row-gray-200 align-middle"
    >
      <tbody>
        <LoadingRing />
      </tbody>
    </table>

    <template
      v-else-if="
        !isLoading &&
        distributionInfo.levelSetting?.distributionType ==
          RebateDistributionTypes.LevelSet
      "
    >
      <table class="table table-row-dashed table-row-gray-200 align-middle">
        <tbody class="fw-semibold text-gray-600">
          <!-- Account Owner Info ============================================== -->
          <!-- ================================================================= -->
          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
              v-for="({ uid }, index) in relationList"
              :key="index"
              style="position: relative"
            >
              <div class="mb-2">
                <strong>{{
                  $t("type.roleType." + accountInfo[uid].role)
                }}</strong
                >:
                {{ accountInfo[uid].user.displayName }}
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
                  v-for="(acc, index) in rebateInfo[uid].calculatedLevelSetting
                    .allowedAccounts"
                  class="accountBadge me-1"
                  :class="['type1', 'type2', 'type3'][index % 3]"
                  :key="'type_' + index"
                >
                  {{ $t("type.shortAccount." + acc.accountType) }}
                </span>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <hr />
      <div
        v-if="
          hasAgentRule &&
          distributionInfo.levelSetting.percentageSetting.length != 0
        "
      >
        <div class="d-flex justify-content-around pb-7">
          <div>
            <div class="text-center fs-2">FOREX</div>
            <div
              v-for="(item, index) in distributionInfo.levelSetting
                .percentageSetting.FOREX"
              :key="index"
              class="d-flex align-items-center mt-5"
            >
              <label class="fs-5 w-80px me-3">Level {{ index + 1 }}</label>
              <input
                class="form-control form-control-solid w-200px h-55px"
                :name="'ibLinkSetting' + index"
                :value="item"
                disabled
              />
              <span class="ms-5">%</span>
            </div>
          </div>
          <div>
            <div class="text-center fs-2">GOLD</div>
            <div
              v-for="(item, index) in distributionInfo.levelSetting
                .percentageSetting.GOLD"
              :key="index"
              class="d-flex align-items-center mt-5"
            >
              <label class="fs-5 w-80px me-3">Level {{ index + 1 }}</label>
              <input
                class="form-control form-control-solid w-200px h-55px"
                :name="'ibLinkSetting' + index"
                :value="item"
                disabled
              />
              <span class="ms-5">%</span>
            </div>
          </div>
        </div>
      </div>

      <div v-else>
        <div class="d-flex justify-content-center fs-3 mt-11">
          For setting please see the second IB
        </div>
        <div class="d-flex justify-content-center fs-3 mt-5">
          设置请查看二級 IB
        </div>
      </div>
    </template>

    <template v-else-if="!isLoading">
      <!-- Available Account Types ==================================================== -->
      <!-- ============================================================================ -->
      <div class="d-flex justify-content-between mt-5 mb-5">
        <div class="d-flex">
          <div class="d-flex">
            <div v-for="(accountType, index) in allAccountType" :key="index">
              <div
                v-if="availableAccountType.includes(accountType)"
                class="accountTypeBtn"
                :class="{
                  'active-accountTypeBtn': selectedAccountType == accountType,
                }"
                @click="selectedAccountType = accountType"
              >
                {{ $t("type.account." + accountType) }}
              </div>
            </div>
          </div>

          <div
            v-if="accountDetail.role != UserRoleTypes.Client"
            class="seperateLine"
          ></div>

          <div v-if="accountDetail.role != UserRoleTypes.Client" class="d-flex">
            <div v-for="(accountType, index) in allAccountType" :key="index">
              <div
                v-if="!availableAccountType.includes(accountType)"
                class="unAvailableAccountTypeBtn"
                :class="{
                  'active-accountTypeBtn': selectedAccountType == accountType,
                }"
                @click="selectedAccountType = accountType"
              >
                {{ $t("type.account." + accountType) }}
              </div>
            </div>
          </div>
        </div>
        <div class="d-flex align-items-center">
          <span class="me-3"
            ><el-button
              @click="viewCheckResult"
              :disabled="checkingStatus == checkingTypes.noNeed"
              ><span class="me-3">Form Check</span>
              <span
                v-if="checkingStatus == checkingTypes.isChecking"
                class="spinner-border spinner-border-sm align-middle"
              ></span>
              <i
                v-else-if="checkingStatus == checkingTypes.passed"
                class="fa-solid fa-circle"
                style="color: green"
              ></i>
              <i
                v-else-if="checkingStatus == checkingTypes.failed"
                class="fa-solid fa-circle fa-fade"
                style="color: red"
              ></i>
              <i
                v-else-if="checkingStatus == checkingTypes.noNeed"
                class="fa-solid fa-circle"
                style="color: lightgray"
              ></i
            ></el-button>
          </span>
        </div>
      </div>

      <!-- ============================================================================ -->
      <!-- ============================================================================ -->
      <table class="table table-row-dashed table-row-gray-200 align-middle">
        <tbody class="fw-semibold text-gray-600">
          <!-- Account Owner Info ============================================== -->
          <!-- ================================================================= -->
          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
              v-for="({ uid }, index) in relationList"
              :key="index"
              style="position: relative"
            >
              <div class="mb-2">
                <strong>{{
                  $t("type.roleType." + accountInfo[uid].role)
                }}</strong
                >:
                {{ accountInfo[uid].user.displayName }}
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
                  v-for="(acc, index) in rebateInfo[uid].calculatedLevelSetting
                    .allowedAccounts"
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
          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
              v-for="({ uid, upperAcc }, index) in relationList"
              :key="index"
            >
              <div
                v-if="
                  rebateInfo[uid]?.calculatedLevelSetting.allowedAccounts[
                    selectedAccountType
                  ]
                "
              >
                <div
                  v-if="rebateInfo[uid].isRoot"
                  class="d-flex flex-column align-items-center"
                >
                  <div>
                    <div>
                      {{ $t("fields.whichPips") }}
                    </div>
                    <el-checkbox-group
                      v-model="
                        rebateInfo[uid].levelSetting.allowedAccounts[
                          selectedAccountType
                        ].allowPips
                      "
                      size="small"
                      style="width: 180px"
                    >
                      <el-checkbox-button
                        v-for="option in defaultRebateRate[selectedAccountType]
                          .allowPipOptions"
                        :key="option"
                        :label="option"
                        >{{ option }}
                      </el-checkbox-button>
                    </el-checkbox-group>
                    <!--                    <el-select-v2-->
                    <!--                      v-model="-->
                    <!--                        rebateInfo[uid].levelSetting.allowedAccounts[-->
                    <!--                          selectedAccountType-->
                    <!--                        ].allowPips-->
                    <!--                      "-->
                    <!--                      :options="-->
                    <!--                        defaultRebateRate[selectedAccountType].allowPipOptions-->
                    <!--                      "-->
                    <!--                      placeholder="Pips"-->
                    <!--                      style="width: 200px"-->
                    <!--                      multiple-->
                    <!--                    />-->
                  </div>
                  <div class="mt-3">
                    <div>{{ $t("fields.whichCommission") }}</div>
                    <el-checkbox-group
                      v-model="
                        rebateInfo[uid].levelSetting.allowedAccounts[
                          selectedAccountType
                        ].allowCommissions
                      "
                      size="small"
                      style="width: 180px"
                    >
                      <el-checkbox-button
                        v-for="option in defaultRebateRate[selectedAccountType]
                          .allowCommissionOptions"
                        :key="option"
                        :label="option"
                        >{{ option }}
                      </el-checkbox-button>
                    </el-checkbox-group>
                    <!--                    <el-select-v2-->
                    <!--                      v-model="-->
                    <!--                        rebateInfo[uid].levelSetting.allowedAccounts[-->
                    <!--                          selectedAccountType-->
                    <!--                        ].allowCommissions-->
                    <!--                      "-->
                    <!--                      :options="-->
                    <!--                        defaultRebateRate[selectedAccountType]-->
                    <!--                          .allowCommissionOptions-->
                    <!--                      "-->
                    <!--                      placeholder="Commissions"-->
                    <!--                      style="width: 200px"-->
                    <!--                      multiple-->
                    <!--                    />-->
                  </div>
                </div>

                <div
                  class="d-flex flex-column justify-content-center align-items-center"
                  v-else-if="accountInfo[uid].role == UserRoleTypes.IB"
                >
                  <div style="width: 150px">
                    <div class="d-flex align-items-center">
                      <div class="me-3">Pips</div>
                      <el-select
                        v-model="
                          rebateInfo[uid].schema[selectedAccountType].pips
                        "
                        placeholder="Select"
                        size="small"
                      >
                        <el-option
                          :key="'Clear'"
                          :label="'Clear'"
                          :value="-1"
                          @click="
                            rebateInfo[uid].schema[selectedAccountType].pips =
                              null
                          "
                        />
                        <el-option
                          v-for="item in topIbRebateInfo.calculatedLevelSetting
                            .allowedAccounts[selectedAccountType].allowPips"
                          :key="item"
                          :label="item"
                          :value="item"
                          @click="
                            rebateInfo[uid].schema[
                              selectedAccountType
                            ].commission = null
                          "
                        />
                      </el-select>
                    </div>

                    <div
                      class="d-flex justify-content-center align-items-center mt-3"
                    >
                      <div class="me-3">Com</div>
                      <el-select
                        v-model="
                          rebateInfo[uid].schema[selectedAccountType].commission
                        "
                        placeholder="Select"
                        size="small"
                      >
                        <el-option
                          :key="'Clear'"
                          :label="'Clear'"
                          :value="-1"
                          @click="
                            rebateInfo[uid].schema[
                              selectedAccountType
                            ].commission = null
                          "
                        />
                        <el-option
                          v-for="item in topIbRebateInfo.calculatedLevelSetting
                            .allowedAccounts[selectedAccountType]
                            .allowCommissions"
                          :key="item"
                          :label="item"
                          :value="item"
                          @click="
                            rebateInfo[uid].schema[selectedAccountType].pips =
                              null
                          "
                        />
                      </el-select>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else>
                <button
                  v-if="
                    selectedAccountType != undefined &&
                    accountInfo[uid].role == UserRoleTypes.IB &&
                    rebateInfo[uid].calculatedLevelSetting.allowedAccounts[
                      selectedAccountType
                    ] == undefined &&
                    rebateInfo[upperAcc]?.calculatedLevelSetting
                      .allowedAccounts[selectedAccountType]
                  "
                  class="btn btn-sm btn-light-primary border-0"
                  @click="addAgentAccountType(uid)"
                >
                  Add {{ $t("type.account." + selectedAccountType) }}
                </button>

                <button
                  v-if="
                    selectedAccountType != undefined &&
                    accountInfo[uid].role == UserRoleTypes.IB &&
                    rebateInfo[uid].calculatedLevelSetting.allowedAccounts[
                      selectedAccountType
                    ] == undefined &&
                    rebateInfo[uid].isRoot
                  "
                  class="btn btn-sm btn-light-primary border-0"
                  @click="addTopAgentAccountType(uid)"
                >
                  Add {{ $t("type.account." + selectedAccountType) }}
                </button>
              </div>
            </td>
          </tr>

          <!-- ================================================================= -->
          <!-- ================================================================= -->

          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
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
                <div>
                  <el-input
                    style="width: 150px"
                    type="text"
                    v-model="
                      rebateInfo[belowAcc].schema[selectedAccountType]
                        .percentage
                    "
                  >
                    <template #append>%</template>
                  </el-input>
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
                  rebateInfo[belowAcc].schema[selectedAccountType].items.find(
                    (acc) => acc.cid == category.key
                  ).r
                "
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

          <tr class="text-center">
            <td></td>
            <td v-for="({ uid, belowAcc }, index) in relationList" :key="index">
              <button
                v-if="
                  ((accountInfo[uid].role == UserRoleTypes.Sales ||
                    accountInfo[uid].role == UserRoleTypes.IB) &&
                    rebateInfo[belowAcc]?.calculatedLevelSetting
                      .allowedAccounts[selectedAccountType]) ||
                  rebateInfo[uid]?.calculatedLevelSetting.allowedAccounts[
                    selectedAccountType
                  ]
                "
                class="btn btn-sm btn-light-primary border-0"
                @click="updateRebateRule(uid)"
              >
                Update
              </button>

              <span v-else></span>
            </td>
          </tr>
        </tbody>
      </table>
    </template>
  </div>

  <AddTopAgentAccountType
    ref="AddTopAgentAccountTypeRef"
    @buildRelationForm="buildRelationForm"
  />
  <CheckRebateSetting
    ref="CheckRebateSettingRef"
    @updateCheckingStatus="updateCheckingStatus"
    @buildRelationForm="buildRelationForm"
  ></CheckRebateSetting>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import { UserRoleTypes } from "@/core/types/RoleTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../services/AccountService";
import RebateService from "../../rebate/services/RebateService";
import { processKeysToCamelCase } from "@/core/services/api.client";
import AddTopAgentAccountType from "./modal/AddTopAgentAccountType.vue";
import CheckRebateSetting from "./modal/CheckRebateSetting.vue";
import { RebateDistributionTypes } from "@/core/types/RebateDistributionTypes";

const props = defineProps<{
  accountId: any;
}>();

const isLoading = ref(true);
const checkingTypes = {
  isChecking: 0,
  passed: 1,
  failed: 2,
  noNeed: 3,
};
const checkingStatus = ref(0);
const rebateInfo = ref({} as any);
const selectedAccountType = ref(0);
const accountInfo = ref({} as any);
const relationList = ref([] as any);
const accountDetail = ref({} as any);
const allAccountType = ref([] as any);
const defaultRebateRate = ref({} as any);
const defaultRebateRateForNewAccountType = ref({} as any);
const productCategory = ref(Array<any>());
const availableAccountType = ref([] as any);
const topIbRebateInfo = ref();
const distributionInfo = ref({} as any);
const hasAgentRule = ref(false);

const AddTopAgentAccountTypeRef =
  ref<InstanceType<typeof AddTopAgentAccountType>>();

const CheckRebateSettingRef = ref<InstanceType<typeof CheckRebateSetting>>();

const addTopAgentAccountType = async (_accountUid) => {
  AddTopAgentAccountTypeRef.value?.show(
    rebateInfo.value[_accountUid].id,
    selectedAccountType.value,
    productCategory.value,
    defaultRebateRateForNewAccountType.value
  );
};

const addAgentAccountType = async (_accountUid) => {
  const requestObject =
    rebateInfo.value[
      relationList.value.find((acc) => acc.uid == _accountUid).upperAcc
    ].calculatedLevelSetting.allowedAccounts[selectedAccountType.value];

  requestObject.pips = null;
  requestObject.commission = null;
  requestObject.allowPips = [];
  requestObject.allowCommissions = [];
  requestObject.percentage = 100;

  try {
    await RebateService.updateAgentRule(
      rebateInfo.value[_accountUid].id,
      JSON.parse(JSON.stringify(requestObject))
    );
    MsgPrompt.success("Update Success").then(() => {
      buildRelationForm();
    });
  } catch (error) {
    console.log(error);
  }
};

const updateTopAgentRule = async (_accountUid) => {
  if (!_accountUid) return;
  const requestObject =
    rebateInfo.value[_accountUid]?.levelSetting.allowedAccounts[
      selectedAccountType.value
    ];

  try {
    await RebateService.updateTopAgentRule(
      rebateInfo.value[_accountUid].id,
      JSON.parse(JSON.stringify(requestObject))
    );
    MsgPrompt.success("Update Success").then(() => {
      buildRelationForm();
    });
  } catch (error) {
    console.log(error);
  }
};

const updateAgentRule = async (_accountUid) => {
  if (!rebateInfo.value[_accountUid]?.schema[selectedAccountType.value]) return;

  let requestObject =
    rebateInfo.value[_accountUid]?.schema[selectedAccountType.value];
  requestObject.accountType = selectedAccountType.value;

  try {
    await RebateService.updateAgentRule(
      rebateInfo.value[_accountUid].id,
      JSON.parse(JSON.stringify(requestObject))
    );
    MsgPrompt.success("Update Success").then(() => {
      buildRelationForm();
    });
  } catch (error) {
    console.log(error);
  }
};

const updateRebateRule = async (_accountUid) => {
  const accountRole = accountInfo.value[_accountUid].role;
  const isRoot = rebateInfo.value[_accountUid]?.isRoot;
  const hasBelowAcc = relationList.value.find(
    (acc) => acc.uid === _accountUid
  )?.belowAcc;

  if (accountRole === UserRoleTypes.Sales) {
    updateTopAgentRule(hasBelowAcc);
  } else {
    if (isRoot) {
      updateTopAgentRule(_accountUid);
    } else {
      updateAgentRule(_accountUid);
    }
    updateAgentRule(hasBelowAcc);
  }
};

// =========================================================================================================
// =========================================================================================================

const buildRelationForm = async () => {
  isLoading.value = true;
  relationList.value = [];
  const _referPath = accountDetail.value.referPathUids;

  try {
    const [getCategory, getDefaultLevelSetting, queryAccounts, getAgentRules] =
      await Promise.all([
        RebateService.getRebateCategory(),
        RebateService.getDefaultLevelSetting(
          accountDetail.value.salesAccountId ?? accountDetail.value.id
        ).then((result) => processKeysToCamelCase(result)),
        AccountService.queryAccounts({
          uids: _referPath,
          size: 100,
        }),
        RebateService.getAgentRules({
          agentUids: _referPath,
        }),
      ]);

    accountInfo.value = queryAccounts.data.reduce((acc, cur) => {
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

    if (getAgentRules.data.length != 0) {
      hasAgentRule.value = true;
      distributionInfo.value = getAgentRules.data[0];
    }
  } catch (error) {
    // MsgPrompt.error(t("error.oopsErrorAccured"), "5263");
  }

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
  // console.log("accountInfo", accountInfo.value);
  // console.log("allAccountType", allAccountType.value);
  // console.log(
  //   "defaultRebateRateForNewAccountType",
  //   defaultRebateRateForNewAccountType.value
  // );
  // console.log("defaultRebateRate", defaultRebateRate.value);
  // console.log("selectedAccountType.value", selectedAccountType.value);

  if (Object.values(rebateInfo.value).find((acc: any) => acc.isRoot)) {
    checkingStatus.value = checkingTypes.isChecking;
    CheckRebateSettingRef.value?.onCheck(
      relationList.value,
      rebateInfo.value,
      defaultRebateRateForNewAccountType.value
    );
  } else {
    isLoading.value = false;
    updateCheckingStatus(checkingTypes.noNeed);
  }
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

    _data[acc].allowPipOptions = [
      ...new Set([
        ..._data[acc].allowPipOptions,
        ...(topIbRebateInfo.value?.calculatedLevelSetting.allowedAccounts[acc]
          ?.allowPips ?? []),
      ]),
    ];

    _data[acc].allowCommissionOptions = [
      ...new Set([
        ..._data[acc].allowCommissionOptions,
        ...(topIbRebateInfo.value?.calculatedLevelSetting.allowedAccounts[acc]
          ?.allowCommissions ?? []),
      ]),
    ];
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

const updateCheckingStatus = (_status) => {
  isLoading.value = false;
  checkingStatus.value = _status;

  // if (_status == checkingTypes.failed) {
  //   CheckRebateSettingRef.value?.show();
  // }
};

const viewCheckResult = () => {
  CheckRebateSettingRef.value?.show();
};

onMounted(async () => {
  try {
    accountDetail.value = await AccountService.getAccountDetailById(
      props.accountId
    );
    buildRelationForm();
  } catch (error) {
    // MsgPrompt.error(t("error.oopsErrorAccured"), "5263");
  }
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
  border: 1px solid black;
  border-radius: 5px;
  margin-right: 15px;
  cursor: pointer;
}

.accountTypeBtn:hover {
  border: 1px solid #5cb85c;
  background-color: #5cb85c;
  color: white;
}

.active-accountTypeBtn {
  border: 1px solid #5cb85c;
  background-color: #5cb85c;
  color: white;
}

.active-unAvailableAccountTypeBtn {
  border: 1px solid #5cb85c;
  background-color: #5cb85c;
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
</style>
