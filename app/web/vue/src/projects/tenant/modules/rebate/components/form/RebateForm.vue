<template>
  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div v-else>
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
        </tr>
      </thead>
      <tbody class="fw-semibold text-gray-900">
        <tr>
          <!--  -->
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
        </tr>
      </tbody>
    </table>

    <hr />
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
        v-for="(symbols, category) in editableRebateSymbols"
        :key="category"
        style="box-shadow: rgba(99, 99, 99, 0.2) 0px 2px 8px 0px"
        class="mb-7 p-7"
      >
        <div class="d-flex justify-content-between">
          <h3>{{ category }}</h3>
          <div class="w-200px">
            <Field
              v-model="applyCategoryForm[formTab][category]"
              class="form-control form-control-lg form-control-solid"
              type="text"
              name="applyCategory"
              autocomplete="off"
            >
              <el-input
                v-model="applyCategoryForm[formTab][category]"
                :placeholder="$t('tip.pleaseInput')"
                size="large"
              >
                <template #append>
                  <el-button @click.prevent="applyCategoryValue(category)">
                    <span>Apply</span>
                  </el-button>
                </template>
              </el-input>
            </Field>
          </div>
        </div>

        <!-- Table Content -->
        <div class="row">
          <div
            class="col-4 mt-5"
            v-for="(rule, symbol) in symbols"
            :key="symbol"
          >
            <Field
              v-model="editableRebateSymbols[category][symbol][formTab]"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="editableRebateSymbols[category][symbol][formTab]"
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

      <div class="mt-13 d-flex flex-row-reverse">
        <button
          class="btn btn-light btn-danger btn-lg me-3 mb-9"
          @click="resetRebateForm"
        >
          Cancel
        </button>
        <button
          class="btn btn-light btn-primary btn-lg me-3 mb-9"
          @click="confirmDirectRule()"
        >
          Confirm
        </button>
        <button
          class="btn btn-light btn-success btn-lg me-3 mb-9"
          @click="updateDirectRule()"
        >
          Update Rebate Rule
        </button>
      </div>
    </div>
  </div>

  <!-- ---------------------------------------------------------- -->
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { rebateSymbols, rebateSymbolCategory } from "@/core/data/rebateSymbols";
import { Field, ErrorMessage, useForm } from "vee-validate";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import RebateService from "../../services/RebateService";

const props = defineProps<{
  fromRebateRuleDetailId: any;
  sourceAccountId: any;
}>();

const emits = defineEmits<{
  (e: "refresh"): void;
  (e: "hide"): void;
}>();

const isLoading = ref(true);
const editableRebateSymbols = ref<any>({});
const originalRebateSymbols = ref<any>({});
const targetAccount = ref<any>({});
const formTab = ref("rate");
const { t } = useI18n();

const formData = ref({
  sourceAccountId: props.sourceAccountId,
  targetAccountId: "",
});

const applyCategoryForm = ref({
  rate: {},
  commission: {},
  pips: {},
});

const resetCategoryInput = () => {
  applyCategoryForm.value = {
    rate: {},
    commission: {},
    pips: {},
  };
};

const resetRebateForm = () => {
  editableRebateSymbols.value = JSON.parse(
    JSON.stringify(originalRebateSymbols.value)
  );

  resetCategoryInput();
  emits("hide");
};

const applyCategoryValue = (_category: string) => {
  Object.keys(editableRebateSymbols.value[_category]).forEach((key) => {
    editableRebateSymbols.value[_category][key][formTab.value] =
      applyCategoryForm.value[formTab.value][_category];
  });
};

const updateDirectRule = async () => {
  const data = {
    sourceTradeAccountId: formData.value.sourceAccountId,
    targetAccountId: formData.value.targetAccountId,
    RebateRuleItems: Object.keys(editableRebateSymbols.value).reduce(
      (result: any, category) => {
        const symbols = editableRebateSymbols.value[category];
        Object.keys(symbols).forEach((symbolCode) => {
          const { rate, pips, commission } = symbols[symbolCode];
          result.push({ symbolCode, rate, pips, commission });
        });
        return result;
      },
      []
    ),
  };

  try {
    await AccountService.updateSourceToTargetRebateRule(
      props.fromRebateRuleDetailId,
      data.RebateRuleItems
    );

    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      resetCategoryInput();
      emits("refresh");
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const confirmDirectRule = async () => {
  try {
    await RebateService.putConfirmRebateRule(props.fromRebateRuleDetailId);

    MsgPrompt.success(t("tip.confirmSuccess")).then(() => {
      emits("refresh");
      emits("hide");
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const setUpForm = async () => {
  isLoading.value = true;

  resetCategoryInput();
  editableRebateSymbols.value = JSON.parse(JSON.stringify(rebateSymbols));

  try {
    const res = await RebateService.getRebateSchema(
      props.fromRebateRuleDetailId
    );
    res.rebateRuleItems.forEach((symbol) => {
      const { symbolCode, rate, pips, commission } = symbol;
      const category = rebateSymbolCategory[symbolCode];
      if (editableRebateSymbols.value[category][symbolCode] !== undefined) {
        editableRebateSymbols.value[category][symbolCode] = {
          symbolCode: symbolCode,
          rate: rate,
          pips: pips,
          commission: commission,
        };
      }
    });

    originalRebateSymbols.value = JSON.parse(
      JSON.stringify(editableRebateSymbols.value)
    );

    const res2 = await AccountService.queryAccounts({
      ids: [res.targetAccountId],
    });

    targetAccount.value = res2.data[0];
    formData.value.targetAccountId = res.targetAccountId;
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

onMounted(async () => {
  setUpForm();
});

watch(
  () => props.fromRebateRuleDetailId,
  async () => {
    setUpForm();
  }
);
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
