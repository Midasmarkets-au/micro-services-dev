<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: criteria.status === TabStatus.Pending },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        href="#"
        data-bs-toggle="tab"
        @click="changeTab(TabStatus.Pending)"
        >{{ $t("status.pending") }}</a
      >
    </li>

    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: criteria.status === TabStatus.Approved },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        href="#"
        data-bs-toggle="tab"
        @click="changeTab(TabStatus.Approved)"
        >{{ $t("status.approved") }}</a
      >
    </li>

    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: criteria.status === TabStatus.Rejected },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        href="#"
        data-bs-toggle="tab"
        @click="changeTab(TabStatus.Rejected)"
        >{{ $t("status.rejected") }}</a
      >
    </li>

    <li class="nav-item">
      <a
        :class="[
          'nav-link text-active-primary pb-4',
          { active: criteria.status === TabStatus.Completed },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        href="#"
        data-bs-toggle="tab"
        @click="changeTab(TabStatus.Completed)"
        >{{ $t("status.completed") }}</a
      >
    </li>
  </ul>
  <div class="card mb-5">
    <div class="card-body py-4">
      <div class="table-responsive">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.client") }}</th>
              <th class="">
                <div v-if="route.params.type == 'referral'">
                  {{ $t("fields.referral") }}
                </div>
                {{ $t("fields.accountNo") }}
              </th>
              <th class="min-w-60px">
                <!--              <span>-->
                <!--                <el-select-->
                <!--                  v-model="activityType"-->
                <!--                  class="m-2"-->
                <!--                  :placeholder="$t('fields.type')"-->
                <!--                  size="small"-->
                <!--                  @change="activityTypeChange"-->
                <!--                >-->
                <!--                  <el-option :label="$t('fields.all')" value="" />-->
                <!--                  <el-option-->
                <!--                    v-for="item in applicationTypeSelections"-->
                <!--                    :key="item.value"-->
                <!--                    :label="item.label"-->
                <!--                    :value="item.value"-->
                <!--                  />-->
                <!--                </el-select>-->
                <!--              </span>-->
                {{ $t("fields.type") }}
              </th>
              <th class="">
                {{ $t("fields.createdOn") }}
              </th>

              <!-- <th
              v-if="
                route.params.type == 'wholesale' &&
                criteria.status == TabStatus.Rejected
              "
            >
              Reset By
            </th> -->

              <th class="">{{ $t("action.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && applications.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in applications" :key="index">
              <td class="d-flex align-items-center">
                <UserInfo :user="item.user" class="me-2" />
              </td>
              <td class="">
                <a href="#" @click="showAccount(item.supplement)">{{
                  item.supplement?.accountNumber ??
                  item.supplement?.AccountNumber ??
                  "View Details"
                }}</a>
              </td>
              <td class="">{{ $t(`type.application.${item.type}`) }}</td>
              <td class="">
                <TimeShow :date-iso-string="item.createdOn" />
              </td>
              <!-- <td class="">--</td> -->

              <td class="">
                <el-button class="" @click="showApplicationDetails(item)">
                  {{ $t("action.viewDetails") }}
                </el-button>

                <el-button
                  @click="resetWholesaleApplication(item.partyId)"
                  v-if="
                    route.params.type == 'wholesale' &&
                    criteria.status == TabStatus.Rejected
                  "
                >
                  {{ $t("action.reset") }}
                </el-button>

                <el-button
                  @click="openReverseBox(item.id)"
                  type="warning"
                  class="ms-3"
                  v-if="
                    route.params.type == 'referral' &&
                    criteria.status == TabStatus.Rejected
                  "
                >
                  {{ $t("action.restore") }}
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <TableFooter :criteria="criteria" @page-change="fetchData" />
    </div>
  </div>
  <ApplicationDetailsPanel
    ref="applicationDetailsPanelRef"
    :before-close="handleBeforeClose"
    @submit="fetchData"
  />
  <AccountDetail ref="accountDetailsPanelRef" />
</template>

<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import {
  ApplicationType,
  ApplicationStatusType,
} from "@/core/types/ApplicationInfos";
import AccountService from "../services/AccountService";
import TimeShow from "@/components/TimeShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import ApplicationDetailsPanel from "@/projects/tenant/modules/accounts/components/ApplicationDetailsPanel.vue";
import TableFooter from "@/components/TableFooter.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useRoute, useRouter } from "vue-router";
import AccountDetail from "@/projects/tenant/modules/accounts/components/AccountDetail.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const route = useRoute();
const isLoading = ref(false);
const applications = ref(Array<any>());

const accountDetailsPanelRef = ref<InstanceType<typeof AccountDetail>>();

const openConfirm = inject(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL,
  (...args) => MsgPrompt.info(...args)
);

const enum TabStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Completed = 4,
}

const changeTab = (tab: TabStatus) => {
  criteria.value.status =
    {
      [TabStatus.Pending]: ApplicationStatusType.AwaitingApproval,
      [TabStatus.Approved]: ApplicationStatusType.Approved,
      [TabStatus.Rejected]: ApplicationStatusType.Rejected,
      [TabStatus.Completed]: ApplicationStatusType.Completed,
    }[tab] ?? ApplicationStatusType.AwaitingApproval;
  fetchData(1);
};

const getInitialCriteria = () => ({
  size: 10,
  page: 1,
  status: ApplicationStatusType.AwaitingApproval,
  types: [
    {
      password: ApplicationType.TradeAccountChangePassword,
      leverage: ApplicationType.TradeAccountChangeLeverage,
      wholesale: ApplicationType.WholeSaleAccount,
      referral: ApplicationType.WholesaleReferral,
    }[route.params.type as string] ??
      ApplicationType.TradeAccountChangeLeverage,
  ],
});

const criteria = ref<any>(getInitialCriteria());

const resetWholesaleApplication = async (_id: number) => {
  try {
    await AccountService.resetWholesaleApplication(_id);
    MsgPrompt.success("Reset successfully");
  } catch (e) {
    MsgPrompt.warning("Wholesale application has been reset");
  }
};

const openReverseBox = (applicationId: number) => {
  openConfirm(() =>
    AccountService.reverseApplicationsById(applicationId).then(() =>
      fetchData(1)
    )
  );
};

const showAccount = async (applicationSupplement: any) => {
  const accountUid =
    applicationSupplement?.AccountUid ?? applicationSupplement?.accountUid;
  if (!accountUid) {
    MsgPrompt.error("Account uid is not found");
    return;
  }

  const { data } = await AccountService.queryAccounts({ uid: accountUid });
  if (data.length === 0) {
    MsgPrompt.error("Account not found");
    return;
  }

  const account = data[0];

  accountDetailsPanelRef.value?.show(account.id, "infos", [] as any);
};

const applicationDetailsPanelRef =
  ref<InstanceType<typeof ApplicationDetailsPanel>>();

const showApplicationDetails = (applicationDetails: any) => {
  applicationDetailsPanelRef.value?.show(applicationDetails);
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  try {
    isLoading.value = true;
    const res = await AccountService.queryApplications(criteria.value);
    criteria.value = res.criteria;
    applications.value = res.data.map((item: any) => ({
      ...item,
      user: {
        ...item.user,
        partyId: item.partyId,
      },
    }));
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const router = useRouter();

const handleBeforeClose = () => {
  router.push({
    name: "AccountActivities",
    params: { type: route.params.type },
  });
};

watch(
  () => route.query.id,
  async () => {
    if (route.query.id) {
      await fetchData(1);
      openApplicationDetailsFromRoute();
    }
  }
);

const openApplicationDetailsFromRoute = () => {
  const applicationDetails = applications.value.find(
    (item) => item.id === Number(route.query.id)
  );
  if (applicationDetails) {
    showApplicationDetails(applicationDetails);
  }
};

watch(
  () => route.params.type,
  () => {
    criteria.value = getInitialCriteria();
    fetchData(1);
  }
);

onMounted(async () => {
  criteria.value = getInitialCriteria();
  await fetchData(1);
  if (route.query?.id) {
    openApplicationDetailsFromRoute();
  }
});
</script>

<style scoped>
.table-responsive {
  overflow-x: scroll !important;
}
.table {
  min-width: 100%;
  width: max-content;
}
.table td,
.table th {
  white-space: nowrap;
}
.crd-claa {
  width: 100px;
  height: 100px;
}
</style>
