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

    <template v-else>
      <div class="d-flex justify-content-between">
        <div class="d-flex justify-content-start mt-7 mb-2">
          <div
            v-for="(accountType, index) in allAccountType"
            :key="index"
            class="accountTypeBtn mt-5 mb-5"
            :class="{
              'active-accountTypeBtn': selectedAccountType == accountType,
            }"
            @click="selectedAccountType = accountType"
          >
            {{ $t("type.account." + accountType) }}
          </div>
        </div>

        <!-- <div class="d-flex justify-content-end mt-7 mb-2">
          <div
            v-for="(option, index) in defaultRebateRate[selectedAccountType]"
            :key="index"
            class="btn btn-sm btn-light-primary mt-5 mb-5 me-3"
            @click="updateDefaultRebateRate(option)"
          >
            Option {{ index + 1 }}
          </div>
        </div> -->
      </div>
      <table class="table table-row-dashed table-row-gray-200 align-middle">
        <tbody class="fw-semibold text-gray-600">
          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
              v-for="(accountUid, index) in accountDetails.referPathUids"
              :key="index"
            >
              <div class="mb-2">
                <strong>{{
                  $t("type.roleType." + levelAccounts[accountUid].role)
                }}</strong
                >:
                {{ levelAccounts[accountUid].user.displayName }}
              </div>

              <span class="typeBadge">UID: {{ accountUid }} </span
              ><span
                v-if="
                  $can('SuperAdmin') &&
                  levelAccounts[accountUid].role == UserRoleTypes.IB
                "
                class="ms-3 typeBadgeDander"
                @click="showLevelSettingEditModal(accountUid)"
              ></span>
            </td>
          </tr>

          <tr class="text-center" style="border-bottom: 1px solid #000">
            <td class="table-title-gray"></td>
            <td
              class="table-title-gray"
              v-for="(accountUid, index) in accountDetails.referPathUids"
              :key="index"
            >
              <div
                v-if="
                  levelAccounts[accountUid].role == UserRoleTypes.Sales &&
                  levelAccounts[accountUid].belowAgentSetting.schema[
                    selectedAccountType
                  ]
                "
              >
                <div>
                  <div>Pips</div>
                  <el-select-v2
                    v-model="pcSettings[selectedAccountType].allowPips"
                    :options="
                      pcSettings[selectedAccountType].allowPipOptions ==
                      undefined
                        ? []
                        : pcSettings[selectedAccountType].allowPipOptions
                    "
                    placeholder="Pips"
                    style="width: 200px"
                    multiple
                  />
                </div>
                <div class="mt-3">
                  <div>Commissions</div>

                  <el-select-v2
                    v-model="pcSettings[selectedAccountType].allowCommissions"
                    :options="
                      pcSettings[selectedAccountType].allowCommissionOptions ==
                      undefined
                        ? []
                        : pcSettings[selectedAccountType].allowCommissionOptions
                    "
                    placeholder="Commissions"
                    style="width: 200px"
                    multiple
                  />
                </div>
              </div>
              <div
                v-else-if="
                  levelAccounts[accountUid].role == UserRoleTypes.IB &&
                  levelAccounts[accountUid].belowAgentSetting.schema[
                    selectedAccountType
                  ] &&
                  !levelAccounts[accountUid].belowAgentSetting.setRemain
                "
              >
                <div class="d-flex justify-content-center align-items-center">
                  <div class="w-100px me-3">
                    <el-select
                      v-model="pcSettings[selectedAccountType].selected"
                      class="m-2"
                      placeholder="Select"
                      size="small"
                      @change="updatePcSetting"
                    >
                      <el-option key="pips" label="pips" value="pips" />
                      <el-option
                        key="commission"
                        label="commission"
                        value="commission"
                      />
                    </el-select>
                  </div>
                  <div class="w-50px">
                    <el-select
                      v-model="
                        levelAccounts[accountUid].belowAgentSetting.schema[
                          selectedAccountType
                        ][pcSettings[selectedAccountType].selected]
                      "
                      class="m-2"
                      placeholder="Select"
                      size="small"
                    >
                      <el-option
                        v-for="item in pcValueOptions"
                        :key="item"
                        :label="item"
                        :value="item"
                      />
                    </el-select>
                  </div>
                </div>

                <div class="mt-3">
                  Percentage:
                  <span
                    ><el-input
                      class="w-50px me-2"
                      type="text"
                      v-model="
                        levelAccounts[accountUid].belowAgentSetting.schema[
                          selectedAccountType
                        ].percentage
                      "
                    />
                    %</span
                  >
                </div>
              </div>
              <div v-else>N/A</div>
            </td>
          </tr>

          <tr
            class="text-center"
            v-for="(category, p_index) in productCategory"
            :key="p_index"
          >
            <td>
              {{ category.value }}
            </td>
            <td
              v-for="(accountUid, a_index) in accountDetails.referPathUids"
              :key="a_index"
            >
              <span
                v-if="
                  (levelAccounts[accountUid].role == UserRoleTypes.IB ||
                    levelAccounts[accountUid].role == UserRoleTypes.Sales) &&
                  levelAccounts[accountUid].belowAgentSetting.schema[
                    selectedAccountType
                  ]
                "
              >
                <span
                  v-if="
                    levelAccounts[accountUid].belowAgentSetting.schema[
                      selectedAccountType
                    ].items
                  "
                >
                  <el-input-number
                    v-if="
                      !levelAccounts[accountUid].belowAgentSetting.setRemain
                    "
                    v-model="
                      levelAccounts[accountUid].belowAgentSetting.schema[
                        selectedAccountType
                      ].items[category.key]
                    "
                    :precision="1"
                    :step="0.1"
                    :min="0"
                  />
                  <span v-else>{{
                    levelAccounts[accountUid].belowAgentSetting.schema[
                      selectedAccountType
                    ].items[category.key]
                  }}</span>
                </span>

                <span v-else>N/A</span>
              </span>
            </td>
          </tr>

          <tr class="text-center">
            <td></td>
            <td
              v-for="(accountUid, index) in accountDetails.referPathUids"
              :key="index"
            >
              <button
                v-if="
                  (levelAccounts[accountUid].role == UserRoleTypes.Sales &&
                    Object.keys(
                      levelAccounts[accountUid].belowAgentSetting.schema
                    ).length != 0) ||
                  (levelAccounts[accountUid].role == UserRoleTypes.IB &&
                    !levelAccounts[accountUid].belowAgentSetting.setRemain)
                "
                class="btn btn-sm btn-light-primary border-0"
                @click="updateIBSetting(accountUid)"
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

  <LevelSettingEditModal
    ref="levelSettingEditModalRef"
    @prepareRebateData="prepareRebateData"
  />
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { UserRoleTypes } from "@/core/types/RoleTypes";

import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../services/AccountService";
import RebateService from "../../rebate/services/RebateService";
import LevelSettingEditModal from "./modal/LevelSettingEditModal.vue";

const props = defineProps<{
  accountDetails: any;
}>();

const isLoading = ref(true);
const productCategory = ref(Array<any>());
const accountDetails = ref({} as any);
const levelAccounts = ref({} as any);
const selectedAccountType = ref(0);
const allAccountType = ref([] as any);
const pcSettings = ref({} as any);
const pcValueOptions = ref([] as any);
const defaultRebateRate = ref({} as any);
const levelRebateRule = ref({} as any);

const levelSettingEditModalRef =
  ref<InstanceType<typeof LevelSettingEditModal>>();

const showLevelSettingEditModal = (_accountUid) => {
  let result = levelRebateRule.value.find(
    (item) => item.agentAccountUid === _accountUid
  );

  levelSettingEditModalRef.value?.show(
    result === undefined ? undefined : result.id,
    result === undefined
      ? levelAccounts.value[_accountUid].type
      : selectedAccountType.value,
    levelAccounts.value[_accountUid].id
  );
};

const updatePcSetting = () => {
  const currentAccountPcSetting = pcSettings.value[selectedAccountType.value];
  if (currentAccountPcSetting.selected == "pips") {
    pcValueOptions.value = currentAccountPcSetting.allowPips;
    pcSettings.value[selectedAccountType.value].value = pcValueOptions.value[0];
  } else {
    pcValueOptions.value = currentAccountPcSetting.allowCommissions;
    pcSettings.value[selectedAccountType.value].value = pcValueOptions.value[0];
  }
};

const updateDefaultRebateRate = (_option: any) => {
  const salesUid = accountDetails.value.referPathUids[1];

  pcSettings.value[selectedAccountType.value].allowPipOptions =
    _option.allowPipOptions.map((option) => ({
      label: _option.allowPipSetting[option].name,
      value: _option.allowPipSetting[option].value,
    }));

  pcSettings.value[selectedAccountType.value].allowCommissionOptions =
    _option.allowCommissionOptions.map((option) => ({
      label: _option.allowCommissionSetting[option].name,
      value: _option.allowCommissionSetting[option].value,
    }));

  for (const key in _option.category) {
    levelAccounts.value[salesUid].belowAgentSetting.schema[
      selectedAccountType.value
    ].items[key] = _option.category[key];
  }
};

const emptyRebateRule = {
  pips: 0,
  commission: 0,
  percentage: 0,
  items: {
    "1": 0,
    "2": 0,
    "3": 0,
    "4": 0,
    "5": 0,
    "6": 0,
    "7": 0,
    "8": 0,
    "9": 0,
    "10": 0,
    "11": 0,
    "12": 0,
    "13": 0,
    "14": 0,
    "15": 0,
    "16": 0,
  },
  allowPips: [],
  allowCommissions: [],
};

const updateIBSetting = async (_accountUid) => {
  var editedObject =
    levelAccounts.value[_accountUid].belowAgentSetting.schema[
      selectedAccountType.value
    ];

  if (editedObject == undefined) {
    editedObject = {
      ...emptyRebateRule,
      accountType: selectedAccountType.value,
    };

    if (pcSettings.value[selectedAccountType.value].selected == "pips") {
      editedObject.pips = pcSettings.value[selectedAccountType.value].value;
      editedObject.allowPips.push(
        pcSettings.value[selectedAccountType.value].value
      );
      editedObject.commission = 0;
    } else {
      editedObject.pips = 0;
      editedObject.commission =
        pcSettings.value[selectedAccountType.value].value;
      editedObject.allowCommissions.push(
        pcSettings.value[selectedAccountType.value].value
      );
    }
  }

  const requestObject = JSON.parse(JSON.stringify(editedObject));

  const convertItems = Object.keys(requestObject.items).map((key) => ({
    cid: parseInt(key),
    r: requestObject.items[key],
  }));

  requestObject.items = convertItems;

  try {
    if (levelAccounts.value[_accountUid].belowAgentSetting.isRoot) {
      // Top Agent Pips and Commissions Setting
      requestObject.allowPips =
        pcSettings.value[selectedAccountType.value].allowPips;
      requestObject.allowCommissions =
        pcSettings.value[selectedAccountType.value].allowCommissions;

      await RebateService.updateTopAgentRule(
        levelAccounts.value[_accountUid].belowAgentSetting.ruleID,
        requestObject
      );
    } else {
      if (pcSettings.value[selectedAccountType.value].selected == "pips") {
        requestObject.pips = editedObject["pips"];
        requestObject.commission = 0;
      } else {
        requestObject.pips = 0;
        requestObject.commission = editedObject["commission"];
      }

      console.log(requestObject);

      await RebateService.updateAgentRule(
        levelAccounts.value[_accountUid].belowAgentSetting.ruleID,
        requestObject
      );
    }
    MsgPrompt.success("Update Success").then(() => {
      prepareRebateData();
    });
  } catch (error) {
    console.log(error);
  }
};

const prepareRebateData = async () => {
  // 關係鏈
  const referPath = accountDetails.value.referPathUids;

  // 获得默认反佣设置
  defaultRebateRate.value = await RebateService.getDefaultLevelSetting(
    referPath[1]
  );

  // 获得产品类别
  productCategory.value = await RebateService.getRebateCategory();

  // 获得上级每层的账号信息
  const _levelAccounts = await AccountService.queryAccounts({
    uids: referPath,
  });

  const agentUids = _levelAccounts.data
    .filter((x) => x.role == UserRoleTypes.IB)
    .map((x) => x.uid);

  if (agentUids.length == 0) {
    agentUids.push(0);
  }

  // 获得上级每层的反佣设置
  const _levelRebateRule = await RebateService.getAgentRules({
    agentUids: agentUids,
  });
  levelRebateRule.value = _levelRebateRule.data;

  // 初始化每层的账号信息
  referPath.forEach((uid) => {
    levelAccounts.value[uid] = {};
  });

  _levelAccounts.data.forEach((account) => {
    levelAccounts.value[account.uid] = account;
    // belowAgentSetting 用來存放下級反佣设置 （為了左移顯示）
    levelAccounts.value[account.uid].belowAgentSetting = {};
    levelAccounts.value[account.uid].belowAgentSetting.schema = {};
  });

  // 填入左移後每层的反佣设置
  _levelRebateRule.data.forEach(async (rule) => {
    // 获得上级账号UID
    const upperAccountUID =
      referPath[referPath.indexOf(rule.agentAccountUid) - 1];

    // 將反佣信息填入上級帳號底下（Rule ID, UID, isRoot, "setRemain" 用來判斷是不是最後一個 IB）
    levelAccounts.value[upperAccountUID].belowAgentSetting.ruleID = rule.id;
    levelAccounts.value[upperAccountUID].belowAgentSetting.agentAccountUid =
      rule.agentAccountUid;
    levelAccounts.value[upperAccountUID].belowAgentSetting.isRoot = rule.isRoot;
    levelAccounts.value[upperAccountUID].belowAgentSetting.setRemain = false;

    // 以下填入反佣與 pips and commissions 设置
    // Top IB 取 Level Setting（我能玩啥）, 其他 IB 取 Schema（上級拿多少）
    const getSchemaInfo = ref([] as any);
    if (rule.isRoot) {
      getSchemaInfo.value = rule.levelSetting.allowedAccounts;
    } else {
      getSchemaInfo.value = rule.schema;
    }

    getSchemaInfo.value.forEach((schema) => {
      // 統整 Pips and Commissions
      // 初始設置
      if (pcSettings.value[schema.accountType] == undefined) {
        pcSettings.value[schema.accountType] = {};
        pcSettings.value[schema.accountType].selected = "pips";
        pcSettings.value[schema.accountType].value = 0;
      }

      if (rule.isRoot) {
        pcSettings.value[schema.accountType].allowCommissions =
          schema.allowCommissions;
        pcSettings.value[schema.accountType].allowPips = schema.allowPips;
      }

      if (schema.commission != 0) {
        pcSettings.value[schema.accountType].selected = "commission";
        pcSettings.value[schema.accountType].value = schema.commission;
      } else if (schema.pips != 0) {
        pcSettings.value[schema.accountType].selected = "pips";
        pcSettings.value[schema.accountType].value = schema.pips;
      }

      // 填入反佣设置 （填入上級 belowAgentSetting.schema 內）
      if (!allAccountType.value.includes(schema.accountType)) {
        allAccountType.value.push(schema.accountType);
      }

      levelAccounts.value[upperAccountUID].belowAgentSetting.schema[
        schema.accountType
      ] = JSON.parse(JSON.stringify(schema));

      levelAccounts.value[upperAccountUID].belowAgentSetting.schema[
        schema.accountType
      ].items = {};

      const copyItems = JSON.parse(JSON.stringify(schema.items));
      copyItems.forEach((item) => {
        levelAccounts.value[upperAccountUID].belowAgentSetting.schema[
          schema.accountType
        ].items[item.cid] = item.r;
      });
    });

    // 找到最後一個 IB, 填入 Remain
    if (
      referPath.indexOf(rule.agentAccountUid) + 1 == referPath.length ||
      levelAccounts.value[
        referPath[referPath.indexOf(rule.agentAccountUid) + 1]
      ].role == UserRoleTypes.Client
    ) {
      const getRemainLevelSetting =
        await RebateService.getRemainLevelSettingById(rule.agentAccountId);

      levelAccounts.value[rule.agentAccountUid].belowAgentSetting.setRemain =
        true;

      getRemainLevelSetting.allowedAccounts.forEach((schema) => {
        levelAccounts.value[rule.agentAccountUid].belowAgentSetting.schema[
          schema.accountType
        ] = JSON.parse(JSON.stringify(schema));

        levelAccounts.value[rule.agentAccountUid].belowAgentSetting.schema[
          schema.accountType
        ].items = {};

        const copyItems = JSON.parse(JSON.stringify(schema.items));
        copyItems.forEach((item) => {
          levelAccounts.value[rule.agentAccountUid].belowAgentSetting.schema[
            schema.accountType
          ].items[item.cid] = item.r;
        });
      });
    }
  });

  // 如果是客戶，只顯示該帳戶的反佣設置
  if (accountDetails.value.role == UserRoleTypes.Client) {
    allAccountType.value = [];
    allAccountType.value.push(accountDetails.value.type);
  }

  selectedAccountType.value = allAccountType.value[0];

  // 確認帳號類型後, 填入 Pips and Commissions 選項

  // 統整 Pips and Commissions
  Object.keys(defaultRebateRate.value).forEach((accountType) => {
    console.log(defaultRebateRate.value);
    if (pcSettings.value[accountType]) {
      pcSettings.value[accountType].allowPipOptions = defaultRebateRate.value[
        accountType
      ][0].allowPipOptions.map((option) => ({
        label:
          defaultRebateRate.value[accountType][0].allowPipSetting[option].name,
        value:
          defaultRebateRate.value[accountType][0].allowPipSetting[option].value,
      }));

      pcSettings.value[accountType].allowCommissionOptions =
        defaultRebateRate.value[accountType][0].allowCommissionOptions.map(
          (option) => ({
            label:
              defaultRebateRate.value[accountType][0].allowCommissionSetting[
                option
              ].name,
            value:
              defaultRebateRate.value[accountType][0].allowCommissionSetting[
                option
              ].value,
          })
        );
    }
  });

  // console.log(props.accountDetails);
  // console.log(pcSettings.value);
  // console.log("levelAccounts", levelAccounts.value);

  isLoading.value = false;
};

watch(
  () => selectedAccountType.value,
  () => {
    if (pcSettings.value[selectedAccountType.value]) {
      if (pcSettings.value[selectedAccountType.value].selected == "pips") {
        pcValueOptions.value =
          pcSettings.value[selectedAccountType.value].allowPips;
      } else {
        pcValueOptions.value =
          pcSettings.value[selectedAccountType.value].allowCommissions;
      }
    }
  }
);

onMounted(async () => {
  accountDetails.value = JSON.parse(JSON.stringify(props.accountDetails));
  prepareRebateData();
});
</script>

<style scoped>
.accountTypeBtn {
  width: 100px;
  text-align: center;
  padding: 10px 15px;
  border: 1px solid black;
  border-radius: 5px;
  margin-right: 10px;
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

.activeBadge {
  background-color: #5cb85c;
  color: white;
  padding: 2px 8px;
  border-radius: 5px;
  cursor: pointer;
}
.inactiveBadge {
  background-color: lightgray;
  color: white;
  padding: 2px 8px;
  cursor: pointer;
  border-radius: 5px;
}

.typeBadgeDander {
  background-color: #ffa400;
  color: white;
  padding: 0px 8px;
  border-radius: 100%;
  cursor: pointer;
}
</style>
