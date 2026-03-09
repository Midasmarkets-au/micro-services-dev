<template>
  <div class="mt-5">
    <div class="row" style="position: relative">
      <!-- Available Accounts -->
      <div
        class="col-11 ms-7 mb-7"
        v-for="(item, index) in initAccountSchema"
        :key="index"
        style="
          border-radius: 10px;
          box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
          padding: 15px;
        "
      >
        <div class="d-flex">
          <input
            :id="item.accountType"
            class="form-check-input widget-9-check me-3"
            type="checkbox"
            :name="item.accountType"
            v-model="item.selected"
            :disabled="item.defaultSelected"
          />
          <div class="fs-4 text-center">
            <label for="rebateStdAccount">{{
              $t("type.account." + item.accountType)
            }}</label>
          </div>
        </div>

        <div v-if="item.selected">
          <hr />
          <div class="row">
            <div
              class="col-2 mb-1"
              v-for="(item, index) in initAccountSchema[item.accountType].items"
              :key="index"
            >
              <div>
                {{ $t("type.clientSymbolCategory." + item.cid) }}
              </div>
              <el-input
                class="w-100% h-35px mt-1 mb-1"
                v-model="item.r"
                disabled
              />
            </div>
          </div>

          <div v-if="item.allowPipOptions.length != 0" class="row mt-7 mb-3">
            <label>{{ t("fields.availablePips") }}</label>
            <el-checkbox-group v-model="item.allowPips" size="large">
              <el-checkbox
                v-for="(p, index) in item.allowPipOptions"
                :key="'p_' + index"
                :label="p"
                border
                class="mt-3"
                :disabled="item.originalAllowPips.includes(p)"
              />
            </el-checkbox-group>
          </div>
          <div v-if="item.allowCommissionOptions.length != 0" class="row mb-5">
            <label class="mt-5">{{ t("fields.availableCommission") }}</label>
            <el-checkbox-group v-model="item.allowCommissions" size="large">
              <el-checkbox
                v-for="(c, index) in item.allowCommissionOptions"
                :key="'c_' + index"
                :label="c"
                border
                class="mt-3"
                :disabled="item.originalAllowCommissions.includes(c)"
              />
            </el-checkbox-group>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../../services/SalesService";
import { ref, onMounted, computed, watch } from "vue";
import { processKeysToCamelCase } from "@/core/services/api.client";

const store = useStore();
const { t, locale } = useI18n();
const initAccountSchema = ref({});
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const availableAccounts = ref({} as any);
const defaultLevelSetting = ref({});

const props = defineProps<{
  productCategory: any;
  rebateRule: any;
}>();

const emits = defineEmits<{
  (e: "hide"): void;
}>();

const update = async () => {
  const allowAccountRequest = ref({} as any);
  allowAccountRequest.value.schema = [];
  for (const key of Object.keys(initAccountSchema.value)) {
    if (initAccountSchema.value[key].selected) {
      var convertPips = initAccountSchema.value[key].allowPips;
      var convertCommissions = initAccountSchema.value[key].allowCommissions;
      allowAccountRequest.value.schema.push({
        accountType: initAccountSchema.value[key].accountType,
        items: initAccountSchema.value[key].items,
        allowPips: convertPips[0] == null ? [] : convertPips,
        allowCommissions:
          convertCommissions[0] == null ? [] : convertCommissions,
        pips: 0,
        commission: 0,
        percentage: 0,
      });
    }
  }

  try {
    await SalesService.putTopIBRebateRule(
      props.rebateRule.agentAccountUid,
      props.rebateRule.id,
      allowAccountRequest.value
    );
    MsgPrompt.success("Update Success").then(() => {
      emits("hide");
    });
  } catch (error) {
    // console.log(error);
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  availableAccounts.value = store.state.AuthModule.config.accountTypeAvailable;

  try {
    defaultLevelSetting.value = await SalesService.getDefaultLevelSetting();
    defaultLevelSetting.value = processKeysToCamelCase(
      defaultLevelSetting.value
    );
  } catch (error) {
    MsgPrompt.error(error);
  }

  availableAccounts.value.forEach((accountType) => {
    if (defaultLevelSetting.value[accountType] === undefined) {
      return;
    }

    const defaultAccountSetting = defaultLevelSetting.value[accountType][0];

    initAccountSchema.value[accountType] = {};
    initAccountSchema.value[accountType]["items"] = [];
    initAccountSchema.value[accountType]["accountType"] = accountType;
    initAccountSchema.value[accountType]["allowPipOptions"] =
      defaultAccountSetting.allowPipOptions;
    initAccountSchema.value[accountType]["allowCommissionOptions"] =
      defaultAccountSetting.allowCommissionOptions;

    if (props.rebateRule.schema[accountType]) {
      initAccountSchema.value[accountType]["selected"] = true;
      initAccountSchema.value[accountType]["defaultSelected"] = true;

      initAccountSchema.value[accountType]["allowPips"] =
        props.rebateRule.calculatedLevelSetting.allowedAccounts.find(
          (item) => item.accountType === accountType
        ).allowPips;
      initAccountSchema.value[accountType]["allowCommissions"] =
        props.rebateRule.calculatedLevelSetting.allowedAccounts.find(
          (item) => item.accountType === accountType
        ).allowCommissions;
    } else {
      initAccountSchema.value[accountType]["selected"] = false;
      initAccountSchema.value[accountType]["defaultSelected"] = false;
      initAccountSchema.value[accountType]["allowPips"] = [];
      initAccountSchema.value[accountType]["allowCommissions"] = [];
    }

    initAccountSchema.value[accountType]["originalAllowPips"] =
      initAccountSchema.value[accountType]["allowPips"];
    initAccountSchema.value[accountType]["originalAllowCommissions"] =
      initAccountSchema.value[accountType]["allowCommissions"];

    props.rebateRule.calculatedLevelSetting.allowedAccounts.find(
      (item) => item.accountType === accountType
    );

    let setting = props.rebateRule.calculatedLevelSetting.allowedAccounts.find(
      (item) => item.accountType === accountType
    );

    if (setting === undefined) {
      setting = defaultLevelSetting.value[accountType][0];
    }

    props.productCategory.forEach((category) => {
      if (setting.items != undefined) {
        initAccountSchema.value[accountType]["items"].push({
          cid: category.key,
          r: setting.items.find((item) => item.cid === category.key).r ?? 0,
        });
      } else {
        initAccountSchema.value[accountType]["items"].push({
          cid: category.key,
          r: setting.category[category.key] ?? 0,
        });
      }
    });
  });

  // console.log(initAccountSchema.value);
});

defineExpose({
  update,
});
</script>
