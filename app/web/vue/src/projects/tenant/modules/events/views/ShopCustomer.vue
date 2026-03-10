<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li
      class="nav-item"
      v-for="tab in EventPartyStatusTypeOptions"
      :key="tab.value"
    >
      <a
        class="nav-link text-active-primary pb-2"
        :class="[
          { active: isTabActive(tab.value) },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tab.value)"
        >{{ tab.label }}</a
      >
    </li>
  </ul>
  <div class="card">
    <div class="card-header d-flex align-items-center">
      <div class="card-title">
        <el-input
          class="w-200px"
          v-model="criteria.email"
          :disabled="isLoading"
          clearable
          :placeholder="$t('title.email')"
          @keyup.enter="fetchData(1)"
        ></el-input>
        <el-button
          type="primary"
          @click="fetchData(1)"
          :loading="isLoading"
          class="ms-2"
          >{{ $t("action.search") }}</el-button
        >
        <el-button @click="reset" class="ms-2" :disabled="isLoading">{{
          $t("action.reset")
        }}</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant-sm table-hover">
        <thead>
          <tr>
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("fields.eventKey") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.points") }}</th>
            <th>{{ $t("fields.operator") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="item in data"
            :key="item.id"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <UserInfo
                :email="item.email"
                :name="item.nativeName"
                :partyId="item.partyId"
                class="me-2"
              />
            </td>
            <td>{{ item.eventKey }}</td>
            <td>
              <el-tag :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>
              <ShopPoints :points="item.totalPoint" :format="4" />
            </td>
            <td>{{ item.lastOperatorName }}</td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button
                v-for="(button, index) in getButtonConfigs(item)"
                :key="index"
                :type="button.type"
                size="small"
                @click="button.action"
              >
                {{ button.label }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <UserDetails ref="UserDetailsRef" />
  <AdjustPoints ref="AdjustPointsRef" @update="fetchData(1)" />
  <UserDetailCustomer ref="userDetailShopRef" />
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import {
  EventPartyStatusTypes,
  EventPartyStatusTypeOptions,
} from "@/core/types/shop/ShopCustomerTypes";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import EventsServices from "../services/EventsServices";
import UserDetails from "@/projects/tenant/components/UserDetails.vue";
import AdjustPoints from "../components/shop/ShopCustomer/AdjustPoints.vue";
import UserDetailCustomer from "../components/shop/UserDetailShop.vue";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  pageSize: 25,
  status: EventPartyStatusTypes.Applied,
});
const UserDetailsRef = ref<InstanceType<typeof UserDetails>>();
const AdjustPointsRef = ref<any>(null);
const userDetailShopRef = ref<any>(null);
const tab = ref<any>(EventPartyStatusTypes.Applied);
const accountSelected = ref(0);
const isTabActive = (_tab: any) => {
  return tab.value === _tab;
};

const reset = () => {
  criteria.value.email = null;
  fetchData(1);
};

const changeTab = (_value: any) => {
  tab.value = _value;
  criteria.value.status = tab.value;
  if (tab.value === 5) {
    criteria.value.status = null;
  }
  fetchData(1);
};

const getStatusLabel = (value: number) => {
  const option = EventPartyStatusTypeOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};

const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;
  try {
    const res = await EventsServices.queryCustomerList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const showUserDetails = (partyId: number) => {
  UserDetailsRef.value?.show(partyId);
};

const showUserDetailShop = (item) => {
  userDetailShopRef.value?.show(item.partyId);
};

const showAdjustPoints = (item) => {
  AdjustPointsRef.value?.show(item);
};

const openConfirmPanel = (_action: EventPartyStatusTypes, id: number) => {
  openConfirmModal?.(() => {
    return {
      [EventPartyStatusTypes.Approved]: () =>
        EventsServices.approveCustomerById(id),
      [EventPartyStatusTypes.Rejected]: () =>
        EventsServices.rejectCustomerById(id),
      [EventPartyStatusTypes.Cancelled]: () =>
        EventsServices.cancelCustomerById(id),
    }
      [_action]()
      .then(() => {
        fetchData(criteria.value.page);
      });
  });
};

function getStatusTag(status: EventPartyStatusTypes) {
  switch (status) {
    case EventPartyStatusTypes.Applied:
      return "";
    case EventPartyStatusTypes.Approved:
      return "success";
    case EventPartyStatusTypes.Rejected:
      return "danger";
    case EventPartyStatusTypes.Cancelled:
      return "warning";
  }
}

function getButtonConfigs(item) {
  let configs: any[] = [];
  switch (item.status) {
    case EventPartyStatusTypes.Applied:
      configs.push(
        {
          label: t("action.approve"),
          type: "success",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Approved, item.id),
        },
        {
          label: t("action.reject"),
          type: "danger",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Rejected, item.id),
        }
      );
      break;

    case EventPartyStatusTypes.Approved:
      configs.push(
        {
          label: t("title.adjustPoints"),
          type: "primary",
          action: () => showAdjustPoints(item),
        },
        {
          label: t("fields.userShopDetail"),
          type: "info",
          action: () => showUserDetailShop(item),
        }
      );
      break;

    case EventPartyStatusTypes.Rejected:
      configs.push(
        {
          label: t("action.approve"),
          type: "success",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Approved, item.id),
        },
        {
          label: t("action.cancel"),
          type: "warning",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Cancelled, item.id),
        }
      );
      break;
    case EventPartyStatusTypes.Cancelled:
      configs.push(
        {
          label: t("action.approve"),
          type: "success",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Approved, item.id),
        },
        {
          label: t("action.reject"),
          type: "danger",
          action: () =>
            openConfirmPanel(EventPartyStatusTypes.Rejected, item.id),
        }
      );
      break;
  }
  return configs;
}

const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
onMounted(() => {
  fetchData(1);
});
</script>
