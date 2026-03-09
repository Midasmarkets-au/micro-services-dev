<template>
  <div class="rebate-form-wrapper">
    <table class="rebate-table">
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else style="color: #3a3e44" class="fs-6">
        <tr class="text-center">
          <td class="table-title-gray">{{ $t("title.category") }}</td>
          <td class="table-title-gray">
            {{ $t("title.totalRebate") }}
          </td>
          <td class="table-title-gray">
            <div class="d-flex" style="flex-direction: column">
              <span>{{ $t("title.personalRebate") }}</span>
              <span>
                <el-tooltip
                  placement="top"
                  :content="$t('title.setRebatePercentageTooltip')"
                  ><el-input-number
                    class="w-60px me-3"
                    type="string"
                    :step="1"
                    :min="0"
                    :max="100"
                    :controls="false"
                    v-model="personCentage"
                    :precision="0"
                  />
                </el-tooltip>
                %</span
              >
            </div>
          </td>
          <td class="table-title-gray">{{ $t("title.remainRebate") }}</td>

          <td
            v-if="
              (props.currentAccountLevelSetting.allowPips.length > 0 ||
                props.currentAccountLevelSetting.allowCommissions.length > 0) &&
              props.isRoot
            "
            class="table-title-blue"
          >
            <div v-if="editMode">
              <div
                class="d-flex align-items-center justify-content-center mb-5"
                style="cursor: pointer"
              >
                <span>{{
                  pcSelection.selectedPC
                    ? $t("title." + pcSelection.selectedPC)
                    : $t("status.none")
                }}</span>
              </div>

              <div
                class="d-flex align-items-center justify-content-center"
                style="cursor: pointer"
              >
                <span
                  >{{ $t("title.options") }} (
                  {{ pcSelection.pcValue ? pcSelection.pcValue : "--" }} )</span
                >
              </div>
            </div>
            <div v-else>
              <!--rebate-->
              <div
                class="d-flex align-items-center justify-content-center mb-5"
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
                  <div
                    v-if="props.currentAccountLevelSetting.allowPips.length > 0"
                    class="dropdownItem"
                    @click="selectPCFn('pips')"
                  >
                    {{ $t("title.pips") }}
                  </div>
                  <div
                    v-if="
                      props.currentAccountLevelSetting.allowCommissions.length >
                      0
                    "
                    class="dropdownItem"
                    @click="selectPCFn('commission')"
                  >
                    {{ $t("title.commission") }}
                  </div>
                </div>
              </div>

              <div
                class="d-flex align-items-center justify-content-center"
                style="cursor: pointer"
                @click="
                  (pcSelection.optionDropdown = !pcSelection.optionDropdown),
                    (pcSelection.pcDropdown = false)
                "
              >
                <span
                  >{{ $t("title.options") }} ( {{ pcSelection.pcValue }} )</span
                >
                <i
                  class="fa-solid fa-angle-down ms-3"
                  style="color: #ffffff"
                ></i>
              </div>

              <!-- dropdown -->
              <div v-if="pcSelection.optionDropdown" style="position: relative">
                <div
                  v-if="pcSelection.selectedPC == 'pips'"
                  class="rebateDropdown"
                >
                  <div
                    v-for="(item, index) in props.currentAccountLevelSetting
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
                    v-for="(item, index) in props.currentAccountLevelSetting
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
          </td>

          <td
            v-if="
              (props.currentAccountLevelSetting.percentage != 0 &&
                (props.currentAccountLevelSetting.pips ||
                  props.currentAccountLevelSetting.commission)) ||
              (props.isRoot &&
                (props.currentAccountLevelSetting.allowPips.length > 0 ||
                  props.currentAccountLevelSetting.allowCommissions.length > 0))
            "
            class="table-title-gray"
          >
            {{ $t("title.addRate") }} {{ $t("title.percentage") }}
          </td>
          <!-- 如果前面有 IB 100% 全拿，後端會返回 percentage = 0 -->
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
              :max="editMode ? editRebateRuleItems[item.cid] : item.total"
            />
          </td>
          <td>
            {{ item.total < item.r ? 0 : calculate(item.total, item.r) }}
          </td>
          <td
            v-if="
              (props.currentAccountLevelSetting.allowPips.length > 0 ||
                props.currentAccountLevelSetting.allowCommissions.length > 0) &&
              props.isRoot
            "
            class="w-150px pe-3"
          >
            <span>
              {{
                pcSelection.schema != undefined
                  ? pcSelection.schema[item.cid]
                  : "-"
              }}
            </span>
          </td>
          <td
            v-if="
              index == 0 &&
              ((props.currentAccountLevelSetting.percentage != 0 &&
                (props.currentAccountLevelSetting.pips ||
                  props.currentAccountLevelSetting.commission)) ||
                (props.isRoot &&
                  (props.currentAccountLevelSetting.allowPips.length > 0 ||
                    props.currentAccountLevelSetting.allowCommissions.length >
                      0)))
            "
            :rowspan="props.productCategory.length"
            style="border-left: 1px solid #f5f5f5"
          >
            <div
              class="d-flex align-items-center justify-content-center p-2"
              style="cursor: pointer"
            >
              <span
                ><el-input-number
                  class="w-60px me-3"
                  type="string"
                  :step="1"
                  :min="0"
                  :max="100"
                  :controls="false"
                  v-model="percentage"
                  :precision="0"
                />
                %</span
              >
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";

import Decimal from "decimal.js";
import LoadingRing from "@/components/LoadingRing.vue";
import { validate } from "vee-validate";
const props = defineProps<{
  isRoot: any;
  productCategory: any;
  defaultLevelSetting: any;
  currentAccountLevelSetting: any; //我能玩啥
  editAccountSchema?: any;
}>();

const accountType = ref("");
const isLoading = ref(true);
const editMode = ref(false);
const formTable = ref([] as any);
const defaultLevelSetting = ref({} as any);
const editRebateRuleItems = ref({} as any);
const percentage = ref(100);
const personCentage = ref(0);
const pcSelection = ref({
  schema: {},
  pcDropdown: false,
  optionDropdown: false,
  selectedPC: "",
  pcValue: null as any,
});

const selectPCFn = (_val: string) => {
  console.log("val", _val);
  console.log("pcSelection", pcSelection);
  pcSelection.value.selectedPC = _val;
  pcSelection.value.pcDropdown = false;
  formTable.value.forEach((item) => {
    item.r = 0;
  });
  if (pcSelection.value.selectedPC == "pips") {
    pcSelection.value.pcValue = props.currentAccountLevelSetting.allowPips[0];
    pcSelection.value.schema =
      defaultLevelSetting.value.allowPipSetting[
        props.currentAccountLevelSetting.allowPips[0]
      ]?.items;
  } else {
    pcSelection.value.pcValue =
      props.currentAccountLevelSetting.allowCommissions[0];
    pcSelection.value.schema =
      defaultLevelSetting.value.allowCommissionSetting[
        props.currentAccountLevelSetting.allowCommissions[0]
      ]?.items;
  }
};

const selectValFn = (_val: number) => {
  if (pcSelection.value.selectedPC == "pips") {
    pcSelection.value.schema =
      defaultLevelSetting.value.allowPipSetting[_val]?.items;
  } else {
    pcSelection.value.schema =
      defaultLevelSetting.value.allowCommissionSetting[_val]?.items;
  }

  pcSelection.value.pcValue = _val;
  pcSelection.value.optionDropdown = false;
};

const findDefaultLevelSettingForCurrentAccount = () => {
  if (
    defaultLevelSetting.value[accountType.value].length > 1 &&
    props.currentAccountLevelSetting.optionName != null
  ) {
    defaultLevelSetting.value = defaultLevelSetting.value[
      accountType.value
    ].find(
      (acc: any) =>
        acc.optionName == props.currentAccountLevelSetting.optionName
    );
  } else {
    defaultLevelSetting.value = defaultLevelSetting.value[accountType.value][0];
  }
};

const setupPipsAndCommission = () => {
  if (editMode.value) {
    percentage.value = props.editAccountSchema.percentage;
    editRebateRuleItems.value = props.editAccountSchema.items.reduce(
      (obj, item) => {
        obj[item.cid] = item.r;
        return obj;
      },
      {}
    );

    if (
      props.editAccountSchema.pips != null &&
      (props.editAccountSchema.commission == 0 ||
        props.editAccountSchema.commission == null)
    ) {
      pcSelection.value.selectedPC = "pips";
      pcSelection.value.pcValue = props.editAccountSchema.pips;
      pcSelection.value.schema =
        defaultLevelSetting.value.allowPipSetting[
          props.editAccountSchema.pips
        ]?.items;
    } else if (
      props.editAccountSchema.commission != null &&
      (props.editAccountSchema.pips == 0 ||
        props.editAccountSchema.pips == null)
    ) {
      pcSelection.value.selectedPC = "commission";
      pcSelection.value.pcValue = props.editAccountSchema.commission;
      pcSelection.value.schema =
        defaultLevelSetting.value.allowCommissionSetting[
          props.editAccountSchema.commission
        ]?.items;
    } else if (
      props.editAccountSchema.pips == 0 &&
      props.editAccountSchema.commission == 0
    ) {
      pcSelection.value.selectedPC = "pips";
      pcSelection.value.pcValue = props.editAccountSchema.pips;
      pcSelection.value.schema =
        defaultLevelSetting.value.allowPipSetting[
          props.editAccountSchema.pips
        ]?.items;
    }
  } else {
    if (props.currentAccountLevelSetting.allowPips.length > 0) {
      pcSelection.value.selectedPC = "pips";
      pcSelection.value.pcValue = props.currentAccountLevelSetting.allowPips[0];
      pcSelection.value.schema =
        defaultLevelSetting.value.allowPipSetting[
          props.currentAccountLevelSetting.allowPips[0]
        ]?.items;
    } else {
      pcSelection.value.selectedPC = "commission";
      pcSelection.value.pcValue =
        props.currentAccountLevelSetting.allowCommissions[0];
      pcSelection.value.schema =
        defaultLevelSetting.value.allowCommissionSetting[
          props.currentAccountLevelSetting.allowCommissions[0]
        ]?.items;
    }
  }
};
watch(personCentage, (newVal, oldVal) => {
  let percentage = newVal;
  if (percentage > 0) {
    formTable.value.forEach((item) => {
      item.r = item.total * (percentage / 100);
      item.r = Number(item.r.toFixed(1));
    });
  }
});
onMounted(async () => {
  isLoading.value = true;
  formTable.value = [];
  editMode.value = props.editAccountSchema != undefined;
  accountType.value = props.currentAccountLevelSetting.accountType;
  defaultLevelSetting.value = JSON.parse(
    JSON.stringify(props.defaultLevelSetting)
  );

  findDefaultLevelSettingForCurrentAccount();
  setupPipsAndCommission();
  props.productCategory.forEach((element) => {
    formTable.value.push({
      cid: element.key,
      name: element.value,
      total:
        props.currentAccountLevelSetting.items[element.key] ??
        defaultLevelSetting.value.category[element.key],
      r: editMode.value ? editRebateRuleItems.value[element.key] : 0,
    });
  });

  isLoading.value = false;
});

const updateTotalRemain = () => {
  formTable.value.forEach((item) => {
    item.total = props.currentAccountLevelSetting.items[item.cid];
  });
};

const calculate = (a, b) => {
  if (b == "" || b == null) {
    b = 0;
  }
  return new Decimal(a).minus(new Decimal(b)).toString();
};

const formCheck = () => {
  return props.currentAccountLevelSetting.selected;
};

const collectData = () => {
  var _p = props.isRoot ? null : props.currentAccountLevelSetting.pips;
  var _c = props.isRoot ? null : props.currentAccountLevelSetting.commission;

  if (pcSelection.value.selectedPC == "pips") {
    _p = pcSelection.value.pcValue == 0 ? null : pcSelection.value.pcValue;
  } else {
    _c = pcSelection.value.pcValue == 0 ? null : pcSelection.value.pcValue;
  }

  var items = [] as any;
  formTable.value.forEach((element) => {
    items.push({
      cid: element.cid,
      r: element.r,
    });
  });

  return {
    optionName: props.currentAccountLevelSetting.optionName,
    accountType: props.currentAccountLevelSetting.accountType,
    pips: _p,
    commission: _c,
    percentage: percentage.value == "" ? "0" : percentage.value,
    items: items,
  };
};

watch(
  () => percentage.value,
  () => {
    if (
      editMode.value &&
      parseInt(props.editAccountSchema.percentage) < parseInt(percentage.value)
    ) {
      percentage.value = props.editAccountSchema.percentage;
    }
  }
);

defineExpose({
  formCheck,
  collectData,
  updateTotalRemain,
});
</script>

<style scoped>
/* 表格容器 */
.rebate-form-wrapper {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

/* 表格基础样式 */
.rebate-table {
  width: 100%;
  min-width: 600px;
  border-collapse: collapse;
  color: #3a3e44;
  font-size: 14px;
}

.rebate-table tbody {
  font-weight: 500;
}

.rebate-table td {
  padding: 12px 8px;
  text-align: center;
  vertical-align: middle;
  border-bottom: 1px dashed #e4e6ef;
}

.table-title-gray {
  background-color: #f8f9fa !important;
  font-weight: 600;
  white-space: nowrap;
}

.table-title-blue {
  color: white !important;
  background-color: #0053ad !important;
  min-width: 140px;
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
  z-index: 100;
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

/* 移动端适配 */
@media (max-width: 768px) {
  .rebate-table {
    font-size: 13px;
  }

  .rebate-table td {
    padding: 10px 6px;
  }

  .table-title-blue {
    min-width: 120px;
  }

  /* 输入框在移动端缩小 */
  .rebate-table :deep(.el-input-number) {
    width: 100px !important;
  }

  .rebate-table :deep(.el-input-number .el-input__inner) {
    padding-left: 8px;
    padding-right: 8px;
  }

  .rebate-table :deep(.el-input-number__decrease),
  .rebate-table :deep(.el-input-number__increase) {
    width: 24px;
  }

  .rebateDropdown {
    width: 150px;
    right: 5px;
  }

  .dropdownItem {
    height: 36px;
    font-size: 13px;
  }
}

/* 小屏幕进一步优化 */
@media (max-width: 480px) {
  .rebate-table {
    font-size: 12px;
  }

  .rebate-table td {
    padding: 8px 4px;
  }

  .rebate-table :deep(.el-input-number) {
    width: 90px !important;
  }

  .table-title-blue {
    min-width: 100px;
  }
}
</style>
