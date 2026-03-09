<template>
  <!-- <div>
    <div v-if="!salesAccount">no sales account</div>
    <div v-else>
      <SalesCenterMenu activeMenuItem="lead" />
    </div> -->
  <!-- <SalesCenterMenu activeMenuItem="lead" /> -->
  <SalesLayout activeMenuItem="lead">
    <div class="card d-flex flex-1">
      <div class="card-header">
        <div class="card-title">{{ $t("title.salesLead") }}</div>
        <div class="card-toolbar"></div>
      </div>

      <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
        <table
          class="table align-middle table-row-bordered gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-uppercase gs-0">
              <!--                <th class=""></th>-->
              <th class="">{{ $t("fields.user") }}</th>
              <th class="">{{ $t("fields.email") }}</th>
              <th class="">{{ $t("fields.phone") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.hasAssigned") }}</th>
              <th class="">{{ $t("fields.updatedOn") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="">{{ $t("fields.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && leads.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else>
            <tr v-for="(item, index) in leads" :key="index">
              <!--                <td>-->
              <!--                  <input-->
              <!--                    class="w-20px h-20px"-->
              <!--                    type="checkbox"-->
              <!--                    v-model="selectedLeads"-->
              <!--                    :id="item.id"-->
              <!--                    :value="item"-->
              <!--                  />-->
              <!--                </td>-->
              <td>
                <div class="d-md-flex align-items-center">
                  <UserAvatar
                    :avatar="item.user?.avatar ?? ''"
                    :name="item.user?.displayName || item.name"
                    size="40px"
                    class="me-3"
                    side="client"
                    rounded
                  />
                  <span>
                    {{
                      item.user?.nativeName ||
                      item.user?.displayName ||
                      item.name
                    }}
                  </span>
                </div>
              </td>

              <td>{{ item.email }}</td>
              <td>{{ item.phoneNumber }}</td>
              <td>{{ $t(`type.leadStatus.${item.status}`) }}</td>
              <td>
                <template v-if="item.hasAssignedToSales"
                  ><i
                    class="fa-solid fa-check fa-xl"
                    style="color: #4ed06e"
                  ></i>
                </template>
                <template v-else>
                  <i class="fa-solid fa-xmark fa-xl" style="color: #d92626"></i>
                </template>
              </td>
              <td>
                <TimeShow type="inFields" :date-iso-string="item.createdOn" />
              </td>

              <td>
                <TimeShow type="inFields" :date-iso-string="item.updatedOn" />
              </td>
              <td>
                <span
                  class="cursor-pointer"
                  style="color: #7c8fa2"
                  @click="openLeadDetailsModal(item)"
                >
                  {{ $t("action.showDetails") }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
    <LeadDetailsModal ref="leadDetailsModalRef" />
  </SalesLayout>
  <!-- </div> -->
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import LeadDetailsModal from "@/projects/client/modules/sales/components/modal/LeadDetailsModal.vue";
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useStore } from "@/store";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import SalesLayout from "../components/SalesLayout.vue";
const leadDetailsModalRef = ref<InstanceType<typeof LeadDetailsModal>>();

const store = useStore();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);

const openLeadDetailsModal = (_lead) => {
  leadDetailsModalRef.value?.show(_lead);
};
const isLoading = ref(true);
const leads = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 10,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await SalesService.querySalesLeads(criteria.value);
    leads.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};
onMounted(async () => {
  moibleNavScroller(".sub-menu", ".scroll-to");
  moibleNavScroller(".ib-menu", ".scroll-to");
  await fetchData(1);
});
</script>

<style scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
</style>
