<template>
  <!-- <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
  <div v-else>
    <SalesCenterMenu activeMenuItem="new-customers" />
  </div> -->
  <SalesLayout activeMenuItem="new-customers">
    <div v-if="isMobile">
      <NewCustomersMobile />
    </div>
    <div class="card flex-1" v-else>
      <div class="card-header">
        <div class="card-title">{{ $t("title.incompleteCustomers") }}</div>
      </div>
      <div class="card-body overflow-auto" style="white-space: nowrap">
        <table class="table align-middle table-row-bordered gy-5">
          <thead>
            <tr class="text-center gs-0">
              <th class="text-start" width="*">
                {{ $t("fields.customer") }}
              </th>

              <th class="text-start">{{ $t("title.email") }}</th>

              <th class="text-start">{{ $t("fields.createdOn") }}</th>
              <th class="text-start">{{ $t("fields.status") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && data.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else>
            <tr v-for="(item, index) in data" :key="index">
              <td>
                <div>
                  <UserAvatar
                    :avatar="item.user?.avatar"
                    :name="item.user?.displayName"
                    size="40px"
                    class="me-3"
                    side="client"
                    rounded
                  />

                  <span>
                    {{ item.user?.displayName }}
                  </span>
                </div>
              </td>
              <td class="text-start">
                {{ item.user?.email }}
              </td>
              <td class="text-start">
                <TimeShow
                  :date-iso-string="item.verification.updatedOn"
                  type="inFields"
                />
              </td>

              <td class="text-start">
                <template v-if="!item.verification.isEmpty">
                  {{
                    $t("title.verification") +
                    " " +
                    $t(
                      `type.verificationStatus.${item.verification.status}`
                    ).toLowerCase()
                  }}
                </template>

                <template v-else>
                  {{
                    $t("title.verification") +
                    " " +
                    $t(`status.notStarted`).toLowerCase()
                  }}
                </template>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="fecthData" :criteria="criteria" />
      </div>
    </div>
  </SalesLayout>
</template>
<script setup lang="ts">
import { useStore } from "@/store";
import { ref, computed, onMounted, provide } from "vue";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../services/SalesService";
import NewCustomersMobile from "../components/newCustomers/NewCustomersMobile.vue";
import { isMobile } from "@/core/config/WindowConfig";
import SalesLayout from "../components/SalesLayout.vue";
const isLoading = ref(true);
const store = useStore();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const data = ref(<any>[]);
provide("data", data);
provide("isLoading", isLoading);
const criteria = ref({
  page: 1,
  size: 10,
  IsUnverified: true,
} as any);

const fecthData = async (_page: number) => {
  isLoading.value = true;

  criteria.value.page = _page;
  try {
    const res = await SalesService.queryAgentClientReferralHistory(
      criteria.value
    );
    data.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error: any) {
    MsgPrompt.error(error);
  }
};

onMounted(() => {
  fecthData(1);
});
</script>
