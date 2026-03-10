<template>
  <template
    v-if="
      props.currentAccountRebateRule.allowCommissions.length > 0 ||
      props.currentAccountRebateRule.allowPips.length > 0
    "
  >
    <div class="row">
      <div class="col-6">
        <div class="d-flex align-items-center mt-3">
          <span class="stepContent">{{
            $t("action.choosePipCommission")
          }}</span>
        </div>

        <Field
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcSelection_' + props.currentAccountRebateRule.accountType"
          v-model="pcSelection.selectedPC"
          as="select"
        >
          <option
            v-if="props.currentAccountRebateRule.allowPips.length > 0"
            value="pips"
          >
            {{ $t("title.pips") }}
          </option>
          <option
            v-if="props.currentAccountRebateRule.allowCommissions.length > 0"
            value="commission"
          >
            {{ $t("title.commission") }}
          </option>
        </Field>
      </div>

      <div class="col-6">
        <div class="d-flex align-items-center mt-3">
          <span class="stepContent">{{
            $t("action.choosePipCommissionValue")
          }}</span>
        </div>

        <Field
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="
            'pcValueSelection' + props.currentAccountRebateRule.accountType
          "
          v-model="pcSelection.pcValue"
          as="select"
        >
          <template v-if="pcSelection.selectedPC == 'pips'">
            <option
              v-for="(item, index) in props.currentAccountRebateRule.allowPips"
              :key="index"
              :value="item"
            >
              {{ $t("type.pipCommission.pips." + item) }}
            </option>
          </template>

          <template v-if="pcSelection.selectedPC == 'commission'">
            <option
              v-for="(item, index) in props.currentAccountRebateRule
                .allowCommissions"
              :key="index"
              :value="item"
            >
              {{ $t("type.pipCommission.commission." + item) }}
            </option>
          </template>
        </Field>
      </div>
    </div>
  </template>

  <template v-else>
    <div class="noPcNeed text-center">{{ $t("tip.noPcNeed") }}</div>
  </template>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted } from "vue";
import { Field } from "vee-validate";
import { AccountTypes } from "@/core/types/AccountInfos";

const props = defineProps<{
  currentAccountRebateRule: any;
}>();

const pcSelection = ref({
  selectedPC: "",
  pcValue: null as any,
});

const formCheck = () => {
  return props.currentAccountRebateRule.selected;
};

const collectData = () => {
  var _p = null;
  var _c = null;

  if (pcSelection.value.selectedPC == "pips") {
    _p = pcSelection.value.pcValue;
  } else {
    _c = pcSelection.value.pcValue;
  }

  return {
    optionName: props.currentAccountRebateRule.optionName,
    accountType: props.currentAccountRebateRule.accountType,
    pips: _p,
    commission: _c,
  };
};

onMounted(() => {
  if (props.currentAccountRebateRule.allowPips.length > 0) {
    pcSelection.value.selectedPC = "pips";
    pcSelection.value.pcValue = props.currentAccountRebateRule.allowPips[0];
  } else {
    pcSelection.value.selectedPC = "commission";
    pcSelection.value.pcValue =
      props.currentAccountRebateRule.allowCommissions[0];
  }
  console.log(pcSelection.value.pcValue);
});

watch(
  () => pcSelection.value.selectedPC,
  () => {
    if (pcSelection.value.selectedPC == "pips") {
      pcSelection.value.pcValue = props.currentAccountRebateRule.allowPips[0];
    } else {
      pcSelection.value.pcValue =
        props.currentAccountRebateRule.allowCommissions[0];
    }
  }
);

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

.noPcNeed {
  box-sizing: border-box;
  padding: 5px 15px;
  border-radius: 100px;
  background-color: #ffecec;
  color: #9f005b;
  margin-bottom: 20px;
}
</style>
