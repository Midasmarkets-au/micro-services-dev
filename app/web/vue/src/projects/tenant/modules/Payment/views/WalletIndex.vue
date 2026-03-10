<template>
  <div class="row">
    <div
      class="col-sm-4 col-xl-3 mb-xl-10"
      v-for="(item, index) in walletStat"
      :key="index"
    >
      <div class="card h-lg-100">
        <div
          class="card-body d-flex justify-content-between align-items-start flex-column"
        >
          <div class="d-flex flex-column my-7">
            <span class="fw-semibold fs-3x text-gray-800 lh-1 ls-n2"
              ><BalanceShow
                :currency-id="item.currencyId"
                :balance="item.balance"
            /></span>
            <div class="m-0">
              <span class="fw-semibold fs-6 text-gray-400">{{
                $t("type.currency." + item.currencyId)
              }}</span>
              <span class="ms-2 fw-semibold fs-6 text-gray-400">{{
                $t(`type.fundType.${item.fundType}`)
              }}</span>
              <span
                class="btn btn-sm cursor-pointer"
                @click="onViewListClicked(item)"
                >{{ $t("action.viewList") }}</span
              >
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="card">
    <div class="card-header d-flex align-items-center justify-content-start">
      <div class="d-flex">
        <div class="ms-5 d-flex align-items-center">
          <div class="me-3">Wallet ID</div>
          <el-checkbox
            v-model="criteria.hasBalance"
            class="me-3"
            @change="fetchData(1)"
            :disabled="isLoading"
            >Has Balance</el-checkbox
          >
          <el-select
            v-model="selectedOption"
            placeholder="Select option"
            class="w-150px me-3"
            :disabled="isLoading"
          >
            <el-option label="Wallet ID" value="id"></el-option>
            <el-option label="Email" value="email"></el-option>
            <el-option label="Account UID" value="accountUid"></el-option>
          </el-select>
          <el-input class="w-200px" v-model="inputValue" :disabled="isLoading">
          </el-input>
        </div>
        <div class="ms-5">
          <el-select
            class="w-125px"
            v-model="criteria.currencyId"
            :placeholder="$t('fields.currency')"
            :disabled="isLoading"
          >
            <el-option label="-- All --" value="" />
            <el-option
              v-for="item in currencySelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>
        <div class="ms-5">
          <el-select
            class="w-125px"
            v-model="criteria.fundType"
            :placeholder="$t('fields.fundType')"
            :disabled="isLoading"
          >
            <el-option label="-- All --" value="" />
            <el-option
              v-for="item in fundTypeSelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div class="d-flex ms-5">
          <el-button type="primary" @click="search" plain :disabled="isLoading"
            >{{ $t("action.search") }}
          </el-button>
          <el-button @click="reset" plain :disabled="isLoading"
            >{{ $t("action.clear") }}
          </el-button>
          <el-button
            type="success"
            @click="openWalletExportModal"
            plain
            :disabled="isLoading"
            >{{ $t("action.export") }}
          </el-button>
        </div>
      </div>
    </div>
    <div class="card-body py-4">
      <div class="table-responsive">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.id") }}</th>
              <th class="">{{ $t("fields.client") }}</th>
              <th class="">
                {{ $t("fields.currency") }}
              </th>
              <th class="">{{ $t("fields.fundType") }}</th>
              <th class="">{{ $t("fields.isPrimary") }}</th>
              <th class="">{{ $t("fields.balance") }}</th>
              <th class="cell-color text-center">
                {{ $t("action.action") }}
              </th>
            </tr>
          </thead>
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && wallets.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in wallets" :key="index">
              <td>{{ item.id }}</td>
              <td class="d-flex align-items-center">
                <UserInfo
                  v-if="item.user"
                  :user="item.user"
                  :isAdmin="
                    item.user.isAdmin == undefined ? false : item.user.isAdmin
                  "
                  class="me-2"
                />
              </td>
              <td>
                {{ $t(`type.currency.${item.currencyId}`) }}
              </td>
              <td>
                {{ $t(`type.fundType.${item.fundType}`) }}
                <el-button
                  type="primary"
                  size="small"
                  class="ms-1"
                  plain
                  :icon="Wallet"
                  circle
                  :disabled="isLoading"
                  @click="changeFundType(item)"
                />
              </td>
              <td>
                {{ item.isPrimary ? $t(`action.yes`) : $t(`action.no`) }}
              </td>
              <td>
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.balance"
                />
              </td>
              <td class="text-center">
                <el-button
                  @click="viewWalletTransactions(item.id, item.currencyId)"
                  :disabled="isLoading"
                  >{{ $t(`action.viewTransactions`) }}</el-button
                >
                <el-button
                  type="success"
                  @click="showCreateRefund(item.id)"
                  :disabled="isLoading"
                  >{{ $t("action.createRefund") }}</el-button
                >
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <WalletExportModal ref="walletExportModalRef" />
    <WalletDetail ref="walletDetailRef" />
    <CreateRefund ref="createRefundRef" @event-Submit="fetchData(1)" />
    <ChangeWalletFundType
      ref="changeWalletFundTypeRef"
      @event-Submit="fetchData(1)"
    />
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import PaymentService, {
  WalletStatisticViewModel,
} from "../services/PaymentService";
import TableFooter from "@/components/TableFooter.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import WalletExportModal from "@/projects/tenant/modules/Payment/modal/WalletExportModal.vue";
import { useI18n } from "vue-i18n";
import { FundTypes, getAllFundTypeSelections } from "@/core/types/FundTypes";
import WalletDetail from "@/projects/tenant/modules/users/components/modal/walletDetail.vue";
import CreateRefund from "../components/modal/CreateRefund.vue";
import ChangeWalletFundType from "../modal/ChangeWalletFundType.vue";
import { Wallet } from "@element-plus/icons-vue";
const isLoading = ref(false);
const isStatLoading = ref(false);
const wallets = ref([]);
const walletStat = ref(Array<WalletStatisticViewModel>());
const walletDetailRef = ref<InstanceType<typeof WalletDetail>>();
const createRefundRef = ref<InstanceType<typeof CreateRefund>>();
const changeWalletFundTypeRef =
  ref<InstanceType<typeof ChangeWalletFundType>>();
const walletExportModalRef = ref<any>(null);
const { t } = useI18n();
const selectedOption = ref("id");
const inputValue = ref("");
const criteria = ref({ hasBalance: true } as any);
const initCriteria = (c?: any) => {
  criteria.value = c ?? {
    ...criteria.value,
    page: 1,
    size: 20,
    sortField: "balance",
  };
};
initCriteria();
const fundTypeSelections = getAllFundTypeSelections([
  FundTypes.Wire,
  FundTypes.Ips,
]);

onMounted(async () => {
  isLoading.value = true;
  await fetchStat();
  await fetchData(1);
});

const openWalletExportModal = () => {
  walletExportModalRef.value?.show(criteria.value, currencySelections.value);
};

const viewWalletTransactions = (walletId: number, currencyId: number) => {
  walletDetailRef.value?.show(walletId, currencyId);
};

const showCreateRefund = (walletId: number) => {
  createRefundRef.value?.show(walletId);
};

const changeFundType = (item: any) => {
  changeWalletFundTypeRef.value?.show(item);
};
const onViewListClicked = (item) => {
  criteria.value.currencyId = item.currencyId;
  criteria.value.fundType = item.fundType;
  fetchData(1);
};

const reset = () => {
  criteria.value = { page: 1, size: 20, sortField: "balance" };
  inputValue.value = "";
  fetchData(1);
};

const search = () => {
  delete criteria.value.id;
  delete criteria.value.email;
  delete criteria.value.accountUid;
  criteria.value[selectedOption.value] = inputValue.value;
  fetchData(1);
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  try {
    isLoading.value = true;
    const result = await PaymentService.queryWallet(criteria.value);
    wallets.value = result.data;
    criteria.value = result.criteria;

    if (criteria.value.id == 0) criteria.value.id = null;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const currencySelections = computed(() => {
  const currencyIds = [
    ...new Set(walletStat.value.map((item) => item.currencyId)),
  ];
  return currencyIds.map((item) => ({
    label: t(`type.currency.${item}`),
    value: item,
  }));
});

const fetchStat = async () => {
  isStatLoading.value = true;
  try {
    isStatLoading.value = true;
    walletStat.value = await PaymentService.queryWalletStatistic();
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isStatLoading.value = false;
  }
};
</script>

<style scoped>
.table-responsive {
  overflow-x: scroll !important;
}
.table {
  min-width: 100%;
  width: max-content;
}
.table td,
.table th {
  white-space: nowrap;
}
</style>
