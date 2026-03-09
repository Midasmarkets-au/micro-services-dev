<template>
  <div></div>
  <div>
    <hr />
    <div class="d-flex justify-content-end align-items center">
      <el-switch
        v-model="rebateType"
        class="mb-2"
        size="large"
        style="--el-switch-on-color: #0275d8; --el-switch-off-color: #13ce66"
        active-text="Direct Rebate"
        inactive-text="Level Rebate"
        active-value="1"
        inactive-value="2"
        @click="updateDistributionType"
      />
    </div>
    <div class="fs-3 text-center mb-3">
      <span v-if="rebateType == RebateDistributionTypes.Direct">
        <span>Direct Rebate Setting ( 直接反佣設置 )</span>
        <div class="row fs-4 text-center mb-5 mt-3" v-if="showSchemaTotalError">
          <div
            style="
              padding: 5px;
              background-color: #ffecec;
              color: #9f005b;
              border-radius: 10px;
            "
          >
            Total Value Exceed Base Schema
          </div>
        </div>
      </span>
      <span v-if="rebateType == RebateDistributionTypes.Allocation">
        Level Rebate Setting ( 層級反佣設置 )
      </span>
    </div>

    <template v-if="!isLoading">
      <div
        class="row fw-bold fs-5"
        style="background-color: #f5f5f5; padding: 10px"
      >
        <div class="col-2">Role</div>
        <div class="col-2">Group</div>
        <div class="col-2">IB Name</div>
        <div class="col-2">IB Account</div>
        <div class="col-4">Category</div>
      </div>
      <div class="row fs-5" style="padding: 10px">
        <div class="col-2">
          {{ $t("type.accountRole." + accountDetails?.role) }}
        </div>
        <div class="col-2">{{ accountInfo.agentAccount.group }}</div>
        <div class="col-2">{{ accountInfo.agentAccount.name }}</div>
        <div class="col-2">
          {{ accountInfo.agentAccount.uid }}({{ accountInfo.agentAccount.id }})
        </div>
        <div class="col-4">--</div>
      </div>
      <div
        class="row fw-bold fs-5"
        style="background-color: #f5f5f5; padding: 10px"
      >
        <div class="col-2">Sales</div>
        <div class="col-2">Sales Account</div>
        <div class="col-2">Sales Code</div>
        <div class="col-2">Terms</div>
        <div class="col-4">
          <template v-if="rebateType == RebateDistributionTypes.Direct"
            >Base Schema</template
          >
          <template v-else-if="rebateType == RebateDistributionTypes.Allocation"
            >Rebate Setting</template
          >
        </div>
      </div>
      <div class="row fs-5" style="padding: 10px">
        <div class="col-2">{{ accountInfo.salesAccount.name }}</div>
        <div class="col-2">
          {{ accountInfo.salesAccount.uid }}({{ accountInfo.salesAccount.id }})
        </div>
        <div class="col-2">{{ accountInfo.salesAccount.code }}</div>
        <div class="col-2">--</div>
        <div class="col-4">
          <template v-if="rebateType == RebateDistributionTypes.Direct"
            ><el-select
              v-model="baseSchemaInfo.id"
              placeholder="Please enter a keyword"
              @change="setBaseSchema"
            >
              <el-option
                v-for="item in baseSchemaInfo.list"
                :key="item.value"
                :label="item.key"
                :value="item.value"
              /> </el-select
          ></template>
          <template
            v-else-if="rebateType == RebateDistributionTypes.Allocation"
          >
            <span class="badge">{{
              pipCommissionDecision.commission == 0
                ? pipCommissionDecision.pips == 0
                  ? "No Setting"
                  : "Pips"
                : "Commission"
            }}</span>
            =>
            {{
              pipCommissionDecision.commission == 0
                ? pipCommissionDecision.pips == 0
                  ? "No Setting"
                  : pipCommissionDecision.pips
                : pipCommissionDecision.commission
            }}</template
          >
        </div>
      </div>
    </template>

    <template v-if="rebateType == RebateDistributionTypes.Direct">
      <hr />

      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-muted text-uppercase">
            <th>Target Uid</th>
            <th>Rebate Name</th>
            <th>Update On</th>
            <th>Create On</th>
            <th>{{ $t("fields.createdBy") }}</th>
            <th>{{ $t("fields.confirmedBy") }}</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody v-if="isLoading" style="height: 150px">
          <tr>
            <td colspan="12"><scale-loader></scale-loader></td>
          </tr>
        </tbody>
        <tbody
          v-if="!isLoading && tableData.length === 0"
          style="height: 150px"
        >
          <NoDataBox />
        </tbody>
        <tbody>
          <tr v-for="(item, index) in tableData" :key="index">
            <td>{{ item.targetAccountUid }}</td>
            <td>{{ item.rebateRuleName }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>{{ item.createdByName }}</td>
            <td>
              <template v-if="item.confirmedByName !== ''">{{
                item.confirmedByName
              }}</template>
              <button
                v-else
                class="btn btn-light btn-info btn-sm me-3"
                :disabled="user.name === item.createdByName"
                @click="openConfirmDirectRebateRuleModal(item)"
              >
                {{ $t("action.confirm") }}
              </button>
            </td>
            <td>
              <button
                class="btn btn-light btn-success btn-sm me-3"
                @click="showRebateRuleDetail(item.id)"
              >
                {{ $t("action.details") }}
              </button>
              <button
                class="btn btn-light btn-danger btn-sm me-3"
                @click="deleteRebateRule(item.id)"
              >
                {{ $t("action.delete") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <hr />

      <div v-if="showTargetAccountSearch" class="mb-5">
        <div style="border: 1px dashed #ccc; padding: 15px; border-radius: 5px">
          <h3 class="mb-5">
            Target Account ( Select one or press ENTER to get all results )
          </h3>
          <SearchFilter
            :custom-search-handler="handleSearchTradeAccount"
            defaultCriteria="{ page: 1, pageSize: 100 }"
            @get-results-ids="(ids) => showTargetAccounts(ids)"
          />
          <div class="card-body py-4">
            <table
              class="table align-middle table-row-dashed fs-6 gy-5"
              id="table_accounts_requests"
            >
              <thead>
                <tr
                  class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                >
                  <th class="">{{ $t("fields.user") }}</th>
                  <th class="">{{ $t("fields.role") }}</th>
                  <th class="">{{ $t("fields.group") }}</th>
                  <th class="">{{ $t("fields.type") }}</th>
                  <th class="">{{ $t("fields.accountNumber") }}</th>
                  <th class="cell-color">{{ $t("fields.ib") }}</th>
                  <th class="cell-color">
                    {{ $t("fields.salesWithCode") }}
                  </th>
                  <th class="text-center">{{ $t("action.action") }}</th>
                </tr>
              </thead>

              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!isLoading && searchList.length === 0">
                <NoDataBox />
              </tbody>

              <tbody v-else class="fw-semibold text-gray-900">
                <tr v-for="(item, index) in searchList" :key="index">
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
                  <td class="text-center">
                    <button
                      class="btn btn-light btn-success btn-sm me-3"
                      @click="selectTargetAccountFn(item)"
                    >
                      {{ $t("action.select") }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
            <!-- <TableFooter @page-change="pageChange" :criteria="criteria" /> -->
          </div>
        </div>
      </div>

      <div v-if="showAddRebateRuleForm" class="mb-5">
        <div style="border: 1px dashed #ccc; padding: 15px; border-radius: 5px">
          <rebateForm
            ref="schemaDetailRef"
            :rebateType="rebateType"
            :accountDetails="accountDetails"
            :fromRebateRuleDetailId="fromRebateRuleDetailId"
            :targetAccount="selectedTargetAccount"
            @changeTargetAccountFn="changeTargetAccountFn"
            @clearSearchResult="clearSearchResult"
            @refresh="refresh"
          ></rebateForm>
        </div>
      </div>
    </template>
    <template v-if="rebateType == RebateDistributionTypes.Allocation">
      <rebateForm
        ref="schemaDetailRef"
        :rebateType="rebateType"
        :accountDetails="accountDetails"
        :fromRebateRuleDetailId="
          accountDetails.rebateClientRule.rebateDirectSchemaId
        "
        :targetAccount="selectedTargetAccount"
        @changeTargetAccountFn="changeTargetAccountFn"
        @clearSearchResult="clearSearchResult"
        @refresh="refresh"
        @updateRebateRuleDetailId="updateRebateRuleDetailId"
      ></rebateForm>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import RebateService from "../../rebate/services/RebateService";
import rebateForm from "./form/RebateForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../services/AccountService";
import SearchFilter from "@/components/SearchFilter.vue";
import { useRoute } from "vue-router";
import { useStore } from "@/store";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { RebateDistributionTypes } from "@/core/types/RebateDistributionTypes";

const route = useRoute();
const store = useStore();
const user = ref(store.state.AuthModule.user);
const isLoading = ref(true);
const tableData = ref(Array<any>());
const accountInfo = ref({} as any);
const showAddRebateRuleForm = ref(false);
const showTargetAccountSearch = ref(true);
const searchList = ref(Array<any>());
const fromRebateRuleDetailId = ref(-1);
const selectedTargetAccount = ref({});
const schemaDetailRef = ref<any>(null);
const showSchemaTotalError = ref(false);
const pipCommissionDecision = ref({} as any);

const openConfirmModal = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

const handleSearchTradeAccount = ref((q: string) =>
  AccountService.fuzzySearchAccountByCriteria({
    keywords: q,
    // hasTradeAccount: true,
  })
);

const baseSchemaInfo = ref<any>({
  loading: false,
  id: "",
});

const props = defineProps<{
  accountDetailsInPage: any;
}>();

const accountDetails = ref(props.accountDetailsInPage);
console.log("account info", accountInfo.value);
const rebateType = ref(
  props.accountDetailsInPage.rebateClientRule.distributionType.toString()
);

const criteria = ref<any>({
  page: 1,
  SourceAccountId: accountDetails.value.id,
});

const refresh = () => {
  fetchData(criteria.value.page);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    const responseBody = await RebateService.getRebateDirectRule(
      criteria.value
    );

    const temp = await AccountService.queryAccounts({
      id: accountDetails.value.id,
    });
    accountInfo.value = temp.data[0];

    criteria.value = responseBody.criteria;
    tableData.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }

  const schemaList = ref([] as any);
  tableData.value.forEach((item) => schemaList.value.push(item.rebateRuleId));
  calculateBaseSchema(schemaList.value);
};

const selectTargetAccountFn = (_item: any) => {
  selectedTargetAccount.value = _item;
  showAddRebateRuleForm.value = true;
  showTargetAccountSearch.value = false;
};

const changeTargetAccountFn = () => {
  showAddRebateRuleForm.value = false;
  showTargetAccountSearch.value = true;
};

const clearSearchResult = () => {
  searchList.value = [];
  fromRebateRuleDetailId.value = -1;
  showAddRebateRuleForm.value = false;
  showTargetAccountSearch.value = true;
};

const showRebateRuleDetail = async (_id: any) => {
  showAddRebateRuleForm.value = false;
  showTargetAccountSearch.value = true;

  fromRebateRuleDetailId.value = _id;

  const res1 = await RebateService.getRebateDirectRuleById(_id);
  const res2 = await AccountService.queryAccounts({
    id: res1.targetAccountId,
  });
  selectedTargetAccount.value = res2.data[0];

  showAddRebateRuleForm.value = true;
  showTargetAccountSearch.value = false;
};

const showTargetAccounts = async (results: { ids: any; criteria: any }) => {
  const res = await AccountService.queryAccounts({
    ids: results.ids,
  });

  searchList.value = res.data;
};

const deleteRebateRule = async (_id: any) => {
  MsgPrompt.warning("Are you sure to delete?").then(async () => {
    try {
      await RebateService.deleteRebateDirectRuleById(_id);
      showAddRebateRuleForm.value = false;
      showTargetAccountSearch.value = true;
      refresh();
    } catch (error) {
      MsgPrompt.error(error);
    }
  });
};

const calculateBaseSchema = async (_schemas: []) => {
  const compareData = ref({} as any);
  if (!accountDetails.value.tradeAccount.rebateBaseSchemaId) return;
  const baseSchema = await RebateService.getRebateBaseSchemaById(
    accountDetails.value.tradeAccount.rebateBaseSchemaId
  );

  baseSchema.rebateBaseSchemaItems.forEach(function (item) {
    compareData.value[item.symbolCode] = {
      rate: item.rate,
      pips: item.pips,
      commission: item.commission,
    };
  });

  try {
    _schemas.forEach(async (id) => {
      const res = await RebateService.getRebateSchema(id);
      for (let item of res.rebateDirectSchemaItems) {
        compareData.value[item.symbolCode].rate =
          compareData.value[item.symbolCode].rate - item.rate;
        compareData.value[item.symbolCode].pips =
          compareData.value[item.symbolCode].pips - item.pips;
        compareData.value[item.symbolCode].commission =
          compareData.value[item.symbolCode].commission - item.commission;
        if (
          compareData.value[item.symbolCode].rate < 0 ||
          compareData.value[item.symbolCode].pips < 0 ||
          compareData.value[item.symbolCode].commission < 0
        ) {
          showSchemaTotalError.value = true;
          // MsgPrompt.warning("Total rebate exceed base schema!");
          break;
        } else {
          showSchemaTotalError.value = false;
        }
      }
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const setBaseSchema = async () => {
  try {
    await RebateService.putClientBaseSchema(
      accountDetails.value.id,
      baseSchemaInfo.value.id
    );
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const setupDropdownOptions = async () => {
  const resRebate = await RebateService.getBaseSchemaList();
  baseSchemaInfo.value["list"] = resRebate.data;
  baseSchemaInfo.value.id = baseSchemaInfo.value["list"].find(
    (item) =>
      item.value === accountDetails.value.tradeAccount.rebateBaseSchemaId
  )?.key;
};

const openConfirmDirectRebateRuleModal = (_item) => {
  openConfirmModal?.(
    () =>
      RebateService.putConfirmDirectRebateRule(_item.id)
        .then(() => MsgPrompt.success("Confirm successfully!"))
        .then(() => fetchData(1)),
    void 0,
    {
      confirmText: "Are you sure to confirm this direct rebate rule?",
      confirmColor: "info",
    }
  );
};

const updateDistributionType = async () => {
  try {
    await RebateService.putRebateAllocationRule(
      accountDetails.value.rebateClientRule.id,
      { distributionType: rebateType.value }
    );
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const updateRebateRuleDetailId = (_id: any) => {
  console.log(_id);
  accountDetails.value.rebateClientRule.rebateDirectSchemaId = _id;
};

onMounted(async () => {
  isLoading.value = true;
  console.log("on mounted: ", props.accountDetailsInPage);
  searchList.value = [];
  showAddRebateRuleForm.value = false;
  showTargetAccountSearch.value = true;

  if (route.query.accountId) {
    accountDetails.value = await AccountService.getAccountDetailById(
      parseInt(route.query.accountId as string)
    );
  }

  if (accountDetails.value.rebateClientRule.schema == "{}") {
    pipCommissionDecision.value = {
      commission: 0,
      pips: 0,
    };
  } else {
    console.log("schema", accountDetails.value.rebateClientRule.schema);
    pipCommissionDecision.value = JSON.parse(
      accountDetails.value.rebateClientRule.schema
    ).find((obj) => obj.accountType === accountDetails.value.type);
  }

  await fetchData(1);
  await setupDropdownOptions();
  if (route.query.rebateId) {
    await showRebateRuleDetail(parseInt(route.query.rebateId as string));
  }
});
</script>

<style scoped>
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

.badge {
  height: 20px;
  border-radius: 8px;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 700;
  background: rgba(88, 168, 255, 0.1);
  color: #4196f0;
  font-size: 15px;
}
</style>
