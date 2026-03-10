<template>
  <div class="card">
    <div class="card-header">
      <h3 class="card-title">
        {{ $t("fields.existingAccounts") }}
      </h3>
    </div>

    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">id</th>
            <th class="">{{ $t("fields.accountNumber") }}</th>
            <th class="">{{ $t("fields.uid") }}</th>
            <th class="">{{ $t("fields.accountRole") }}</th>
            <th class="">{{ $t("fields.salesAccount") }}</th>
            <th class="">{{ $t("fields.agentAccount") }}</th>
            <th class="">{{ $t("fields.createdOn") }}</th>
            <th class="text-center min-w-150px">
              {{ $t("action.action") }}
            </th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && userAccounts.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in userAccounts" :key="index">
            <td>{{ item.id }}</td>
            <td>
              {{ item.tradeAccount?.accountNumber ?? "***" }}
            </td>
            <td>
              {{ item.uid ?? "***" }}
            </td>
            <td>{{ $t(`type.accountRole.${item.role}`) }}</td>

            <td>
              {{ item.salesAccount?.user?.displayName }}
            </td>

            <td>
              {{ item.agentAccount?.user?.displayName }}
            </td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" />
            </td>

            <td class="text-center">
              <el-dropdown
                class="me-3"
                trigger="click"
                v-if="!item.hasTradeAccount"
              >
                <el-button type="primary">
                  {{ $t("action.openAccountActions") }}
                  <el-icon class="el-icon--right"><arrow-down /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="openMT4Account(item.id)">
                      {{ $t("title.createTradeAccount") }}</el-dropdown-item
                    >
                    <el-dropdown-item
                      @click="openOpenProcedureForm(item.id)"
                      disabled
                    >
                      {{ $t("tip.accountOpenProcedure") }}
                    </el-dropdown-item>
                    <el-dropdown-item disabled>
                      {{ $t("tip.welcomeLetter") }}
                    </el-dropdown-item>
                    <el-dropdown-item disabled>{{
                      $t("tip.readOnlyNotice")
                    }}</el-dropdown-item>
                    <el-dropdown-item disabled>{{
                      $t("tip.kycForm")
                    }}</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>

              <el-button v-else @click="showAccountDetail(item.id)">
                {{ $t("action.details") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="accountCriteria" />
      <!-- <AccountCreate
        ref="accountCreateRef"
        @account-submitted="mt4AccountSubmitted"
      /> -->
      <div>
        <el-divider />
        <h3 class="card-title" style="font-weight: 500">
          {{ $t("title.demoAccounts") }}
        </h3>
        <table class="table align-middle table-row-dashed fs-6 gy-5">
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">id</th>
              <th>{{ $t("fields.accountNo") }}</th>
              <th>{{ $t("fields.platform") }}</th>
              <th class="">{{ $t("fields.type") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.balance") }}</th>
              <th class="">{{ $t("fields.leverage") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="">{{ $t("fields.expireDate") }}</th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && demoAccounts.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in demoAccounts" :key="index">
              <td>{{ item.id }}</td>
              <td>{{ item.accountNumber }}</td>
              <td>{{ ServiceToPlatform[item.serviceId] }}</td>
              <td>
                {{ item?.type ? $t(`type.account.${item?.type}`) : "***" }}
              </td>
              <td>
                {{
                  item?.currencyId
                    ? $t(`type.currency.${item?.currencyId}`)
                    : "***"
                }}
              </td>
              <td><BalanceShow :balance="item.balance * 100" /></td>

              <td>
                {{ item?.leverage ? item?.leverage + ":1" : "***" }}
              </td>
              <td>
                <TimeShow :date-iso-string="item.createdOn" />
              </td>
              <td>
                <TimeShow :date-iso-string="item.expireOn" />
              </td>
              <td class="text-center">
                <el-button
                  type="danger"
                  @click="openConfirm(item)"
                  :disabled="isLoading"
                >
                  {{ $t("action.delete") }}
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="fetchDataDemo" :criteria="demoCriteria" />
      </div>
      <OpenProcedureForm
        ref="openProcedureFormRef"
        @step-finished="getUserAccounts(1)"
      />
    </div>
  </div>
  <div>
    <!-- <AccountApplicationForm
      v-for="(item, index) in pendingApplications"
      :key="index"
      :pending-application="item"
      @application-submit="onApplicationSubmit"
    />
    <div class="separator border-gray-200 my-5"></div> -->
    <AccountDetail ref="accountDetailRef" />
  </div>
</template>

<script setup lang="ts">
import TableFooter from "@/components/TableFooter.vue";
import { ref, onMounted } from "vue";
import { ArrowDown } from "@element-plus/icons-vue";
import UserService from "../services/UserService";
import { ServiceToPlatform } from "@/core/types/ServiceTypes";
import OpenProcedureForm from "../../accounts/components/OpenProcedureForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

import AccountDetail from "@/projects/tenant/modules/accounts/components/AccountDetail.vue";
import { ElNotification, ElMessageBox } from "element-plus";
import i18n from "@/core/plugins/i18n";
import { is } from "@vee-validate/rules";

const t = i18n.global.t;
const accountDetailRef = ref<InstanceType<typeof AccountDetail>>();
const criteria = ref({
  size: 100,
});

const props = defineProps<{
  partyId: number;
}>();
const isLoading = ref(true);
const userAccounts = ref(Array<any>());
const demoAccounts = ref(Array<any>());
const openProcedureFormRef = ref<InstanceType<typeof OpenProcedureForm>>();
const accountCriteria = ref({
  partyId: props.partyId,
  page: 1,
  size: 10,
} as any);
const demoCriteria = ref({
  page: 1,
  size: 10,
  partyId: props.partyId,
});
const procedureFormRef = ref<InstanceType<typeof OpenProcedureForm>>();

onMounted(async () => {
  isLoading.value = true;
  await getUserAccounts(accountCriteria.value.page);
  await fetchDataDemo(1);
  isLoading.value = false;
});

const getUserAccounts = async (selectedPage: number) => {
  isLoading.value = true;
  try {
    accountCriteria.value.page = selectedPage;
    const res = await UserService.queryUserAccounts(accountCriteria.value);
    accountCriteria.value = res.criteria;
    userAccounts.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const fetchDataDemo = async (_page: number) => {
  demoCriteria.value.page = _page;
  try {
    const res = await UserService.queryDemoAccounts(demoCriteria.value);
    demoAccounts.value = res.data;
    demoCriteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
};
const openConfirm = async (_item: any) => {
  ElMessageBox.confirm(t("tip.confirmPrompt"), {
    confirmButtonText: t("tip.confirm"),
    cancelButtonText: t("action.cancel"),
    type: "warning",
  }).then(() => {
    deleteDemoAccount(_item);
  });
};
const deleteDemoAccount = async (_item: any) => {
  isLoading.value = true;
  try {
    await UserService.deleteDemoAccount(_item.id);
    ElNotification({
      title: t("status.success"),
      type: "success",
    });
    fetchDataDemo(1);
  } catch (error) {
    ElNotification({
      message: t("tip.unexpectedError"),
      type: "error",
    });
  }
  isLoading.value = false;
};
const pageChange = (page: number) => {
  getUserAccounts(page);
};

const openMT4Account = async (accountId: number) => {
  const res = await UserService.queryApplications({
    referenceId: accountId,
  });
  if (res.data.length <= 0) {
    MsgPrompt.error("未找到相应的申请记录");
    return;
  }
  const applicationDetails = res.data[0];
  openProcedureFormRef.value?.show(2, { applicationDetails });
};

const showAccountDetail = (id: number) => {
  accountDetailRef.value?.show(id, "infos", [] as any);
};
const openOpenProcedureForm = (accountId: number) => {
  procedureFormRef.value?.show(accountId);
};
</script>

<style scoped>
.el-dropdown-link {
  cursor: pointer;
  color: var(--el-color-primary);
  display: flex;
  align-items: center;
}
</style>
