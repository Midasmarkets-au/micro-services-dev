<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li
      class="nav-item"
      v-for="tab in EventShopOrderStatusOptions"
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
          clearable
          :placeholder="$t('title.email')"
          @keyup.enter="fetchData(1)"
        />
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
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.itemName") }}</th>
            <th>{{ $t("fields.totalPoints") }}</th>
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
                :email="item.eventPartyEmail"
                :name="item.eventPartyNativeName"
                class="me-2"
              />
            </td>
            <td>
              <el-tag :type="getStatusTag(item.status)">{{
                getStatusLabel(item.status)
              }}</el-tag>
            </td>
            <td>{{ item.eventShopItemName }}</td>
            <td><ShopPoints :points="item.totalPoint" /></td>
            <td>{{ item.operatorName }}</td>
            <td>
              <TimeShow :date-iso-string="item.createdOn" type="GMToneLiner" />
            </td>
            <td>
              <el-button size="small" @click="showDetail(item)">{{
                $t("action.detail")
              }}</el-button>
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
  <OrderDetail ref="orderDetailRef" />
  <AddTrackingModal ref="addTrackingModalRef" @fetch-data="fetchData" />
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import {
  EventShopOrderStatusTypes,
  EventShopOrderStatusOptions,
} from "@/core/types/shop/ShopPointsTypes";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import EventsServices from "../services/EventsServices";
import OrderDetail from "../components/shop/OrderDetail.vue";
import AddTrackingModal from "../components/shop/AddTrackingModal.vue";
import TimeShow from "@/components/TimeShow.vue";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const data = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  pageSize: 25,
  status: EventShopOrderStatusTypes.Pending,
});

const reset = () => {
  criteria.value.email = null;
  fetchData(1);
};

const getStatusLabel = (value: number) => {
  const option = EventShopOrderStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};
const orderDetailRef = ref<any>(null);
const addTrackingModalRef = ref<any>(null);
const tab = ref(EventShopOrderStatusTypes.Pending);
const isTabActive = (_tab: any) => {
  return tab.value === _tab;
};
const accountSelected = ref(0);
const changeTab = (_value: any) => {
  tab.value = _value;
  criteria.value.status = tab.value;
  if (tab.value === 5) {
    criteria.value.status = null;
  }
  fetchData(1);
};

const showDetail = (item: any) => {
  orderDetailRef.value?.show(item);
};

const fetchData = async (page: number) => {
  isLoading.value = true;
  criteria.value.page = page;
  try {
    const res = await EventsServices.queryShopOrderList(criteria.value);
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

const showAddTrackingModal = (item: any) => {
  addTrackingModalRef.value?.show(item);
};

const openConfirmPanel = (_action: EventShopOrderStatusTypes, id: number) => {
  openConfirmModal?.(() => {
    return {
      [EventShopOrderStatusTypes.Pending]: () =>
        EventsServices.pendingOrder(id),
      [EventShopOrderStatusTypes.Processing]: () =>
        EventsServices.processOrder(id),
      [EventShopOrderStatusTypes.Shipped]: () => EventsServices.shipOrder(id),
      [EventShopOrderStatusTypes.Succeed]: () =>
        EventsServices.succeedOrder(id),
      [EventShopOrderStatusTypes.Cancelled]: () =>
        EventsServices.cancelOrder(id),
    }
      [_action]()
      .then(() => {
        ElNotification({
          title: t("status.success"),
          message: t("status.success"),
          type: "success",
        });
        fetchData(criteria.value.page);
      });
  });
};

function getStatusTag(status: EventShopOrderStatusTypes) {
  switch (status) {
    case EventShopOrderStatusTypes.Pending:
      return "warning";
    case EventShopOrderStatusTypes.Processing:
      return "info";
    case EventShopOrderStatusTypes.Shipped:
      return "primary";
    case EventShopOrderStatusTypes.Succeed:
      return "success";
    case EventShopOrderStatusTypes.Cancelled:
      return "danger";
  }
}

function getButtonConfigs(item) {
  let configs: any[] = [];
  switch (item.status) {
    case EventShopOrderStatusTypes.Pending:
      configs.push(
        {
          label: "Process",
          type: "primary",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Processing, item.id),
        },
        {
          label: "Cancel",
          type: "danger",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Cancelled, item.id),
        }
      );
      break;

    case EventShopOrderStatusTypes.Processing:
      configs.push(
        {
          label: "Ship",
          type: "warning",
          action: () => showAddTrackingModal(item),
        },
        {
          label: "Cancel",
          type: "danger",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Cancelled, item.id),
        }
      );
      break;

    case EventShopOrderStatusTypes.Shipped:
      configs.push(
        {
          label: "Succeed",
          type: "success",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Succeed, item.id),
        },
        {
          label: "Cancel",
          type: "danger",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Cancelled, item.id),
        }
      );
      break;

    case EventShopOrderStatusTypes.Succeed:
      configs.push({
        label: "Cancel",
        type: "danger",
        action: () =>
          openConfirmPanel(EventShopOrderStatusTypes.Cancelled, item.id),
      });
      break;

    case EventShopOrderStatusTypes.Cancelled:
      configs.push(
        {
          label: "Process",
          type: "primary",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Processing, item.id),
        },
        {
          label: "Pending",
          type: "info",
          action: () =>
            openConfirmPanel(EventShopOrderStatusTypes.Pending, item.id),
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
