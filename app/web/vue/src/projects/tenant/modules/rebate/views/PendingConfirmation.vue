<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Source</th>
            <th>Target</th>
            <th>Create By</th>
            <th>Rule Name</th>
            <th>Update On</th>
            <th>Create On</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12"><scale-loader></scale-loader></td>
          </tr>
        </tbody>
        <tbody v-if="!isLoading && rebateRules.length === 0">
          <tr>
            <td colspan="12">{{ $t("tip.nodata") }}</td>
          </tr>
        </tbody>
        <TransitionGroup
          v-if="!isLoading && rebateRules.length != 0"
          tag="tbody"
          name="table-delete-fade"
          class="table-delete-fade-container fw-semibold"
        >
          <tr
            v-for="(item, index) in rebateRules"
            :key="item"
            :class="{ 'table-delete-fade-active': index === deleteIndex }"
          >
            <!--  -->
            <td>
              <span>{{ item.sourceAccount.name }}</span
              ><br />
              <span class="typeBadge"
                >Account No: {{ item.sourceAccount.accountNumber }}</span
              >
            </td>
            <td>
              <span>{{ item.targetAccount.name }}</span
              ><br />
              <span class="typeBadge">UID: {{ item.targetAccount.uid }}</span>
            </td>
            <td>{{ item.rebateRuleName }}</td>
            <td>{{ item.createdByName }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <button
                class="btn btn-light btn-success btn-sm me-3"
                @click="handleCheckRebate(item)"
              >
                {{ $t("action.check") }}
              </button>
            </td>
          </tr>
        </TransitionGroup>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import RebateService from "../services/RebateService";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { useRouter } from "vue-router";

const router = useRouter();
const isLoading = ref(true);
const deleteIndex = ref(-1);
const rebateRules = ref(Array<any>());

const criteria = ref({
  page: 1,
  size: 10,
  numPages: 1,
  total: 0,
  isConfirmed: false,
});

onMounted(async () => {
  await fetchData(1);
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const responseBody = await RebateService.queryDirectRebateRules(
      criteria.value
    );
    criteria.value = responseBody.criteria;
    criteria.value.isConfirmed = false;

    rebateRules.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const pageChange = (_page: number) => {
  fetchData(_page);
};

const handleCheckRebate = (item) => {
  router.push({
    name: "accountClients",
    query: {
      action: "checkRebate",
      rebateId: item.id,
      accountId: item.sourceAccount.id,
    },
  });
};
</script>

<style scoped>
.typeBadge {
  width: 43px;
  height: 20px;
  background: rgba(88, 168, 255, 0.1);
  border-radius: 8px;
  color: #4196f0;
  padding: 2px 8px;
  font-size: 12px;
  font-weight: 700;
}
</style>
