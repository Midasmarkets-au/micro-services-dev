<template>
  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div v-else>
    <template v-if="props.rebateType == RebateDistributionTypes.Direct">
      <h2>Selected Target Account</h2>
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
            <th>{{ $t("fields.ib") }}</th>
            <th>
              {{ $t("fields.salesWithCode") }}
            </th>
            <th v-if="!isViewDetail" class="text-center">
              {{ $t("action.action") }}
            </th>
          </tr>
        </thead>
        <tbody class="fw-semibold text-gray-900">
          <tr>
            <td class="d-flex align-items-center">
              <UserInfo url="#" :user="targetAccount.user" class="me-2" />
            </td>
            <td>{{ $t("type.accountRole." + targetAccount.role) }}</td>
            <td>{{ targetAccount.group }}</td>
            <td>{{ $t("type.account." + targetAccount.type) }}</td>
            <td>
              <span v-if="targetAccount.role == 400">{{
                targetAccount.tradeAccount.accountNumber
              }}</span>
              <span v-else class="typeBadge">UID: {{ targetAccount.uid }}</span>
            </td>
            <td>
              <IbSalesInfo
                url="#"
                :user="targetAccount.agentAccount.user"
                :uid="targetAccount.agentAccount.uid"
                class="me-2"
              />
            </td>
            <td>
              <IbSalesInfo
                url="#"
                :user="targetAccount.salesAccount.user"
                :code="targetAccount.salesAccount.code"
                :uid="targetAccount.salesAccount.uid"
                class="me-2"
              />
            </td>
            <td class="text-center">
              <button
                v-if="!isViewDetail"
                class="btn btn-light btn-success btn-sm me-3"
                @click="emits('changeTargetAccountFn')"
              >
                {{ $t("action.change") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </template>

    <hr />

    <div class="row mb-9 mt-5">
      <label class="form-label fs-6 fw-bold">Search Rebate Schema </label>
      <div class="d-flex">
        <div>
          <el-select
            v-model="rebateSchemaInfo.id"
            clearable
            filterable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="rebateSchemaInfo.loading"
            @change="setUpForm"
          >
            <el-option
              v-for="item in rebateSchemaInfo.options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>

        <button
          v-if="isViewDetail"
          class="btn btn-light btn-success btn-sm ms-5"
          @click="createUpdateDirectRule('update')"
        >
          Update Rebate Schema
        </button>
      </div>
    </div>

    <div
      v-if="rebateSchemaInfo.id != '' && schemaLoading"
      class="d-flex justify-content-center"
    >
      <LoadingRing />
    </div>
    <div v-if="rebateSchemaInfo.id != '' && !schemaLoading">
      <div class="row mb-9">
        <div class="col-3">
          <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
            Schema Name
          </label>
          <Field
            v-model="rebateSchemaInfo.name"
            class="form-control form-control-lg form-control-solid"
            type="number"
            name="symbolRule"
            autocomplete="off"
          >
            <el-input v-model="rebateSchemaInfo.name" size="large"> </el-input>
          </Field>
        </div>
        <div class="col-9">
          <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
            Note
          </label>
          <Field
            v-model="rebateSchemaInfo.note"
            class="form-control form-control-lg form-control-solid"
            type="number"
            name="symbolRule"
            autocomplete="off"
          >
            <el-input v-model="rebateSchemaInfo.note" size="large"> </el-input>
          </Field>
        </div>
      </div>

      <div class="mt-11">
        <div class="fv-row mb-7">
          <ul
            class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
          >
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'rate' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'rate'"
                >Rate</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'pips' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'pips'"
                >Pips</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'commission' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'commission'"
                >Commission</a
              >
            </li>
          </ul>
        </div>

        <!-- Table Header -->
        <div
          v-for="(symbols, category) in selectRebateSchema"
          :key="category"
          style="box-shadow: rgba(99, 99, 99, 0.2) 0px 2px 8px 0px"
          class="mb-7 p-7"
        >
          <div class="d-flex justify-content-between">
            <h3>{{ category }}</h3>
          </div>

          <!-- Table Content -->
          <div class="row">
            <div
              class="col-4 mt-5"
              v-for="(rule, symbol) in symbols"
              :key="symbol"
            >
              <Field
                v-model="selectRebateSchema[category][symbol][formTab]"
                class="form-control form-control-lg form-control-solid"
                type="number"
                name="symbolRule"
                autocomplete="off"
              >
                <el-input
                  v-model="selectRebateSchema[category][symbol][formTab]"
                  size="large"
                >
                  <template #prepend>
                    <div style="width: 80px">
                      <label>{{ symbol }}</label>
                    </div>
                  </template>
                </el-input>
              </Field>
            </div>
          </div>
        </div>

        <div v-if="showErrorMsg" class="text-center fs-4" style="color: red">
          Please input target account number.
        </div>

        <div class="mt-13 d-flex flex-row-reverse">
          <button
            class="btn btn-light btn-danger btn-lg me-3 mb-9"
            @click="resetRebateForm"
          >
            Cancel
          </button>

          <button
            v-if="!isViewDetail"
            class="btn btn-light btn-success btn-lg me-3 mb-9"
            @click="createUpdateDirectRule('create')"
          >
            Apply to Client
          </button>
        </div>
      </div>
    </div>
  </div>

  <!-- ---------------------------------------------------------- -->
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { Field } from "vee-validate";
// import AccountService from "../../services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import RebateService from "../../../rebate/services/RebateService";
import { RebateDistributionTypes } from "@/core/types/RebateDistributionTypes";

const props = defineProps<{
  rebateType: any;
  accountDetails: any;
  fromRebateRuleDetailId: any;
  targetAccount?: any;
  needConfirm?: any;
}>();

const emits = defineEmits<{
  (e: "refresh"): void;
  (e?: "clearSearchResult"): void;
  (e?: "changeTargetAccountFn"): void;
  (e?: "updateRebateRuleDetailId", id): void;
}>();

const { t } = useI18n();
const formTab = ref("rate");
const isLoading = ref(true);
const schemaLoading = ref(true);
const showErrorMsg = ref(false);
const isViewDetail = ref(false);
const targetAccount = ref<any>({});
const selectRebateSchema = ref<any>({});

const rebateSchemaInfo = ref<any>({
  loading: false,
  id: "",
});

const formData = ref({
  sourceAccountUid: props.accountDetails.uid,
  targetAccountUid: props.targetAccount?.uid,
});

const resetRebateForm = () => {
  emits("clearSearchResult");
  formData.value.targetAccountUid = "";
};

const createUpdateDirectRule = async (_op: string) => {
  formData.value["rebateRuleId"] = parseInt(rebateSchemaInfo.value.id);

  try {
    if (_op == "create") {
      await RebateService.postRebateDirectRule(formData.value);
    } else if (_op == "update") {
      await RebateService.putRebateDirectRule(props.fromRebateRuleDetailId, {
        rebateRuleId: formData.value.rebateRuleId,
      });
    }

    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("clearSearchResult");
      emits("refresh");
      resetRebateForm();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const setUpForm = async () => {
  if (rebateSchemaInfo.value.id.length == 0) {
    rebateSchemaInfo.value.id = "";
    return;
  }

  schemaLoading.value = true;

  try {
    const resSymbolList = await RebateService.getSymbolsList();

    selectRebateSchema.value = resSymbolList.reduce((acc, item) => {
      const category = item.category;

      if (!acc[category]) {
        acc[category] = {};
      }

      acc[category][item.code] = {
        symbolCode: item.code,
        rate: 0,
        pips: 0,
        commission: 0,
      };

      return acc;
    }, {});

    const categoryMap = {};
    for (const item of resSymbolList) {
      categoryMap[item.code] = item.category;
    }

    const resSchema = await RebateService.getRebateSchema(
      parseInt(rebateSchemaInfo.value.id)
    );

    rebateSchemaInfo.value.name = resSchema.name;
    rebateSchemaInfo.value.note = resSchema.note;

    resSchema.rebateDirectSchemaItems.forEach(function (item) {
      console.log(item.symbolCode);
      if (
        selectRebateSchema.value[categoryMap[item.symbolCode]] !== undefined
      ) {
        selectRebateSchema.value[categoryMap[item.symbolCode]][
          item.symbolCode
        ] = {
          symbolCode: item.symbolCode,
          rate: item.rate,
          pips: item.pips,
          commission: item.commission,
        };
      }
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  schemaLoading.value = false;
};

const remoteMethod = async (query: string) => {
  if (query) {
    rebateSchemaInfo.value.loading = true;

    const options = await RebateService.getRebateSchemaList({
      keyword: query,
    });

    rebateSchemaInfo.value["options"] = options.data;
    rebateSchemaInfo.value.loading = false;
  } else {
    rebateSchemaInfo.value["options"] = [];
  }
};

const viewDetail = async (_id: any) => {
  try {
    const res = await RebateService.getRebateDirectRuleById(_id);
    rebateSchemaInfo.value.id = [res.rebateDirectSchemaId];

    setUpForm();
  } catch (error) {
    MsgPrompt.error(error);
  }
};

onMounted(async () => {
  if (
    props.fromRebateRuleDetailId != -1 &&
    props.fromRebateRuleDetailId != null
  ) {
    isViewDetail.value = true;
    viewDetail(props.fromRebateRuleDetailId);
  }
  targetAccount.value = props.targetAccount;
  isLoading.value = false;
});
</script>

<style scoped>
#userSideBar {
  width: 50%;
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
</style>
