<template>
  <SimpleForm
    ref="rebateSchemaDetailShowRef"
    :title="'Schema Users'"
    :is-loading="isLoading"
    :width="1300"
    disable-footer
  >
    <div
      class="my-3 px-0 d-flex gap-1"
      style="border-bottom: 3px #ffd400 solid"
    >
      <span
        class="basic-tab"
        :class="{ 'active-tab': activeTab === tabType.direct }"
        @click="activeTab = tabType.direct"
      >
        Used by Direct Rules
      </span>
      <span
        class="basic-tab"
        :class="{ 'active-tab': activeTab === tabType.client }"
        @click="activeTab = tabType.client"
      >
        Used by Client Rules
      </span>
    </div>

    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.user") }}</th>
            <th class="">{{ $t("fields.role") }}</th>
            <th class="">{{ $t("fields.group") }}</th>
            <th class="">{{ $t("fields.type") }}</th>
            <th class="">{{ $t("fields.accountNumber") }}</th>
            <th class="cell-color">{{ $t("fields.ib") }}</th>
            <th class="cell-color">
              {{ $t("fields.salesWithCode") }}
            </th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && schemaUsers.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in schemaUsers" :key="index">
            <!--  -->
            <td class="d-flex align-items-center">
              <UserInfo url="#" :user="item.user" class="me-2" />
            </td>
            <td>{{ $t("type.accountRole." + item.role) }}</td>
            <td>{{ item.group }}</td>
            <td>{{ $t("type.account." + item.type) }}</td>
            <td>
              <span v-if="item.role == 400">{{
                item.tradeAccount.accountNumber
              }}</span>
              <span v-else class="typeBadge">UID: {{ item.uid }}</span>
            </td>
            <td class="cell-color">
              <IbSalesInfo
                url="#"
                :user="item.agentAccount.user"
                :uid="item.agentAccount.uid"
                class="me-2"
              />
            </td>
            <td class="cell-color">
              <IbSalesInfo
                url="#"
                :user="item.salesAccount.user"
                :code="item.salesAccount.code"
                :uid="item.salesAccount.uid"
                class="me-2"
              />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter
        @page-change="pageChange"
        :criteria="
          activeTab === tabType.direct ? directCriteria : clientCriteria
        "
      />
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import RebateService from "../services/RebateService";
import SimpleForm from "@/components/SimpleForm.vue";
import TableFooter from "@/components/TableFooter.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const tabType = {
  direct: "direct",
  client: "client",
};

const directCriteria = ref({
  page: 1,
  size: 6,
});

const clientCriteria = ref({
  page: 1,
  size: 6,
});

const activeTab = ref(tabType.direct);

const schemaId = ref(0);
const isLoading = ref(true);
const schemaUsers = ref([] as any);
const rebateSchemaDetailShowRef = ref<any>(null);

const pageChange = (page: number) => {
  if (activeTab.value === tabType.direct) {
    directCriteria.value.page = page;
    fetchData(page);
  } else {
    clientCriteria.value.page = page;
    fetchData(page);
  }
};

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;

  try {
    if (activeTab.value === tabType.direct) {
      directCriteria.value.page = selectedPage;
      const res = await RebateService.getSchemaUsedByDirectRule(
        schemaId.value,
        directCriteria.value
      );
      schemaUsers.value = res.data;
      directCriteria.value = res.criteria;
    } else {
      clientCriteria.value.page = selectedPage;
      const res = await RebateService.getSchemaUsedByClientRule(
        schemaId.value,
        clientCriteria.value
      );
      schemaUsers.value = res.data;
      clientCriteria.value = res.criteria;
    }

    isLoading.value = false;
  } catch (error: any) {
    MsgPrompt.error(error);
  }
};
const show = async (_id: number) => {
  schemaId.value = _id;
  activeTab.value = tabType.direct;
  fetchData(1);
  rebateSchemaDetailShowRef.value?.show();
};

const close = () => {
  rebateSchemaDetailShowRef.value?.hide();
};

watch(
  () => activeTab.value,
  () => {
    fetchData(1);
  }
);

defineExpose({ show });
</script>

<style>
.rebate-long-btn {
  width: 100%;
  height: 40px;
  font-size: 24px;
  border-radius: 8px;
  color: gray;
  background-color: white;
  border: 1px solid lightgray;
}

.rebate-long-btn:focus {
  background-color: lightgray;
}

.cell-color {
  background-color: rgb(255, 255, 224);
}

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

.basic-tab {
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 14px;
  border-radius: 5px 5px 0 0;
  width: 150px;
  height: 40px;
  border: 2px solid #ffd400;
  cursor: pointer;
  border-bottom: 0;
  transition: background-color 0.3s;

  @media (max-width: 768px) {
    flex: 1;
  }
}

.active-tab {
  background-color: #ffd400;
}
</style>
