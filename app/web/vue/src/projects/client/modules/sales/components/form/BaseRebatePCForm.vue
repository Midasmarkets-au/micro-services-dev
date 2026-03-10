<template>
  <template
    v-if="
      props.accountRule.allowPipOptions.length != 0 ||
      props.accountRule.allowCommissionOptions.length != 0 ||
      props.accountRule.defaultRebateOptions.length > 1
    "
  >
    <div class="row">
      <div class="col-12 col-md-4">
        <div class="d-flex align-items-center mt-3">
          <span class="stepContent">{{ $t("action.rateOption") }}</span>
        </div>

        <Field
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcSelection_rateOption' + props.accountRule.accountType"
          v-model="pcSelection.rateOption"
          as="select"
          @change="initPcValue()"
        >
          <option
            v-for="(item, index) in props.accountRule.defaultRebateOptions"
            :key="index"
            :value="index"
          >
            {{ item.optionName }}
          </option>
        </Field>
      </div>

      <div class="col-12 col-md-4">
        <div class="d-flex align-items-center mt-3">
          <span class="stepContent">{{
            $t("action.choosePipCommission")
          }}</span>
        </div>

        <Field
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcSelection_' + props.accountRule.accountType"
          v-model="pcSelection.selectedPC"
          as="select"
        >
          <option value="pips">
            {{ $t("title.pips") }}
          </option>
          <option value="commission">
            {{ $t("title.commission") }}
          </option>
        </Field>
      </div>

      <div class="col-12 col-md-4">
        <div class="d-flex align-items-center mt-3">
          <span class="stepContent">{{
            $t("action.choosePipCommissionValue")
          }}</span>
        </div>

        <Field
          v-if="
            pcSelection.selectedPC == 'pips' &&
            props.accountRule.allowPipOptions.length != 0
          "
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcValueSelection' + props.accountRule.accountType"
          v-model="pcSelection.pcValue"
          as="select"
        >
          <option
            v-for="(item, index) in props.accountRule.allowPipOptions"
            :key="index"
            :value="item"
          >
            {{ t("type.pipOptions." + item) }}
          </option>
        </Field>

        <Field
          v-else-if="
            pcSelection.selectedPC == 'commission' &&
            props.accountRule.allowCommissionOptions.length != 0
          "
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcValueSelection' + props.accountRule.accountType"
          v-model="pcSelection.pcValue"
          as="select"
        >
          <option
            v-for="(item, index) in props.accountRule.allowCommissionOptions"
            :key="index"
            :value="item"
          >
            {{ t("type.commissionOptions." + item) }}
          </option>
        </Field>

        <Field
          v-else
          class="form-select form-select-solid mt-1 IB-rebate-link-select"
          :name="'pcValueSelection' + props.accountRule.accountType"
          v-model="pcSelection.pcValue"
          as="select"
        >
          <option key="index">
            {{ t("tip.noDataAvailable") }}
          </option>
        </Field>
      </div>
    </div>
  </template>

  <template v-else>
    <div class="noPcNeed text-center">{{ $t("tip.noPcNeed") }}</div>
  </template>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import { Field } from "vee-validate";
import { ref, watch, onMounted } from "vue";

const props = defineProps<{
  accountRule: any;
}>();

const emit = defineEmits(["setAccountRule"]);

const { t } = useI18n();
const pcSelection = ref({} as any);

const initPcValue = () => {
  emit(
    "setAccountRule",
    props.accountRule.accountType,
    pcSelection.value.rateOption
  );
};

const formCheck = () => {
  return props.accountRule.selected;
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
    optionName:
      props.accountRule.defaultRebateOptions[pcSelection.value.rateOption]
        .optionName,
    accountType: props.accountRule.accountType,
    pips: _p == null ? _p : Number(_p),
    commission: _c == null ? _c : Number(_c),
  };
};

onMounted(() => {
  pcSelection.value.rateOption = 0;
  pcSelection.value.selectedPC = "pips";
});

watch(
  () => pcSelection.value.selectedPC,
  () => {
    if (pcSelection.value.selectedPC == "pips") {
      pcSelection.value.pcValue =
        props.accountRule.allowPipOptions[0]?.toString() ?? null;
    } else {
      pcSelection.value.pcValue =
        props.accountRule.allowCommissionOptions[0]?.toString() ?? null;
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

.IB-rebate-link-select {
  padding: 16px 20px;
  width: 300px;
  height: 56px;
}
</style>
