<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="card mb-5 mb-xl-8">
      <div v-if="!props.partyId" class="card-header">
        <div class="card-title">
          <el-input
            v-model="criteria.infoKey"
            placeholder="Search Bank Info"
            @keyup.enter="fetchData(1)"
          >
            <template #append>
              <el-button :icon="Search" @click="fetchData(1)" /> </template
          ></el-input>
          <el-button class="ms-5" @click="reset">Reset</el-button>
        </div>
      </div>
      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.id") }}</th>
              <th>{{ $t("fields.info") }}</th>
              <th>{{ $t("fields.updatedOn") }}</th>
              <th>{{ $t("fields.createdOn") }}</th>
              <th>{{ $t("action.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading" style="height: 1500px">
            <tr>
              <td colspan="12"><scale-loader></scale-loader></td>
            </tr>
          </tbody>
          <tbody v-if="!isLoading && paymentInfos.length === 0">
            <tr>
              <td colspan="12">{{ $t("tip.nodata") }}</td>
            </tr>
          </tbody>
          <TransitionGroup
            v-if="!isLoading && paymentInfos.length != 0"
            tag="tbody"
            name="table-delete-fade"
            class="table-delete-fade-container text-gray-600 fw-semibold"
          >
            <tr
              v-for="(item, index) in paymentInfos"
              :key="item"
              :class="{ 'table-delete-fade-active': index === deleteIndex }"
            >
              <!--  -->
              <td class="d-flex align-items-center">
                <UserInfo
                  url="#"
                  :name="item.partyName"
                  :email="item.email"
                  :partyId="item.partyId"
                  class="me-2"
                />
              </td>
              <td>
                <div
                  class="key-value-pair"
                  v-for="(value, key) in item.info"
                  :key="key"
                >
                  <span class="key">{{ key }} : </span>
                  <span class="value">{{ value }}</span>
                </div>
              </td>
              <td><TimeShow :date-iso-string="item.updatedOn" /></td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td>
                <button
                  class="btn btn-light btn-success btn-sm me-3"
                  @click="showEditModal(item)"
                >
                  {{ $t("action.edit") }}
                </button>
                <button
                  class="btn btn-light btn-danger btn-sm"
                  @click="openConfirmPanel(item.id)"
                >
                  {{ $t("action.delete") }}
                </button>
              </td>
            </tr>
          </TransitionGroup>
        </table>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>

    <BankInfoDetailModal ref="EditBankInfoRef" @refresh="refresh" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import { useI18n } from "vue-i18n";
import TableFooter from "@/components/TableFooter.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Search } from "@element-plus/icons-vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import BankInfoDetailModal from "../modal/BankInfoDetail.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
const { t } = useI18n();
const isLoading = ref(true);
const deleteIndex = ref(-1);
const paymentInfos = ref(Array<any>());
const openConfirmBox = inject<any>(InjectKeys.OPEN_CONFIRM_MODAL);
const EditBankInfoRef = ref<any>(null);

const props = defineProps<{
  partyId?: any;
}>();

const criteria = ref({
  page: 1,
  size: 20,
  infoKey: "",
  partyId: "",
});

const reset = () => {
  criteria.value.infoKey = "";
  fetchData(1);
};

onMounted(async () => {
  fetchData(1);
});

const refresh = () => {
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  if (props.partyId) {
    criteria.value.partyId = props.partyId;
  } else {
    criteria.value.partyId = "";
  }

  try {
    var res = await PaymentService.getPaymentInformation(criteria.value);
    criteria.value = res.criteria;
    paymentInfos.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const openConfirmPanel = (_id: number) => {
  const _handler = () => PaymentService.deletePaymentInformation(_id);
  if (!_handler) {
    MsgPrompt.error(t("tip.fail"));
    return;
  }
  openConfirmBox(async () => {
    try {
      await _handler()
        .then(() => MsgPrompt.success())
        .then(() => {
          fetchData(criteria.value.page);
        });
    } catch (error) {
      MsgPrompt.error("聯繫 IT 人員");
    }
  });
};

const pageChange = (_page: number) => {
  fetchData(_page);
};

const showEditModal = (_item: any) => {
  EditBankInfoRef.value.show(_item);
};
</script>
<style scoped>
.table td,
.table th {
  white-space: nowrap;
}

.key-value-pair {
  display: flex;
  align-items: center;
  margin-bottom: 8px; /* Optional: Add some space between pairs */
}

.key {
  flex: 0 0 150px; /* Adjust the width as needed */
  font-weight: bold;
}

.value {
  flex: 1;
  padding-left: 8px; /* Optional: Add some space between key and value */
}
</style>
