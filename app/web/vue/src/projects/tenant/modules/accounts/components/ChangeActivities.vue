<template>
  <div class="card mb-5">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.pendingAccountActivities") }}
      </div>
      <div class="card-toolbar">
        <router-link to="/account/activity/password">
          {{ $t("title.viewMore") }}</router-link
        >
      </div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.client") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.type") }}</th>
            <th class="min-w-60px text-end">
              {{ $t("fields.createdOn") }}
            </th>

            <th class="text-end">{{ $t("action.action") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && applications.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr
            class="text-center"
            v-for="(item, index) in applications"
            :key="index"
          >
            <td class="d-flex align-items-center">
              <UserInfo
                url="#"
                :avatar="item.user.avatar"
                :title="item.user.name"
                :sub="item.user.email"
                :name="item.user.name"
                class="me-2"
              />
            </td>
            <td class="text-end">{{ item.type }}</td>
            <td class="text-end">
              <TimeShow :date-iso-string="item.createdOn" />
            </td>

            <td class="text-end">
              <button
                class="btn btn-light btn-success btn-sm"
                @click="showApplicationDetails(index)"
              >
                {{ $t("action.details") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <ApplicationDetailsPanel
    ref="applicationDetailsPanelRef"
    @submit="fetchData"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import {
  ApplicationType,
  ApplicationStatusType,
} from "@/core/types/ApplicationInfos";
import AccountService from "../services/AccountService";
import ApplicationDetailsPanel from "./ApplicationDetailsPanel.vue";
import { useI18n } from "vue-i18n";
import TimeShow from "@/components/TimeShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";

const { t } = useI18n();

const isLoading = ref(false);
const applications = ref(Array<any>());
const applicationDetails = ref(Array<any>());
const criteria = ref({
  status: ApplicationStatusType.AwaitingApproval,
});

const applicationDetailsPanelRef =
  ref<InstanceType<typeof ApplicationDetailsPanel>>();

const showApplicationDetails = (index: number) => {
  applicationDetailsPanelRef.value?.show(applicationDetails.value[index]);
};

const fetchData = async () => {
  const changeLeverageRes = await AccountService.queryApplications({
    ...criteria.value,
    type: ApplicationType.TradeAccountChangeLeverage,
  });

  const changePasswordRes = await AccountService.queryApplications({
    ...criteria.value,
    type: ApplicationType.TradeAccountChangePassword,
  });

  const applyWholeSaleRes = await AccountService.queryApplications({
    ...criteria.value,
    type: ApplicationType.WholeSaleAccount,
  });

  applicationDetails.value = [
    ...changeLeverageRes.data,
    ...changePasswordRes.data,
    ...applyWholeSaleRes.data,
  ];

  applications.value = applicationDetails.value.map((item) => ({
    id: item.id,
    partyId: item.partyId,
    type: t(`type.application.${item.type}`),
    createdOn: item.createdOn,
    user: item.user,
  }));
};

onMounted(async () => {
  await fetchData();
});
</script>

<style scoped>
.crd-claa {
  width: 100px;
  height: 100px;
}
</style>
