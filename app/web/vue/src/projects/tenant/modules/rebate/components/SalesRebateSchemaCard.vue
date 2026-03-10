<template>
  <div
    class="d-flex justify-content-between"
    style="padding-top: 15px; font-size: 16px"
  >
    <div>{{ props.items[0].salesName }} - {{ props.items[0].salesCode }}</div>
    <div>
      {{
        salesTypeOptions.find(
          (option) => option.value === props.items[0].salesType
        )?.label
      }}
    </div>
  </div>
  <hr />
  <table
    class="table align-middle table-row-dashed fs-6 table-hover"
    id="table_accounts_requests"
  >
    <thead>
      <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
        <th>Sales UID</th>
        <th>Target UID</th>
        <th>Rebate</th>
        <th>Raw</th>
        <th>Pro</th>
        <th>{{ $t("fields.note") }}</th>
        <th>{{ $t("fields.operator") }}</th>
        <th>{{ $t("action.action") }}</th>
      </tr>
    </thead>

    <tbody v-if="props.isLoading" style="height: 300px">
      <tr>
        <td colspan="12"><scale-loader></scale-loader></td>
      </tr>
    </tbody>
    <tbody v-if="!props.isLoading && props.items.length === 0">
      <tr>
        <td colspan="12">{{ $t("tip.nodata") }}</td>
      </tr>
    </tbody>
    <TransitionGroup
      v-if="!props.isLoading && props.items.length != 0"
      tag="tbody"
      name="table-delete-fade"
      class="table-delete-fade-container text-gray-600 fw-semibold"
    >
      <tr
        v-for="item in items"
        :key="item"
        :class="{
          'account-select': item.id === accountSelected,
          'waiting-approve': item.status,
        }"
        @click="selectedAccount(item.id)"
      >
        <td>
          <i
            class="fa-solid fa-circle-notch me-2"
            :class="dotClass[item.salesType % 6]"
          ></i
          >{{ item.salesAccountUid }}
        </td>
        <td>
          <i class="me-2" :class="dotClass[item.schedule % 6]">{{
            scheduleType[item.schedule]
          }}</i
          >{{ item.rebateAccountUid }}
        </td>

        <td>
          <BalanceShow :balance="item.rebate" />
        </td>
        <td>
          <BalanceShow :balance="item.alphaRebate" />
        </td>
        <td>
          <BalanceShow :balance="item.proRebate" />
        </td>
        <td @click="showSchemaNote(item)">
          <i
            class="fa-regular fa-comment-dots"
            :class="{ 'text-secondary': item.note == '' }"
          ></i>
        </td>
        <td>{{ item.operatorFirstName }}</td>
        <td>
          <button
            class="btn btn-light btn-success btn-sm me-3"
            @click="showSchemaDetail(item)"
          >
            {{ $t("title.details") }}
          </button>
          <button
            v-if="item.status == -1"
            class="btn btn-light btn-danger btn-sm me-3"
            @click="deleteSchema(item)"
          >
            {{ $t("action.delete") }}
          </button>
          <button
            v-if="item.status == -1 && $can('SuperAdmin')"
            class="btn btn-light btn-warning btn-sm me-3"
            @click="approve(item)"
          >
            Approve
          </button>
        </td>
      </tr>
      <tr
        key="card-total"
        :class="{
          'total-error':
            totalRebate >
              salesTypeOptions.find(
                (option) => option.value === props.items[0].salesType
              )?.rebate ||
            totalAlpha >
              salesTypeOptions.find(
                (option) => option.value === props.items[0].salesType
              )?.alphaRebate ||
            totalPro >
              salesTypeOptions.find(
                (option) => option.value === props.items[0].salesType
              )?.proRebate,
        }"
      >
        <td>
          <i
            v-if="
              totalRebate >
                salesTypeOptions.find(
                  (option) => option.value === props.items[0].salesType
                )?.rebate ||
              totalAlpha >
                salesTypeOptions.find(
                  (option) => option.value === props.items[0].salesType
                )?.alphaRebate ||
              totalPro >
                salesTypeOptions.find(
                  (option) => option.value === props.items[0].salesType
                )?.proRebate
            "
            class="fa-solid fa-triangle-exclamation me-2"
            style="color: red"
          ></i
          >Total
        </td>
        <td></td>
        <td>
          <BalanceShow :balance="totalRebate" />
        </td>
        <td><BalanceShow :balance="totalAlpha" /></td>
        <td><BalanceShow :balance="totalPro" /></td>
        <td></td>
        <td></td>
      </tr>
    </TransitionGroup>
  </table>

  <SchemaDetail ref="SchemaDetailRef" @refresh="emits('refresh')" />
  <SchemaNote ref="SchemaNoteRef" />
</template>
<script lang="ts" setup>
import { ref, computed, inject } from "vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { Search } from "@element-plus/icons-vue";
import BalanceShow from "@/components/BalanceShow.vue";
import TimeShow from "@/components/TimeShow.vue";
import SchemaDetail from "./SalesRebateSchemaDetail.vue";
import SchemaNote from "./SalesRebateSchemaNote.vue";
import RebateService from "../services/RebateService";
import { salesTypeOptions } from "@/core/types/SalesTypes";

const accountSelected = ref(0);
const SchemaNoteRef = ref<any>(null);
const SchemaDetailRef = ref<any>(null);

const props = defineProps<{
  items: any;
  isLoading: boolean;
}>();

const emits = defineEmits<{
  (e: "refresh"): void;
  (e: "hasError"): void;
}>();

const dotClass = ref({
  0: "text-success",
  1: "text-primary",
  2: "text-danger",
  3: "text-info",
  4: "text-warning",
  5: "text-secondary",
});

const scheduleType = ref({
  0: "D",
  1: "P",
  3: "M",
});

let totalRebate = computed(() => {
  let total = props.items.reduce((total, item) => total + item.rebate, 0);

  if (
    total >
    salesTypeOptions.find((option) => option.value === props.items[0].salesType)
      ?.rebate
  ) {
    emits("hasError");
  }

  return total;
});

let totalAlpha = computed(() => {
  let total = props.items.reduce((total, item) => total + item.alphaRebate, 0);

  if (
    total >
    salesTypeOptions.find((option) => option.value === props.items[0].salesType)
      ?.alphaRebate
  ) {
    emits("hasError");
  }
  return total;
});

let totalPro = computed(() => {
  let total = props.items.reduce((total, item) => total + item.proRebate, 0);

  if (
    total >
    salesTypeOptions.find((option) => option.value === props.items[0].salesType)
      ?.proRebate
  ) {
    emits("hasError");
  }
  return total;
});

const deleteSchema = async (item: any) => {
  try {
    await RebateService.deleteSalesRebateSchema(item.id);
    emits("refresh");
  } catch (error) {
    console.log(error);
  }
};

const approve = async (item: any) => {
  try {
    await RebateService.putSalesRebateSchemaStatus(item.id);
    emits("refresh");
  } catch (e) {
    console.log(e);
  }
};

const selectedAccount = (id: number) => {
  if (accountSelected.value == id) {
    accountSelected.value = 0;
    return;
  }
  accountSelected.value = id;
};

const showSchemaDetail = (_item: any) => {
  SchemaDetailRef.value.show(_item);
};

const showSchemaNote = (_item: any) => {
  SchemaNoteRef.value.show(_item);
};
</script>

<style scoped>
.account-select {
  background-color: #ffffe0 !important;
}

.waiting-approve {
  background-color: rgba(254, 215, 215, 0.5) !important;
}

.total-error {
  background-color: rgba(254, 215, 215, 0.5) !important;
}
</style>
