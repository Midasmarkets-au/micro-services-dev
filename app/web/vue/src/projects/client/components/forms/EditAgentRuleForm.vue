<template>
  <div
    v-if="isLoading"
    class="card-body pt-0 overflow-auto"
    style="white-space: nowrap"
  >
    <table class="table align-middle table-row-bordered fs-6 gy-5">
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
    </table>
  </div>

  <div v-else class="mt-15 ms-5">
    <div class="d-flex">
      <div v-for="(account, index) in currentAccountLevelSetting" :key="index">
        <input
          type="checkbox"
          v-model="account.selected"
          :name="'rebateAccount' + account.accountType"
          :disabled="account.defaultSelected"
          class="form-check-input widget-9-check me-3"
        />
        <label class="me-9" for="rebateStdAccount">{{
          $t("type.account." + account.accountType)
        }}</label>
      </div>
    </div>

    <div v-for="(account, index) in currentAccountLevelSetting" :key="index">
      <div v-if="account.selected">
        <div
          class="d-flex align-items-center justify-content-between mt-9 mb-3"
          style="cursor: pointer"
        >
          <div class="d-flex align-items-center">
            <div
              class="vertical-line"
              style="
                border-left: 3px solid #800020;
                height: 16px;
                margin-right: 15px;
              "
            ></div>
            <div class="fw-500 fs-4">
              {{ $t("type.account." + account.accountType) }}
            </div>
          </div>
        </div>

        <BaseRebateForm
          ref="BaseRebateFormRef"
          :productCategory="productCategory"
          :isRoot="parentRebateRuleDetail.isRoot"
          :currentAccountLevelSetting="account"
          :defaultLevelSetting="defaultLevelSetting"
          :editAccountSchema="editRebateRuleDetail[account.accountType]"
        />
      </div>
    </div>

    <div class="d-flex justify-content-center">
      <button
        class="btn btn-primary btn-md mb-10 btn-radius mt-5 w-250px"
        @click="update"
      >
        {{ $t("action.updateRebateRule") }}
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import MsgPrompt from "@/core/plugins/MsgPrompt";
import LoadingRing from "@/components/LoadingRing.vue";
import BaseRebateForm from "@/projects/client/modules/ib/components/form/BaseRebateForm.vue";

import { ref, onMounted } from "vue";
import { processKeysToCamelCase } from "@/core/services/api.client";

const props = defineProps<{
  productCategory: any;
  parentUid: any;
  editUid: any;
  service: any;
}>();

const emits = defineEmits<{
  (e: "hide"): void;
}>();

const isLoading = ref(true);
const rebateAgentRuleId = ref(0);
const defaultLevelSetting = ref({} as any);
const editRebateRuleDetail = ref({} as any);
const parentRebateRuleDetail = ref({} as any);
const currentAccountLevelSetting = ref({} as any);
const BaseRebateFormRef = ref<InstanceType<typeof BaseRebateForm>>();

const update = async () => {
  const allowAccountRequest = ref([] as any);
  BaseRebateFormRef.value?.forEach(function (formRef) {
    if (formRef.formCheck()) {
      allowAccountRequest.value.push(formRef.collectData());
    }
  });

  try {
    await props.service.putIBRebateRule(
      props.parentUid,
      rebateAgentRuleId.value,
      allowAccountRequest.value
    );
    MsgPrompt.success("Update Success").then(() => {
      emits("hide");
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  isLoading.value = true;

  try {
    [
      defaultLevelSetting.value,
      editRebateRuleDetail.value,
      parentRebateRuleDetail.value,
    ] = await Promise.all([
      props.service
        .getAccountDefaultLevelSetting(props.parentUid)
        .then((result) => {
          return processKeysToCamelCase(result);
        }),
      props.service.getRebateRuleDetailByUid(props.editUid).then((result) => {
        rebateAgentRuleId.value = result.id;
        return result.schema.reduce((obj, item) => {
          obj[item.accountType] = item;
          return obj;
        }, {});
      }),
      props.service.getRebateRuleDetailByUid(props.parentUid),
    ]);

    parentRebateRuleDetail.value.calculatedLevelSetting.allowedAccounts.forEach(
      (account: any) => {
        let currentAccount = (currentAccountLevelSetting.value[
          account.accountType
        ] = {} as any);

        if (editRebateRuleDetail.value[account.accountType]) {
          currentAccount.selected = true;
          currentAccount.defaultSelected = true;
        } else {
          currentAccount.selected = false;
          currentAccount.defaultSelected = false;
        }
        currentAccount.optionName = account.optionName;
        currentAccount.accountType = account.accountType;
        currentAccount.percentage =
          editRebateRuleDetail.value[account.accountType]?.percentage ?? 100;
        currentAccount.allowPips = account.allowPips;
        currentAccount.allowCommissions = account.allowCommissions;
        currentAccount.pips =
          editRebateRuleDetail.value[account.accountType]?.pips ?? null;
        currentAccount.commission =
          editRebateRuleDetail.value[account.accountType]?.commission ?? null;

        currentAccount.items = {};
        account.items.forEach((item: any) => {
          currentAccount.items[item.cid] = item.r;
        });
      }
    );
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
});
</script>
