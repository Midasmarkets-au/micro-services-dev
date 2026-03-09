<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-body">
      <button
        class="btn btn-light btn-primary btn-lg me-3 mb-9"
        @click="showExRateDetail(isUpdate.false)"
      >
        Add Symbol
      </button>
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Name</th>
            <th class="cell-color">From</th>
            <th class="cell-color">To</th>
            <th>Buy</th>
            <th>Sell</th>
            <th>Adjust</th>
            <th>Update On</th>
            <th>Create On</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody v-if="isLoading" style="height: 1500px">
          <tr>
            <td colspan="12"><scale-loader></scale-loader></td>
          </tr>
        </tbody>
        <tbody v-if="!isLoading && tableData.length === 0">
          <tr>
            <td colspan="12">{{ $t("tip.nodata") }}</td>
          </tr>
        </tbody>
        <TransitionGroup
          v-if="!isLoading && tableData.length != 0"
          tag="tbody"
          name="table-delete-fade"
          class="table-delete-fade-container text-gray-600 fw-semibold"
        >
          <tr
            v-for="(item, index) in tableData"
            :key="item"
            :class="{ 'table-delete-fade-active': index === deleteIndex }"
          >
            <td>{{ item.name }}</td>
            <td class="cell-color">{{ item.fromCurrencyCode }}</td>
            <td class="cell-color">{{ item.toCurrencyCode }}</td>
            <td>{{ item.buyingRate }}</td>
            <td>{{ item.sellingRate }}</td>
            <td>{{ item.adjustRate }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <button
                class="btn btn-light btn-success btn-sm me-3"
                @click="showExRateDetail(isUpdate.true, item)"
              >
                {{ $t("action.details") }}
              </button>
            </td>
          </tr>
        </TransitionGroup>
      </table>
      <!-- <TableFooter @page-change="pageChange" :criteria="criteria" /> -->
    </div>
    <ExchangeRateModal ref="ExchangeRateModalRef" @refresh="refresh" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import RebateService from "../services/RebateService";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import ExchangeRateModal from "../components/ExchangeRateModal.vue";

const criteria = ref<any>({
  page: 1,
  size: 100,
  numPages: 1,
  total: 0,
});

const isUpdate = ref({
  true: true,
  false: false,
});

const isLoading = ref(true);
const deleteIndex = ref(-1);
const tableData = ref(Array<any>());
const ExchangeRateModalRef = ref<InstanceType<typeof ExchangeRateModal>>();

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const responseBody = await RebateService.getExchangeRate(criteria.value);
    criteria.value = responseBody.criteria;
    tableData.value = responseBody.data;
  } catch (error) {
    // console.log(error);
  } finally {
    isLoading.value = false;
  }
};
const refresh = () => {
  fetchData(criteria.value.page);
};
const showExRateDetail = (_isUpdate: boolean, _item?: object) => {
  ExchangeRateModalRef.value?.show(_isUpdate, _item);
};
onMounted(async () => {
  fetchData(1);
});
</script>
<style scoped>
.cell-color {
  background-color: rgb(255, 255, 224);
}
</style>
