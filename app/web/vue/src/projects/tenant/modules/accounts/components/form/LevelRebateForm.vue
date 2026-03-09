<template>
  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div v-else>
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
            Reset
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
  (e?: "updateRebateRuleDetailId", id): void;
}>();

const { t } = useI18n();
const formData = ref({});
const formTab = ref("rate");
const isLoading = ref(true);
const schemaLoading = ref(true);
const showErrorMsg = ref(false);
const isViewDetail = ref(false);
const selectRebateSchema = ref<any>({});

const rebateSchemaInfo = ref<any>({
  loading: false,
  id: "",
});

const resetRebateForm = () => {
  setUpForm();
};

const createUpdateDirectRule = async (_op: string) => {
  formData.value["rebateRuleId"] = parseInt(rebateSchemaInfo.value.id);

  try {
    if (_op == "create") {
      await RebateService.putRebateAllocationRule(
        props.accountDetails.rebateClientRule.id,
        {
          distributionType: props.rebateType,
          rebateDirectSchemaId: formData.value.rebateRuleId,
        }
      );
      emits("updateRebateRuleDetailId", formData.value.rebateRuleId);
    } else if (_op == "update") {
      await RebateService.putRebateAllocationRule(
        props.accountDetails.rebateClientRule.id,
        {
          distributionType: props.rebateType,
          rebateDirectSchemaId: formData.value.rebateRuleId,
        }
      );
      emits("updateRebateRuleDetailId", formData.value.rebateRuleId);
    }

    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      resetRebateForm();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
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

const setUpForm = async () => {
  schemaLoading.value = true;

  if (rebateSchemaInfo.value.id.length == 0) {
    rebateSchemaInfo.value.id = "";
    return;
  }

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
      if (!categoryMap[item.symbolCode]) return;
      selectRebateSchema.value[categoryMap[item.symbolCode]][item.symbolCode] =
        {
          symbolCode: item.symbolCode,
          rate: item.rate,
          pips: item.pips,
          commission: item.commission,
        };
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  schemaLoading.value = false;
};

onMounted(async () => {
  if (
    props.fromRebateRuleDetailId != -1 &&
    props.fromRebateRuleDetailId != null
  ) {
    isViewDetail.value = true;
    rebateSchemaInfo.value.id = props.fromRebateRuleDetailId;
    setUpForm();
  }
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
