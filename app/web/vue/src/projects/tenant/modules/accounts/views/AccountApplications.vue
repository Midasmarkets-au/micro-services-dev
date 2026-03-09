<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="d-flex flex-wrap gap-10 justify-content-between">
      <ul
        class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
      >
        <!--begin:::Tab item-->
        <li class="nav-item">
          <a
            :class="[
              'nav-link text-active-primary pb-4',
              { active: tab === TabStatus.pending },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(TabStatus.pending)"
            >{{ $t("status.pending") }}</a
          >
        </li>
        <!--end:::Tab item-->

        <!--begin:::Tab item-->
        <li class="nav-item">
          <a
            :class="[
              'nav-link text-active-primary pb-4',
              { active: tab === TabStatus.approved },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(TabStatus.approved)"
            >{{ $t("status.approved") }}</a
          >
        </li>
        <!--end:::Tab item-->

        <!--begin:::Tab item-->
        <li class="nav-item">
          <a
            :class="[
              'nav-link text-active-primary pb-4',
              { active: tab === TabStatus.completed },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(TabStatus.completed)"
            >{{ $t("status.completed") }}</a
          >
        </li>

        <!--end:::Tab item-->

        <!--begin:::Tab item-->
        <li class="nav-item">
          <a
            :class="[
              'nav-link text-active-primary pb-4',
              { active: tab === TabStatus.rejected },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-kt-countup-tabs="true"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(TabStatus.rejected)"
            >{{ $t("status.rejected") }}</a
          >
        </li>
        <!--end:::Tab item-->

        <li class="nav-item">
          <a
            :class="[
              'nav-link text-active-primary pb-4',
              { active: tab === TabStatus.all },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(TabStatus.all)"
            >{{ $t("status.all") }}</a
          >
        </li>
      </ul>
      <div>
        <el-input
          class="w-400px"
          v-model="criteria.email"
          :clearable="true"
          placeholder="Email"
        >
          <template #append>
            <el-button :icon="Search" @click="search" />
          </template>
        </el-input>
        <el-button class="ms-4" type="info" plain @click="reset">{{
          $t("action.reset")
        }}</el-button>
      </div>
    </div>
    <div class="card">
      <div class="card-body py-4">
        <div class="table-responsive">
          <table
            class="table align-middle table-row-dashed fs-6 gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr
                class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
              >
                <th>{{ $t("fields.id") }}</th>
                <th>{{ $t("fields.client") }}</th>
                <th v-if="tab === TabStatus.all">{{ $t("fields.status") }}</th>
                <th>{{ $t("fields.site") }}</th>
                <th>{{ $t("fields.accountType") }}</th>
                <th>{{ $t("fields.currency") }}</th>
                <th>{{ $t("fields.leverage") }}</th>
                <th>{{ $t("fields.platform") }}</th>
                <th>{{ $t("fields.server") }}</th>
                <th class="text-center">{{ $t("fields.operatedBy") }}</th>
                <th class="min-w-125px">{{ $t("fields.createdOn") }}</th>
                <th class="text-center min-w-150px">
                  {{ $t("action.action") }}
                </th>
              </tr>
            </thead>

            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody
              v-else-if="!isLoading && tradeAccountApplications.length === 0"
            >
              <NoDataBox />
            </tbody>

            <tbody v-else class="fw-semibold text-gray-900">
              <tr
                v-for="(item, index) in tradeAccountApplications"
                :key="index"
              >
                <td>
                  {{ item.id }}
                  <span
                    v-if="$can('Admin')"
                    class="ms-2 cursor-pointer"
                    role="button"
                    @click="
                      openCommentView(
                        CommentType.Application,
                        item.id,
                        item.user?.firstName + ' ' + item.user?.lastName
                      )
                    "
                  >
                    <i
                      v-if="item.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i>
                    <i
                      v-else
                      class="fa-regular fa-comment-dots text-secondary"
                    ></i>
                  </span>
                  /
                  {{ item.partyId }}
                </td>
                <td class="d-flex align-items-center">
                  <UserInfo url="#" :user="item.user" class="me-2" />
                </td>
                <td v-if="tab === TabStatus.all">
                  {{ statusTypes[item.status] }}
                </td>
                <td>{{ $t(`type.siteType.${item.siteId}`) }}</td>
                <td>{{ $t(`type.account.${item.supplement.accountType}`) }}</td>
                <!--  -->
                <td>
                  {{
                    item.supplement.currencyId
                      ? $t(`type.currency.${item.supplement.currencyId}`)
                      : "***"
                  }}
                </td>
                <td>
                  {{
                    item.supplement.leverage
                      ? item.supplement.leverage + ":1"
                      : "***"
                  }}
                </td>
                <td>
                  {{
                    serviceMap[item.supplement.serviceId]?.platformName ?? "***"
                  }}
                </td>
                <td>
                  {{
                    serviceMap[item.supplement.serviceId]?.serverName ?? "***"
                  }}
                </td>
                <td>
                  {{
                    {
                      2: item.operatorName.split(",")[0],
                      3: item.operatorName.split(",")[1],
                      4: item.operatorName.split(",")[2],
                    }[item.status] ?? "--"
                  }}
                </td>
                <td>
                  <TimeShow :date-iso-string="item.createdOn" />
                </td>
                <td :class="{ 'text-center': tab != TabStatus.all }">
                  <button
                    v-show="tab !== TabStatus.approved"
                    class="btn btn-light btn-success btn-sm me-3"
                    @click="
                      openProcedureFormRef?.show(1, {
                        applicationDetails: item,
                      })
                    "
                  >
                    {{ $t("action.details") }}
                  </button>

                  <button
                    v-show="
                      item.status == TabStatus.approved &&
                      item.supplement.role === AccountRoleTypes.Client
                    "
                    class="btn btn-light btn-success btn-sm me-3"
                    @click="
                      openProcedureFormRef?.show(2, {
                        applicationDetails: item,
                      })
                    "
                  >
                    {{ $t("action.openMt") }}
                  </button>

                  <button
                    v-if="item.status == TabStatus.pending"
                    href="#"
                    class="btn btn-light btn-danger btn-sm"
                    @click="openRejectReasonBox(item.id)"
                  >
                    {{ $t("action.reject") }}
                  </button>

                  <button
                    v-if="item.status == TabStatus.rejected"
                    class="btn btn-light btn-info btn-sm"
                    @click="showRejectReason(item)"
                  >
                    {{ $t("action.viewReasons") }}
                  </button>

                  <el-button
                    v-if="item.status == TabStatus.rejected"
                    type="warning"
                    class="ms-3"
                    @click="openReverseBox(item.id)"
                  >
                    Reverse
                  </el-button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>
  </div>
  <OpenProcedureForm
    ref="openProcedureFormRef"
    @step-finished="fetchData(1)"
    :before-close="handleBeforeClose"
  />
  <AccountRequestApprove
    ref="AccountRequestApproveModel"
    @approved="approveSubmit"
  />
  <UserDetails ref="userDetailsRef" />
  <!-- <AccountCreate
    ref="accountCreateRef"
    @account-submitted="fetchData(criteria.page)"
  /> -->
  <RejectReasonBox
    ref="rejectReasonBoxRef"
    :handleSubmit="rejectHandler"
    @rejected="fetchData(criteria.page)"
  />
</template>
<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import { CommentType } from "@/core/types/CommentType";
import TableFooter from "@/components/TableFooter.vue";
import AccountRequestApprove from "../components/AccountRequestApprove.vue";
import AccountService from "../services/AccountService";
import TimeShow from "@/components/TimeShow.vue";
import UserDetails from "../../../components/UserDetails.vue";
import { FetchedDataType } from "../types/FetchedDataType";
import {
  ApplicationStatusType,
  ApplicationType,
  getApplicationStatusType,
} from "@/core/types/ApplicationInfos";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import RejectReasonBox from "@/components/RejectReasonBox.vue";
import OpenProcedureForm from "../components/OpenProcedureForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useRoute, useRouter } from "vue-router";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { Search } from "@element-plus/icons-vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";

const isLoading = ref(true);
const tradeAccountApplications = ref(Array<any>());
// const tradeAccountApplications = ref(Array<any>());
const userDetailsRef = ref<InstanceType<typeof UserDetails>>();
const rejectReasonBoxRef = ref<InstanceType<typeof RejectReasonBox>>();
const openProcedureFormRef = ref<InstanceType<typeof OpenProcedureForm>>();

const rejectHandler = ref(() => Promise.resolve());

// current chosen tab: Pending, Approved, Rejected
enum TabStatus {
  pending = 1,
  approved = 2,
  rejected = 3,
  completed = 4,
  all = 5,
}
const statusTypes = getApplicationStatusType();
const tab = ref(TabStatus.pending);

const route = useRoute();
const router = useRouter();

const AccountRequestApproveModel = ref(null);

const serviceMap = ref({} as any);

const criteria = ref<any>({
  page: 1,
  size: 10,
  status: TabStatus.pending,
  type: ApplicationType.TradeAccount,
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  try {
    isLoading.value = true;
    const res = await AccountService.queryApplications(criteria.value);
    criteria.value = res.criteria;
    tradeAccountApplications.value = res.data.map((item: any) => ({
      ...item,
      user: {
        ...item.user,
        partyId: item.partyId,
      },
    }));
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const openCommentView = inject<
  (type: CommentType, id: number, title: string) => any
>(InjectKeys.OPEN_COMMENT_VIEW, () => null);

const handleBeforeClose = () => {
  router.push({ name: "AccountApplications" });
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
  const applicationDetails = tradeAccountApplications.value.find(
    (item) => item.id === Number(route.query.id)
  );
  if (applicationDetails) {
    openProcedureFormRef.value?.show(1, { applicationDetails });
  }
};

onMounted(async () => {
  serviceMap.value = await GlobalService.getServiceMap();
  await fetchData(criteria.value.page);
  //  openProcedureFormRef?.show(1, { applicationDetails: item })
  if (route.query?.id) {
    openApplicationDetailsFromRoute();
  }
});
const changeTab = (_tab: TabStatus) => {
  tab.value = _tab;
  criteria.value.status =
    {
      [TabStatus.approved]: ApplicationStatusType.Approved,
      [TabStatus.pending]: ApplicationStatusType.AwaitingApproval,
      [TabStatus.rejected]: ApplicationStatusType.Rejected,
      [TabStatus.completed]: ApplicationStatusType.Completed,
    }[_tab] ?? null;

  fetchData(1);
};

const pageChange = (page: number) => {
  fetchData(page);
};

const approveSubmit = (data: FetchedDataType) => {
  console.log(data);
};

const openReject = inject(
  TenantGlobalInjectionKeys.OPEN_REJECT_REASON_MODAL,
  (...args) => MsgPrompt.info(...args)
);

const openConfirm = inject(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL,
  (...args) => MsgPrompt.info(...args)
);
const openRejectReasonBox = (applicationId: number) => {
  // rejectHandler.value = (formData?: any) =>
  //   AccountService.rejectApplicationsById(applicationId, formData);
  // rejectReasonBoxRef.value?.show();

  openReject((formData) =>
    AccountService.rejectApplicationsById(applicationId, formData).then(() =>
      fetchData(criteria.value.page)
    )
  );
};

const openReverseBox = (applicationId: number) => {
  openConfirm(() =>
    AccountService.reverseApplicationsById(applicationId).then(() =>
      fetchData(criteria.value.page)
    )
  );
};

const search = () => {
  tab.value = TabStatus.all;
  changeTab(TabStatus.all);
  criteria.value.status = null;
  fetchData(1);
};
const reset = () => {
  criteria.value.email = "";
  criteria.value.partyId = null;
  fetchData(1);
};
const showRejectReason = (item: any) => {
  openConfirm(void 0, void 0, {
    confirmText: item.rejectedReason,
    disableFooter: true,
  });
};
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
</style>
