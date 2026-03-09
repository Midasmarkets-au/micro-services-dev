<template>
  <div
    class="modal fade"
    id="kt_modal_check_rebate"
    tabindex="-1"
    aria-hidden="true"
    ref="checkRebateSettingRef"
  >
    <div
      style="
        position: relative;
        background-color: black;
        opacity: 0.5;
        width: 100%;
        height: 100%;
      "
      @click="hide"
    ></div>

    <div
      class="modal-dialog modal-dialog-centered"
      :class="ITView ? 'fullWidth' : 'mw-550px'"
      style="position: absolute; top: 0; bottom: 0; right: 0; left: 0"
    >
      <div class="modal-content">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          style="max-height: 90vh; overflow: scroll"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2 class="fw-bold">Check Rebate Setting</h2>
            <div
              class="btn btn-icon btn-sm btn-active-icon-primary"
              data-bs-dismiss="modal"
            >
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <div v-if="isChecking">Checking</div>
          <div v-else class="p-7">
            <div v-if="$can('SuperAdmin')" class="d-flex flex-end mb-5">
              <el-button @click="ITView = !ITView">IT Department</el-button>
            </div>

            <div v-for="(value, key) in ErrorMessages" :key="key" class="mb-5">
              <div class="d-flex">
                <div class="accountTypeHead"></div>
                <h4>{{ $t("type.account." + key) }}</h4>
              </div>

              <div class="ms-5 mt-3">
                <div v-if="value['rateNotMatchDefaultSetting']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >一級代理的 Rate 不符合设置 (請更新正確數值)
                  </h5>
                  <div>說明：此一級代理的 Rate 總合與配置不同</div>
                </div>
                <div v-if="value['allowPipOptionsError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >一級代理的 Pip 組合與設置不符 (特殊設置請通知 IT)
                  </h5>
                  <div>說明：此一級代理的 Pip 組合有數值不在配置內</div>
                  <div>- 可用加佣：{{ value["allowPipOptions"] }}</div>
                  <div>- 目前設置：{{ value["currentPipOptions"] }}</div>
                </div>

                <div v-if="value['allowCommissionOptionsError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >一級代理的 Commission 組合與設置不符
                  </h5>
                  <div>說明：此一級代理的 Commission 組合有數值不在配置內</div>
                  <div>- 可用加佣：{{ value["allowCommissionOptions"] }}</div>
                  <div>- 目前設置：{{ value["currentCommissionOptions"] }}</div>
                </div>

                <div v-if="value['PipValueError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >代理的 Pip 數值不在一級代理的組合內
                  </h5>
                  <div>說明：一級代理沒有開此加點的權限</div>
                </div>

                <div v-if="value['CommissionValueError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >代理的 Commission 數值不在一級代理的組合內
                  </h5>
                  <div>說明：一級代理沒有開此加佣的權限</div>
                </div>

                <div v-if="value['noPipAllowError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >銷售沒有允許代理設置 Pip ( 清除代理加點 )
                  </h5>
                  <div>說明：銷售沒有給這個帳戶類型開放加點權限</div>
                </div>

                <div v-if="value['noCommissionAllowError']" class="mb-3">
                  <h5>
                    <i
                      class="fa-solid fa-circle me-2 mt-1"
                      style="color: #900000"
                    ></i
                    >銷售沒有允許代理設置 Commission ( 清除代理加佣 )
                  </h5>
                  <div>說明：銷售沒有給這個帳戶類型開放加佣權限</div>
                </div>

                <h5 v-else></h5>
              </div>
            </div>
          </div>

          <div v-if="$can('SuperAdmin')">
            <div v-if="ITView" class="d-flex p-7">
              <div v-for="(rule, index) in rebateInfo" :key="index">
                <VueJsonView
                  :src="rule"
                  :collapsed="true"
                  theme="rjv-default"
                />
              </div>
            </div>

            <VueJsonView v-else :src="autoUpdateItem" theme="rjv-default" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import RebateService from "../../../rebate/services/RebateService";
import VueJsonView from "@matpool/vue-json-view";

const emit = defineEmits(["updateCheckingStatus", "buildRelationForm"]);

const relationList = ref([]);
const rebateInfo = ref({});
const originalRebateInfo = ref({});
const defaultRebateRate = ref({});
const topAgentLevelSetting = ref({} as any);
const ErrorMessages = ref({} as any);
const checkRebateSettingRef = ref<null | HTMLElement>(null);
const isChecking = ref(true);
const ITView = ref(false);
const checkHasError = ref(false);
const updatedNeedReLoad = ref(false);
const autoUpdateItem = ref([] as any);
const topAgentAccountTypes = ref([] as any);

const checkingTypes = {
  isChecking: 0,
  passed: 1,
  failed: 2,
  noNeed: 3,
};

const pcOption = ref({
  pip: "pip",
  commission: "commission",
});

const hide = () => {
  hideModal(checkRebateSettingRef.value);
};

const updateTopAgentRule = (_accountUid, _accountType) => {
  if (!_accountUid) {
    return Promise.resolve();
  }

  const requestObject =
    rebateInfo.value[_accountUid]?.levelSetting.allowedAccounts[_accountType];

  return RebateService.updateTopAgentRule(
    rebateInfo.value[_accountUid].id,
    JSON.parse(JSON.stringify(requestObject))
  )
    .then(() => {
      updatedNeedReLoad.value = true;
    })
    .catch((error) => {
      console.log(error);
      throw error;
    });
};

const updateAgentRule = (_accountUid, _accountType) => {
  if (!rebateInfo.value[_accountUid]?.schema[_accountType]) {
    return Promise.resolve();
  }

  let requestObject = rebateInfo.value[_accountUid]?.schema[_accountType];
  requestObject.accountType = _accountType;

  return RebateService.updateAgentRule(
    rebateInfo.value[_accountUid].id,
    JSON.parse(JSON.stringify(requestObject))
  )
    .then(() => {
      updatedNeedReLoad.value = true;
    })
    .catch((error) => {
      console.log(error);
      throw error;
    });
};

const checkPipCommissionOptions = (
  accountType,
  key,
  defaultOptions,
  currentOptions
) => {
  if (
    defaultOptions.length === 0 &&
    currentOptions.length === 1 &&
    currentOptions[0] === 0
  ) {
    autoUpdateItem.value.push({
      key: key,
      accountType: accountType,
      defaultOption: defaultOptions,
      oldOption: currentOptions,
      newOption: [],
    });
    currentOptions = [];
  } else if (!currentOptions.every((val) => defaultOptions.includes(val))) {
    if (key == pcOption.value.pip) {
      ErrorMessages.value[accountType].allowPipOptionsError = true;
      ErrorMessages.value[accountType].allowPipOptions = defaultOptions;
      ErrorMessages.value[accountType].currentPipOptions = currentOptions;
      checkHasError.value = true;
    }
    if (key == pcOption.value.commission) {
      ErrorMessages.value[accountType].allowCommissionOptionsError = true;
      ErrorMessages.value[accountType].allowCommissionOptions = defaultOptions;
      ErrorMessages.value[accountType].currentCommissionOptions =
        currentOptions;
      checkHasError.value = true;
    }
  }

  return currentOptions;
};

const checkTopAgentLevelSetting = async () => {
  topAgentLevelSetting.value = (
    Object.values(rebateInfo.value).find((acc: any) => acc.isRoot) as any
  )?.levelSetting.allowedAccounts;

  topAgentAccountTypes.value = Object.keys(topAgentLevelSetting.value);
  // Loop Available Account Types
  topAgentAccountTypes.value.forEach((accountType: any) => {
    // Loop Default Rebate Rate
    var match = true;
    if (!defaultRebateRate.value[accountType]) {
      return;
    }
    for (let op = 0; op < defaultRebateRate.value[accountType].length; op++) {
      const defaultOption = defaultRebateRate.value[accountType][op];
      const defaultSettingItems = defaultOption.category;
      ErrorMessages.value[accountType] = {};
      match = true;

      // Loop Top Agent Level Setting to check if it matches with Default
      for (
        let index = 0;
        index < topAgentLevelSetting.value[accountType].items.length;
        index++
      ) {
        const item = topAgentLevelSetting.value[accountType].items[index];
        if (defaultSettingItems[item.cid] !== item.r) {
          match = false;
          break;
        }
      }

      //auto update
      topAgentLevelSetting.value[accountType].pips = null;
      topAgentLevelSetting.value[accountType].commission = null;
      topAgentLevelSetting.value[accountType].percentage = 0;

      if (match) {
        autoUpdateItem.value.push({
          accountType: accountType,
          oldName: topAgentLevelSetting.value[accountType].optionName,
          matchName: defaultOption.optionName,
        });
        //Update Default
        topAgentLevelSetting.value[accountType].optionName =
          defaultOption.optionName;

        // Check allowPipOptions
        topAgentLevelSetting.value[accountType].allowPips =
          checkPipCommissionOptions(
            accountType,
            pcOption.value.pip,
            defaultOption.allowPipOptions,
            topAgentLevelSetting.value[accountType].allowPips
          );
        topAgentLevelSetting.value[accountType].allowCommissions =
          checkPipCommissionOptions(
            accountType,
            pcOption.value.commission,
            defaultOption.allowCommissionOptions,
            topAgentLevelSetting.value[accountType].allowCommissions
          );
        break;
      }
    }
    if (!match) {
      ErrorMessages.value[accountType].rateNotMatchDefaultSetting = true;
      checkHasError.value = true;
    }
  });
};

const checkAgentSchema = async () => {
  // Loop Agent Rules (except top agent)
  for (let uid in rebateInfo.value) {
    if (!rebateInfo.value[uid].isRoot) {
      const schema = rebateInfo.value[uid].schema;
      for (const accountType of topAgentAccountTypes.value) {
        if (!schema[accountType]) {
          continue;
        }
        // Auto Fix
        autoUpdateItem.value.push({
          accountType: accountType,
          oldName: schema[accountType].optionName,
          matchName: topAgentLevelSetting.value[accountType].optionName,
        });

        schema[accountType].optionName =
          topAgentLevelSetting.value[accountType].optionName;

        // Auto Fix
        if (schema[accountType].pips > 0) {
          if (schema[accountType].commission == 0) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [schema[accountType].pips, null],
            });
            schema[accountType].commission = null;
          }
        }

        // Auto Fix
        if (schema[accountType].commission > 0) {
          if (schema[accountType].pips == 0) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [null, schema[accountType].commission],
            });
            schema[accountType].pips = null;
          }
        }

        // Auto Fix
        if (
          schema[accountType].pips == 0 &&
          schema[accountType].commission == 0
        ) {
          if (topAgentLevelSetting.value[accountType].allowPips.includes(0)) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              allowPips: topAgentLevelSetting.value[accountType].allowPips,
              allowCommissions:
                topAgentLevelSetting.value[accountType].allowCommissions,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [schema[accountType].pips, null],
            });
            schema[accountType].commission = null;
          } else if (
            topAgentLevelSetting.value[accountType].allowCommissions.includes(0)
          ) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              allowPips: topAgentLevelSetting.value[accountType].allowPips,
              allowCommissions:
                topAgentLevelSetting.value[accountType].allowCommissions,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [null, schema[accountType].commission],
            });
            schema[accountType].pips = null;
          } else {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              allowPips: topAgentLevelSetting.value[accountType].allowPips,
              allowCommissions:
                topAgentLevelSetting.value[accountType].allowCommissions,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [null, null],
            });
            schema[accountType].pips = null;
            schema[accountType].commission = null;
          }
        }

        // Manual Fix
        if (topAgentLevelSetting.value[accountType].allowPips.length == 0) {
          // auto update
          if (schema[accountType].pips == 0) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              allowPips: topAgentLevelSetting.value[accountType].allowPips,
              allowCommissions:
                topAgentLevelSetting.value[accountType].allowCommissions,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [null, schema[accountType].commission],
            });
            schema[accountType].pips = null;
          } else if (schema[accountType].pips != null) {
            ErrorMessages.value[accountType].noPipAllowError = true;
            checkHasError.value = true;
          }
        } else {
          if (
            schema[accountType].pips != null &&
            !topAgentLevelSetting.value[accountType].allowPips.includes(
              schema[accountType].pips
            )
          ) {
            ErrorMessages.value[accountType].PipValueError = true;
            checkHasError.value = true;
          }
        }

        // Manual Fix
        if (
          topAgentLevelSetting.value[accountType].allowCommissions.length == 0
        ) {
          if (schema[accountType].commission == 0) {
            autoUpdateItem.value.push({
              uid: uid,
              accountType: accountType,
              allowPips: topAgentLevelSetting.value[accountType].allowPips,
              allowCommissions:
                topAgentLevelSetting.value[accountType].allowCommissions,
              old: [schema[accountType].pips, schema[accountType].commission],
              new: [schema[accountType].pips, null],
            });
            schema[accountType].commission = null;
          } else if (schema[accountType].commission != null) {
            ErrorMessages.value[accountType].noCommissionAllowError = true;
            checkHasError.value = true;
          }
        } else {
          if (
            schema[accountType].commission != null &&
            !topAgentLevelSetting.value[accountType].allowCommissions.includes(
              schema[accountType].commission
            )
          ) {
            ErrorMessages.value[accountType].CommissionValueError = true;
            checkHasError.value = true;
          }
        }
      }
    }
  }
};

const onCheck = async (_relationList, _rebateInfo, _defaultRebateRate) => {
  rebateInfo.value = _rebateInfo;
  relationList.value = _relationList;
  defaultRebateRate.value = _defaultRebateRate;
  originalRebateInfo.value = JSON.parse(JSON.stringify(_rebateInfo));

  isChecking.value = true;
  checkHasError.value = false;
  updatedNeedReLoad.value = false;

  ErrorMessages.value = {};
  autoUpdateItem.value = [];

  await checkTopAgentLevelSetting();
  await checkAgentSchema();
  await updateAgentRebateRule();

  if (updatedNeedReLoad.value) {
    console.log(autoUpdateItem.value);
    emit("buildRelationForm");
    return;
  }

  isChecking.value = false;

  if (checkHasError.value) {
    emit("updateCheckingStatus", checkingTypes.failed);
  } else {
    emit("updateCheckingStatus", checkingTypes.passed);
  }
};

const updateAgentRebateRule = async () => {
  const promises = [];
  const existPromise = [];

  for (const accountType of topAgentAccountTypes.value) {
    for (const uid of Object.keys(rebateInfo.value)) {
      if (
        originalRebateInfo.value[uid].levelSetting.allowedAccounts[
          accountType
        ] == undefined
      ) {
        continue;
      }
      if (
        rebateInfo.value[uid].isRoot &&
        JSON.stringify(
          originalRebateInfo.value[uid].levelSetting.allowedAccounts[
            accountType
          ]
        ) !==
          JSON.stringify(
            rebateInfo.value[uid].levelSetting.allowedAccounts[accountType]
          )
      ) {
        if (existPromise.includes(uid)) {
          await updateTopAgentRule(uid, accountType);
        } else {
          existPromise.push(uid);
          promises.push(() => updateTopAgentRule(uid, accountType));
        }
      } else if (
        JSON.stringify(originalRebateInfo.value[uid].schema[accountType]) !==
        JSON.stringify(rebateInfo.value[uid].schema[accountType])
      ) {
        if (existPromise.includes(uid)) {
          await updateAgentRule(uid, accountType);
        } else {
          existPromise.push(uid);
          promises.push(() => updateAgentRule(uid, accountType));
        }
      }
    }
  }

  await Promise.all(promises.map((func) => func()));
};

const show = async () => {
  showModal(checkRebateSettingRef.value);
};

defineExpose({
  show,
  onCheck,
});
</script>

<style scoped lang="scss">
.accountTypeHead {
  width: 5px;
  height: 22px;
  margin-right: 10px;
  background-color: #ffd400;
}

.fullWidth {
  max-width: 100%;
}
</style>
