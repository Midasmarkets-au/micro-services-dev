<template>
  <div class="d-flex gap-10 justify-content-between">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.record },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = TabStatus.record"
          >Record</a
        >
      </li>
      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.search },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = TabStatus.search"
          >Search</a
        >
      </li>
    </ul>
  </div>

  <div>
    <div v-if="tab === TabStatus.search" class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div class="card-title">
          <h2 class="card-label">Search Details</h2>
        </div>
      </div>

      <div class="card-body">
        <AccountTradeStat
          :accountId="null"
          :fromBrirfDetail="true"
        ></AccountTradeStat>
      </div>
    </div>

    <div v-if="tab === TabStatus.record" class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div
          class="card-title d-flex justify-content-between"
          style="width: 100%"
        >
          <h2 class="card-label">Record List</h2>

          <div>
            <span class="me-5">Set Search Time</span>
            <el-date-picker
              v-model="period"
              type="datetimerange"
              start-placeholder="Start date"
              end-placeholder="End date"
              range-separator="-"
              value-format="YYYY-MM-DD HH:mm:ss"
              :disabled="isLoading"
            />

            <el-button
              type="success"
              plain
              @click="addRecord"
              :loading="isLoading"
              class="ms-9"
              :disabled="addNewRecord"
            >
              {{ $t("action.add") }}
            </el-button>
          </div>
        </div>
      </div>

      <div class="card-body py-4">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.id") }}</th>
              <th class="">{{ $t("fields.name") }}</th>
              <th class="">
                {{ $t("tip.mt4") }}
              </th>
              <th class="">
                {{ $t("tip.mt5") }}
              </th>
              <th class="">{{ $t("action.detail") }}</th>
              <th class="">{{ $t("action.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && records.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in records.value" :key="index">
              <td class="">{{ index + 1 }}</td>
              <td class="">
                <Field
                  v-if="editRow == index"
                  class="form-control form-control-solid"
                  name="name"
                  v-model="item.name"
                />
                <span v-else>{{ item.name }}</span>
              </td>
              <td style="max-width: 150px">
                <Field
                  v-if="editRow == index"
                  class="form-control form-control-solid"
                  name="mt4"
                  v-model="item.mt4"
                />
                <span v-else>{{ item.mt4 }}</span>
              </td>
              <td style="max-width: 150px">
                <Field
                  v-if="editRow == index"
                  class="form-control form-control-solid"
                  name="mt5"
                  v-model="item.mt5"
                />
                <span v-else>{{ item.mt5 }}</span>
              </td>

              <td class="">
                <el-button class="" @click="showWithdrawInfo(item)">
                  {{ $t("action.viewDetails") }}
                </el-button>
              </td>

              <td class="">
                <button
                  v-if="editRow == index"
                  href="#"
                  class="btn btn-light btn-success btn-sm ms-5"
                  @click="updateListInfo()"
                  :disabled="isLoading"
                >
                  {{ $t("action.save") }}
                </button>
                <button
                  v-else
                  href="#"
                  class="btn btn-light btn-primary btn-sm ms-5"
                  @click="editRow = index"
                  :disabled="isLoading"
                >
                  {{ $t("action.edit") }}
                </button>
                <button
                  href="#"
                  class="btn btn-light btn-danger btn-sm ms-5"
                  @click="openConfirmPanel(index, item)"
                  :disabled="isLoading"
                >
                  <i class="fa-regular fa-trash-can"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <BriefDetailModal ref="BriefDetailRef" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import AccountTradeStat from "@/projects/tenant/modules/accounts/components/accountTradeStat/AccountTradeStat.vue";
import TradeServices from "../services/TradeServices";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Field, ErrorMessage, useForm } from "vee-validate";
import BriefDetailModal from "../modals/BriefDetailModal.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";

const openConfirmBox = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

const BriefDetailRef = ref<InstanceType<typeof BriefDetailModal>>();
const today = new Date();

const isLoading = ref(false);
const tab = ref(TabStatus.record);
const records = ref([]);
const editRow = ref(-1);
const addNewRecord = ref(false);

enum TabStatus {
  search = 1,
  record = 2,
}
const formatDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const period = ref([
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 0, 0, 0)
  ),
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 23, 59, 59)
  ),
]);

const handleDateChange = () => {
  // Ensure that period.value[1] is set to 23:59:59
  const endDate = new Date(period.value[1]);
  endDate.setHours(23, 59, 59, 999);
  period.value[1] = formatDate(endDate);
};

const fetchData = async () => {
  try {
    isLoading.value = true;
    records.value = await TradeServices.queryBriefDetailList(
      "Public",
      0,
      "BriefDetailLists"
    );
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const updateListInfo = async () => {
  console.log(records.value.value);
  addNewRecord.value = false;

  isLoading.value = true;

  try {
    await TradeServices.updateBriefDetailList(
      "Public",
      0,
      "BriefDetailLists",
      records.value
    );
    MsgPrompt.success("Update successfully").then(() => {
      editRow.value = -1;
    });
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    fetchData();
  }
};

const showWithdrawInfo = async (_item: any) => {
  BriefDetailRef.value?.show(_item, period.value);
};

const addRecord = async () => {
  addNewRecord.value = true;
  const newItem = {
    name: "",
    mt4: "",
    mt5: "",
  };
  records.value.value.push(newItem);
  editRow.value = records.value.value.length - 1;
};

const deleteRecord = async (_index: any) => {
  records.value.value.splice(_index, 1);
  updateListInfo();
};

const openConfirmPanel = (_index: any, _item: any) => {
  openConfirmBox?.(() => deleteRecord(_index), void 0, {
    confirmTitle: "Delete Record",
    confirmText: "Confirm to delete this record - " + _item.name,
  });
};

watch(
  () => period.value,
  () => {
    handleDateChange();
  }
);

onMounted(async () => {
  await fetchData();
  handleDateChange();
});
</script>
