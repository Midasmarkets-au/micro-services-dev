<template>
  <div class="d-flex justify-content-center" v-if="isLoading">
    <LoadingRing />
  </div>

  <div v-else class="table-responsive">
    <table
      class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
    >
      <tbody class="fw-semibold text-gray-600">
        <tr class="text-center" style="border-bottom: 1px solid #000">
          <td class="table-title-gray">{{ $t("title.category") }}</td>
          <td class="table-title-gray">
            {{ $t("title.totalRebate") }}
          </td>
          <td class="table-title-gray">
            {{ $t("title.personalRebate") }}
          </td>
          <td class="table-title-gray">
            {{ $t("title.remainRebate") }}
          </td>

          <!-- Only standard account has pip and commission -->
          <template
            v-if="
              defaultAccountSetting.allowPipOptions.length > 1 ||
              defaultAccountSetting.allowCommissionOptions.length > 1
            "
          >
            <td v-if="props.isRoot" class="table-title-blue" colspan="2">
              <div
                v-if="addNewAccountType"
                class="table-title-blue"
                colspan="2"
              >
                <div
                  class="d-flex align-items-center justify-content-center p-2"
                  style="cursor: pointer"
                  @click="
                    (pcSelection.pcDropdown = !pcSelection.pcDropdown),
                      (pcSelection.optionDropdown = false)
                  "
                >
                  <span>{{ $t("title." + pcSelection.selectedPC) }}</span>
                  <i
                    class="fa-solid fa-angle-down ms-3"
                    style="color: #ffffff"
                  ></i>
                </div>

                <!-- dropdown -->
                <div v-if="pcSelection.pcDropdown" style="position: relative">
                  <div class="rebateDropdown">
                    <div class="dropdownItem" @click="selectPCFn('pips')">
                      {{ $t("title.pips") }}
                    </div>
                    <div class="dropdownItem" @click="selectPCFn('commission')">
                      {{ $t("title.commission") }}
                    </div>
                  </div>
                </div>

                <div
                  class="d-flex align-items-center justify-content-center p-2"
                  style="cursor: pointer"
                  @click="
                    (pcSelection.optionDropdown = !pcSelection.optionDropdown),
                      (pcSelection.pcDropdown = false)
                  "
                >
                  <span
                    >{{ $t("title.options") }} (
                    {{ pcSelection.pcValue }} )</span
                  >
                  <i
                    class="fa-solid fa-angle-down ms-3"
                    style="color: #ffffff"
                  ></i>
                </div>

                <!-- dropdown -->
                <div
                  v-if="pcSelection.optionDropdown"
                  style="position: relative"
                >
                  <div
                    v-if="pcSelection.selectedPC == 'pips'"
                    class="rebateDropdown"
                  >
                    <div
                      v-for="(item, index) in props.currentAccountRebateRule
                        .allowPips"
                      class="dropdownItem"
                      @click="selectValFn(item)"
                      :key="index"
                    >
                      {{ $t("type.pipOptions." + item) }}
                    </div>
                  </div>
                  <div
                    v-else-if="pcSelection.selectedPC == 'commission'"
                    class="rebateDropdown"
                  >
                    <div
                      v-for="(item, index) in props.currentAccountRebateRule
                        .allowCommissions"
                      class="dropdownItem"
                      :key="index"
                      @click="selectValFn(item)"
                    >
                      {{ $t("type.commissionOptions." + item) }}
                    </div>
                  </div>
                </div>
              </div>

              <div
                v-else
                class="d-flex align-items-center justify-content-center p-2"
              >
                <span>{{ pcSelection.selectedPC }}</span>
              </div>
            </td>

            <!-- 如果前面有 IB 100% 全拿，後端會返回 remain percentage = 0 -->
            <td
              v-if="props.currentAccountRebateRule.percentage != 0"
              class="table-title-gray"
            >
              {{ $t("title.percentage") }}
            </td>
          </template>
        </tr>
        <tr class="text-center" v-for="(item, index) in formTable" :key="index">
          <td>{{ $t("type.productCategory." + item.name) }}</td>
          <td>{{ item.total }}</td>
          <td>
            <el-input-number
              v-model="item.r"
              :precision="1"
              :step="0.1"
              :min="0"
              :max="personalRebateInitialValue[index]"
            />
          </td>
          <td>{{ item.total < item.r ? 0 : calculate(item.total, item.r) }}</td>

          <!-- Only standard account has pip and commission -->
          <template
            v-if="
              defaultAccountSetting.allowPipOptions.length > 1 ||
              defaultAccountSetting.allowCommissionOptions.length > 1
            "
          >
            <td
              v-if="props.isRoot"
              class="w-150px pe-3"
              colspan="2"
              style="border-left: 1px solid #f5f5f5"
            >
              <span>
                {{
                  {
                    pips: pcSelection.schema[item.cid],
                    commission: "$ " + pcSelection.schema[item.cid],
                  }[pcSelection.selectedPC]
                }}
              </span>
            </td>

            <!-- 如果前面有 IB 100% 全拿，後端會返回 remain percentage = 0 -->
            <td
              class=""
              v-if="
                index == 0 && props.currentAccountRebateRule.percentage != 0
              "
              :rowspan="props.productCategory.length"
              style="border-left: 1px solid #f5f5f5"
            >
              <div
                class="d-flex align-items-center justify-content-center p-2"
                style="cursor: pointer"
              >
                <el-input
                  class="w-60px me-3"
                  type="text"
                  v-model="percentage"
                  @blur="onPercentageBlur"
                  @input="editPercentageCheck"
                />
                %
              </div>
            </td>
          </template>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts" setup>
import Decimal from "decimal.js";
import { ref, onMounted, computed } from "vue";
import { AccountTypes } from "@/core/types/AccountInfos";
import { processKeysToCamelCase } from "@/core/services/api.client";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import IbService from "../../services/IbService";

const props = defineProps<{
  isRoot: any;
  productCategory: any;
  targetAccountRebateRule: any;
  currentAccountRebateRule: any;
}>();

const isLoading = ref(true);
const accountDefaultRebateRate = ref({});
const formTable = ref([] as any);
const formAccountType = ref();
const percentage = ref("0");
const addNewAccountType = ref(false);
const defaultAccountSetting = ref({} as any);

const pcSelection = ref({
  schema: {},
  pcDropdown: false,
  optionDropdown: false,
  selectedPC: "pips",
  pcValue: 0,
});

const onPercentageBlur = () => {
  const val = parseInt(percentage.value);
  if (val > 100) {
    percentage.value = "100";
  } else if (val < 0) {
    percentage.value = "0";
  }
};

const editPercentageCheck = () => {
  if (parseInt(personalRebateInitialPercentage) < parseInt(percentage.value)) {
    percentage.value = personalRebateInitialPercentage;
  }
};

const selectPCFn = (_val: string) => {
  pcSelection.value.pcValue = 0;
  pcSelection.value.selectedPC = _val;
  pcSelection.value.pcDropdown = false;

  if (pcSelection.value.selectedPC == "pips") {
    pcSelection.value.schema =
      accountDefaultRebateRate.value[
        formAccountType.value
      ][0]?.allowPipSetting[0]?.items;
  } else {
    pcSelection.value.schema =
      accountDefaultRebateRate.value[
        formAccountType.value
      ][0]?.allowCommissionSetting[0]?.items;
  }
};

const selectValFn = (_val: number) => {
  if (pcSelection.value.selectedPC == "pips") {
    pcSelection.value.schema =
      accountDefaultRebateRate.value[formAccountType.value][0].allowPipSetting[
        _val
      ]?.items;
  } else {
    pcSelection.value.schema =
      accountDefaultRebateRate.value[
        formAccountType.value
      ][0].allowCommissionSetting[_val]?.items;
  }

  pcSelection.value.pcValue = _val;
  pcSelection.value.optionDropdown = false;
};

let personalRebateInitialValue = Array<number>();
let personalRebateInitialPercentage = String();

onMounted(async () => {
  formTable.value = [];
  formAccountType.value = props.currentAccountRebateRule.accountType;

  const temp = ref({} as any);

  try {
    accountDefaultRebateRate.value = await IbService.getDefaultLevelSetting();
    accountDefaultRebateRate.value = processKeysToCamelCase(
      accountDefaultRebateRate.value
    );
  } catch (error) {
    MsgPrompt.error(error);
  }

  // console.log("targetAccountRebateRule", props.targetAccountRebateRule);
  // console.log("accountDefaultRebateRate", accountDefaultRebateRate.value);

  defaultAccountSetting.value =
    accountDefaultRebateRate.value[formAccountType.value][0];

  if (props.targetAccountRebateRule) {
    addNewAccountType.value = false;

    for (const item of props.targetAccountRebateRule.items) {
      temp.value[item.cid] = item.r;
    }

    if (
      defaultAccountSetting.value.allowPipOptions.length > 1 ||
      defaultAccountSetting.value.allowCommissionOptions.length > 1
    ) {
      percentage.value = props.targetAccountRebateRule.percentage;
      if (props.targetAccountRebateRule.pips > 0) {
        pcSelection.value.schema =
          defaultAccountSetting.value.allowPipSetting[
            props.targetAccountRebateRule.pips
          ].items;
        pcSelection.value.selectedPC = "pips";
        pcSelection.value.pcValue = props.targetAccountRebateRule.pips;
      } else {
        pcSelection.value.schema =
          defaultAccountSetting.value.allowCommissionSetting[
            props.targetAccountRebateRule.commission
          ].items;
        pcSelection.value.selectedPC = "commission";
        pcSelection.value.pcValue = props.targetAccountRebateRule.commission;
      }
    }
  } else {
    addNewAccountType.value = true;

    Object.keys(props.currentAccountRebateRule.items).forEach((cid) => {
      temp.value[cid] = 0;
    });

    if (
      defaultAccountSetting.value.allowPipOptions.length > 1 ||
      defaultAccountSetting.value.allowCommissionOptions.length > 1
    ) {
      pcSelection.value.schema =
        defaultAccountSetting.value.allowPipSetting[0].items;

      percentage.value = "100";
      pcSelection.value.selectedPC = "pips";
      pcSelection.value.pcValue = 0;
    }
  }

  props.productCategory.forEach((element, index) => {
    formTable.value.push({
      cid: element.key,
      name: element.value,
      total: props.currentAccountRebateRule.items[element.key],
      r: temp.value[element.key],
    });
  });

  if (props.currentAccountRebateRule.defaultSelected) {
    personalRebateInitialValue = formTable.value.map((item: any) => item.r);
    personalRebateInitialPercentage = percentage.value;
  } else {
    personalRebateInitialValue = formTable.value.map((item: any) => item.total);
    personalRebateInitialPercentage = "100";
  }

  isLoading.value = false;
});

const calculate = (a, b) => {
  return new Decimal(a).minus(new Decimal(b)).toString();
};

const formCheck = () => {
  return props.currentAccountRebateRule.selected;
};

const collectData = () => {
  var _p = 0;
  var _c = 0;

  if (pcSelection.value.selectedPC == "pips") {
    _p = pcSelection.value.pcValue;
  } else {
    _c = pcSelection.value.pcValue;
  }

  var items = [] as any;
  formTable.value.forEach((element) => {
    items.push({
      cid: element.cid,
      r: element.r,
    });
  });

  return {
    accountType: formAccountType.value,
    pips: _p,
    commission: _c,
    percentage: percentage.value == "" ? "0" : percentage.value,
    items: items,
  };
};

defineExpose({
  formCheck,
  collectData,
});
</script>

<style>
.table-title-gray {
  background-color: #f5f7fa !important;
}

.table-title-blue {
  color: white !important;
  background-color: #0053ad !important;
}

.rebateDropdown {
  position: absolute;
  overflow: hidden;
  right: 10px;

  color: black;
  background-color: white;
  width: 172px;
  filter: drop-shadow(0px 4px 14px rgba(0, 0, 0, 0.1));
  border-radius: 8px;
}

.dropdownItem {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 40px;
  cursor: pointer;
  border: 1px solid #f5f7fa;
}

.dropdownItem:hover {
  background-color: #f5f7fa;
}
</style>
