<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <li class="nav-item" v-for="tab in tabOptions" :key="tab.id">
      <a
        v-if="$cans(tab.permission)"
        class="nav-link text-active-primary pb-4"
        :class="[
          { active: isTabActive(tab.id) },
          { 'disabled opacity-50 pe-none': isLoading },
        ]"
        data-bs-toggle="tab"
        href="#"
        @click="changeTab(tab.id)"
        >{{ tab.name }}</a
      >
    </li>
  </ul>
  <div class="card" v-if="tab === 1">
    <div class="card-header">
      <div class="card-title">
        <div class="d-flex align-items-end gap-4">
          <div>
            <el-input v-model="criteria.accountNumber" placeholder="Login NO.">
            </el-input>
          </div>

          <div>
            <el-select
              v-model="criteria.type"
              clearable
              :placeholder="t('fields.type')"
            >
              <el-option
                v-for="item in creditAdjustTypeOptions"
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
          type="success"
          @click="openChangeAdjustPanel"
          v-if="$can('TenantAdmin')"
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
            <td>{{ $t("fields.accountNo") }}</td>
            <td>{{ $t("fields.role") }}</td>
            <td>{{ $t("fields.salesCode") }}</td>
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
              {{ creditAdjustTypes[item.type] }}
            </td>
            <td>{{ item.accountNumber }}</td>
            <td>{{ $t(`type.accountRole.${item.role}`) }}</td>
            <td>{{ item.code }}</td>
            <td>{{ item.ticket }}</td>
            <td>
              {{ item.operatorName }}
            </td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>
            <td>
              {{ item.amount / 100 }}
            </td>
            <td>{{ item.comment }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <TableFooter @page-change="fetchData" :criteria="criteria" />
  </div>
  <template v-else>
    <BatchIndex />
  </template>
  <ChangeCreditForm
    ref="changeCreditFormRef"
    @event-submit="onEventSubmitted"
  />
  <ChangeAdjustForm
    ref="changeAdjustFormRef"
    @event-submit="onEventSubmitted"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TableFooter from "@/components/TableFooter.vue";
import PaymentService from "../services/PaymentService";
import ChangeCreditForm from "@/projects/tenant/modules/Payment/components/ChangeCreditForm.vue";
import ChangeAdjustForm from "@/projects/tenant/modules/Payment/components/ChangeAdjustForm.vue";
import BatchIndex from "@/projects/tenant/modules/Payment/components/modal/BatchIndex.vue";
import {
  creditAdjustTypes,
  creditAdjustTypeOptions,
} from "@/core/types/CreditAdjustTypes";
import { useI18n } from "vue-i18n";

const t = useI18n().t;
const isLoading = ref(false);
const changeCreditFormRef = ref<InstanceType<typeof ChangeCreditForm>>();
const changeAdjustFormRef = ref<InstanceType<typeof ChangeAdjustForm>>();
const data = ref<any>({});
const tabOptions = [
  {
    name: t("fields.manual"),
    id: 1,
    permission: ["TenantAdmin", "WebCreditAdjustment"],
  },
  {
    name: t("fields.batch"),
    id: 2,
    permission: ["TenantAdmin", "WebBatchAdjust"],
  },
];
const tab = ref(1);

const isTabActive = (_tab: number) => {
  return _tab === tab.value;
};
const changeTab = (id: number) => {
  tab.value = id;
};

const criteria = ref<any>({
  page: 1,
  size: 20,
  adjustBatchId: 0,
});
const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    adjustBatchId: 0,
  };
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await PaymentService.getAdjustRecord(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    MsgPrompt.error(e);
    console.log(e);
  }
  isLoading.value = false;
};

const onEventSubmitted = () => {
  reset();
};

onMounted(() => {
  fetchData(1);
});

const openChangeCreditPanel = () => {
  changeCreditFormRef.value?.show();
};

const openChangeAdjustPanel = () => {
  changeAdjustFormRef.value?.show();
};
</script>
