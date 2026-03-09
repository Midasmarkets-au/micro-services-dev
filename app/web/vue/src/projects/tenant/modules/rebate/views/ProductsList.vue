<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Name</th>
            <th>Code</th>
            <th>Pip Rate</th>
            <th>Update On</th>
            <th>Create On</th>
            <th class="text-center">Action</th>
          </tr>
        </thead>

        <tbody v-if="isLoading" style="height: 300px">
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
            <!-- <td>
              <el-dropdown trigger="click">
                <el-button type="primary" class="btn btn-secondary btn-sm">
                  {{ $t("action.action")
                  }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item
                      @click="linkExchangeRate(item.id, item.rebatePipFormula)"
                    >
                      Link Exchange Rate
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </td> -->
            <td>{{ item.name }}</td>
            <td>{{ item.code }}</td>
            <td>
              <span v-if="item.rebatePipFormula" class="linkExChange me-3"
                >Linked</span
              >
            </td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td class="text-center">
              <button
                class="btn btn-light btn-success btn-sm me-3"
                @click="linkExchangeRate(item.id, item.rebatePipFormula)"
              >
                Link Exchange Rate
              </button>
            </td>
          </tr>
        </TransitionGroup>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <LinkExchangeRateModal ref="LinkExchangeRateModalRef" @refresh="refresh" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import TimeShow from "@/components/TimeShow.vue";

import RebateService from "../services/RebateService";
import LinkExchangeRateModal from "../components/LinkExchangeRateModal.vue";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { ArrowDown } from "@element-plus/icons-vue";

const isLoading = ref(true);
const deleteIndex = ref(-1);
const tableData = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 20,
  numPages: 1,
  total: 0,
});

const LinkExchangeRateModalRef =
  ref<InstanceType<typeof LinkExchangeRateModal>>();

onMounted(async () => {
  fetchData(1);
});

const refresh = () => {
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    const responseBody = await RebateService.getProductsList(criteria.value);
    criteria.value = responseBody.criteria;
    tableData.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const linkExchangeRate = (_sid: number, _item: object) => {
  LinkExchangeRateModalRef.value?.show(_sid, _item);
};
</script>

<style>
.linkExChange {
  width: 43px;
  height: 20px;
  background: rgba(88, 168, 255, 0.1);
  border-radius: 8px;
  color: #4196f0;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 700;
}
</style>
