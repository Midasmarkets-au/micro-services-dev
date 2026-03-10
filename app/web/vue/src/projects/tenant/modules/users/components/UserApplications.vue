<template>
  <div>
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">
          {{ $t("title.tradeAccount") }}
        </h3>
        <div class="card-toolbar">
          <el-button @click="openAccounts()" :disabled="isLoading">
            {{ $t("action.createTradeAccount") }}
          </el-button>
        </div>
      </div>
      <!-- <div class="border border-secondary border-top-0 p-5 px-20 rounded"> -->
      <div class="card-body">
        <!-- <AccountApplicationForm
        v-for="(item, index) in pendingApplications"
        :key="index"
        :pending-application="item"
      /> -->

        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">id</th>
              <th class="">{{ $t("fields.fullName") }}</th>
              <th class="">{{ $t("fields.type") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.leverage") }}</th>
              <th class="">{{ $t("fields.accountRole") }}</th>
              <th class="">{{ $t("fields.referralCode") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="text-center min-w-150px">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && pendingApplications.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in pendingApplications" :key="index">
              <!--  -->
              <td>{{ item.id }}</td>
              <td>{{ item.user?.firstName + " " + item.user?.lastName }}</td>
              <td>
                {{
                  item.supplement?.accountType
                    ? $t(`type.account.${item.supplement?.accountType}`)
                    : "***"
                }}
              </td>
              <td>
                {{
                  item.supplement?.currencyId
                    ? $t(`type.currency.${item.supplement?.currencyId}`)
                    : "***"
                }}
              </td>

              <td>
                {{
                  item.supplement?.leverage
                    ? item.supplement?.leverage + ":1"
                    : "***"
                }}
              </td>

              <td>
                {{
                  item.supplement?.role
                    ? $t(`type.accountRole.${item.supplement?.role}`)
                    : "***"
                }}
              </td>
              <td>
                {{
                  item.supplement?.referCode === ""
                    ? "***"
                    : item.supplement?.referCode
                }}
              </td>
              <td>
                <span
                  class="badge"
                  :class="{
                    'badge-primary':
                      item.status === ApplicationStatusType.Approved,
                    'badge-warning':
                      item.status === ApplicationStatusType.AwaitingApproval,
                    'badge-danger':
                      item.status === ApplicationStatusType.Rejected,
                    'badge-success':
                      item.status === ApplicationStatusType.Completed,
                  }"
                  >{{ $t(`type.applicationStatus.${item.status}`) }}</span
                >
              </td>
              <td>
                <TimeShow :date-iso-string="item.createdOn" />
              </td>

              <td class="text-center">
                <el-button @click="openApplicationDetailForm(item)">
                  {{ $t("action.details") }}
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
    <SimpleForm
      ref="modelRef"
      disable-footer
      :is-loading="isLoading"
      :title="$t('title.pendingApplications')"
      :width="900"
      style="overflow: hidden"
    >
      <div class="w-100 overflow-hidden">
        <OpenAccountForm
          @application-submitted="onApplicationApproved"
          :siteId="selectedApplication.siteId"
        />
      </div>
    </SimpleForm>
  </div>
  <OpenTradeAccount
    ref="openTradeAccountRef"
    @application-submitted="fetchData(1)"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, provide } from "vue";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";
import UserService from "../services/UserService";
import { ApplicationType } from "@/core/types/ApplicationInfos";
import SimpleForm from "@/components/SimpleForm.vue";
import OpenAccountForm from "@/projects/tenant/modules/accounts/components/form/OpenAccountForm.vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import OpenTradeAccount from "./modal/OpenTradeAccount.vue";
const props = defineProps<{
  partyId: number;
}>();

const emits = defineEmits<{
  (e: "application-approved"): void;
}>();

const isLoading = ref(true);
const pendingApplications = ref(Array<any>());
const criteria = ref({
  page: 1,
  size: 10,
  partyId: props.partyId,
  type: ApplicationType.TradeAccount,
  // status: ApplicationStatusType.AwaitingApproval,
  sortField: "Status",
  sortFlag: false,
});
const openTradeAccountRef = ref(null);
const selectedApplication = ref<any>();

provide(AccountInjectionKeys.APPLICATION_DETAILS, selectedApplication);

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  try {
    const res = await UserService.queryApplications(criteria.value);
    pendingApplications.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const modelRef = ref<InstanceType<typeof SimpleForm>>();
const openAccounts = () => {
  openTradeAccountRef.value?.show(props.partyId);
};
const openApplicationDetailForm = (_item: any) => {
  selectedApplication.value = _item;
  modelRef.value?.show();
};

const onApplicationApproved = async () => {
  modelRef.value?.hide();
  emits("application-approved");
};

onMounted(async () => {
  await fetchData(1);
});
</script>
