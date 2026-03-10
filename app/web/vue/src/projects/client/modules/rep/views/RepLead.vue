<template>
  <div>
    <div v-if="!repAccount">no rep account</div>
    <div v-else>
      <div class="overflow-auto rep-nav">
        <div class="sub-menu d-flex">
          <router-link to="/rep" class="sub-menu-item">{{
            $t("title.customer")
          }}</router-link>
          <router-link to="/rep/trade" class="sub-menu-item">{{
            $t("title.trade")
          }}</router-link>
          <router-link to="/rep/transaction" class="sub-menu-item">{{
            $t("title.transfer")
          }}</router-link>
          <router-link to="/rep/deposit" class="sub-menu-item">{{
            $t("title.deposit")
          }}</router-link>
          <router-link to="/rep/withdrawal" class="sub-menu-item">{{
            $t("title.withdrawal")
          }}</router-link>
          <!-- <router-link to="/rep/lead" class="sub-menu-item active">{{
            $t("title.repLeadSystem")
          }}</router-link> -->
        </div>
      </div>

      <div class="card">
        <div class="card-header">
          <div class="card-title">{{ $t("title.repLead") }}</div>
          <div class="card-toolbar">
            <button
              @click="openAssignSalesModal"
              v-if="selectedLeads.length > 0"
              class="btn btn-sm btn-primary mx-4 d-flex align-items-center gap-2"
            >
              <i class="fa-solid fa-user-plus fa-sm"></i>
              <span>{{ $t("action.assign") }}</span>
            </button>

            <el-switch
              class="me-5"
              v-model="criteria.isArchived"
              size="large"
              width="90"
              :active-value="0"
              :inactive-value="1"
              inline-prompt
              style="
                --el-switch-on-color: #0053ad;
                --el-switch-off-color: #b1b1b1;
              "
              :active-text="$t('status.active')"
              :inactive-text="$t('status.archived')"
              @change="fetchData(1)"
            />

            <button
              @click="openCreateLeadModal"
              class="btn btn-sm btn-secondary d-flex align-items-center gap-2"
            >
              <i class="fa-solid fa-plus fa-sm"></i>
              <span>{{ $t("action.create") }}</span>
            </button>
          </div>
        </div>

        <div class="card-body">
          <table
            class="table align-middle table-row-bordered gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr class="text-start text-muted text-uppercase gs-0">
                <th class="">{{ $t("fields.user") }}</th>
                <th class="">{{ $t("fields.email") }}</th>
                <th class="">{{ $t("fields.phone") }}</th>
                <th class="">{{ $t("fields.source") }}</th>
                <th class="">{{ $t("fields.status") }}</th>
                <th class="">{{ $t("fields.hasAssigned") }}</th>
                <th class="">{{ $t("fields.updatedOn") }}</th>
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
                <td>{{ $t(`type.leadSource.${item.sourceType}`) }}</td>
                <td>{{ $t(`type.leadStatus.${item.status}`) }}</td>
                <td>
                  <template v-if="item.hasAssignedToSales"
                    ><i
                      class="fa-solid fa-check fa-xl"
                      style="color: #4ed06e"
                    ></i>
                  </template>
                  <template v-else>
                    <i
                      class="fa-solid fa-xmark fa-xl"
                      style="color: #d92626"
                    ></i>
                  </template>
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
    </div>
    <AssignToSalesModal
      ref="assignToSalesModalRef"
      @assign-success="assignSalesSuccess"
    />
    <LeadDetailsModal ref="leadDetailsModalRef" @update="assignSalesSuccess" />
    <LeadCreateModal
      ref="leadCreateModalRef"
      @create-success="assignSalesSuccess"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFooter from "@/components/TableFooter.vue";
import RepService from "../services/RepService";
import { useStore } from "@/store";
import TimeShow from "@/components/TimeShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserAvatar from "@/components/UserAvatar.vue";
import AssignToSalesModal from "@/projects/client/modules/rep/components/modal/AssignToSalesModal.vue";
import LeadDetailsModal from "@/projects/client/modules/rep/components/modal/LeadDetailsModal.vue";
import LeadCreateModal from "@/projects/client/modules/rep/components/modal/LeadCreateModal.vue";
import { LeadIsArchivedTypes } from "@/core/types/LeadIsArchivedTypes";

const store = useStore();
const repAccount = computed(() => store.state.RepModule.repAccount);
const selectedLeads = ref(Array<any>());
const assignToSalesModalRef = ref<InstanceType<typeof AssignToSalesModal>>();
const leadDetailsModalRef = ref<InstanceType<typeof LeadDetailsModal>>();
const leadCreateModalRef = ref<InstanceType<typeof LeadCreateModal>>();

const isLoading = ref(true);
const leads = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 10,
  isArchived: LeadIsArchivedTypes.Unarchived,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await RepService.queryRepLeads(criteria.value);
    leads.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const openAssignSalesModal = () => {
  assignToSalesModalRef.value?.show(selectedLeads.value);
};

const openLeadDetailsModal = (_lead) => {
  leadDetailsModalRef.value?.show(_lead);
};

const openCreateLeadModal = () => {
  leadCreateModalRef.value?.show();
};

const assignSalesSuccess = () => {
  selectedLeads.value = [];
  fetchData(1);
};

onMounted(async () => {
  await fetchData(1);
});
</script>

<style scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
</style>
