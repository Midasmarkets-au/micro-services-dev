<template>
  <SiderDetail
    :save="submit"
    :discard="close"
    :title="$t('action.edit')"
    elId="rebate_detail_show"
    :isLoading="isLoading"
    :submited="submited"
    :isDisabled="false"
    :savingTitle="$t('action.saving')"
    :show-footer="false"
    width="{default:'40%'}"
    ref="rebateDetailShowRef"
  >
    <!-- ============================ Base Rule ==================================== -->
    <!-- =========================================================================== -->
    <div class="d-flex align-items-center justify-content-between mb-5">
      <h1>Account Number: {{ rebateDetails.accountId }}</h1>
      <button
        ref="replaceRulFormBtnRef"
        class="btn btn-light btn-success btn-md me-3"
        @click="replaceRebateRule"
      >
        Update by Form
      </button>
    </div>

    <hr />
    <div class="row mb-5">
      <div class="col-4">
        <div class="d-flex flex-column mb-5 fv-row">
          <label class="fs-5 fw-semobold mb-2 required">Rate</label>
          <input
            class="form-control form-control-solid"
            placeholder=""
            name="rate"
            v-model="rebateDetails.rate"
          />
        </div>
      </div>
      <div class="col-4">
        <div class="d-flex flex-column mb-5 fv-row">
          <label class="fs-5 fw-semobold mb-2 required">Pips</label>
          <input
            class="form-control form-control-solid"
            placeholder=""
            name="pips"
            v-model="rebateDetails.pipe"
          />
        </div>
      </div>
      <div class="col-4">
        <div class="d-flex flex-column mb-5 fv-row">
          <label class="fs-5 fw-semobold mb-2 required">Commission</label>
          <input
            class="form-control form-control-solid"
            placeholder=""
            name="commission"
            v-model="rebateDetails.commission"
          />
        </div>
      </div>
    </div>

    <div class="mb-5" style="text-align: right">
      <button
        ref="updateRulBtnRef"
        class="btn btn-light btn-primary btn-sm me-3"
        @click="updateRebateRule"
      >
        <span v-if="btnLoading"
          >Please wait...<span
            class="spinner-border spinner-border-sm align-middle ms-2"
          ></span
        ></span>
        <span v-else> Update </span>
      </button>
    </div>

    <!-- ============================= Symbol Rule form ============================ -->
    <!-- =========================================================================== -->

    <h1 class="mb-5">Rebate Rule by Symbol</h1>
    <hr />
    <div class="mb-5">
      <div v-if="showAddRuleForm">
        <form
          class="form"
          id="kt_modal_new_address_form"
          @submit.prevent="postSymbolRebate"
          style="border: 1px dashed #ccc; padding: 20px; border-radius: 5px"
        >
          <!-- Select Payment Method -->
          <div class="d-flex flex-column mb-5 fv-row">
            <label class="d-flex align-items-center fs-5 fw-semobold mb-2">
              <span class="required">Select a symbol</span>
            </label>

            <Field
              name="symbolRebateId"
              class="form-select form-select-solid"
              as="select"
              v-model="symbolRebate.sid"
            >
              <option value="">
                {{ $t("wallet.deposit.selectMethod") }}
              </option>
              <option v-for="item in symbols" :key="item.id" :value="item.id">
                {{ item.code }}
              </option>
            </Field>
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="symbolRebateId" />
              </div>
            </div>
          </div>

          <div class="row mb-5">
            <div class="col-4">
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="fs-5 fw-semobold mb-2 required">Rate</label>
                <Field
                  class="form-control form-control-solid"
                  placeholder=""
                  name="newRate"
                  v-model="symbolRebate.r"
                />
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="newRate" />
                  </div>
                </div>
              </div>
            </div>
            <div class="col-4">
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="fs-5 fw-semobold mb-2 required">Pips</label>
                <Field
                  class="form-control form-control-solid"
                  placeholder=""
                  name="newPips"
                  v-model="symbolRebate.p"
                />
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="newPips" />
                  </div>
                </div>
              </div>
            </div>
            <div class="col-4">
              <div class="d-flex flex-column mb-5 fv-row">
                <label class="fs-5 fw-semobold mb-2 required">Commission</label>
                <Field
                  class="form-control form-control-solid"
                  placeholder=""
                  name="newCommission"
                  v-model="symbolRebate.c"
                />
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="newCommission" />
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="d-flex justify-content-end">
            <div class="mb-5" style="text-align: right">
              <button
                ref="submitSymRulBtnRef"
                type="submit"
                class="btn btn-light btn-success btn-sm me-3"
                @click="postSymbolRebate"
              >
                <span v-if="postingSymbolRule"
                  >Please wait...<span
                    class="spinner-border spinner-border-sm align-middle ms-2"
                  ></span
                ></span>
                <span v-else>Submits</span>
              </button>
            </div>
            <div class="mb-5" style="text-align: right">
              <button
                class="btn btn-light btn-danger btn-sm me-3"
                @click="toggleAddRuleFormFn(false)"
              >
                Cancel
              </button>
            </div>
          </div>
        </form>
      </div>
      <button v-else class="rebate-long-btn" @click="toggleAddRuleFormFn(true)">
        +
      </button>
    </div>

    <!-- ============================ Symbol Rule table ============================ -->
    <!-- =========================================================================== -->

    <table
      class="table align-middle table-row-dashed fs-6 gy-5"
      id="table_symbol_rebate"
    >
      <thead>
        <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
          <th class="text-center">Symbol</th>
          <th class="text-center">Rate</th>
          <th class="text-center">Pips</th>
          <th class="text-center">Commission</th>
          <th class="text-center min-w-150px">{{ $t("action.action") }}</th>
        </tr>
      </thead>
      <!-- ---------------------------------------------------------------------- -->
      <tbody v-if="waitingSymbolRules">
        <tr>
          <td colspan="12">
            <scale-loader></scale-loader>
          </td>
        </tr>
      </tbody>

      <TransitionGroup
        v-else
        tag="tbody"
        name="table-delete-fade"
        class="table-delete-fade-container text-gray-600 fw-semibold"
      >
        <tr
          v-for="(item, index) in rebateDetails.rules"
          :key="item"
          :class="{ 'table-delete-fade-active': index === deleteIndex }"
        >
          <td class="text-center">{{ symbols[item.sid].code }}</td>

          <td class="text-center">
            <Field
              v-if="editIndex == index"
              class="form-control form-control-solid"
              placeholder=""
              name="ruleRate"
              v-model="item.r"
              style="width: 70px; margin: 0 auto"
            />
            <span v-else>{{ item.r }}</span>
          </td>

          <td class="text-center">
            <Field
              v-if="editIndex == index"
              class="form-control form-control-solid"
              placeholder=""
              name="rulePips"
              v-model="item.p"
              style="width: 70px; margin: 0 auto"
            />
            <span v-else>{{ item.p }}</span>
          </td>

          <td class="text-center">
            <Field
              v-if="editIndex == index"
              class="form-control form-control-solid"
              placeholder=""
              name="ruleCommission"
              v-model="item.c"
              style="width: 70px; margin: 0 auto"
            />
            <span v-else>{{ item.c }}</span>
          </td>

          <td class="text-center">
            <span v-if="editIndex == index">
              <button
                class="btn btn-light btn-success btn-sm me-3"
                data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom-end"
                @click="putSymbolRebate(item)"
              >
                Update
              </button>
              <button
                class="btn btn-light btn-info btn-sm me-3"
                data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom-end"
                @click="editIndex = -1"
              >
                Cancel
              </button>
            </span>
            <span v-else>
              <button
                class="btn btn-light btn-primary btn-sm me-3"
                data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom-end"
                @click="editIndex = index"
              >
                Edit
              </button>
              <button
                class="btn btn-light btn-danger btn-sm me-3"
                data-kt-menu-trigger="click"
                data-kt-menu-placement="bottom-end"
                @click="deleteSymbolRule(item.sid, index)"
              >
                Delete
              </button>
            </span>
          </td>
        </tr>
      </TransitionGroup>
    </table>
    <ReplaceRule ref="replaceRuleRef" />
  </SiderDetail>
</template>

<script setup lang="ts">
import * as Yup from "yup";
import { ref, nextTick } from "vue";
// import { useI18n } from "vue-i18n";
import ReplaceRule from "./AddNewRule.vue";
import RebateService from "../services/RebateService";
import SiderDetail from "@/components/SiderDetail.vue";

import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { RebateInterface, SymbolsCriteria } from "../models/Rebate";

const symbols = ref({} as any);
const replaceRuleRef = ref<any>(null);
const rebateDetails = ref({} as any);
const submitSymRulBtnRef = ref<null | HTMLButtonElement>(null);
const updateRulBtnRef = ref<null | HTMLButtonElement>(null);
const symbolRebate = ref<RebateInterface>({} as RebateInterface);
const rebateDetailShowRef = ref<InstanceType<typeof SiderDetail>>();
const symbolsCriteria = ref<SymbolsCriteria>({} as SymbolsCriteria);
const id = ref();
const editIndex = ref(-1);
const deleteIndex = ref(-1);
const isLoading = ref(true);
const submited = ref(false);
const showAddRuleForm = ref(false);
const btnLoading = ref(false);
const postingSymbolRule = ref(false);
const waitingSymbolRules = ref(true);

const { handleSubmit: handleSymbolRule, resetForm: resetSymbolRuleForm } =
  useForm({
    validationSchema: Yup.object().shape({
      symbolRebateId: Yup.string().required().label("Symbol"),
      newRate: Yup.number().required().label("Rate"),
      newPips: Yup.number().required().label("Pips"),
      newCommission: Yup.number().required().label("Commission"),
    }),
  });

const toggleAddRuleFormFn = async (_show: boolean) => {
  resetSymbolRuleForm();
  await nextTick();
  showAddRuleForm.value = _show;
};

const reset = () => {
  resetSymbolRuleForm();
  editIndex.value = -1;
  isLoading.value = true;
  submited.value = false;
  showAddRuleForm.value = false;
  waitingSymbolRules.value = true;
};

const show = async (_id: number) => {
  rebateDetailShowRef.value?.show();
  reset();

  getRebateRule(_id);
  id.value = _id;

  try {
    symbolsCriteria.value.size = 100;
    symbolsCriteria.value.sortField = "id";
    symbolsCriteria.value.sortFlag = false;
    const responseBody = await RebateService.getSymbols(symbolsCriteria.value);
    symbolsCriteria.value = responseBody.criteria;

    responseBody.data.forEach(function (item) {
      symbols.value[item.id] = {
        id: item.id,
        code: item.code,
      };
    });

    isLoading.value = false;
    waitingSymbolRules.value = false;
  } catch (error) {
    console.log(error);
  }
};

const getRebateRule = async (_id: number) => {
  waitingSymbolRules.value = true;

  try {
    rebateDetails.value = await RebateService.getRebateSchema(_id);
    rebateDetails.value.rules = Array.from(rebateDetails.value.rules);
  } catch (error) {
    // console.log(error);
  }

  setTimeout(() => {
    waitingSymbolRules.value = false;
  }, 1000);
};

const postSymbolRebate = handleSymbolRule(async () => {
  if (!submitSymRulBtnRef.value) return;

  postingSymbolRule.value = true;
  submitSymRulBtnRef.value.disabled = true;

  try {
    await RebateService.postSymbolRebate(
      rebateDetails.value.id,
      symbolRebate.value
    );
    getRebateRule(id.value);
  } catch (error) {
    // console.log(error);
  }

  resetSymbolRuleForm();

  setTimeout(() => {
    postingSymbolRule.value = false;
    submitSymRulBtnRef.value.disabled = false;
  }, 1000);
});

const deleteSymbolRule = async (_sid: number, _index: number) => {
  deleteIndex.value = _index;

  // timeout for after animation 0.2s
  setTimeout(async () => {
    try {
      await RebateService.deleteSymbolRebate(id.value, _sid);
      deleteIndex.value = -1;
      rebateDetails.value.rules.splice(_index, 1);
    } catch (error) {
      console.log(error);
    }
  }, 200);
};

const putSymbolRebate = async (_item: object) => {
  try {
    await RebateService.putSymbolRebate(rebateDetails.value.id, _item);
    editIndex.value = -1;
  } catch (error) {
    // console.log(error);
  }
};

const updateRebateRule = async () => {
  if (!updateRulBtnRef.value) return;

  btnLoading.value = true;
  updateRulBtnRef.value.disabled = true;

  try {
    await RebateService.putRebateRule(rebateDetails.value.id, {
      commission: rebateDetails.value.commission,
      rate: rebateDetails.value.rate,
      pipe: rebateDetails.value.pipe,
    });
  } catch (error) {
    // console.log(error);
  }

  setTimeout(() => {
    btnLoading.value = false;
    updateRulBtnRef.value.disabled = false;
  }, 1000);
};

const replaceRebateRule = () => {
  replaceRuleRef.value.show(rebateDetails.value);
  rebateDetailShowRef.value?.hide();
};

const submit = async () => {
  isLoading.value = true;
};
const close = () => {
  rebateDetailShowRef.value?.hide();
};

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
</style>
