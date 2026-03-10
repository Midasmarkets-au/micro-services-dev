<template>
  <div
    v-if="isLoading"
    class="card-body pt-0 overflow-auto"
    style="white-space: nowrap"
  >
    <table class="table align-middle table-row-bordered gy-5">
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
    </table>
  </div>

  <div v-else class="mt-5">
    <div class="row" style="position: relative">
      <!-- Available Accounts -->
      <div
        class="col-11 mb-7"
        :class="isMobile ? 'ms-4' : 'ms-7'"
        v-for="(acc, index) in filledAccountSchema"
        :key="index"
        style="
          border-radius: 10px;
          box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
          padding: 15px;
        "
      >
        <div v-if="acc.selected" class="d-flex justify-content-end">
          <button
            v-for="(option, index) in acc.defaultRebateOptions"
            :key="index"
            class="btn btn-sm btn-light-primary border-0 me-3"
            :class="acc.selectedDefaultRebateOptions == index ? 'active' : ''"
            @click="setAccountRule(acc.accountType, index)"
          >
            {{ option.optionName }}
          </button>
        </div>
        <div class="d-flex">
          <input
            type="checkbox"
            v-model="acc.selected"
            :id="acc.accountType"
            :name="acc.accountType"
            :disabled="acc.defaultSelected"
            class="form-check-input widget-9-check me-3"
          />
          <div class="fs-4 text-center">
            <label for="rebateStdAccount">{{
              $t("type.account." + acc.accountType)
            }}</label>
          </div>
        </div>

        <div v-if="acc.selected">
          <hr />
          <div class="row">
            <div
              class="col-6 col-lg-2 mb-1"
              v-for="(item, index) in filledAccountSchema[acc.accountType]
                .items"
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

          <div v-if="acc.allowPipOptions.length != 0" class="row mt-7 mb-3">
            <label>{{ t("fields.availablePips") }}</label>
            <el-checkbox-group v-model="acc.allowPips" size="large">
              <el-checkbox
                v-for="(p, index) in acc.allowPipOptions"
                class="mt-3"
                :key="'p_' + index"
                :label="p"
                :disabled="acc.defaultAllowPips.includes(p)"
                border
                >{{ t("type.pipOptions." + p) }}
              </el-checkbox>
            </el-checkbox-group>
          </div>
          <div v-if="acc.allowCommissionOptions.length != 0" class="row mb-5">
            <label class="mt-5">{{ t("fields.availableCommission") }}</label>
            <el-checkbox-group v-model="acc.allowCommissions" size="large">
              <el-checkbox
                v-for="(c, index) in acc.allowCommissionOptions"
                class="mt-3"
                :key="'c_' + index"
                :label="c"
                :disabled="acc.defaultAllowCommissions.includes(c)"
                border
                >{{ t("type.commissionOptions." + c) }}</el-checkbox
              >
            </el-checkbox-group>
          </div>
        </div>
      </div>
    </div>
    <div class="d-flex justify-content-center">
      <button
        class="btn btn-primary btn-radius btn-md mb-10 mt-5 w-250px"
        @click="update"
      >
        {{ $t("action.updateRebateRule") }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, onMounted } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../../services/SalesService";
import { processKeysToCamelCase } from "@/core/services/api.client";
import { isMobile } from "@/core/config/WindowConfig";
const { t } = useI18n();
const isLoading = ref(true);
const rebateAgentRuleId = ref(0);
const availableAccounts = ref([] as any);
const filledAccountSchema = ref({} as any);
const editRebateRuleDetail = ref({} as any);
const salesDefaultLevelSetting = ref({} as any);

const props = defineProps<{
  productCategory: any;
  parentUid: number;
  editUid: number;
}>();

const emits = defineEmits<{
  (e: "hide"): void;
}>();

const update = async () => {
  var allowAccountRequest = {} as any;
  allowAccountRequest.schema = [];

  for (const key of Object.keys(filledAccountSchema.value)) {
    if (filledAccountSchema.value[key].selected) {
      allowAccountRequest.schema.push({
        accountType: filledAccountSchema.value[key].accountType,
        optionName: filledAccountSchema.value[key].optionName,
        items: filledAccountSchema.value[key].items,
        allowPips: filledAccountSchema.value[key].allowPips,
        allowCommissions: filledAccountSchema.value[key].allowCommissions,
        pips: null,
        commission: null,
        percentage: 100,
      });
    }
  }

  try {
    await SalesService.putTopIBRebateRule(
      props.editUid,
      rebateAgentRuleId.value,
      allowAccountRequest
    );
    MsgPrompt.success("Update Success").then(() => {
      emits("hide");
    });
  } catch (error) {
    // console.log(error);
    MsgPrompt.error(error);
  }
};

const setAccountRule = async (_accountType: string, _index: number) => {
  const editAccountRebateRule = editRebateRuleDetail?.value[_accountType];
  var _initAccount = filledAccountSchema.value[_accountType];
  var _defaultAccount = salesDefaultLevelSetting.value[_accountType][_index];

  _initAccount["selectedDefaultRebateOptions"] = _index;
  _initAccount["optionName"] = _index == -1 ? null : _defaultAccount.optionName;
  _initAccount["allowPipOptions"] =
    _index == -1
      ? editAccountRebateRule?.allowPips
      : _defaultAccount.allowPipOptions;
  _initAccount["allowCommissionOptions"] =
    _index == -1
      ? editAccountRebateRule?.allowCommissions
      : _defaultAccount.allowCommissionOptions;

  _initAccount["items"] = [];
  props.productCategory.forEach((category) => {
    _initAccount["items"].push({
      cid: category.key,
      r:
        _index == -1
          ? editAccountRebateRule?.items.find(
              (item) => item.cid == category.key
            ).r
          : _defaultAccount.category[category.key] ?? 0,
    });
  });
};

onMounted(async () => {
  isLoading.value = true;
  // availableAccounts.value = store.state.AuthModule.config.accountTypeAvailable;
  availableAccounts.value = await SalesService.getAvailableAccountTypes();

  editRebateRuleDetail.value = await SalesService.getRebateRuleDetailByUid(
    props.editUid
  );
  rebateAgentRuleId.value = editRebateRuleDetail.value.id;
  editRebateRuleDetail.value =
    editRebateRuleDetail.value.levelSetting.allowedAccounts.reduce(
      (obj, item) => {
        obj[item.accountType] = item;
        return obj;
      },
      {}
    );

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

    const editAccountRebateRule = editRebateRuleDetail.value[accountType];

    filledAccountSchema.value[accountType] = {};
    filledAccountSchema.value[accountType]["accountType"] = accountType;

    if (editAccountRebateRule) {
      filledAccountSchema.value[accountType]["selected"] = true;
      filledAccountSchema.value[accountType]["defaultSelected"] = true;
      filledAccountSchema.value[accountType]["allowPips"] =
        editAccountRebateRule.allowPips;
      filledAccountSchema.value[accountType]["allowCommissions"] =
        editAccountRebateRule.allowCommissions;

      let index = salesDefaultLevelSetting.value[accountType].findIndex(
        (item) => item.optionName === editAccountRebateRule?.optionName
      );
      setAccountRule(accountType, index);
    } else {
      filledAccountSchema.value[accountType]["selected"] = false;
      filledAccountSchema.value[accountType]["defaultSelected"] = false;
      filledAccountSchema.value[accountType]["allowPips"] = [];
      filledAccountSchema.value[accountType]["allowCommissions"] = [];
      setAccountRule(accountType, 0);
    }

    filledAccountSchema.value[accountType]["defaultAllowPips"] =
      filledAccountSchema.value[accountType]["allowPips"];
    filledAccountSchema.value[accountType]["defaultAllowCommissions"] =
      filledAccountSchema.value[accountType]["allowCommissions"];
  });
  isLoading.value = false;
});
</script>
