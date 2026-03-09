<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="d-flex align-items-end gap-4">
          <div>
            <el-select
              v-model="criteria.adjustType"
              clearable
              placeholder="Type"
            >
              <el-option
                v-for="item in creditTypes"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>
          <div>
            <el-button @click="fetchData(1)" type="primary">
              {{ $t("action.search") }}
            </el-button>
          </div>
          <div>
            <el-button @click="reset" type="info" plain>
              {{ $t("action.reset") }}
            </el-button>
          </div>
        </div>
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="openChangeCreditPanel">
          {{ $t("action.changeCredit") }}
        </el-button>
        <el-button
          v-if="$can('TenantAdmin')"
          type="success"
          @click="openChangeAdjustPanel"
        >
          {{ $t("action.changeAdjust") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <td>{{ $t("fields.type") }}</td>
            <td>{{ $t("fields.ticket") }}</td>
            <td>{{ $t("fields.staff") }}</td>
            <td>{{ $t("fields.createdOn") }}</td>
            <td>{{ $t("fields.amount") }}</td>
            <td>{{ $t("fields.remark") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>
              {{ item.data.adjustType === "Credit" ? "Credit" : "Adjust" }}
            </td>

            <td>{{ item.data.ticket }}</td>
            <td>
              {{
                (item.user.firstName + " " + item.user.lastName).trim() === ""
                  ? item.user.nativeName
                  : item.user.firstName + " " + item.user.lastName
              }}
            </td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <BalanceShow
                :balance="item.data.amount * 100"
                :currency-id="item.account.currencyId"
              />
            </td>
            <td>{{ item.data.comment }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <TableFooter @page-change="fetchData" :criteria="criteria" />
  </div>
  <ChangeCreditForm
    ref="changeCreditFormRef"
    @event-submit="onEventSubmitted"
  />
  <ChangeAdjustForm
    ref="changeAdjustFormRef"
    @event-submit="onEventSubmitted"
  />
</template>
<script lang="ts" setup>
import { ref, inject, onMounted } from "vue";
import AccountService from "../services/AccountService";
import ChangeCreditForm from "@/projects/tenant/modules/Payment/components/ChangeCreditForm.vue";
import ChangeAdjustForm from "@/projects/tenant/modules/Payment/components/ChangeAdjustForm.vue";
import { AuditTypes } from "@/core/types/AuditTypes";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const changeCreditFormRef = ref<InstanceType<typeof ChangeCreditForm>>();
const changeAdjustFormRef = ref<InstanceType<typeof ChangeAdjustForm>>();

const isLoading = ref(false);
const data = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 12,
  type: AuditTypes.TradeAccountBalance,
  accountNumber: accountDetails.value.accountNumber,
});

const creditTypes = [
  { label: "Credit", value: "Credit" },
  { label: "Adjust", value: "Balance" },
];

const reset = async () => {
  criteria.value.page = 1;
  criteria.value.size = 12;
  criteria.value.type = AuditTypes.TradeAccountBalance;
  criteria.value.accountNumber = accountDetails.value.accountNumber;
  criteria.value.adjustType = "";
  criteria.value.rowId = null;
  fetchData(1);
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  try {
    const audit = await AccountService.getCreditList(criteria.value);
    criteria.value = audit.criteria;
    data.value = audit.data;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const onEventSubmitted = () => {
  fetchData(1);
};
const openChangeCreditPanel = () => {
  changeCreditFormRef.value?.show(
    accountDetails.value.accountNumber,
    "hideLoginNo"
  );
};

const openChangeAdjustPanel = () => {
  changeAdjustFormRef.value?.show(
    accountDetails.value.accountNumber,
    "hideLoginNo"
  );
};
onMounted(() => {
  fetchData(1);
});
</script>
